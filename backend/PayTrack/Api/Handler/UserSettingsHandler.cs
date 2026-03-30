// <copyright file="UserSettingsHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Application.Dto.UserSettings;
using PayTrack.Application.Services.Model;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler which gets called from Endpoint to manage incoming UserSettings requests.
    /// </summary>
    public static class UserSettingsHandler
    {
        /// <summary>
        /// Gets the current user's settings.
        /// </summary>
        /// <param name="user">The authenticated user claims.</param>
        /// <param name="userSettingsService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<UserSettingsDto>, UnauthorizedHttpResult, BadRequest<ProblemDetails>>> GetUserSettingsAsync(
            ClaimsPrincipal user,
            IUserSettingsService userSettingsService)
        {
            var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return TypedResults.Unauthorized();
            }

            var settings = await userSettingsService.GetUserSettingsAsync(userId);
            return TypedResults.Ok(settings);
        }

        /// <summary>
        /// Updates the current user's settings.
        /// </summary>
        /// <param name="updateDto">The DTO containing the updated settings.</param>
        /// <param name="user">The authenticated user claims.</param>
        /// <param name="userSettingsService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<NoContent, UnauthorizedHttpResult, BadRequest<ProblemDetails>>> UpdateUserSettingsAsync(
            [FromBody] UserSettingsDto updateDto,
            ClaimsPrincipal user,
            IUserSettingsService userSettingsService)
        {
            var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return TypedResults.Unauthorized();
            }

            await userSettingsService.UpdateUserSettingsAsync(userId, updateDto);
            return TypedResults.NoContent();
        }
    }
}