// <copyright file="PaymentRequestByUserService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
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
        IBudgetService _budgetService) : IPaymentRequestByUserService
    {
        private const int DuplicateMatchThreshold = 1;
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
            int? bankAccountId)
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
                // Ensure bank account is null if payout type is external
                bankAccountId = null;
            }

            var paymentRequest = new PaymentRequestByUser
            {
                // Transaction settings
                UserId = userId,
                Amount = amount,
                PurposeOfPayment = purposeOfPayment,
                PaymentReference = string.Empty, // Payment reference will be set later by the finance team
                Status = TransactionStatus.Submitted,
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
            };

            return await this.repo.AddAsync(paymentRequest, receipt);
        }

        /// <inheritdoc/>
        public async Task<List<DuplicatePaymentRequestByUserMatch>> GetDuplicatePaymentRequestsByUserAsync(
            int userId,
            int teamId,
            decimal amount)
        {
            var duplicateCandidates = await this.repo.GetPotentialDuplicatesAsync(userId, teamId, amount);

            return duplicateCandidates
                .Select(paymentRequestByUser => this.CreateDuplicateMatch(paymentRequestByUser, userId, teamId, amount))
                .Where(duplicateMatch => duplicateMatch.Score >= DuplicateMatchThreshold)
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
        public async Task<PaymentRequestByUser> MarkPaymentRequestByUserAsPaidAsync(
            int id,
            int changedById,
            string paymentReference,
            string purposeOfPayment,
            DateTime paymentDate)
        {
            var transaction = await this.repo.GetByIdAsync(
                    id,
                    new GetPaymentRequestByUserQueryById { IncludeStatusHistory = true })
                ?? throw new NotFoundException("Transaction not found");

            if (string.IsNullOrWhiteSpace(paymentReference))
            {
                throw new InvalidStateException("Payment reference is required");
            }

            if (string.IsNullOrWhiteSpace(purposeOfPayment))
            {
                throw new InvalidStateException("Purpose of payment is required");
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

            return await this.repo.UpdateAsync(transaction);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> ApprovePaymentRequestByUserAsync(
            int id,
            int changedById,
            int costCentreId,
            string? reason)
        {
            var transaction = await this.repo.GetByIdAsync(
                    id,
                    new GetPaymentRequestByUserQueryById { IncludeStatusHistory = true })
                ?? throw new NotFoundException("Transaction not found");

            if (costCentreId <= 0)
            {
                throw new InvalidStateException("Cost centre is required");
            }

            var costCentre = await this.costCentreService.GetByIdAsync(costCentreId)
                ?? throw new NotFoundException("Cost centre not found");

            var (budgets, _) = await this.budgetService.GetBudgetsAsync(new GetBudgetQuery
            {
                TeamId = transaction.TeamId,
                CostCentreId = costCentre.Id,
                Limit = 1,
            });
            var budget = budgets.FirstOrDefault()
                ?? throw new NotFoundException("Budget for cost centre and team not found");

            transaction.BudgetId = budget.Id;

            AddStatusHistory(
                transaction,
                TransactionStatus.Approved,
                changedById,
                NormalizeOptionalReason(reason));

            return await this.repo.UpdateAsync(transaction);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> DeclinePaymentRequestByUserAsync(
            int id,
            int changedById,
            string reason)
        {
            var transaction = await this.repo.GetByIdAsync(
                    id,
                    new GetPaymentRequestByUserQueryById { IncludeStatusHistory = true })
                ?? throw new NotFoundException("Transaction not found");

            var normalizedReason = NormalizeRequiredReason(reason, "Decline reason is required");
            AddStatusHistory(
                transaction,
                TransactionStatus.Declined,
                changedById,
                normalizedReason);

            return await this.repo.UpdateAsync(transaction);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> RequestChangesPaymentRequestByUserAsync(
            int id,
            int changedById,
            string reason)
        {
            var transaction = await this.repo.GetByIdAsync(
                    id,
                    new GetPaymentRequestByUserQueryById { IncludeStatusHistory = true })
                ?? throw new NotFoundException("Transaction not found");

            var normalizedReason = NormalizeRequiredReason(reason, "Change request reason is required");
            AddStatusHistory(
                transaction,
                TransactionStatus.ChangesRequested,
                changedById,
                normalizedReason);

            return await this.repo.UpdateAsync(transaction);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> UndoLastStatusChangeAsync(
            int id,
            int changedById)
        {
            var transaction = await this.repo.GetByIdAsync(
                    id,
                    new GetPaymentRequestByUserQueryById { IncludeStatusHistory = true })
                ?? throw new NotFoundException("Transaction not found");

            var latestStatusChange = transaction.StatusHistory
                .OrderByDescending(entry => entry.ChangedAt)
                .FirstOrDefault(entry => entry.ToStatus == transaction.Status)
                ?? throw new InvalidStateException("No status change can be undone");

            UndoStatusChange(transaction, latestStatusChange.FromStatus, changedById);

            return await this.repo.UpdateAsync(transaction);
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

            return reason.Trim();
        }

        private static string? NormalizeOptionalReason(string? reason)
        {
            return string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
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

        private DuplicatePaymentRequestByUserMatch CreateDuplicateMatch(
            PaymentRequestByUser paymentRequestByUser,
            int userId,
            int teamId,
            decimal amount)
        {
            bool isAmountAndUserMatch = paymentRequestByUser.UserId == userId && paymentRequestByUser.Amount == amount;
            bool isAmountAndTeamMatch = paymentRequestByUser.TeamId == teamId && paymentRequestByUser.Amount == amount;

            int score = 0;

            if (isAmountAndUserMatch)
            {
                score++;
            }

            if (isAmountAndTeamMatch)
            {
                score++;
            }

            return new DuplicatePaymentRequestByUserMatch(
                paymentRequestByUser,
                score,
                isAmountAndUserMatch,
                isAmountAndTeamMatch);
        }
    }
}
