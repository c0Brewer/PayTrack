// <copyright file="CostCentreEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Api.Extensions;
using PayTrack.Api.Handler;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Contains Endpoints for CostCentres.
    /// </summary>
    public static class CostCentreEndpoints
    {
        private const string GroupName = "CostCentre";
        private const string GroupRoute = "cost-centre";

        /// <summary>
        /// Maps the Endpoints necessary for CostCentres.
        /// </summary>
        /// <param name="app">WebApplication.</param>
        public static void MapCostCentreEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup($"/{GroupRoute}")
                .WithTags(GroupName)
                .RequireAuthorization();

            group.MapGet("/", CostCentreHandler.GetAllAsync);
            group.MapGet("/{id:int}", CostCentreHandler.GetByIdAsync);

            group.MapPost("/", CostCentreHandler.CreateAsync)
                .RequireRole(Role.Admin);

            group.MapPut("/{id:int}", CostCentreHandler.UpdateAsync)
                .RequireRole(Role.Admin);

            group.MapGet("/{id:int}/delete-preview", CostCentreHandler.GetDeletePreviewAsync)
                .RequireRole(Role.Admin);

            group.MapDelete("/{id:int}", CostCentreHandler.DeleteAsync)
                .RequireRole(Role.Admin);
        }
    }
}
