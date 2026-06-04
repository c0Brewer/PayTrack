// <copyright file="PaymentRequestNotificationSettings.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Settings
{
    /// <summary>
    /// Notification channel settings for PaymentRequestByTeam lifecycle events.
    /// </summary>
    // TODO: Expose OnCreation and OnConfirmation channels via the admin settings UI (feature: admin notification preferences).
    public sealed class PaymentRequestNotificationSettings
    {
        /// <summary>
        /// Gets or sets the channels used when a new payment request is created.
        /// </summary>
        public NotificationChannelSettings OnCreation { get; set; } = new();

        /// <summary>
        /// Gets or sets the channels used when a payment request is confirmed as paid.
        /// </summary>
        public NotificationChannelSettings OnConfirmation { get; set; } = new();
    }
}
