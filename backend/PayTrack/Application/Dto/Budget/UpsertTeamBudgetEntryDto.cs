// <copyright file="UpsertTeamBudgetEntryDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Data.Entities;

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

        decimal? TargetAmount,

        [property: Required]
        DateTime PeriodStart,

        [property: Required]
        DateTime PeriodEnd,

        BudgetType Type = BudgetType.Expense) : IBudgetEntryDto;
}
