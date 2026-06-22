// <copyright file="SystemSettingService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.AdminSettings;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class SystemSettingService(ISystemSettingRepository repo) : ISystemSettingService
    {
        private const bool DefaultSendEmail = true;
        private const bool DefaultSendSlack = false;
        private const bool DefaultSendPush = true;
        private const int DefaultRunAtHourUtc = 8;
        private const int DefaultRunAtMinuteUtc = 0;
        private const int DefaultEmailDelayMs = 500;
        private static readonly int[] DefaultDaysBeforeDue = [7, 2, 1];

        private readonly ISystemSettingRepository repo = repo;

        /// <inheritdoc/>
        public async Task<CsvColumnSettingsDto> GetCsvColumnSettingsAsync()
        {
            var nameRow = await this.repo.GetByKeyAsync(SystemSettingKeys.CsvColumnName);
            var summeRow = await this.repo.GetByKeyAsync(SystemSettingKeys.CsvColumnSumme);

            return new CsvColumnSettingsDto(
                nameRow?.Value ?? "Name",
                summeRow?.Value ?? "Summe");
        }

        /// <inheritdoc/>
        public async Task UpdateCsvColumnSettingsAsync(UpdateCsvColumnSettingsRequestDto dto, int userId)
        {
            await this.repo.UpsertManyAsync(
                new Dictionary<string, string>
                {
                    [SystemSettingKeys.CsvColumnName] = dto.NameColumn,
                    [SystemSettingKeys.CsvColumnSumme] = dto.SummeColumn,
                },
                userId);
        }

        /// <inheritdoc/>
        public async Task<NotificationChannelGroupsDto> GetNotificationChannelGroupsAsync()
        {
            var creation = new NotificationChannelDto(
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsCreationEmail, DefaultSendEmail),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsCreationSlack, DefaultSendSlack),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsCreationPush, DefaultSendPush));

            var confirmation = new NotificationChannelDto(
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsConfirmationEmail, DefaultSendEmail),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsConfirmationSlack, DefaultSendSlack),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsConfirmationPush, DefaultSendPush));

            var reminders = new NotificationChannelDto(
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsRemindersEmail, DefaultSendEmail),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsRemindersSlack, DefaultSendSlack),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsRemindersPush, DefaultSendPush));

            var deletion = new NotificationChannelDto(
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsDeletionEmail, DefaultSendEmail),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsDeletionSlack, DefaultSendSlack),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsDeletionPush, DefaultSendPush));

            var invoiceApproval = new NotificationChannelDto(
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsInvoiceApprovalEmail, DefaultSendEmail),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsInvoiceApprovalSlack, DefaultSendSlack),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsInvoiceApprovalPush, DefaultSendPush));

            var invoiceRejection = new NotificationChannelDto(
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsInvoiceRejectionEmail, DefaultSendEmail),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsInvoiceRejectionSlack, DefaultSendSlack),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsInvoiceRejectionPush, DefaultSendPush));

            var invoiceChangesRequested = new NotificationChannelDto(
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsInvoiceChangesRequestedEmail, DefaultSendEmail),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsInvoiceChangesRequestedSlack, DefaultSendSlack),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsInvoiceChangesRequestedPush, DefaultSendPush));

            var invoicePaymentCompleted = new NotificationChannelDto(
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsInvoicePaymentCompletedEmail, DefaultSendEmail),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsInvoicePaymentCompletedSlack, DefaultSendSlack),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsInvoicePaymentCompletedPush, DefaultSendPush));

            return new NotificationChannelGroupsDto(
                creation,
                confirmation,
                reminders,
                deletion,
                invoiceApproval,
                invoiceRejection,
                invoiceChangesRequested,
                invoicePaymentCompleted);
        }

        /// <inheritdoc/>
        public async Task UpdateNotificationChannelGroupsAsync(UpdateNotificationChannelGroupsRequestDto dto, int userId)
        {
            await this.repo.UpsertManyAsync(
                new Dictionary<string, string>
                {
                    [SystemSettingKeys.NotificationsCreationEmail] = dto.Creation.SendEmail.ToString(),
                    [SystemSettingKeys.NotificationsCreationSlack] = dto.Creation.SendSlack.ToString(),
                    [SystemSettingKeys.NotificationsCreationPush] = dto.Creation.SendPush.ToString(),
                    [SystemSettingKeys.NotificationsConfirmationEmail] = dto.Confirmation.SendEmail.ToString(),
                    [SystemSettingKeys.NotificationsConfirmationSlack] = dto.Confirmation.SendSlack.ToString(),
                    [SystemSettingKeys.NotificationsConfirmationPush] = dto.Confirmation.SendPush.ToString(),
                    [SystemSettingKeys.NotificationsRemindersEmail] = dto.Reminders.SendEmail.ToString(),
                    [SystemSettingKeys.NotificationsRemindersSlack] = dto.Reminders.SendSlack.ToString(),
                    [SystemSettingKeys.NotificationsRemindersPush] = dto.Reminders.SendPush.ToString(),
                    [SystemSettingKeys.NotificationsDeletionEmail] = dto.Deletion.SendEmail.ToString(),
                    [SystemSettingKeys.NotificationsDeletionSlack] = dto.Deletion.SendSlack.ToString(),
                    [SystemSettingKeys.NotificationsDeletionPush] = dto.Deletion.SendPush.ToString(),
                    [SystemSettingKeys.NotificationsInvoiceApprovalEmail] = dto.InvoiceApproval.SendEmail.ToString(),
                    [SystemSettingKeys.NotificationsInvoiceApprovalSlack] = dto.InvoiceApproval.SendSlack.ToString(),
                    [SystemSettingKeys.NotificationsInvoiceApprovalPush] = dto.InvoiceApproval.SendPush.ToString(),
                    [SystemSettingKeys.NotificationsInvoiceRejectionEmail] = dto.InvoiceRejection.SendEmail.ToString(),
                    [SystemSettingKeys.NotificationsInvoiceRejectionSlack] = dto.InvoiceRejection.SendSlack.ToString(),
                    [SystemSettingKeys.NotificationsInvoiceRejectionPush] = dto.InvoiceRejection.SendPush.ToString(),
                    [SystemSettingKeys.NotificationsInvoiceChangesRequestedEmail] = dto.InvoiceChangesRequested.SendEmail.ToString(),
                    [SystemSettingKeys.NotificationsInvoiceChangesRequestedSlack] = dto.InvoiceChangesRequested.SendSlack.ToString(),
                    [SystemSettingKeys.NotificationsInvoiceChangesRequestedPush] = dto.InvoiceChangesRequested.SendPush.ToString(),
                    [SystemSettingKeys.NotificationsInvoicePaymentCompletedEmail] = dto.InvoicePaymentCompleted.SendEmail.ToString(),
                    [SystemSettingKeys.NotificationsInvoicePaymentCompletedSlack] = dto.InvoicePaymentCompleted.SendSlack.ToString(),
                    [SystemSettingKeys.NotificationsInvoicePaymentCompletedPush] = dto.InvoicePaymentCompleted.SendPush.ToString(),
                },
                userId);
        }

        /// <inheritdoc/>
        public async Task<ReminderScheduleDto> GetReminderScheduleAsync()
        {
            return new ReminderScheduleDto(
                await this.GetDaysBeforeDueAsync(),
                await this.GetRunAtHourUtcAsync(),
                await this.GetRunAtMinuteUtcAsync(),
                await this.GetEmailDelayMsAsync());
        }

        /// <inheritdoc/>
        public async Task UpdateReminderScheduleAsync(UpdateReminderScheduleRequestDto dto, int userId)
        {
            await this.repo.UpsertManyAsync(
                new Dictionary<string, string>
                {
                    [SystemSettingKeys.RemindersDaysBeforeDue] = string.Join(',', dto.DaysBeforeDue),
                    [SystemSettingKeys.RemindersRunAtHourUtc] = dto.RunAtHourUtc.ToString(),
                    [SystemSettingKeys.RemindersRunAtMinuteUtc] = dto.RunAtMinuteUtc.ToString(),
                    [SystemSettingKeys.RemindersEmailDelayMs] = dto.EmailDelayMs.ToString(),
                },
                userId);
        }

        /// <inheritdoc/>
        public async Task<bool> GetBoolSettingAsync(string key, bool defaultValue)
        {
            var row = await this.repo.GetByKeyAsync(key);
            if (row is null)
            {
                return defaultValue;
            }

            return bool.TryParse(row.Value, out var parsed) ? parsed : defaultValue;
        }

        /// <inheritdoc/>
        public async Task<int[]> GetDaysBeforeDueAsync()
        {
            var row = await this.repo.GetByKeyAsync(SystemSettingKeys.RemindersDaysBeforeDue);
            if (row is null)
            {
                return DefaultDaysBeforeDue;
            }

            if (string.IsNullOrWhiteSpace(row.Value))
            {
                return [];
            }

            var parts = row.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var days = new List<int>();
            foreach (var part in parts)
            {
                if (int.TryParse(part, out var day) && day >= 0)
                {
                    days.Add(day);
                }
            }

            return [.. days];
        }

        /// <inheritdoc/>
        public async Task<int> GetRunAtHourUtcAsync()
        {
            var row = await this.repo.GetByKeyAsync(SystemSettingKeys.RemindersRunAtHourUtc);
            if (row is null)
            {
                return DefaultRunAtHourUtc;
            }

            return int.TryParse(row.Value, out var hour) && hour is >= 0 and <= 23 ? hour : DefaultRunAtHourUtc;
        }

        /// <inheritdoc/>
        public async Task<int> GetRunAtMinuteUtcAsync()
        {
            var row = await this.repo.GetByKeyAsync(SystemSettingKeys.RemindersRunAtMinuteUtc);
            if (row is null)
            {
                return DefaultRunAtMinuteUtc;
            }

            return int.TryParse(row.Value, out var minute) && minute is >= 0 and <= 59 ? minute : DefaultRunAtMinuteUtc;
        }

        /// <inheritdoc/>
        public async Task<int> GetEmailDelayMsAsync()
        {
            var row = await this.repo.GetByKeyAsync(SystemSettingKeys.RemindersEmailDelayMs);
            if (row is null)
            {
                return DefaultEmailDelayMs;
            }

            return int.TryParse(row.Value, out var ms) && ms >= 0 ? ms : DefaultEmailDelayMs;
        }
    }
}
