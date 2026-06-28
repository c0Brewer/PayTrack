// <copyright file="PushSubscription.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Browser push subscription for a user.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PushSubscription
    {
        /// <summary>
        /// Gets or sets the subscription id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the owning user id.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the owning user.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the browser push endpoint.
        /// </summary>
        [Required]
        [MaxLength(2048)]
        public string Endpoint { get; set; } = null!;

        /// <summary>
        /// Gets or sets the subscription p256dh key.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string P256dh { get; set; } = null!;

        /// <summary>
        /// Gets or sets the subscription auth secret.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Auth { get; set; } = null!;

        /// <summary>
        /// Gets or sets the browser name reported by the subscribing client.
        /// </summary>
        [MaxLength(120)]
        public string? BrowserName { get; set; }

        /// <summary>
        /// Gets or sets the device name reported by the subscribing client.
        /// </summary>
        [MaxLength(160)]
        public string? DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the platform reported by the subscribing client.
        /// </summary>
        [MaxLength(120)]
        public string? Platform { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this subscription is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the timestamp the subscription was created at.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp the subscription was last updated at.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
