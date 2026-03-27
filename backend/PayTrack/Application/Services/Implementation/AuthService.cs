// <copyright file="AuthService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Google.Apis.Auth;
using Newtonsoft.Json;
using PayTrack.Application.Dto.Auth;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class AuthService(IJWTService _jwtService, IUserService _userService, IConfiguration config) : IAuthService
    {
        private readonly IJWTService jwtService = _jwtService;
        private readonly IUserService userService = _userService;
        private readonly string googleUserInfoUrl = config["Google:UserInfoUrl"] !;

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

        private async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

            if (payload == null || string.IsNullOrEmpty(payload.Email))
            {
                throw new UnauthorizedException("Invalid Google Token");
            }

            return payload;
        }

        private async Task<GoogleUserProfileDto> GetGoogleUserProfileAsync(string accessToken)
        {
            using var httpClient = new HttpClient();

            // Send GET request to Google UserInfo API
            var response = await httpClient.GetAsync($"{this.googleUserInfoUrl}?access_token={accessToken}");
            response.EnsureSuccessStatusCode();

            // Parse the JSON response
            var content = await response.Content.ReadAsStringAsync();
            var userProfile = JsonConvert.DeserializeObject<GoogleUserProfileDto>(content);

            return userProfile!;
        }
    }
}
