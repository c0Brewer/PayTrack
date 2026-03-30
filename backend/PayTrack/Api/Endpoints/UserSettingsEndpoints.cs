// <copyright file="UserSettingsEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PayTrack.Api.Handler;

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Contains Endpoints for User Settings.
    /// </summary>
    public static class UserSettingsEndpoints
    {
        private const string GroupName = "UserSettings";
        private const string GroupRoute = "usersettings";

        /// <summary>
        /// Maps the Endpoints necessary for User Settings.
        /// </summary>
        /// <param name="app">Web application.</param>
        public static void MapUserSettingsEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup($"/{GroupRoute}")
                .WithTags(GroupName)
                .RequireAuthorization();

            // GET usersettings
            group.MapGet("/", UserSettingsHandler.GetUserSettingsAsync);

            // PUT usersettings
            group.MapPut("/", UserSettingsHandler.UpdateUserSettingsAsync);
        }
    }
}