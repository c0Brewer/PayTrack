// <copyright file="AuthEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Api.Handler;

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Contains Endpoints for Authentication.
    /// </summary>
    public static class AuthEndpoints
    {
        private const string GroupName = "Authentication";
        private const string GroupRoute = "auth";

        /// <summary>
        /// Maps the Endpoints necessary for Authentication.
        /// </summary>
        /// <param name="app">Webapplication.</param>
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup($"/{GroupRoute}")
                .WithTags(GroupName);

            // Unauthorized
            group.MapPost("/google", AuthHandler.GoogleAuthCallback);

            // Authorized
            group.MapGet("/currentuser", AuthHandler.GetCurrentUserAsync)
                .RequireAuthorization();
        }
    }
}
