// <copyright file="IJWTService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service which handles JWT-related requests.
    /// </summary>
    public interface IJWTService
    {
        /// <summary>
        /// Generates a JWT Token from email.
        /// </summary>
        /// <param name="email">email to generate token from.</param>
        /// <returns>Generated Token.</returns>
        Task<string> GenerateJWTToken(string email);
    }
}
