// <copyright file="HealthEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Data;

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Contains infrastructure health endpoints for Kubernetes.
    /// </summary>
    public static class HealthEndpoints
    {
        /// <summary>
        /// Maps health endpoints used by Kubernetes probes and graceful shutdown.
        /// </summary>
        /// <param name="app">Web application.</param>
        public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/health");

            group.MapGet("/live", () => Results.Ok(new { status = "live" }));

            group.MapGet("/prepareShutdown", (HealthState healthState) =>
            {
                healthState.ShutdownRequested = true;
                return Results.Ok(new { status = "draining" });
            });

            group.MapGet("/ready", async (HealthState healthState, IHostEnvironment environment, IServiceProvider services) =>
            {
                if (healthState.ShutdownRequested)
                {
                    return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
                }

                if (environment.IsEnvironment("Test"))
                {
                    return Results.Ok(new { status = "ready" });
                }

                await using var scope = services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var canConnect = await db.Database.CanConnectAsync();
                return canConnect
                    ? Results.Ok(new { status = "ready" })
                    : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            });
        }
    }
}
