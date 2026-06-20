// <copyright file="PaymentRequestByTeamService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.PaymentRequestByTeam;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class PaymentRequestByTeamService(
        ITransactionRepository repo,
        ITeamService _teamService,
        IUserService _userService,
        IBudgetService _budgetService,
        INotificationDispatchService _notifications,
        ISystemSettingService _systemSettings,
        ILogger<PaymentRequestByTeamService> _logger,
        IPushNotificationService? _pushNotifications = null) : IPaymentRequestByTeamService
    {
        /// <summary>
        /// Repository for PaymentRequestByTeams.
        /// </summary>
        private readonly ITransactionRepository repo = repo;
        private readonly ITeamService teamService = _teamService;
        private readonly IUserService userService = _userService;
        private readonly IBudgetService budgetService = _budgetService;
        private readonly INotificationDispatchService notifications = _notifications;
        private readonly ISystemSettingService systemSettings = _systemSettings;
        private readonly ILogger<PaymentRequestByTeamService> logger = _logger;
        private readonly IPushNotificationService? pushNotifications = _pushNotifications;

        /// <inheritdoc/>
        public async Task<(List<PaymentRequestByTeam> paymentRequestByTeam, int totalCount)> GetAllAsync(
            GetPaymentRequestByTeamQuery? query = null)
        {
            return await this.repo.GetAllAsync(query);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByTeam?> GetPaymentRequestByTeamByIdAsync(int id, GetPaymentRequestByTeamQueryById? query = null)
        {
            return await this.repo.GetByIdAsync(id, query);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByTeam> CreatePaymentRequestByTeamAsync(
            int userToAssignToId,
            int creatingUserId,
            int teamId,
            decimal amount,
            string purposeOfPayment,
            DateTime dueDate,
            int? budgetId = null)
        {
            var team = await this.teamService.GetTeamByIdAsync(teamId) ?? throw new NotFoundException("Team could not be found");
            var userToAssignTo = await this.userService.GetUserByIdAsync(userToAssignToId) ?? throw new NotFoundException("Assigned user could not be found");
            var creatingUser = await this.userService.GetUserByIdAsync(creatingUserId) ?? throw new NotFoundException("Creating user could not be found");
            if (budgetId.HasValue)
            {
                _ = await this.budgetService.GetByIdAsync(budgetId.Value) ?? throw new NotFoundException("Budget could not be found");
            }

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0");
            }

            if (dueDate.Date < DateTime.Today)
            {
                throw new ArgumentException("Due date cannot be in the past");
            }

            var paymentRequest = new PaymentRequestByTeam
            {
                // Transaction settings
                UserId = userToAssignTo.Id,
                Amount = amount,
                PurposeOfPayment = purposeOfPayment,
                PaymentReference = string.Empty, // Payment reference will be set later by the finance team
                Status = TransactionStatus.Submitted,
                BudgetId = budgetId,
                TeamId = team.Id,
                PaymentDirection = PaymentDirection.In, // Payment direction is in for payment requests to user
                DueDate = dueDate,

                // Created at is set automatically
                RequestedById = creatingUser.Id,
            };

            var created = await this.repo.AddAsync(paymentRequest);

            var sendCreationEmail = await this.systemSettings.GetBoolSettingAsync(SystemSettingKeys.NotificationsCreationEmail, true);
            var sendCreationSlack = await this.systemSettings.GetBoolSettingAsync(SystemSettingKeys.NotificationsCreationSlack, false);
            var sendCreationPush = await this.systemSettings.GetBoolSettingAsync(SystemSettingKeys.NotificationsCreationPush, true);

            if (sendCreationEmail)
            {
                try
                {
                    var subject = $"New Payment Request: {purposeOfPayment}";
                    var body =
                        $"Dear {userToAssignTo.Name},\n\n" +
                        $"A new payment request has been created for you.\n\n" +
                        $"Amount: {amount:C2}\n" +
                        $"Purpose: {purposeOfPayment}\n" +
                        $"Due Date: {dueDate:yyyy-MM-dd}\n\n" +
                        $"Please ensure payment is made before the due date.\n\n" +
                        $"PayTrack";

                    await this.notifications.SendEmailAsync(userToAssignTo.Email, subject, body);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to send new-payment-request email to {Email}.", userToAssignTo.Email);
                }
            }

            if (sendCreationSlack)
            {
                try
                {
                    var slackMsg =
                        $"New Payment Request: {purposeOfPayment}\n" +
                        $"Amount: {amount:C2} - Due: {dueDate:yyyy-MM-dd}";

                    // NotificationDispatchService resolves the Slack user by email address.
                    await this.notifications.SendSlackAsync(userToAssignTo.Email, slackMsg);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to send new-payment-request Slack notification to {Email}.", userToAssignTo.Email);
                }
            }

            if (sendCreationPush)
            {
                await this.SendTeamRequestPushAsync(
                    created,
                    userToAssignTo.Id,
                    "New payment request",
                    $"A new payment request was assigned to you: {purposeOfPayment}",
                    $"/my-team-requests/{created.Id}");
            }

            return created;
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByTeam> UpdatePaymentRequestByTeamAsync(
            int id,
            int? teamId = null,
            decimal? amount = null,
            string? purposeOfPayment = null,
            DateTime? paidAt = null)
        {
            var transaction = await this.repo.GetByIdAsync(id, new GetPaymentRequestByTeamQueryById())
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

            return await this.repo.UpdateAsync(transaction);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByTeam> MarkAsPaidAsync(int id, int adminUserId, string? comment)
        {
            var transaction = await this.repo.GetByIdAsync(id, new GetPaymentRequestByTeamQueryById { IncludeUser = true })
                ?? throw new NotFoundException("Transaction not found");

            if (transaction.Status is TransactionStatus.Paid or TransactionStatus.Declined)
            {
                throw new InvalidStateException(
                    $"Cannot mark a transaction as Paid when its current status is {transaction.Status}.");
            }

            var fromStatus = transaction.Status;
            transaction.Status = TransactionStatus.Paid;
            transaction.PaidAt = DateTime.UtcNow;

            var result = await this.repo.UpdateAndAddStatusHistoryAsync(
                transaction,
                new TransactionStatusHistory
                {
                    TransactionId = transaction.Id,
                    ChangedById = adminUserId,
                    FromStatus = fromStatus,
                    ToStatus = TransactionStatus.Paid,
                    Comment = comment,
                });

            var sendConfirmationEmail = await this.systemSettings.GetBoolSettingAsync(SystemSettingKeys.NotificationsConfirmationEmail, true);
            var sendConfirmationSlack = await this.systemSettings.GetBoolSettingAsync(SystemSettingKeys.NotificationsConfirmationSlack, false);
            var sendConfirmationPush = await this.systemSettings.GetBoolSettingAsync(SystemSettingKeys.NotificationsConfirmationPush, true);

            if (sendConfirmationEmail)
            {
                try
                {
                    var subject = $"Payment Confirmed: {transaction.PurposeOfPayment}";
                    var body =
                        $"Dear {transaction.User.Name},\n\n" +
                        $"Your payment has been marked as paid.\n\n" +
                        $"Amount: {transaction.Amount:C2}\n" +
                        $"Purpose: {transaction.PurposeOfPayment}\n" +
                        $"Paid on: {transaction.PaidAt:yyyy-MM-dd}\n\n" +
                        $"Thank you,\n" +
                        $"PayTrack";

                    await this.notifications.SendEmailAsync(transaction.User.Email, subject, body);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to send payment-confirmed email to {Email}.", transaction.User.Email);
                }
            }

            if (sendConfirmationSlack)
            {
                try
                {
                    var slackMsg =
                        $"Payment Confirmed: {transaction.PurposeOfPayment}\n" +
                        $"Amount: {transaction.Amount:C2} - Paid on: {transaction.PaidAt:yyyy-MM-dd}";

                    // NotificationDispatchService resolves the Slack user by email address.
                    await this.notifications.SendSlackAsync(transaction.User.Email, slackMsg);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to send payment-confirmed Slack notification to {Email}.", transaction.User.Email);
                }
            }

            if (sendConfirmationPush)
            {
                await this.SendTeamRequestPushAsync(
                    result,
                    transaction.UserId,
                    "Payment request paid",
                    $"Your payment request has been marked as paid: {transaction.PurposeOfPayment}",
                    $"/my-team-requests/{transaction.Id}");
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task DeletePaymentRequestByTeamAsync(int id, string? reason = null)
        {
            var transaction = await this.repo.GetByIdAsync(id, new GetPaymentRequestByTeamQueryById { IncludeUser = true })
                ?? throw new NotFoundException("PaymentRequestByTeam could not be found");

            if (transaction.Status != TransactionStatus.Submitted)
            {
                throw new InvalidStateException(
                    $"Cannot delete a payment request that is not in Submitted status.");
            }

            var wasDeleted = await this.repo.DeletePaymentRequestByTeamAsync(id);
            if (!wasDeleted)
            {
                throw new InvalidStateException(
                    "Cannot delete a payment request that is not in Submitted status.");
            }

            var normalizedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            var sendDeletionEmail = await this.systemSettings.GetBoolSettingAsync(SystemSettingKeys.NotificationsDeletionEmail, true);
            var sendDeletionSlack = await this.systemSettings.GetBoolSettingAsync(SystemSettingKeys.NotificationsDeletionSlack, false);
            var sendDeletionPush = await this.systemSettings.GetBoolSettingAsync(SystemSettingKeys.NotificationsDeletionPush, true);

            if (sendDeletionEmail)
            {
                try
                {
                    var subject = $"Payment Request Deleted: {transaction.PurposeOfPayment}";
                    var body =
                        $"Dear {transaction.User.Name},\n\n" +
                        $"Your payment request has been deleted by an administrator.\n\n" +
                        $"Amount: {transaction.Amount:C2}\n" +
                        $"Purpose: {transaction.PurposeOfPayment}\n" +
                        (normalizedReason is not null ? $"Reason: {normalizedReason}\n\n" : "\n") +
                        $"If you have questions, please contact your administrator.\n\n" +
                        $"PayTrack";

                    await this.notifications.SendEmailAsync(transaction.User.Email, subject, body);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to send payment-request-deleted email to {Email}.", transaction.User.Email);
                }
            }

            if (sendDeletionSlack)
            {
                try
                {
                    var slackMsg =
                        $"Payment Request Deleted: {transaction.PurposeOfPayment}\n" +
                        $"Amount: {transaction.Amount:C2}" +
                        (normalizedReason is not null ? $" - Reason: {normalizedReason}" : string.Empty);

                    await this.notifications.SendSlackAsync(transaction.User.Email, slackMsg);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to send payment-request-deleted Slack notification to {Email}.", transaction.User.Email);
                }
            }

            if (sendDeletionPush)
            {
                await this.SendTeamRequestPushAsync(
                    transaction,
                    transaction.UserId,
                    "Payment request deleted",
                    $"Your payment request was deleted: {transaction.PurposeOfPayment}",
                    "/my-team-requests");
            }
        }

        /// <inheritdoc/>
        public bool ValidateQuery(GetPaymentRequestByTeamQuery query, User currentUser)
        {
            return currentUser.Role switch
            {
                Role.RegularUser => query.UserId == currentUser.Id,

                Role.TeamLead => currentUser.TeamId.HasValue
                                  && query.TeamId == currentUser.TeamId,

                Role.Admin => true,

                _ => false
            };
        }

        private async Task SendTeamRequestPushAsync(PaymentRequestByTeam transaction, int userId, string title, string body, string url)
        {
            if (this.pushNotifications is null)
            {
                return;
            }

            var purpose = string.IsNullOrWhiteSpace(transaction.PurposeOfPayment)
                ? $"Payment request #{transaction.Id}"
                : transaction.PurposeOfPayment.Trim();

            await this.pushNotifications.SendWorkflowStatusChangedAsync(
                userId,
                title,
                $"{body}\nAmount: {transaction.Amount:C2}\n{purpose}",
                url);
        }
    }
}
