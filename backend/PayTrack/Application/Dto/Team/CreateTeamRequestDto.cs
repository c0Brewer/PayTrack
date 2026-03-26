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
        string name,

        [property: MinLength(3)]
        string description,

        [property: MinLength(3)]
        string displayColor);
}
