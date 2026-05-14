// <copyright file="Team.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Entity representing a Team.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Team
    {
        /// <summary>
        /// Id of the Team.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Name of the Team.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Description of Team.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Hex color string, e.g. "#FF5733".
        /// </summary>
        [MaxLength(7)]
        public string? DisplayColor { get; set; }

        /// <summary>
        /// Indicates if Team is set active or inactive.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // --- Navigation ---

        /// <summary>
        /// Reference to linked Budgets.
        /// </summary>
        public ICollection<Budget> Budgets { get; set; } = [];

        /// <summary>
        /// Reference to linked Members.
        /// </summary>
        public ICollection<User> Members { get; set; } = [];

        /// <summary>
        /// Reference to linked Transactions.
        /// </summary>
        public ICollection<Transaction> Transactions { get; set; } = [];
    }
}
