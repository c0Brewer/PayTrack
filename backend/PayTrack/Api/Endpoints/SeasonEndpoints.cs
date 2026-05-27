// <copyright file="SeasonEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Api.Extensions;
using PayTrack.Api.Handler;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Contains Endpoints for Seasons.
    /// </summary>
    public static class SeasonEndpoints
    {
        private const string GroupName = "Season";
        private const string GroupRoute = "season";

        /// <summary>
        /// Maps the Endpoints necessary for Seasons.
        /// </summary>
        /// <param name="app">WebApplication.</param>
        public static void MapSeasonEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup($"/{GroupRoute}")
                .WithTags(GroupName)
                .RequireAuthorization();

            group.MapGet("/", SeasonHandler.GetSeasonsAsync);

            group.MapPost("/", SeasonHandler.CreateSeasonAsync)
                .RequireRole(Role.Admin);

            group.MapPut("/{id:int}", SeasonHandler.UpdateSeasonAsync)
                .RequireRole(Role.Admin);

            group.MapDelete("/{id:int}", SeasonHandler.DeleteSeasonAsync)
                .RequireRole(Role.Admin);
        }
    }
}
