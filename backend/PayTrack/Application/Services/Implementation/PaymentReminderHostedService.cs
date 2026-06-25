// <copyright file="PaymentReminderHostedService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <summary>
    /// Background service that sends payment due-date reminder notifications daily.
    /// </summary>
    public sealed class PaymentReminderHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentReminderHostedService> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly ILogger<PaymentReminderHostedService> logger = logger;

        /// <summary>
        /// Sends reminder notifications for all payment requests due in the configured number of days.
        /// Extracted for testability.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendRemindersAsync(CancellationToken cancellationToken)
        {
            using var scope = this.scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
            var notifications = scope.ServiceProvider.GetRequiredService<INotificationDispatchService>();
            var systemSettings = scope.ServiceProvider.GetRequiredService<ISystemSettingService>();

            var sendEmail = await systemSettings.GetBoolSettingAsync(SystemSettingKeys.NotificationsRemindersEmail, true);
            var sendSlack = await systemSettings.GetBoolSettingAsync(SystemSettingKeys.NotificationsRemindersSlack, false);
            var sendPush = await systemSettings.GetBoolSettingAsync(SystemSettingKeys.NotificationsRemindersPush, true);
            var pushNotifications = sendPush
                ? scope.ServiceProvider.GetService<IPushNotificationService>()
                : null;
            var daysBeforeDue = await systemSettings.GetDaysBeforeDueAsync();
            var emailDelayMs = await systemSettings.GetEmailDelayMsAsync();

            foreach (var daysAhead in daysBeforeDue)
            {
                var targetDate = DateTime.UtcNow.Date.AddDays(daysAhead);

                List<PaymentRequestByTeam> dueSoon;
                try
                {
                    dueSoon = await repo.GetPaymentRequestsByTeamDueOnAsync(targetDate);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to query payment requests due on {Date}.", targetDate);
                    continue;
                }

                foreach (var request in dueSoon)
                {
                    if (sendEmail)
                    {
                        try
                        {
                            var subject = $"Payment Reminder: {request.PurposeOfPayment} due in {daysAhead} day(s)";
                            var body =
                                $"Dear {request.User.Name},\n\n" +
                                $"This is a reminder that the following payment is due in {daysAhead} day(s).\n\n" +
                                $"Amount: {request.Amount:C2}\n" +
                                $"Purpose: {request.PurposeOfPayment}\n" +
                                $"Due Date: {request.DueDate:yyyy-MM-dd}\n\n" +
                                $"Please ensure payment is made before the due date.\n\n" +
                                $"PayTrack";

                            await notifications.SendEmailAsync(request.User.Email, subject, body);
                            await Task.Delay(emailDelayMs, cancellationToken);
                        }
                        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError(
                                ex,
                                "Failed to send reminder email for transaction {Id} to {Email}.",
                                request.Id,
                                request.User.Email);
                        }
                    }

                    if (sendSlack)
                    {
                        try
                        {
                            var slackMsg =
                                $"Payment Reminder: {request.PurposeOfPayment} is due in {daysAhead} day(s) " +
                                $"on {request.DueDate:yyyy-MM-dd}. Amount: {request.Amount:C2}";

                            // NotificationDispatchService resolves the Slack user by email address.
                            await notifications.SendSlackAsync(request.User.Email, slackMsg);
                        }
                        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError(
                                ex,
                                "Failed to send Slack reminder for transaction {Id} to {Email}.",
                                request.Id,
                                request.User.Email);
                        }
                    }

                    if (pushNotifications is not null)
                    {
                        try
                        {
                            await pushNotifications.SendWorkflowStatusChangedAsync(
                                request.UserId,
                                "Payment reminder",
                                $"Payment request due in {daysAhead} day(s): {request.PurposeOfPayment}\nAmount: {request.Amount:C2}",
                                $"/my-team-requests/{request.Id}");
                        }
                        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError(
                                ex,
                                "Failed to send push reminder for transaction {Id} to user {UserId}.",
                                request.Id,
                                request.UserId);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                int runAtHour;
                int runAtMinute;
                using (var scope = this.scopeFactory.CreateScope())
                {
                    var systemSettings = scope.ServiceProvider.GetRequiredService<ISystemSettingService>();
                    runAtHour = await systemSettings.GetRunAtHourUtcAsync();
                    runAtMinute = await systemSettings.GetRunAtMinuteUtcAsync();
                }

                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddHours(runAtHour).AddMinutes(runAtMinute);

                if (nextRun <= now)
                {
                    nextRun = nextRun.AddDays(1);
                }

                var delay = nextRun - now;
                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await this.SendRemindersAsync(stoppingToken);
            }
        }
    }
}
