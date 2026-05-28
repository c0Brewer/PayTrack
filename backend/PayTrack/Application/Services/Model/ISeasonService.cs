// <copyright file="ISeasonService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

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
        /// <returns>List of seasons.</returns>
        Task<List<Season>> GetAllAsync();

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
        /// <returns>The updated season.</returns>
        Task<Season> UpdateAsync(int id, string? name);

        /// <summary>
        /// Deletes a season.
        /// </summary>
        /// <param name="id">Id of the season.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteAsync(int id);
    }
}
