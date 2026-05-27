// <copyright file="CostCentre.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Cost Centre of the TUWIEN Racing Team.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CostCentre
    {
        /// <summary>
        /// Id of the Cost Centre.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Name of the Cost Centre.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Description of the Cost Centre.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Hex color string, e.g. "#FF5733".
        /// </summary>
        [MaxLength(7)]
        public string? DisplayColor { get; set; }

        /// <summary>
        /// Indicates if CostCentre is set active or inactive.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // --- Navigation ---

        /// <summary>
        /// Reference to linked Budgets.
        /// </summary>
        public ICollection<Budget> Budgets { get; set; } = [];
    }
}
