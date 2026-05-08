// <copyright file="CreateCostCentreBudgetEntryDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Budget
{
    /// <summary>
    /// Dto for a budget entry supplied during cost center creation.
    /// </summary>
    public sealed record class CreateCostCentreBudgetEntryDto(
        [property: Required]
        [property: MinLength(3)]
        string Name,

        string? Description,

        [property: Required]
        int TeamId,

        [property: Required]
        int SeasonId,

        [property: Required]
        [property: Range(0, double.MaxValue, ErrorMessage = "Target amount must be non-negative.")]
        decimal TargetAmount,

        [property: Required]
        DateTime PeriodStart,

        [property: Required]
        DateTime PeriodEnd)
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCostCentreBudgetEntryDto"/> class.
        /// </summary>
        /// <param name="TeamId">Team id.</param>
        /// <param name="TargetAmount">Target amount.</param>
        /// <param name="PeriodStart">Period start.</param>
        /// <param name="PeriodEnd">Period end.</param>
        public CreateCostCentreBudgetEntryDto(int TeamId, decimal TargetAmount, DateTime PeriodStart, DateTime PeriodEnd)
            : this("Budget", null, TeamId, 0, TargetAmount, PeriodStart, PeriodEnd)
        {
        }
    }
}
