// <copyright file="AuthHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Application.Dto.Auth;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler which gets called from Endpoint to manage incoming Auth-related requests.
    /// </summary>
    public static class AuthHandler
    {
        /// <summary>
        /// Callback Method for Google Return on Login attempt.
        /// </summary>
        /// <param name="googleCallback">Callback Response from Google.</param>
        /// <param name="authService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<GoogleAuthResponseDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> GoogleAuthCallback(
            GoogleAuthCallbackDto googleCallback,
            IAuthService authService)
        {
            var jwtToken = await authService.GoogleValidateCallback(googleCallback) ?? throw new InternalErrorException("Error during Google Callback Validation");

            return TypedResults.Ok(new GoogleAuthResponseDto(jwtToken));
        }
    }
}
