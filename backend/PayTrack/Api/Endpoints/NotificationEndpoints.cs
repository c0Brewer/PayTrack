// <copyright file="NotificationEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Api.Extensions;
using PayTrack.Api.Handler;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Contains endpoints for dispatching notifications.
    /// </summary>
    public static class NotificationEndpoints
    {
        private const string GroupName = "Notifications";
        private const string GroupRoute = "notify";

        /// <summary>
        /// Maps the endpoints for notification dispatch.
        /// </summary>
        /// <param name="app">WebApplication.</param>
        public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup($"/{GroupRoute}")
                .WithTags(GroupName)
                .RequireAuthorization()
                .RequireRole(Role.Admin);

            group.MapPost("/email", NotificationHandler.SendEmailNotificationAsync);
            group.MapPost("/slack", NotificationHandler.SendSlackNotificationAsync);
        }
    }
}
