// <copyright file="PushNotificationSettings.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Settings
{
    /// <summary>
    /// Settings for browser push notifications.
    /// </summary>
    public sealed class PushNotificationSettings
    {
        /// <summary>
        /// Gets or sets the VAPID public key in base64url format.
        /// </summary>
        public string PublicKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the VAPID private key in base64url format.
        /// </summary>
        public string PrivateKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the VAPID subject, usually a mailto: or HTTPS contact URI.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Web Push TTL in seconds.
        /// </summary>
        public int TimeToLiveSeconds { get; set; } = 86400;

        /// <summary>
        /// Gets a value indicating whether the required push settings are present.
        /// </summary>
        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(this.PublicKey) &&
            !string.IsNullOrWhiteSpace(this.PrivateKey) &&
            !string.IsNullOrWhiteSpace(this.Subject);
    }
}
