// <copyright file="UpdateUserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.User
{
    /// <summary>
    /// Dto containing necessary information for updating a User.
    /// </summary>
    public sealed record class UpdateUserDto(
        [property: Required]
        Role? Role,

        [property: Required]
        bool? IsActive,

        [property: Required]
        int? TeamId);
}
