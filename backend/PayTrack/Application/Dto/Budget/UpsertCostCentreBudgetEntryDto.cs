// <copyright file="UpsertCostCentreBudgetEntryDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Budget
{
    /// <summary>
    /// Dto for upserting a budget entry during cost centre update.
    /// </summary>
    public sealed record class UpsertCostCentreBudgetEntryDto(
        int? Id,

        [property: Required]
        int TeamId,

        [property: Required]
        [property: Range(0, double.MaxValue, ErrorMessage = "Target amount must be non-negative.")]
        decimal TargetAmount,

        [property: Required]
        DateTime PeriodStart,

        [property: Required]
        DateTime PeriodEnd);
}
