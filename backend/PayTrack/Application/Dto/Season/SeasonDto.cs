// <copyright file="SeasonDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Dto.Budget;

namespace PayTrack.Application.Dto.Season
{
    /// <summary>
    /// Dto representing a season.
    /// </summary>
    public sealed record class SeasonDto(
        [property: Required]
        int Id,

        [property: Required]
        string Name,

        IList<BudgetDto>? Budgets);
}
