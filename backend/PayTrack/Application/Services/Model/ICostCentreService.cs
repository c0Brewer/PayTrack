// <copyright file="ICostCentreService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Dto.CostCentre;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service which handles CostCentre-related requests.
    /// </summary>
    public interface ICostCentreService
    {
        /// <summary>
        /// Returns all cost centers.
        /// </summary>
        /// <returns>List of CostCentre objects.</returns>
        Task<List<CostCentre>> GetAllAsync();

        /// <summary>
        /// Gets a cost center by ID.
        /// </summary>
        /// <param name="id">Id of the cost center.</param>
        /// <returns>CostCentre or null.</returns>
        Task<CostCentre?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a cost center, optionally with initial budget entries.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="description">Description.</param>
        /// <param name="displayColor">Display color hex string.</param>
        /// <param name="budgetEntries">Optional initial budgets.</param>
        /// <returns>The created CostCentre.</returns>
        Task<CostCentre> CreateAsync(
            string name,
            string? description,
            string? displayColor,
            IList<CreateBudgetEntryDto>? budgetEntries);

        /// <summary>
        /// Partially updates a cost center. Null fields are left unchanged.
        /// </summary>
        /// <param name="id">Id of the cost center to update.</param>
        /// <param name="name">New name, or null to leave unchanged.</param>
        /// <param name="description">New description, or null to leave unchanged.</param>
        /// <param name="displayColor">New display color, or null to leave unchanged.</param>
        /// <param name="budgetsToUpsert">Budget entries to add (Id null/0) or update (Id > 0). Null means no change.</param>
        /// <param name="budgetIdsToDelete">Ids of budget entries to remove. Null means no change.</param>
        /// <returns>The updated CostCentre.</returns>
        Task<CostCentre> UpdateAsync(
            int id,
            string? name,
            string? description,
            string? displayColor,
            IList<UpsertBudgetEntryDto>? budgetsToUpsert,
            IList<int>? budgetIdsToDelete);

        /// <summary>
        /// Returns a preview of entities affected by deleting the given cost center.
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
