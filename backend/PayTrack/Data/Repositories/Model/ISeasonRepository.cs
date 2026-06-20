// <copyright file="ISeasonRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Season;
using PayTrack.Data.Entities;

namespace PayTrack.Data.Repositories.Model
{
    /// <summary>
    /// Repository for all Season-related operations.
    /// </summary>
    public interface ISeasonRepository
    {
        /// <summary>
        /// Returns all seasons including their budgets.
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
        /// Stores a new season.
        /// </summary>
        /// <param name="season">Season entity.</param>
        /// <returns>The created season.</returns>
        Task<Season> AddAsync(Season season);

        /// <summary>
        /// Updates an existing season.
        /// </summary>
        /// <param name="id">Id of the season.</param>
        /// <param name="name">New name, or null to leave unchanged.</param>
        /// <param name="isActive">Active status, or null to leave unchanged.</param>
        /// <returns>The updated season.</returns>
        Task<Season> UpdateAsync(int id, string? name, bool? isActive);

        /// <summary>
        /// Deletes a season when no budgets are linked, otherwise deactivates it.
        /// </summary>
        /// <param name="id">Id of the season.</param>
        /// <returns>Null when hard-deleted, otherwise the deactivated season.</returns>
        Task<Season?> DeleteAsync(int id);
    }
}
