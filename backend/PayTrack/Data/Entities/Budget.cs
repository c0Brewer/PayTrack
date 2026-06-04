// <copyright file="Budget.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Budget Target of a particular Team or Cost Centre.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Budget
    {
        /// <summary>
        /// Id of Budget.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Name of the Budget.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Description of the Budget.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Foreign Key for Season.
        /// </summary>
        [Required]
        public int SeasonId { get; set; }

        /// <summary>
        /// Reference to Season.
        /// </summary>
        [ForeignKey(nameof(SeasonId))]
        public Season Season { get; set; } = null!;

        /// <summary>
        /// Foreign Key for Team.
        /// </summary>
        [Required]
        public int TeamId { get; set; }

        /// <summary>
        /// Reference to Team.
        /// </summary>
        [ForeignKey(nameof(TeamId))]
        public Team Team { get; set; } = null!;

        /// <summary>
        /// Forieng Key for Cost Centre.
        /// </summary>
        [Required]
        public int CostCentreId { get; set; }

        /// <summary>
        /// Reference to Cost Centre.
        /// </summary>
        [ForeignKey(nameof(CostCentreId))]
        public CostCentre CostCentre { get; set; } = null!;

        /// <summary>
        /// Target Amount of Budget. Required for Expense budgets; must be null for Income budgets.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TargetAmount { get; set; }

        /// <summary>
        /// Type of this Budget (Expense or Income).
        /// </summary>
        public BudgetType Type { get; set; } = BudgetType.Expense;

        /// <summary>
        /// Start of Budget Period.
        /// </summary>
        [Required]
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// End of Budget Period.
        /// </summary>
        [Required]
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Reference to linked Transactions.
        /// </summary>
        public ICollection<Transaction> Transactions { get; set; } = [];
    }
}
