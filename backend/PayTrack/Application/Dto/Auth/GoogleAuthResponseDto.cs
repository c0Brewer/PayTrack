// <copyright file="GoogleAuthResponseDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Auth
{
    /// <summary>
    /// Dto containing informatino we get from a google callback
    /// </summary>
    public sealed record class GoogleAuthResponseDto(
        [property: Required]
        string JwtToken
    );
}
