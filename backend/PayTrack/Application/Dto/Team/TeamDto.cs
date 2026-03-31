// <copyright file="TeamDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

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

        [property: MinLength(3)]
        string? DisplayColor);
}
