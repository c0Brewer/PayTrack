// <copyright file="BudgetEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Api.Extensions;
using PayTrack.Api.Handler;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Contains Endpoints for Budgets.
    /// </summary>
    public static class BudgetEndpoints
    {
        private const string GroupName = "Budget";
        private const string GroupRoute = "budget";

        /// <summary>
        /// Maps the Endpoints necessary for Budgets.
        /// </summary>
        /// <param name="app">WebApplication.</param>
        public static void MapBudgetEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup($"/{GroupRoute}")
                .WithTags(GroupName)
                .RequireAuthorization();

            group.MapGet("/", BudgetHandler.GetBudgetsAsync);
            group.MapGet("/{id:int}", BudgetHandler.GetBudgetByIdAsync);

            group.MapPost("/", BudgetHandler.CreateBudgetAsync)
                .RequireRole(Role.Admin);

            group.MapPut("/{id:int}", BudgetHandler.UpdateBudgetAsync)
                .RequireRole(Role.Admin);

            group.MapDelete("/{id:int}", BudgetHandler.DeleteBudgetAsync)
                .RequireRole(Role.Admin);
        }
    }
}
