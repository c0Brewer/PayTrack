// <copyright file="UnsubscribePushNotificationDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Notification
{
    /// <summary>
    /// Request body for disabling a browser push subscription.
    /// </summary>
    public class UnsubscribePushNotificationDto
    {
        /// <summary>
        /// Gets or sets the browser push endpoint.
        /// </summary>
        [Required]
        public string Endpoint { get; set; } = null!;
    }
}
