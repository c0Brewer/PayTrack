// <copyright file="UpdateSeasonRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Season
{
    /// <summary>
    /// Dto containing necessary information for updating a season.
    /// </summary>
    public sealed record class UpdateSeasonRequestDto(
        [property: MinLength(3)]
        string? Name);
}
