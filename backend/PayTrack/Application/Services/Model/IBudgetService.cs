// <copyright file="IBudgetService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Budget;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service which handles Budget-related requests.
    /// </summary>
    public interface IBudgetService
    {
        /// <summary>
        /// Returns all Budgets from DB.
        /// </summary>
        /// <param name="query">Query information for search.</param>
        /// <returns>List of Budget objects.</returns>
        Task<(List<Budget> budget, int totalCount)> GetBudgetsAsync(GetBudgetQuery? query = null);

        /// <summary>
        /// Gets a specific Budget by its ID.
        /// </summary>
        /// <param name="id">Id of Budget to find.</param>
        /// <returns>Budget with given ID.</returns>
        Task<Budget?> GetBudgetByIdAsync(int id);

        /// <summary>
        /// Creates a Budget using the given input.
        /// </summary>
        /// <param name="name">Name of budget.</param>
        /// <param name="description">Description of budget.</param>
        /// <param name="teamId">Team id.</param>
        /// <param name="costCentreId">Cost centre id.</param>
        /// <param name="seasonId">Season id.</param>
        /// <param name="targetAmount">Target amount.</param>
        /// <param name="periodStart">Period start.</param>
        /// <param name="periodEnd">Period end.</param>
        /// <returns>Instance of created Budget object.</returns>
        Task<Budget> CreateBudgetAsync(
            string name,
            string? description,
            int teamId,
            int costCentreId,
            int seasonId,
            decimal targetAmount,
            DateTime periodStart,
            DateTime periodEnd);

        /// <summary>
        /// Updates a Budget using the given input.
        /// </summary>
        /// <param name="id">Budget id.</param>
        /// <param name="name">Name of budget.</param>
        /// <param name="description">Description of budget.</param>
        /// <param name="teamId">Team id.</param>
        /// <param name="costCentreId">Cost centre id.</param>
        /// <param name="seasonId">Season id.</param>
        /// <param name="targetAmount">Target amount.</param>
        /// <param name="periodStart">Period start.</param>
        /// <param name="periodEnd">Period end.</param>
        /// <returns>Instance of updated Budget object.</returns>
        Task<Budget> UpdateBudgetAsync(
            int id,
            string? name = null,
            string? description = null,
            int? teamId = null,
            int? costCentreId = null,
            int? seasonId = null,
            decimal? targetAmount = null,
            DateTime? periodStart = null,
            DateTime? periodEnd = null);

        /// <summary>
        /// Deletes a Budget by id.
        /// </summary>
        /// <param name="id">Budget id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteBudgetAsync(int id);
    }
}
