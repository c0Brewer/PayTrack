// <copyright file="ISeasonService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Season;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service which handles Season-related requests.
    /// </summary>
    public interface ISeasonService
    {
        /// <summary>
        /// Returns all seasons.
        /// </summary>
        /// <param name="query">Query options.</param>
        /// <returns>Matching seasons and total count before pagination.</returns>
        Task<(List<Season> seasons, int totalCount)> GetAllAsync(GetSeasonQuery? query = null);

        /// <summary>
        /// Gets a season by ID.
        /// </summary>
        /// <param name="id">Id of the season.</param>
        /// <returns>Season or null.</returns>
        Task<Season?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a season.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>The created season.</returns>
        Task<Season> CreateAsync(string name);

        /// <summary>
        /// Updates a season.
        /// </summary>
        /// <param name="id">Id of the season.</param>
        /// <param name="name">New name, or null to leave unchanged.</param>
        /// <param name="isActive">Active status, or null to leave unchanged.</param>
        /// <returns>The updated season.</returns>
        Task<Season> UpdateAsync(int id, string? name, bool? isActive);

        /// <summary>
        /// Deletes a season or deactivates it when linked budgets exist.
        /// </summary>
        /// <param name="id">Id of the season.</param>
        /// <returns>Null when hard-deleted, otherwise the deactivated season.</returns>
        Task<Season?> DeleteAsync(int id);
    }
}
