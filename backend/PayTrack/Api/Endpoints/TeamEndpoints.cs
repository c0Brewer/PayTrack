// <copyright file="TeamEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Api.Extensions;
using PayTrack.Api.Handler;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Contains Endpoints for Teams.
    /// </summary>
    public static class TeamEndpoints
    {
        private const string GroupName = "Team";
        private const string GroupRoute = "team";

        /// <summary>
        /// Maps the Endpoints necessary for Teams.
        /// </summary>
        /// <param name="app">Web application.</param>
        public static void MapTeamEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup($"/{GroupRoute}")
                .WithTags(GroupName)
                .RequireAuthorization()
                .RequireRole(Role.Admin); // Who is finance team user?

            group.MapGet("/", TeamHandler.GetTeamsAsync);
            group.MapGet("/{id:int}", TeamHandler.GetTeamByIdAsync);
            group.MapPost("/", TeamHandler.CreateTeamAsync);
        }
    }
}
