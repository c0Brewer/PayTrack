// <copyright file="BudgetDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.CostCentre
{
    /// <summary>
    /// Dto representing a Budget entry linked to a cost center.
    /// </summary>
    public sealed record class BudgetDto(
        [property: Required]
        int Id,

        [property: Required]
        int TeamId,

        [property: Required]
        int CostCentreId,

        [property: Required]
        decimal TargetAmount,

        [property: Required]
        DateTime PeriodStart,

        [property: Required]
        DateTime PeriodEnd);
}
