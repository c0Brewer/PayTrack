// <copyright file="BudgetDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Budget
{
    /// <summary>
    /// Dto representing a Budget entry.
    /// </summary>
    public sealed record class BudgetDto(
        [property: Required]
        int Id,

        [property: Required]
        [property: MinLength(3)]
        string Name,

        string? Description,

        [property: Required]
        int TeamId,

        [property: Required]
        int CostCentreId,

        [property: Required]
        int SeasonId,

        [property: Required]
        decimal TargetAmount,

        [property: Required]
        DateTime PeriodStart,

        [property: Required]
        DateTime PeriodEnd,

        [property: Required]
        IList<int> TransactionIds);
}
