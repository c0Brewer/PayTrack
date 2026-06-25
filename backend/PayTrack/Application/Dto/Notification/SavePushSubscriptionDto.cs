// <copyright file="SavePushSubscriptionDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Validation;

namespace PayTrack.Application.Dto.Notification
{
    /// <summary>
    /// Request body for registering a browser push subscription.
    /// </summary>
    public class SavePushSubscriptionDto
    {
        /// <summary>
        /// Gets or sets the browser push endpoint.
        /// </summary>
        [Required]
        [MaxLength(2048)]
        [WebPushEndpoint]
        public string Endpoint { get; set; } = null!;

        /// <summary>
        /// Gets or sets the subscription p256dh key.
        /// </summary>
        [Required]
        public string P256dh { get; set; } = null!;

        /// <summary>
        /// Gets or sets the subscription auth secret.
        /// </summary>
        [Required]
        public string Auth { get; set; } = null!;

        /// <summary>
        /// Gets or sets the browser name reported by the client.
        /// </summary>
        [MaxLength(120)]
        public string? BrowserName { get; set; }

        /// <summary>
        /// Gets or sets the device name reported by the client.
        /// </summary>
        [MaxLength(160)]
        public string? DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the platform reported by the client.
        /// </summary>
        [MaxLength(120)]
        public string? Platform { get; set; }
    }
}
