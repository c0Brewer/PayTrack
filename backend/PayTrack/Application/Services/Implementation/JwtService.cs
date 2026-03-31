// <copyright file="JwtService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <summary>
    /// Service for JWT use-cases.
    /// </summary>
    public class JwtService(IConfiguration _config) : IJwtService
    {
        private readonly IConfiguration config = _config;

        /// <inheritdoc/>
        public async Task<string> GenerateJWTToken(string email)
        {
            var jwtSecret = this.config["JWT:Secret"] ?? throw new InternalErrorException("Could not load JWT Secret");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: [new Claim(ClaimTypes.Email, email)],
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
