// <copyright file="GoogleAuthCallbackDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Auth
{
    /// <summary>
    /// Dto containing information we get from a google authorization code callback.
    /// </summary>
    public sealed record class GoogleAuthCallbackDto(
        [property: Required]
        string Code
    );
}
