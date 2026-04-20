// <copyright file="TeamMemberDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.Team
{
    /// <summary>
    /// Lightweight DTO for a team member.
    /// </summary>
    public sealed record class TeamMemberDto(
        [property: Required]
        int Id,

        [property: Required]
        [property: MinLength(3)]
        string Name,

        [property: Required]
        [property: MinLength(3)]
        string Email,

        string? ProfilePictureUrl,

        [property: Required]
        Role Role,

        [property: Required]
        bool IsActive);
}
