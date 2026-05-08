// <copyright file="UpsertTeamBudgetEntryDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Budget
{
    /// <summary>
    /// Dto for upserting a budget entry during team update.
    /// </summary>
    public sealed record class UpsertTeamBudgetEntryDto(
        int? Id,

        [property: Required]
        [property: MinLength(3)]
        string Name,

        string? Description,

        [property: Required]
        int CostCentreId,

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
        /// Initializes a new instance of the <see cref="UpsertTeamBudgetEntryDto"/> class.
        /// </summary>
        /// <param name="Id">Budget id.</param>
        /// <param name="CostCentreId">Cost centre id.</param>
        /// <param name="TargetAmount">Target amount.</param>
        /// <param name="PeriodStart">Period start.</param>
        /// <param name="PeriodEnd">Period end.</param>
        public UpsertTeamBudgetEntryDto(int? Id, int CostCentreId, decimal TargetAmount, DateTime PeriodStart, DateTime PeriodEnd)
            : this(Id, "Budget", null, CostCentreId, 0, TargetAmount, PeriodStart, PeriodEnd)
        {
        }
    }
}
