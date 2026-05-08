// <copyright file="CreateSeasonRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Season
{
    /// <summary>
    /// Dto containing necessary information for creating a season.
    /// </summary>
    public sealed record class CreateSeasonRequestDto(
        [property: Required]
        [property: MinLength(3)]
        string Name);
}
