// <copyright file="ISystemSettingService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.AdminSettings;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service for reading and updating admin-configurable system settings.
    /// </summary>
    public interface ISystemSettingService
    {
        /// <summary>Returns the current CSV column name settings.</summary>
        /// <returns>The current CSV column name settings.</returns>
        Task<CsvColumnSettingsDto> GetCsvColumnSettingsAsync();

        /// <summary>Persists updated CSV column name settings.</summary>
        /// <param name="dto">New column names.</param>
        /// <param name="userId">ID of the admin performing the update.</param>
        /// <returns>A task representing the async operation.</returns>
        Task UpdateCsvColumnSettingsAsync(UpdateCsvColumnSettingsRequestDto dto, int userId);

        /// <summary>Returns the current notification channel toggles for all event types.</summary>
        /// <returns>The current notification channel toggles.</returns>
        Task<NotificationChannelGroupsDto> GetNotificationChannelGroupsAsync();

        /// <summary>Persists updated notification channel toggles.</summary>
        /// <param name="dto">New channel settings.</param>
        /// <param name="userId">ID of the admin performing the update.</param>
        /// <returns>A task representing the async operation.</returns>
        Task UpdateNotificationChannelGroupsAsync(UpdateNotificationChannelGroupsRequestDto dto, int userId);

        /// <summary>Returns the current reminder schedule settings.</summary>
        /// <returns>The current reminder schedule settings.</returns>
        Task<ReminderScheduleDto> GetReminderScheduleAsync();

        /// <summary>Persists updated reminder schedule settings.</summary>
        /// <param name="dto">New schedule settings.</param>
        /// <param name="userId">ID of the admin performing the update.</param>
        /// <returns>A task representing the async operation.</returns>
        Task UpdateReminderScheduleAsync(UpdateReminderScheduleRequestDto dto, int userId);

        /// <summary>
        /// Returns a boolean setting from the DB, or <paramref name="defaultValue"/> if no row exists.
        /// Used internally by notification services to read channel toggles at runtime.
        /// </summary>
        /// <param name="key">The setting key to look up.</param>
        /// <param name="defaultValue">Fallback value when no DB row exists.</param>
        /// <returns>The stored boolean value, or <paramref name="defaultValue"/> if not found.</returns>
        Task<bool> GetBoolSettingAsync(string key, bool defaultValue);

        /// <summary>
        /// Returns the DaysBeforeDue array from the DB, or the hardcoded default if no row exists.
        /// </summary>
        /// <returns>The stored days array, or the hardcoded default if not found.</returns>
        Task<int[]> GetDaysBeforeDueAsync();

        /// <summary>
        /// Returns RunAtHourUtc from the DB, or the hardcoded default if no row exists.
        /// </summary>
        /// <returns>The stored hour value, or the hardcoded default if not found.</returns>
        Task<int> GetRunAtHourUtcAsync();

        /// <summary>
        /// Returns RunAtMinuteUtc from the DB, or the hardcoded default if no row exists.
        /// </summary>
        /// <returns>The stored minute value, or the hardcoded default if not found.</returns>
        Task<int> GetRunAtMinuteUtcAsync();

        /// <summary>
        /// Returns EmailDelayMs from the DB, or the hardcoded default (500) if no row exists.
        /// </summary>
        /// <returns>The stored delay value in milliseconds, or 500 if not found.</returns>
        Task<int> GetEmailDelayMsAsync();
    }
}
