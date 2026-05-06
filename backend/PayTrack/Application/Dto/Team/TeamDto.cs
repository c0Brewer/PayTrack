// <copyright file="TeamDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Dto.User;

namespace PayTrack.Application.Dto.Team
{
    /// <summary>
    /// Dto containing necessary information about a Team
    /// </summary>
    public sealed record class TeamDto(
        [property: Required]
        int Id,

        [property: Required]
        [property: MinLength(3)]
        string Name,

        [property: MinLength(3)]
        string? Description,

        [property: MaxLength(7)]
        [property: RegularExpression("^#[0-9A-Fa-f]{6}$")]
        string? DisplayColor,

        List<UserDto> Members,

        List<BudgetDto> Budgets,

        bool IsActive);
}
