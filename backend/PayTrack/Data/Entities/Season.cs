// <copyright file="Season.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Entity representing a season.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Season
    {
        /// <summary>
        /// Id of the Season.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Name of the Season.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Indicates if Season is set active or inactive.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Budgets assigned to this Season.
        /// </summary>
        public ICollection<Budget> Budgets { get; set; } = [];
    }
}
