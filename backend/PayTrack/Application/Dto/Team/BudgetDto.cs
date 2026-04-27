// <copyright file="BudgetDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Team
{
    /// <summary>
    /// Lightweight DTO for a team budget.
    /// </summary>
    public sealed record class BudgetDto(
        [property: Required]
        int Id,

        [property: Required]
        int CostCentreId,

        [property: Required]
        decimal TargetAmount,

        [property: Required]
        DateTime PeriodStart,

        [property: Required]
        DateTime PeriodEnd);
}
