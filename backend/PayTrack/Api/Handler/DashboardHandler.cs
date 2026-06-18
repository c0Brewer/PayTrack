// <copyright file="DashboardHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Application.Dto.Dashboard;
using PayTrack.Application.Dto.User;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler for dashboard-related endpoints.
    /// </summary>
    public static class DashboardHandler
    {
        /// <summary>
        /// Returns the home dashboard payload for the current user.
        /// </summary>
        /// <param name="authService">Dependency-Injected Authentication Service.</param>
        /// <param name="homeDashboardService">Dependency-Injected dashboard service.</param>
        /// <returns>The dashboard payload.</returns>
        public static async Task<Results<Ok<HomeDashboardDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> GetHomeDashboardAsync(
            IAuthService authService,
            IHomeDashboardService homeDashboardService)
        {
            var currentUser = await authService.GetCurrentUser(new GetUserQueryById
            {
                IncludeBankAccounts = true,
            }) ?? throw new NotFoundException("Current user not found");

            var dashboard = await homeDashboardService.GetHomeDashboardAsync(currentUser);

            return TypedResults.Ok(dashboard);
        }
    }
}
