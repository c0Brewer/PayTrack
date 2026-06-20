// <copyright file="SystemSettingKeys.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Well-known keys for the SystemSetting key-value store.
    /// </summary>
    public static class SystemSettingKeys
    {
        /// <summary>CSV import: header name for the recipient/payee column.</summary>
        public const string CsvColumnName = "csv.column.name";

        /// <summary>CSV import: header name for the amount column.</summary>
        public const string CsvColumnSumme = "csv.column.summe";

        /// <summary>Notification channels: send email on new payment request creation.</summary>
        public const string NotificationsCreationEmail = "notifications.creation.email";

        /// <summary>Notification channels: send Slack on new payment request creation.</summary>
        public const string NotificationsCreationSlack = "notifications.creation.slack";

        /// <summary>Notification channels: send push notification on new payment request creation.</summary>
        public const string NotificationsCreationPush = "notifications.creation.push";

        /// <summary>Notification channels: send email when a payment is confirmed as paid.</summary>
        public const string NotificationsConfirmationEmail = "notifications.confirmation.email";

        /// <summary>Notification channels: send Slack when a payment is confirmed as paid.</summary>
        public const string NotificationsConfirmationSlack = "notifications.confirmation.slack";

        /// <summary>Notification channels: send push notification when a payment is confirmed as paid.</summary>
        public const string NotificationsConfirmationPush = "notifications.confirmation.push";

        /// <summary>Notification channels: send email for due-date reminders.</summary>
        public const string NotificationsRemindersEmail = "notifications.reminders.email";

        /// <summary>Notification channels: send Slack for due-date reminders.</summary>
        public const string NotificationsRemindersSlack = "notifications.reminders.slack";

        /// <summary>Notification channels: send push notification for due-date reminders.</summary>
        public const string NotificationsRemindersPush = "notifications.reminders.push";

        /// <summary>Notification channels: send email when a payment request is deleted.</summary>
        public const string NotificationsDeletionEmail = "notifications.deletion.email";

        /// <summary>Notification channels: send Slack when a payment request is deleted.</summary>
        public const string NotificationsDeletionSlack = "notifications.deletion.slack";

        /// <summary>Notification channels: send push notification when a payment request is deleted.</summary>
        public const string NotificationsDeletionPush = "notifications.deletion.push";

        /// <summary>Reminder schedule: comma-separated list of days before due date (e.g. "7,2,1").</summary>
        public const string RemindersDaysBeforeDue = "reminders.days-before-due";

        /// <summary>Reminder schedule: UTC hour (0–23) at which the daily reminder job runs.</summary>
        public const string RemindersRunAtHourUtc = "reminders.run-at-hour-utc";

        /// <summary>Reminder schedule: UTC minute (0–59) at which the daily reminder job runs.</summary>
        public const string RemindersRunAtMinuteUtc = "reminders.run-at-minute-utc";

        /// <summary>Reminder schedule: delay in ms between individual reminder emails (SMTP rate-limit guard).</summary>
        public const string RemindersEmailDelayMs = "reminders.email-delay-ms";
    }
}
