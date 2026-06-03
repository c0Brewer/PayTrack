// <copyright file="BudgetDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.Budget
{
    /// <summary>
    /// Dto representing a Budget entry.
    /// </summary>
    public sealed record class BudgetDto(
        [property: Required]
        int Id,

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

        [property: Required]
        BudgetType Type,

        [property: Required]
        IList<int> TransactionIds,

        [property: Required]
        decimal PaidAmount,

        [property: Required]
        decimal ApprovedAmount);
}
