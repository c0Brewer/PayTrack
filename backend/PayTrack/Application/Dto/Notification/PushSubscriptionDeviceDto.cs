// <copyright file="PushSubscriptionDeviceDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.Notification
{
    /// <summary>
    /// Describes a browser or device with enabled push notifications.
    /// </summary>
    public class PushSubscriptionDeviceDto
    {
        /// <summary>
        /// Gets or sets the subscription id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the browser name reported by the client.
        /// </summary>
        public string BrowserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the device name reported by the client.
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the platform reported by the client.
        /// </summary>
        public string Platform { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this subscription belongs to the current browser.
        /// </summary>
        public bool IsCurrentDevice { get; set; }

        /// <summary>
        /// Gets or sets the timestamp the subscription was last updated at.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
