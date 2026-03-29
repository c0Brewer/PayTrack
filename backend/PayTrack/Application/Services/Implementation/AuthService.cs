// <copyright file="AuthService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Google.Apis.Auth;
using PayTrack.Application.Dto.Auth;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class AuthService(IJwtService _jwtService, IUserService _userService) : IAuthService
    {
        private readonly IJwtService jwtService = _jwtService;
        private readonly IUserService userService = _userService;

        /// <inheritdoc/>
        public async Task<string> GoogleValidateCallback(
            GoogleAuthCallbackDto googleCallback)
        {
            var payload = await this.ValidateGoogleTokenAsync(googleCallback.IdToken);

            // Check if user exists
            var user = await this.userService.GetUserByEmailAsync(payload.Email);

            if (user == null)
            {
                await this.userService.CreateUserAsync(payload.Name, payload.Email, payload.Picture);
            }

            return await this.jwtService.GenerateJWTToken(payload.Email);
        }

        /// <summary>
        /// Validates a google token. Is protected virtual so that the tests work.
        /// </summary>
        /// <param name="idToken">token from google callback.</param>
        /// <returns>Payload from Google.</returns>
        [ExcludeFromCodeCoverage]
        protected virtual async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

            if (payload == null || string.IsNullOrEmpty(payload.Email))
            {
                throw new UnauthorizedException("Invalid Google Token");
            }

            return payload;
        }
    }
}
