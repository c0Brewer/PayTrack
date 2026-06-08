// <copyright file="NotificationChannelSettings.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Settings
{
    /// <summary>
    /// Configures which notification channels are active for a specific event trigger.
    /// </summary>
    // TODO: Expose SendEmail and SendSlack via the admin settings UI (feature: admin notification preferences).
    public sealed class NotificationChannelSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether email notifications are sent for this trigger.
        /// </summary>
        public bool SendEmail { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether Slack notifications are sent for this trigger.
        /// </summary>
        public bool SendSlack { get; set; } = false;
    }
}
