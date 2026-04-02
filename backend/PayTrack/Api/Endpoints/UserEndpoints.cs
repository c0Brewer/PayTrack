// <copyright file="UserEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Api.Extensions;
using PayTrack.Api.Handler;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Contains Endpoints for User.
    /// </summary>
    public static class UserEndpoints
    {
        private const string GroupName = "User";
        private const string GroupRoute = "user";

        /// <summary>
        /// Maps the Endpoints necessary for Users.
        /// </summary>
        /// <param name="app">Webapplication.</param>
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup($"/{GroupRoute}")
                .WithTags(GroupName)
                .RequireAuthorization()
                .RequireRole(Role.Admin);

            group.MapGet("/", UserHandler.GetUsersAsync);
            group.MapGet("/{id:int}", UserHandler.GetUserByIdAsync);
            group.MapPut("/{id:int}", UserHandler.UpdateUserAsync);
        }
    }
}
