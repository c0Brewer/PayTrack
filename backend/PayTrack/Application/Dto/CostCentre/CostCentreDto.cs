// <copyright file="CostCentreDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Dto.Budget;

namespace PayTrack.Application.Dto.CostCentre
{
    /// <summary>
    /// Dto representing a Cost Center.
    /// </summary>
    public sealed record class CostCentreDto(
        [property: Required]
        int Id,

        [property: Required]
        [property: MinLength(3)]
        string Name,

        string? Description,

        string? DisplayColor,

        IList<BudgetDto> Budgets);
}
