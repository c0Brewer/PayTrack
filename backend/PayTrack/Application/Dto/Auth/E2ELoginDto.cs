// <copyright file="E2ELoginDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.Auth
{
    /// <summary>
    /// Dto containing the test user identity for E2E login.
    /// </summary>
    public sealed record class E2ELoginDto(
        [property: Required]
        [property: EmailAddress]
        string Email,

        [property: Required]
        Role Role
    );
}
