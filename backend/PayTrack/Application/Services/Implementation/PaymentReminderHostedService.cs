// <copyright file="PaymentReminderHostedService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.Extensions.Options;
using PayTrack.Application.Services.Model;
using PayTrack.Application.Settings;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <summary>
    /// Background service that sends payment due-date reminder notifications daily.
    /// </summary>
    public sealed class PaymentReminderHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<ReminderSettings> settings,
        ILogger<PaymentReminderHostedService> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly ReminderSettings settings = settings.Value;
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

            var channels = this.settings.Channels;

            foreach (var daysAhead in this.settings.DaysBeforeDue)
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
                    if (channels.SendEmail)
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
                            await Task.Delay(this.settings.EmailDelayMs, cancellationToken);
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

                    if (channels.SendSlack)
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
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddHours(this.settings.RunAtHourUtc).AddMinutes(this.settings.RunAtMinuteUtc);

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
