// <copyright file="CreateBudgetRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.Budget
{
    /// <summary>
    /// Dto containing necessary information for creating a budget.
    /// </summary>
    public sealed record class CreateBudgetRequestDto(
        [property: Required]
        string Name,

        string? Description,

        [property: Required]
        int TeamId,

        [property: Required]
        int CostCentreId,

        [property: Required]
        int SeasonId,

        decimal? TargetAmount,

        [property: Required]
        DateTime PeriodStart,

        [property: Required]
        DateTime PeriodEnd,

        BudgetType? Type = null);
}
