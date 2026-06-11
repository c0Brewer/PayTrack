// <copyright file="ReminderSettings.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Settings
{
    /// <summary>
    /// Configuration for due-date payment reminders.
    /// </summary>
    public sealed class ReminderSettings
    {
        // TODO: expose DaysBeforeDue and RunAtHourUtc in admin settings once that feature is available

        /// <summary>
        /// Gets or sets the number of days before the due date at which reminders are sent.
        /// </summary>
        public int[] DaysBeforeDue { get; set; } = [];

        /// <summary>
        /// Gets or sets the UTC hour (0–23) at which the reminder job runs each day.
        /// </summary>
        public int RunAtHourUtc { get; set; } = 8;

        /// <summary>
        /// Gets or sets the UTC minute (0–59) at which the reminder job runs each day.
        /// </summary>
        public int RunAtMinuteUtc { get; set; } = 0;

        /// <summary>
        /// Gets or sets the delay in milliseconds between individual reminder emails to avoid SMTP rate limits.
        /// </summary>
        public int EmailDelayMs { get; set; } = 500;

        /// <summary>
        /// Gets or sets the notification channels used when sending payment reminders.
        /// </summary>
        public NotificationChannelSettings Channels { get; set; } = new();
    }
}
