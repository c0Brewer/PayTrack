// <copyright file="UpdateBudgetRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

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

        decimal? TargetAmount,

        DateTime? PeriodStart,

        DateTime? PeriodEnd,

        BudgetType? Type = null);
}
