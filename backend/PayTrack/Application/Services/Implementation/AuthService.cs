// <copyright file="AuthService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Google.Apis.Auth;
using PayTrack.Application.Dto.Auth;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class AuthService(IJwtService _jwtService, IUserService _userService, IHttpContextAccessor _httpContextAccessor) : IAuthService
    {
        private readonly IJwtService jwtService = _jwtService;
        private readonly IUserService userService = _userService;
        private readonly IHttpContextAccessor httpContextAccessor = _httpContextAccessor;

        /// <inheritdoc/>
        public Task<User?> GetCurrentUser()
        {
            var email = this.httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email) ?? throw new InternalErrorException("Could not find ClaimTypes");

            return this.userService.GetUserByEmailAsync(email.Value);
        }

        /// <inheritdoc/>
        public async Task<string> GoogleValidateCallback(
            GoogleAuthCallbackDto googleCallback)
        {
            var payload = await this.ValidateGoogleTokenAsync(googleCallback.IdToken);

            // Check if user exists
            var user = await this.userService.GetUserByEmailAsync(payload.Email);

            if (user == null)
            {
                user = await this.userService.CreateUserAsync(payload.Name, payload.Email, payload.Picture);
            }

            if (!user.IsActive)
            {
                throw new ForbiddenException("Your Account is deactivated");
            }

            return await this.jwtService.GenerateJWTToken(payload.Email, user.Role);
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
