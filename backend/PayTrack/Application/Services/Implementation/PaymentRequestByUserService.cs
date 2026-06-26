// <copyright file="PaymentRequestByUserService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Helpers;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class PaymentRequestByUserService(
        ITransactionRepository repo,
        ITeamService _teamService,
        IFileRepository _fileRepo,
        IBankAccountService _bankAccountService,
        ICostCentreService _costCentreService,
        IBudgetService _budgetService,
        INotificationDispatchService? _notificationDispatchService = null,
        ILogger<PaymentRequestByUserService>? _logger = null,
        IPushNotificationService? _pushNotifications = null,
        ISystemSettingService? _systemSettings = null) : IPaymentRequestByUserService
    {
        private const int MaxDuplicateResults = 10;

        /// <summary>
        /// Repository for PaymentRequestByUsers.
        /// </summary>
        private readonly ITransactionRepository repo = repo;
        private readonly IFileRepository fileRepo = _fileRepo;
        private readonly ITeamService teamService = _teamService;
        private readonly IBankAccountService bankAccountService = _bankAccountService;
        private readonly ICostCentreService costCentreService = _costCentreService;
        private readonly IBudgetService budgetService = _budgetService;
        private readonly INotificationDispatchService? notificationDispatchService = _notificationDispatchService;
        private readonly ILogger<PaymentRequestByUserService>? logger = _logger;
        private readonly IPushNotificationService? pushNotifications = _pushNotifications;
        private readonly ISystemSettingService? systemSettings = _systemSettings;

        /// <inheritdoc/>
        public async Task<(List<PaymentRequestByUser> paymentRequestByUser, int totalCount)> GetAllAsync(
            GetPaymentRequestByUserQuery? query = null)
        {
            return await this.repo.GetAllAsync(query);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser?> GetPaymentRequestByUserByIdAsync(int id, GetPaymentRequestByUserQueryById? query = null)
        {
            return await this.repo.GetByIdAsync(id, query);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> CreatePaymentRequestByUserAsync(
            int userId,
            int teamId,
            decimal amount,
            string purposeOfPayment,
            IFormFile receipt,
            DateTime PaidAt,
            string invoiceNumber,
            string? comment,
            PayoutType payoutType,
            int? bankAccountId,
            string? creditorName,
            DateTime? dueDate)
        {
            var team = await this.teamService.GetTeamByIdAsync(teamId) ?? throw new NotFoundException("Team could not be found");

            if (PaidAt.Date > DateTime.Today)
            {
                throw new InvalidStateException("Paid at cannot be in the future!");
            }

            // Ensure bank account id is set if payout type is user
            if (payoutType == PayoutType.User)
            {
                if (!bankAccountId.HasValue)
                {
                    throw new InvalidStateException("If the money should be paid out to you, you must specify a bankAccount");
                }

                var bankAccounts = await this.bankAccountService.GetBankAccountsAsync(userId) ?? throw new NotFoundException("Bank Accounts could not be found");

                if (!bankAccounts.Any(b => b.Id == bankAccountId.Value))
                {
                    throw new InvalidStateException("Could not find specified bank account");
                }
            }
            else
            {
                bankAccountId = null;
            }

            if (payoutType == PayoutType.NotYetPaid && string.IsNullOrWhiteSpace(creditorName))
            {
                throw new InvalidStateException("Creditor name is required when the payout type is NotYetPaid");
            }

            if (payoutType == PayoutType.NotYetPaid && !dueDate.HasValue)
            {
                throw new InvalidStateException("Due date is required when the payout type is NotYetPaid");
            }

            var isAlreadyPaid = payoutType == PayoutType.AlreadyPaid;

            var paymentRequest = new PaymentRequestByUser
            {
                // Transaction settings
                UserId = userId,
                Amount = amount,
                PurposeOfPayment = purposeOfPayment,
                PaymentReference = string.Empty, // Payment reference will be set later by the finance team
                Status = isAlreadyPaid ? TransactionStatus.Paid : TransactionStatus.Submitted,
                BudgetId = null, // Budget will be set later by the finance team
                TeamId = team.Id,
                PaymentDirection = PaymentDirection.Out, // Payment direction is out for payment requests by user

                // Created at is set automatically
                PaidAt = PaidAt.ToUniversalTime(),

                // Payment request specific settings
                InvoiceNumber = invoiceNumber,
                Comment = comment,
                ReceiptUrl = string.Empty, // will be set in the repo later
                PayoutType = payoutType,
                BankAccountId = bankAccountId,
                CreditorName = payoutType == PayoutType.NotYetPaid ? creditorName : null,
                DueDate = payoutType == PayoutType.NotYetPaid ? dueDate?.ToUniversalTime() : null,
                StatusHistory = isAlreadyPaid
                    ?
                    [
                        new TransactionStatusHistory
                        {
                            ChangedById = userId,
                            FromStatus = TransactionStatus.Paid,
                            ToStatus = TransactionStatus.Paid,
                            ChangedAt = DateTime.UtcNow,
                        },
                    ]
                    : [],
            };

            return await this.repo.AddAsync(paymentRequest, receipt);
        }

        /// <inheritdoc/>
        public async Task<List<DuplicatePaymentRequestByUserMatch>> GetDuplicatePaymentRequestsByUserAsync(
            int userId,
            int teamId,
            decimal amount,
            DateTime paidAt,
            string? invoiceNumber = null,
            int? paymentRequestByUserId = null,
            bool includeOtherUsers = false)
        {
            var matchUserId = userId;
            var matchTeamId = teamId;
            var matchAmount = amount;
            var matchPaidAt = paidAt;
            var matchInvoiceNumber = invoiceNumber;

            if (paymentRequestByUserId.HasValue)
            {
                var sourcePaymentRequest = await this.repo.GetByIdAsync(paymentRequestByUserId.Value, new GetPaymentRequestByUserQueryById())
                    ?? throw new NotFoundException("PaymentRequestByUser could not be found");

                if (!sourcePaymentRequest.PaidAt.HasValue)
                {
                    throw new InvalidStateException("Duplicate lookup is missing paid date.");
                }

                matchUserId = sourcePaymentRequest.UserId;
                matchTeamId = sourcePaymentRequest.TeamId;
                matchAmount = sourcePaymentRequest.Amount;
                matchPaidAt = sourcePaymentRequest.PaidAt.Value;
                matchInvoiceNumber = sourcePaymentRequest.InvoiceNumber;
            }

            var duplicateCandidates = await this.repo.GetPotentialDuplicatesAsync(
                matchUserId,
                matchTeamId,
                matchAmount,
                matchPaidAt,
                matchInvoiceNumber,
                paymentRequestByUserId,
                includeOtherUsers);

            return duplicateCandidates
                .Where(paymentRequestByUser => includeOtherUsers || paymentRequestByUser.UserId == matchUserId)
                .Select(paymentRequestByUser => this.CreateDuplicateMatch(paymentRequestByUser, matchUserId, matchTeamId, matchAmount, matchPaidAt, matchInvoiceNumber))
                .Where(duplicateMatch => duplicateMatch.Score >= DuplicatePaymentRequestByUserScorer.MatchThreshold)
                .OrderByDescending(duplicateMatch => duplicateMatch.Score)
                .ThenByDescending(duplicateMatch => duplicateMatch.PaymentRequestByUser.CreatedAt)
                .Take(MaxDuplicateResults)
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> UpdatePaymentRequestByUserAsync(
            int id,
            int? teamId = null,
            decimal? amount = null,
            string? purposeOfPayment = null,
            DateTime? paidAt = null,
            string? invoiceNumber = null,
            string? comment = null,
            PayoutType? payoutType = null,
            int? bankAccountId = null)
        {
            var transaction = await this.repo.GetByIdAsync(id, new GetPaymentRequestByUserQueryById())
                ?? throw new NotFoundException("Transaction not found");

            if (teamId.HasValue)
            {
                var team = await this.teamService.GetTeamByIdAsync(teamId.Value)
                    ?? throw new NotFoundException("Team not found");

                transaction.TeamId = team.Id;
            }

            if (amount.HasValue)
            {
                transaction.Amount = amount.Value;
            }

            if (purposeOfPayment != null)
            {
                transaction.PurposeOfPayment = purposeOfPayment;
            }

            if (paidAt.HasValue)
            {
                transaction.PaidAt = paidAt.Value;
            }

            if (invoiceNumber != null)
            {
                transaction.InvoiceNumber = invoiceNumber;
            }

            if (comment != null)
            {
                transaction.Comment = comment;
            }

            if (payoutType.HasValue)
            {
                transaction.PayoutType = payoutType.Value;
            }

            if (bankAccountId.HasValue)
            {
                var bankAccounts = await this.bankAccountService.GetBankAccountsAsync(transaction.UserId) ?? throw new NotFoundException("Bank Accounts could not be found");

                if (!bankAccounts.Any(b => b.Id == bankAccountId.Value))
                {
                    throw new InvalidStateException("Could not find specified bank account");
                }

                transaction.BankAccountId = bankAccountId;
            }

            return await this.repo.UpdateAsync(transaction);
        }

        /// <inheritdoc/>
        public async Task DeletePaymentRequestByUserAsync(int id)
        {
            var wasDeleted = await this.repo.DeletePaymentRequestByUserAsync(id);

            if (!wasDeleted)
            {
                throw new NotFoundException("PaymentRequestByUser could not be found");
            }
        }

        /// <inheritdoc/>
        public async Task DismissDuplicatePaymentRequestByUserAsync(int paymentRequestByUserId, int duplicatePaymentRequestByUserId)
        {
            if (paymentRequestByUserId == duplicatePaymentRequestByUserId)
            {
                throw new InvalidStateException("A payment request cannot be a duplicate of itself.");
            }

            await this.repo.DismissDuplicatePaymentRequestByUserAsync(paymentRequestByUserId, duplicatePaymentRequestByUserId);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> MarkPaymentRequestByUserAsPaidAsync(
            int id,
            int changedById,
            string paymentReference,
            string purposeOfPayment,
            DateTime paymentDate)
        {
            var transaction = await this.repo.GetByIdAsync(
                    id,
                    new GetPaymentRequestByUserQueryById
                    {
                        IncludeUser = true,
                        IncludeStatusHistory = true,
                    })
                ?? throw new NotFoundException("Transaction not found");

            if (string.IsNullOrWhiteSpace(paymentReference))
            {
                throw new InvalidStateException("Payment reference is required");
            }

            if (paymentReference.Trim().Length < 3)
            {
                throw new InvalidStateException("Payment reference must be at least 3 characters long");
            }

            if (string.IsNullOrWhiteSpace(purposeOfPayment))
            {
                throw new InvalidStateException("Purpose of payment is required");
            }

            if (purposeOfPayment.Trim().Length < 3)
            {
                throw new InvalidStateException("Purpose of payment must be at least 3 characters long");
            }

            if (paymentDate.Date > DateTime.Today)
            {
                throw new InvalidStateException("Payment date cannot be in the future!");
            }

            var normalizedPaymentDate = DateTime.SpecifyKind(paymentDate, DateTimeKind.Utc);

            transaction.PaymentReference = paymentReference.Trim();
            transaction.PurposeOfPayment = purposeOfPayment.Trim();
            transaction.FinancePaidAt = normalizedPaymentDate;

            AddStatusHistory(
                transaction,
                TransactionStatus.Paid,
                changedById,
                $"Payment reference: {transaction.PaymentReference}");

            return await this.SaveStatusChangeAndNotifyAsync(
                transaction,
                SystemSettingKeys.NotificationsInvoicePaymentCompletedEmail,
                SystemSettingKeys.NotificationsInvoicePaymentCompletedSlack,
                SystemSettingKeys.NotificationsInvoicePaymentCompletedPush,
                "Payment completed",
                "Your invoice payment has been completed.");
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> ApprovePaymentRequestByUserAsync(
            int id,
            int changedById,
            int budgetId,
            string? reason)
        {
            var transaction = await this.repo.GetByIdAsync(
                    id,
                    new GetPaymentRequestByUserQueryById
                    {
                        IncludeUser = true,
                        IncludeStatusHistory = true,
                    })
                ?? throw new NotFoundException("Transaction not found");

            if (budgetId <= 0)
            {
                throw new InvalidStateException("Budget is required");
            }

            var budget = await this.budgetService.GetByIdAsync(budgetId)
                ?? throw new NotFoundException("Budget not found");

            if (budget.TeamId != transaction.TeamId)
            {
                throw new InvalidStateException("Budget does not belong to the invoice team");
            }

            transaction.BudgetId = budget.Id;

            AddStatusHistory(
                transaction,
                TransactionStatus.Approved,
                changedById,
                NormalizeOptionalReason(reason));

            return await this.SaveStatusChangeAndNotifyAsync(
                transaction,
                SystemSettingKeys.NotificationsInvoiceApprovalEmail,
                SystemSettingKeys.NotificationsInvoiceApprovalSlack,
                SystemSettingKeys.NotificationsInvoiceApprovalPush,
                "Invoice approved",
                "Your submitted invoice has been approved.");
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> DeclinePaymentRequestByUserAsync(
            int id,
            int changedById,
            string reason)
        {
            var transaction = await this.repo.GetByIdAsync(
                    id,
                    new GetPaymentRequestByUserQueryById
                    {
                        IncludeUser = true,
                        IncludeStatusHistory = true,
                    })
                ?? throw new NotFoundException("Transaction not found");

            var normalizedReason = NormalizeRequiredReason(reason, "Decline reason is required");
            AddStatusHistory(
                transaction,
                TransactionStatus.Declined,
                changedById,
                normalizedReason);

            return await this.SaveStatusChangeAndNotifyAsync(
                transaction,
                SystemSettingKeys.NotificationsInvoiceRejectionEmail,
                SystemSettingKeys.NotificationsInvoiceRejectionSlack,
                SystemSettingKeys.NotificationsInvoiceRejectionPush,
                "Invoice rejected",
                $"Your submitted invoice was rejected: {normalizedReason}");
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> RequestChangesPaymentRequestByUserAsync(
            int id,
            int changedById,
            string reason)
        {
            var transaction = await this.repo.GetByIdAsync(
                    id,
                    new GetPaymentRequestByUserQueryById
                    {
                        IncludeUser = true,
                        IncludeStatusHistory = true,
                    })
                ?? throw new NotFoundException("Transaction not found");

            var normalizedReason = NormalizeRequiredReason(reason, "Change request reason is required");
            AddStatusHistory(
                transaction,
                TransactionStatus.ChangesRequested,
                changedById,
                normalizedReason);

            return await this.SaveStatusChangeAndNotifyAsync(
                transaction,
                SystemSettingKeys.NotificationsInvoiceChangesRequestedEmail,
                SystemSettingKeys.NotificationsInvoiceChangesRequestedSlack,
                SystemSettingKeys.NotificationsInvoiceChangesRequestedPush,
                "Invoice changes requested",
                $"Changes were requested for your invoice: {normalizedReason}");
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> ResubmitPaymentRequestByUserAsync(
            int id,
            int userId,
            int teamId,
            decimal amount,
            string purposeOfPayment,
            DateTime paidAt,
            string invoiceNumber,
            string? comment,
            PayoutType payoutType,
            int? bankAccountId,
            string? creditorName,
            DateTime? dueDate,
            IFormFile? receipt)
        {
            var transaction = await this.repo.GetByIdAsync(
                    id,
                    new GetPaymentRequestByUserQueryById
                    {
                        IncludeUser = true,
                        IncludeStatusHistory = true,
                    })
                ?? throw new NotFoundException("Transaction not found");

            if (transaction.UserId != userId)
            {
                throw new ForbiddenException("You do not have permission to edit this invoice.");
            }

            if (transaction.Status != TransactionStatus.ChangesRequested)
            {
                throw new InvalidStateException("Only invoices with requested changes can be edited");
            }

            var team = await this.teamService.GetTeamByIdAsync(teamId)
                ?? throw new NotFoundException("Team could not be found");

            if (paidAt.Date > DateTime.Today)
            {
                throw new InvalidStateException("Paid at cannot be in the future!");
            }

            if (payoutType == PayoutType.User)
            {
                if (!bankAccountId.HasValue)
                {
                    throw new InvalidStateException("If the money should be paid out to you, you must specify a bankAccount");
                }

                var bankAccounts = await this.bankAccountService.GetBankAccountsAsync(userId)
                    ?? throw new NotFoundException("Bank Accounts could not be found");

                if (!bankAccounts.Any(bankAccount => bankAccount.Id == bankAccountId.Value))
                {
                    throw new InvalidStateException("Could not find specified bank account");
                }
            }
            else
            {
                bankAccountId = null;
            }

            if (payoutType == PayoutType.NotYetPaid && string.IsNullOrWhiteSpace(creditorName))
            {
                throw new InvalidStateException("Creditor name is required");
            }

            if (payoutType == PayoutType.NotYetPaid && !dueDate.HasValue)
            {
                throw new InvalidStateException("Due date is required");
            }

            transaction.TeamId = team.Id;
            transaction.Amount = amount;
            transaction.PurposeOfPayment = purposeOfPayment;
            transaction.PaidAt = paidAt.ToUniversalTime();
            transaction.InvoiceNumber = invoiceNumber;
            transaction.Comment = comment;
            transaction.PayoutType = payoutType;
            transaction.BankAccountId = bankAccountId;
            transaction.CreditorName = payoutType == PayoutType.NotYetPaid ? creditorName : null;
            transaction.DueDate = payoutType == PayoutType.NotYetPaid
                ? dueDate?.ToUniversalTime()
                : null;

            if (receipt != null)
            {
                transaction.ReceiptUrl = await this.fileRepo.SaveFile(
                    receipt,
                    $"invoice_{transaction.InvoiceNumber}_{DateTime.UtcNow:yyyyMMdd_HHmmss}");
            }

            AddStatusHistory(
                transaction,
                TransactionStatus.Review,
                userId,
                "Invoice resubmitted after requested changes");

            return await this.SaveStatusChangeAndNotifyAsync(transaction);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> UndoLastStatusChangeAsync(
            int id,
            int changedById)
        {
            var transaction = await this.repo.GetByIdAsync(
                    id,
                    new GetPaymentRequestByUserQueryById
                    {
                        IncludeUser = true,
                        IncludeStatusHistory = true,
                    })
                ?? throw new NotFoundException("Transaction not found");

            var latestStatusChange = transaction.StatusHistory
                .OrderByDescending(entry => entry.ChangedAt)
                .FirstOrDefault(entry => entry.ToStatus == transaction.Status)
                ?? throw new InvalidStateException("No status change can be undone");

            UndoStatusChange(transaction, latestStatusChange.FromStatus, changedById);

            return await this.SaveStatusChangeAndNotifyAsync(transaction);
        }

        /// <inheritdoc/>
        public async Task<(byte[] content, string contentType)> GetReceiptForPaymentRequestByUserByIdAsync(int id)
        {
            var paymentRequest = await this.GetPaymentRequestByUserByIdAsync(id);

            if (string.IsNullOrEmpty(paymentRequest?.ReceiptUrl))
            {
                throw new InvalidStateException("Receipt URL is null although it should not be.");
            }

            var content = await this.fileRepo.GetByPath(paymentRequest.ReceiptUrl);
            var contentType = GetContentTypeFromPath(paymentRequest.ReceiptUrl);
            return (content, contentType);
        }

        /// <inheritdoc/>
        public bool ValidateQuery(GetPaymentRequestByUserQuery query, User currentUser)
        {
            return currentUser.Role switch
            {
                Role.RegularUser => query.UserId == currentUser.Id,

                Role.TeamLead => query.UserId == currentUser.Id
                                  || (currentUser.TeamId.HasValue
                                      && query.TeamId == currentUser.TeamId),

                Role.Admin => true,

                _ => false
            };
        }

        /// <inheritdoc/>
        public bool ValidateAccessToInvoice(PaymentRequestByUser invoice, User currentUser)
        {
            return currentUser.Role switch
            {
                Role.RegularUser => invoice.UserId == currentUser.Id,

                Role.TeamLead => currentUser.TeamId.HasValue
                                && invoice.TeamId == currentUser.TeamId,

                Role.Admin => true,

                _ => false
            };
        }

        private static bool IsStatusTransitionAllowed(TransactionStatus fromStatus, TransactionStatus toStatus)
        {
            if (fromStatus == toStatus)
            {
                return false;
            }

            if (toStatus == TransactionStatus.Declined)
            {
                return fromStatus != TransactionStatus.Paid;
            }

            return (fromStatus, toStatus) switch
            {
                (TransactionStatus.Submitted, TransactionStatus.Approved) => true,
                (TransactionStatus.Submitted, TransactionStatus.ChangesRequested) => true,
                (TransactionStatus.ChangesRequested, TransactionStatus.Review) => true,
                (TransactionStatus.Review, TransactionStatus.ChangesRequested) => true,
                (TransactionStatus.Review, TransactionStatus.Approved) => true,
                (TransactionStatus.Approved, TransactionStatus.Paid) => true,
                _ => false,
            };
        }

        private static string GetStatusLabel(TransactionStatus status)
        {
            return status switch
            {
                TransactionStatus.ChangesRequested => "Changes requested",
                TransactionStatus.Review => "In review",
                _ => status.ToString(),
            };
        }

        private static string FormatNotificationComment(string? comment)
        {
            return string.IsNullOrWhiteSpace(comment) ? string.Empty : $"Comment: {comment.Trim()}";
        }

        private static void AddStatusHistory(
            PaymentRequestByUser transaction,
            TransactionStatus toStatus,
            int changedById,
            string? comment)
        {
            if (!IsStatusTransitionAllowed(transaction.Status, toStatus))
            {
                throw new InvalidStateException($"Cannot change invoice status from {transaction.Status} to {toStatus}");
            }

            var previousStatus = transaction.Status;
            transaction.Status = toStatus;
            transaction.StatusHistory.Add(new TransactionStatusHistory
            {
                TransactionId = transaction.Id,
                ChangedById = changedById,
                FromStatus = previousStatus,
                ToStatus = toStatus,
                ChangedAt = DateTime.UtcNow,
                Comment = comment,
            });
        }

        private static void UndoStatusChange(
            PaymentRequestByUser transaction,
            TransactionStatus restoredStatus,
            int changedById)
        {
            var previousStatus = transaction.Status;
            transaction.Status = restoredStatus;

            if (previousStatus == TransactionStatus.Approved)
            {
                transaction.BudgetId = null;
            }

            if (previousStatus == TransactionStatus.Paid)
            {
                transaction.FinancePaidAt = null;
                transaction.PaymentReference = string.Empty;
            }

            transaction.StatusHistory.Add(new TransactionStatusHistory
            {
                TransactionId = transaction.Id,
                ChangedById = changedById,
                FromStatus = previousStatus,
                ToStatus = restoredStatus,
                ChangedAt = DateTime.UtcNow,
                Comment = "Undo status change",
            });
        }

        private static string NormalizeRequiredReason(string reason, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new InvalidStateException(errorMessage);
            }

            var normalizedReason = reason.Trim();
            if (normalizedReason.Length < 3)
            {
                throw new InvalidStateException("Reason must be at least 3 characters long");
            }

            return normalizedReason;
        }

        private static string? NormalizeOptionalReason(string? reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return null;
            }

            var normalizedReason = reason.Trim();
            if (normalizedReason.Length < 3)
            {
                throw new InvalidStateException("Reason must be at least 3 characters long");
            }

            return normalizedReason;
        }

        private static string GetContentTypeFromPath(string filePath)
        {
            return Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream",
            };
        }

        private async Task<PaymentRequestByUser> SaveStatusChangeAndNotifyAsync(
            PaymentRequestByUser transaction,
            string? emailSettingKey = null,
            string? slackSettingKey = null,
            string? pushSettingKey = null,
            string? pushTitle = null,
            string? pushBody = null)
        {
            var updatedTransaction = await this.repo.UpdateAsync(transaction);
            await this.NotifyUserAboutStatusChangeAsync(updatedTransaction, emailSettingKey, slackSettingKey);
            if (pushTitle != null
                && pushBody != null
                && await this.IsNotificationEnabledAsync(pushSettingKey, true))
            {
                await this.SendInvoiceStatusPushAsync(updatedTransaction, pushTitle, pushBody);
            }

            return updatedTransaction;
        }

        private async Task NotifyUserAboutStatusChangeAsync(
            PaymentRequestByUser transaction,
            string? emailSettingKey,
            string? slackSettingKey)
        {
            if (this.notificationDispatchService == null
                || transaction.User == null
                || string.IsNullOrWhiteSpace(transaction.User.Email))
            {
                return;
            }

            var statusLabel = GetStatusLabel(transaction.Status);
            var latestComment = transaction.StatusHistory
                .OrderByDescending(entry => entry.ChangedAt)
                .FirstOrDefault()
                ?.Comment;
            var subject = $"Invoice {transaction.InvoiceNumber} status changed to {statusLabel}";
            var body = $"""
                Hello {transaction.User.Name},

                the status of invoice {transaction.InvoiceNumber} has changed.

                New status: {statusLabel}
                {FormatNotificationComment(latestComment)}

                Best regards,
                PayTrack
                """;

            if (await this.IsNotificationEnabledAsync(emailSettingKey, true))
            {
                try
                {
                    await this.notificationDispatchService.SendEmailAsync(
                        transaction.User.Email,
                        subject,
                        body);
                }
                catch (Exception exception)
                {
                    this.logger?.LogError(
                        exception,
                        "Sending status change notification email for invoice {InvoiceNumber} to {RecipientEmail} failed.",
                        transaction.InvoiceNumber,
                        transaction.User.Email);
                }
            }

            if (await this.IsNotificationEnabledAsync(slackSettingKey, false))
            {
                try
                {
                    await this.notificationDispatchService.SendSlackAsync(
                        transaction.User.Email,
                        $"{subject}\n\n{body}");
                }
                catch (Exception exception)
                {
                    this.logger?.LogError(
                        exception,
                        "Sending Slack status change notification for invoice {InvoiceNumber} to {RecipientEmail} failed.",
                        transaction.InvoiceNumber,
                        transaction.User.Email);
                }
            }
        }

        private async Task<bool> IsNotificationEnabledAsync(string? settingKey, bool defaultValue)
        {
            if (settingKey == null || this.systemSettings == null)
            {
                return defaultValue;
            }

            return await this.systemSettings.GetBoolSettingAsync(settingKey, defaultValue);
        }

        private async Task SendInvoiceStatusPushAsync(PaymentRequestByUser transaction, string title, string body)
        {
            if (this.pushNotifications is null)
            {
                return;
            }

            var purpose = string.IsNullOrWhiteSpace(transaction.PurposeOfPayment)
                ? $"Invoice #{transaction.Id}"
                : transaction.PurposeOfPayment.Trim();

            await this.pushNotifications.SendWorkflowStatusChangedAsync(
                transaction.UserId,
                title,
                $"{body}\n{purpose}",
                $"/my-invoices/{transaction.Id}");
        }

        private DuplicatePaymentRequestByUserMatch CreateDuplicateMatch(
            PaymentRequestByUser paymentRequestByUser,
            int userId,
            int teamId,
            decimal amount,
            DateTime paidAt,
            string? invoiceNumber)
        {
            var duplicateScore = DuplicatePaymentRequestByUserScorer.Calculate(
                paymentRequestByUser,
                userId,
                teamId,
                amount,
                paidAt,
                invoiceNumber);

            return new DuplicatePaymentRequestByUserMatch(
                paymentRequestByUser,
                duplicateScore.Score,
                duplicateScore.MatchedFields);
        }
    }
}
