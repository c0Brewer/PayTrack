// <copyright file="IJwtService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service which handles JWT-related requests.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a JWT Token from email.
        /// </summary>
        /// <param name="email">email to generate token from.</param>
        /// <param name="role">role of user.</param>
        /// <returns>Generated Token.</returns>
        Task<string> GenerateJWTToken(string email, Role role);
    }
}
