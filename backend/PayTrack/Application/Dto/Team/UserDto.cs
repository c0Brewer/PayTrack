// <copyright file="UserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.Team
{
    /// <summary>
    /// Dto containing necessary information about a User
    /// </summary>
    public sealed record class UserDto(
        [property: Required]
        int Id,

        [property: Required]
        [property: MinLength(3)]
        string Name,

        [property: Required]
        [property: MinLength(3)]
        string Email,

        [property: Required]
        [property: MinLength(3)]
        string? ProfilePictureUrl,

        [property: Required]
        Role Role,

        [property: Required]
        bool IsActive);
}
