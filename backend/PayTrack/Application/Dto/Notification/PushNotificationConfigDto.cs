// <copyright file="PushNotificationConfigDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.Notification
{
    /// <summary>
    /// Describes the current user's push notification configuration.
    /// </summary>
    public class PushNotificationConfigDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether server-side push notifications are configured.
        /// </summary>
        public bool IsConfigured { get; set; }

        /// <summary>
        /// Gets or sets the VAPID public key used by browsers to create push subscriptions.
        /// </summary>
        public string? VapidPublicKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user has enabled push notifications.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the user's enabled push devices.
        /// </summary>
        public IReadOnlyCollection<PushSubscriptionDeviceDto> Devices { get; set; } = [];
    }
}
