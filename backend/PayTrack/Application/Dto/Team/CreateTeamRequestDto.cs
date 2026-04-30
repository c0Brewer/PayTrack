// <copyright file="CreateTeamRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Team
{
    /// <summary>
    /// Dto containing necessary information for creating a team
    /// </summary>
    public sealed record class CreateTeamRequestDto(
        [property: Required]
        [property: MinLength(3)]
        string Name,

        [property: MinLength(2)]
        string? Description,

        [property: MaxLength(7)]
        [property: RegularExpression("^#[0-9A-Fa-f]{6}$")]
        string? DisplayColor,

        IList<CreateTeamBudgetEntryDto>? Budgets);
}
