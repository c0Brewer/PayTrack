// <copyright file="UpdateBudgetRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Budget
{
    /// <summary>
    /// Dto containing necessary information for updating a budget.
    /// </summary>
    public sealed record class UpdateBudgetRequestDto(
        string? Name,

        string? Description,

        int? TeamId,

        int? CostCentreId,

        int? SeasonId,

        [property: Range(0, double.MaxValue, ErrorMessage = "Target amount must be non-negative.")]
        decimal? TargetAmount,

        DateTime? PeriodStart,

        DateTime? PeriodEnd);
}
