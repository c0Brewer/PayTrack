// <copyright file="SystemSetting.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Generic key-value store for admin-configurable runtime settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SystemSetting
    {
        /// <summary>
        /// Id of SystemSetting.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Unique setting key (e.g. "csv.column.name").
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Key { get; set; } = null!;

        /// <summary>
        /// Serialized setting value.
        /// </summary>
        [Required]
        public string Value { get; set; } = null!;

        /// <summary>
        /// Timestamp of the last modification.
        /// </summary>
        public DateTime? LastModifiedAt { get; set; }

        /// <summary>
        /// Foreign key of the admin who last modified this setting.
        /// </summary>
        public int? LastModifiedByUserId { get; set; }

        /// <summary>
        /// Navigation property to the last modifier.
        /// </summary>
        [ForeignKey(nameof(LastModifiedByUserId))]
        public User? LastModifiedByUser { get; set; }
    }
}
