// <copyright file="GoogleUserProfileDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Auth
{
    /// <summary>
    /// Dto containing informatino we get from google about a user
    /// </summary>
    public sealed record class GoogleUserProfileDto(
        [property: Required]
        string Email,

        [property: Required]
        string Name,

        [property: Required]
        string Picture
    );
}
