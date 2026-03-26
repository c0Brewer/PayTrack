// <copyright file="TeamEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using PayTrack.Api.Handler;

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
        /// <param name="app">Webapplication.</param>
        public static void MapTeamEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup($"/{GroupRoute}")
                .WithTags(GroupName);

            group.MapGet("/", TeamHandler.GetTeamsAsync);
            group.MapGet("/{id:int}", TeamHandler.GetTeamByIdAsync);
            group.MapPost("/", TeamHandler.CreateTeamAsync);
        }
    }
}
