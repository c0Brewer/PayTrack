// <copyright file="IBudgetRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Budget;
using PayTrack.Data.Entities;

namespace PayTrack.Data.Repositories.Model
{
    /// <summary>
    /// Repository for all Budget-related operations.
    /// </summary>
    public interface IBudgetRepository
    {
        /// <summary>
        /// Gets all Budgets with optional filtering.
        /// </summary>
        /// <param name="query">Query information to include in search.</param>
        /// <returns>List of Budget.</returns>
        Task<(List<Budget> budget, int totalCount)> GetAllAsync(GetBudgetQuery? query = null);

        /// <summary>
        /// Gets a specific Budget by their ID.
        /// </summary>
        /// <param name="id">id of Budget to find.</param>
        /// <returns>Budget with given ID.</returns>
        Task<Budget?> GetByIdAsync(
            int id);

        /// <summary>
        /// Adds a new Budget.
        /// </summary>
        /// <param name="teamId">The ID of the Team this Budget belongs to.</param>
        /// <param name="costCentreId">The ID of the Cost Centre this Budget belongs to.</param>
        /// <param name="targetAmount">The target amount for this Budget. Must be non-negative.</param>
        /// <param name="periodStart">The start date of the Budget period.</param>
        /// <param name="periodEnd">The end date of the Budget period.</param>
        /// <returns>The newly created Budget.</returns>
        Task<Budget> AddAsync(int teamId, int costCentreId, decimal targetAmount, DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Stages a range of Budget entries for a given Cost Centre without persisting to the database.
        /// The caller is responsible for calling SaveChangesAsync to commit the changes.
        /// </summary>
        /// <param name="costCentre">The Cost Centre to associate the Budget entries with.</param>
        /// <param name="entries">The list of Budget entries to stage.</param>
        /// <returns>A completed task when all entries have been staged.</returns>
        Task AddRangeAsync(CostCentre costCentre, IList<CreateCostCentreBudgetEntryDto> entries);

        /// <summary>
        /// Updates an existing Budget.
        /// </summary>
        /// <param name="id">The ID of the Budget to update.</param>
        /// <param name="teamId">If provided, updates the Team this Budget belongs to.</param>
        /// <param name="costCentreId">If provided, updates the Cost Centre this Budget belongs to.</param>
        /// <param name="targetAmount">If provided, updates the target amount. Must be non-negative.</param>
        /// <param name="periodStart">If provided, updates the start date of the Budget period.</param>
        /// <param name="periodEnd">If provided, updates the end date of the Budget period.</param>
        /// <returns>The updated Budget.</returns>
        Task<Budget> UpdateAsync(int id, int? teamId = null, int? costCentreId = null, decimal? targetAmount = null, DateTime? periodStart = null, DateTime? periodEnd = null);
    }
}
