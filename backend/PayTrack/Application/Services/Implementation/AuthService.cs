// <copyright file="AuthService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Google.Apis.Auth;
using PayTrack.Application.Dto.Auth;
using PayTrack.Application.Dto.User;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class AuthService(
        IJwtService _jwtService,
        IUserService _userService,
        IHttpContextAccessor _httpContextAccessor,
        IHttpClientFactory _httpClientFactory,
        IConfiguration _configuration) : IAuthService
    {
        private const int DefaultGoogleClockSkewSeconds = 300;

        private readonly IJwtService jwtService = _jwtService;
        private readonly IUserService userService = _userService;
        private readonly IHttpContextAccessor httpContextAccessor = _httpContextAccessor;
        private readonly IHttpClientFactory httpClientFactory = _httpClientFactory;
        private readonly IConfiguration configuration = _configuration;

        /// <inheritdoc/>
        public Task<User?> GetCurrentUser(GetUserQueryById? query = null)
        {
            var email = this.httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email) ?? throw new InternalErrorException("Could not find ClaimTypes");

            return this.userService.GetUserByEmailAsync(email.Value, query);
        }

        /// <inheritdoc/>
        public async Task<string> GoogleValidateCallback(
            GoogleAuthCallbackDto googleCallback)
        {
            var idToken = await this.ExchangeCodeForIdTokenAsync(googleCallback.Code);
            var payload = await this.ValidateGoogleTokenAsync(idToken);

            // Check if user exists
            var user = await this.userService.GetUserByEmailAsync(payload.Email);

            if (user == null)
            {
                user = await this.userService.CreateUserAsync(payload.Name, payload.Email, payload.Picture);
            }

            if (!user.IsActive)
            {
                throw new LockedException("Your Account is deactivated");
            }

            return await this.jwtService.GenerateJWTToken(payload.Email, user.Role);
        }

        /// <summary>
        /// Exchanges a Google authorization code for a Google ID token.
        /// </summary>
        /// <param name="code">Authorization code from Google Identity Services.</param>
        /// <returns>Google ID token.</returns>
        [ExcludeFromCodeCoverage]
        protected virtual async Task<string> ExchangeCodeForIdTokenAsync(string code)
        {
            var clientId = this.GetRequiredGoogleConfig("ClientId");
            var clientSecret = this.GetRequiredGoogleConfig("ClientSecret");

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["code"] = code,
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["redirect_uri"] = "postmessage",
                    ["grant_type"] = "authorization_code",
                }),
            };

            var httpClient = this.httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new UnauthorizedException("Could not exchange Google authorization code");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>();

            if (string.IsNullOrWhiteSpace(tokenResponse?.IdToken))
            {
                throw new UnauthorizedException("Google token response did not include an id_token");
            }

            return tokenResponse.IdToken;
        }

        /// <summary>
        /// Validates a google token. Is protected virtual so that the tests work.
        /// </summary>
        /// <param name="idToken">token from google callback.</param>
        /// <returns>Payload from Google.</returns>
        [ExcludeFromCodeCoverage]
        protected virtual async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken)
        {
            var clientId = this.GetRequiredGoogleConfig("ClientId");
            var clockSkew = this.GetGoogleClockSkew();
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId },
                    ExpirationTimeClockTolerance = clockSkew,
                    IssuedAtClockTolerance = clockSkew,
                });

            if (payload == null || string.IsNullOrEmpty(payload.Email))
            {
                throw new UnauthorizedException("Invalid Google Token");
            }

            return payload;
        }

        private TimeSpan GetGoogleClockSkew()
        {
            var value = this.configuration["Authentication:Google:ClockSkewSeconds"];

            if (int.TryParse(value, out var parsedSeconds) && parsedSeconds >= 0)
            {
                return TimeSpan.FromSeconds(parsedSeconds);
            }

            return TimeSpan.FromSeconds(DefaultGoogleClockSkewSeconds);
        }

        private string GetRequiredGoogleConfig(string key)
        {
            var value = this.configuration[$"Authentication:Google:{key}"];

            if (string.IsNullOrWhiteSpace(value))
            {
                value = Environment.GetEnvironmentVariable(key switch
                {
                    "ClientId" => "GOOGLE_CLIENT_ID",
                    "ClientSecret" => "GOOGLE_CLIENT_SECRET",
                    _ => $"GOOGLE_{key.ToUpperInvariant()}",
                });
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InternalErrorException($"Could not load Google {key}");
            }

            return value;
        }

        private sealed class GoogleTokenResponse
        {
            [JsonPropertyName("id_token")]
            public string? IdToken { get; set; }
        }
    }
}
