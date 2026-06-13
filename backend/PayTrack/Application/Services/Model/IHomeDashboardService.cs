// <copyright file="IHomeDashboardService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Dashboard;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service which assembles the home dashboard payload.
    /// </summary>
    public interface IHomeDashboardService
    {
        /// <summary>
        /// Builds the home dashboard payload for the supplied user.
        /// </summary>
        /// <param name="currentUser">Current authenticated user.</param>
        /// <returns>The dashboard payload.</returns>
        Task<HomeDashboardDto> GetHomeDashboardAsync(User currentUser);
    }
}
