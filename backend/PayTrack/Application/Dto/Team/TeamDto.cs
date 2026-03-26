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

        // This is just an example for dto mapping. This is necessary to e.g. hide information like password hash
        [property: Required]
        int id,

        [property: Required]
        [property: MinLength(3)]
        string name);
}
