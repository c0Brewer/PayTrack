// <copyright file="UpdateUserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.User
{
    /// <summary>
    /// Dto containing necessary information for updating a User.
    /// </summary>
    public sealed record class UpdateUserDto(

        // Optional:
        string? Name,

        // Optional:
        Role? Role,

        // Optional:
        bool? IsActive,

        // Optional:
        int? TeamId);
}
