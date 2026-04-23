// <copyright file="CostCentreService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Dto.CostCentre;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class CostCentreService(ICostCentreRepository repo) : ICostCentreService
    {
        /// <summary>
        /// Repository for CostCentres.
        /// </summary>
        private readonly ICostCentreRepository repo = repo;

        /// <inheritdoc/>
        public async Task<List<CostCentre>> GetAllAsync()
        {
            return await this.repo.GetAllAsync();
        }

        /// <inheritdoc/>
        public async Task<CostCentre?> GetByIdAsync(int id)
        {
            return await this.repo.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<CostCentre> CreateAsync(
            string name,
            string? description,
            string? displayColor,
            IList<CreateBudgetEntryDto>? budgetEntries)
        {
            var costCentre = new CostCentre
            {
                Name = name,
                Description = description,
                DisplayColor = displayColor,
            };

            return await this.repo.AddAsync(costCentre, budgetEntries);
        }

        /// <inheritdoc/>
        public async Task<CostCentre> UpdateAsync(int id, string? name, string? description, string? displayColor, IList<UpsertBudgetEntryDto>? budgetsToUpsert, IList<int>? budgetIdsToDelete)
        {
            if (budgetsToUpsert is not null && budgetIdsToDelete is not null)
            {
                var upsertIds = budgetsToUpsert.Where(e => e.Id > 0).Select(e => e.Id!.Value).ToHashSet();
                if (upsertIds.Overlaps(budgetIdsToDelete))
                {
                    throw new InvalidStateException("A budget ID cannot appear in both BudgetsToUpsert and BudgetIdsToDelete.");
                }
            }

            return await this.repo.UpdateAsync(id, name, description, displayColor, budgetsToUpsert, budgetIdsToDelete);
        }

        /// <inheritdoc/>
        public async Task<DeleteCostCentrePreviewDto> GetDeletePreviewAsync(int id)
        {
            return await this.repo.GetDeletePreviewAsync(id);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            await this.repo.DeleteAsync(id);
        }
    }
}
