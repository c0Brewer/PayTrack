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
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsCreationSlack, DefaultSendSlack));

            var confirmation = new NotificationChannelDto(
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsConfirmationEmail, DefaultSendEmail),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsConfirmationSlack, DefaultSendSlack));

            var reminders = new NotificationChannelDto(
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsRemindersEmail, DefaultSendEmail),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsRemindersSlack, DefaultSendSlack));

            var deletion = new NotificationChannelDto(
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsDeletionEmail, DefaultSendEmail),
                await this.GetBoolSettingAsync(SystemSettingKeys.NotificationsDeletionSlack, DefaultSendSlack));

            return new NotificationChannelGroupsDto(creation, confirmation, reminders, deletion);
        }

        /// <inheritdoc/>
        public async Task UpdateNotificationChannelGroupsAsync(UpdateNotificationChannelGroupsRequestDto dto, int userId)
        {
            await this.repo.UpsertManyAsync(
                new Dictionary<string, string>
                {
                    [SystemSettingKeys.NotificationsCreationEmail] = dto.Creation.SendEmail.ToString(),
                    [SystemSettingKeys.NotificationsCreationSlack] = dto.Creation.SendSlack.ToString(),
                    [SystemSettingKeys.NotificationsConfirmationEmail] = dto.Confirmation.SendEmail.ToString(),
                    [SystemSettingKeys.NotificationsConfirmationSlack] = dto.Confirmation.SendSlack.ToString(),
                    [SystemSettingKeys.NotificationsRemindersEmail] = dto.Reminders.SendEmail.ToString(),
                    [SystemSettingKeys.NotificationsRemindersSlack] = dto.Reminders.SendSlack.ToString(),
                    [SystemSettingKeys.NotificationsDeletionEmail] = dto.Deletion.SendEmail.ToString(),
                    [SystemSettingKeys.NotificationsDeletionSlack] = dto.Deletion.SendSlack.ToString(),
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
