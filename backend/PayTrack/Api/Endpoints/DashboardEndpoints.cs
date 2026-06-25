// <copyright file="DashboardEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Api.Handler;

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Contains endpoints for dashboard views.
    /// </summary>
    public static class DashboardEndpoints
    {
        private const string GroupName = "Dashboard";
        private const string GroupRoute = "dashboard";

        /// <summary>
        /// Maps the endpoints necessary for dashboard views.
        /// </summary>
        /// <param name="app">Web application.</param>
        public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGroup($"/{GroupRoute}")
                .WithTags(GroupName)
                .RequireAuthorization()
                .MapGet("/home", DashboardHandler.GetHomeDashboardAsync);
        }
    }
}
