// <copyright file="CreateCostCentreBudgetEntryDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Data.Entities;

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

        decimal? TargetAmount,

        [property: Required]
        DateTime PeriodStart,

        [property: Required]
        DateTime PeriodEnd,

        BudgetType Type = BudgetType.Expense) : IBudgetEntryDto;
}
