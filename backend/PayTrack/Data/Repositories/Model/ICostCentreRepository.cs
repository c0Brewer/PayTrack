// <copyright file="ICostCentreRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Dto.CostCentre;
using PayTrack.Data.Entities;

namespace PayTrack.Data.Repositories.Model
{
    /// <summary>
    /// Repository for all CostCentre-related operations.
    /// </summary>
    public interface ICostCentreRepository
    {
        /// <summary>
        /// Returns all cost centers from DB, including their budgets.
        /// </summary>
        /// <returns>List of CostCentre objects.</returns>
        Task<List<CostCentre>> GetAllAsync();

        /// <summary>
        /// Gets a specific cost center by ID, including its budgets.
        /// </summary>
        /// <param name="id">Id of the cost center.</param>
        /// <returns>CostCentre or null.</returns>
        Task<CostCentre?> GetByIdAsync(int id);

        /// <summary>
        /// Stores a new cost center, optionally with budget entries.
        /// </summary>
        /// <param name="costCentre">CostCentre entity.</param>
        /// <param name="budgetEntries">Optional budget entries to create alongside.</param>
        /// <returns>The created CostCentre.</returns>
        Task<CostCentre> AddAsync(CostCentre costCentre, IList<CreateBudgetEntryDto>? budgetEntries);

        /// <summary>
        /// Updates an existing cost center. Only non-null fields are applied.
        /// </summary>
        /// <param name="id">Id of the cost center to update.</param>
        /// <param name="name">New name, or null to leave unchanged.</param>
        /// <param name="description">New description, or null to leave unchanged.</param>
        /// <param name="displayColor">New display color, or null to leave unchanged.</param>
        /// <param name="budgetsToUpsert">Budget entries to add (Id null/0) or update (Id > 0).</param>
        /// <param name="budgetIdsToDelete">Ids of budget entries to remove.</param>
        /// <returns>The updated CostCentre.</returns>
        Task<CostCentre> UpdateAsync(
            int id,
            string? name,
            string? description,
            string? displayColor,
            IList<UpsertBudgetEntryDto>? budgetsToUpsert,
            IList<int>? budgetIdsToDelete);

        /// <summary>
        /// Returns a preview of what would be affected by deleting the given cost center.
        /// </summary>
        /// <param name="id">Id of the cost center.</param>
        /// <returns>DeleteCostCentrePreviewDto with affected counts.</returns>
        Task<DeleteCostCentrePreviewDto> GetDeletePreviewAsync(int id);

        /// <summary>
        /// Deletes the cost center. Throws InvalidStateException if linked records exist.
        /// </summary>
        /// <param name="id">Id of the cost center to delete.</param>
        /// <returns>A <see cref="Task"/> representing the async operation.</returns>
        Task DeleteAsync(int id);
    }
}
