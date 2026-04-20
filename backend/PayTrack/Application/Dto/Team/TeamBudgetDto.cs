// <copyright file="TeamBudgetDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Team
{
    /// <summary>
    /// Lightweight DTO for a team budget.
    /// </summary>
    public sealed record class TeamBudgetDto(
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
