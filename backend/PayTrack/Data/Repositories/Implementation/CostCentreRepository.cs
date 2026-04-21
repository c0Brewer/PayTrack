// <copyright file="CostCentreRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Dto.CostCentre;
using PayTrack.Application.Exceptions;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class CostCentreRepository(AppDbContext _context) : ICostCentreRepository
    {
        /// <summary>
        /// Database Context for accessing the DB.
        /// </summary>
        private readonly AppDbContext context = _context;

        /// <inheritdoc/>
        public async Task<List<CostCentre>> GetAllAsync()
        {
            return await this.context.CostCentres
                .Include(c => c.Budgets)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<CostCentre?> GetByIdAsync(int id)
        {
            return await this.context.CostCentres
                .Include(c => c.Budgets)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <inheritdoc/>
        public async Task<CostCentre> AddAsync(CostCentre costCentre, IList<CreateBudgetEntryDto>? budgetEntries)
        {
            this.context.CostCentres.Add(costCentre);

            if (budgetEntries is not null)
            {
                foreach (var entry in budgetEntries)
                {
                    this.context.Budgets.Add(new Budget
                    {
                        CostCentre = costCentre,
                        TeamId = entry.TeamId,
                        TargetAmount = entry.TargetAmount,
                        PeriodStart = entry.PeriodStart,
                        PeriodEnd = entry.PeriodEnd,
                    });
                }
            }

            int res = await this.context.SaveChangesAsync();
            int expectedCount = 1 + (budgetEntries?.Count ?? 0);

            if (res != expectedCount)
            {
                throw new InternalErrorException($"Saving CostCentre did not end as expected. Saved {res} records.");
            }

            return costCentre;
        }

        /// <inheritdoc/>
        public async Task<CostCentre> UpdateAsync(int id, string? name, string? description, string? displayColor)
        {
            var costCentre = await this.context.CostCentres
                .Include(c => c.Budgets)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException($"CostCentre with id {id} could not be found.");

            if (name is not null)
            {
                costCentre.Name = name;
            }

            if (description is not null)
            {
                costCentre.Description = description;
            }

            if (displayColor is not null)
            {
                costCentre.DisplayColor = displayColor;
            }

            int res = await this.context.SaveChangesAsync();

            if (res != 1)
            {
                throw new InternalErrorException($"Updating CostCentre did not end as expected. Saved {res} records.");
            }

            return costCentre;
        }

        /// <inheritdoc/>
        public async Task<DeleteCostCentrePreviewDto> GetDeletePreviewAsync(int id)
        {
            var costCentre = await this.context.CostCentres
                .Include(c => c.Budgets)
                    .ThenInclude(b => b.Team)
                .Include(c => c.Transactions)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException($"CostCentre with id {id} could not be found.");

            var affectedTeamNames = costCentre.Budgets
                .Select(b => b.Team.Name)
                .Distinct()
                .ToList();

            return new DeleteCostCentrePreviewDto(
                costCentre.Name,
                costCentre.Budgets.Count,
                costCentre.Transactions.Count,
                affectedTeamNames);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            var costCentre = await this.context.CostCentres
                .Include(c => c.Budgets)
                .Include(c => c.Transactions)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException($"CostCentre with id {id} could not be found.");

            if (costCentre.Budgets.Count > 0 || costCentre.Transactions.Count > 0)
            {
                throw new InvalidStateException(
                    $"Cannot delete CostCentre '{costCentre.Name}': " +
                    $"{costCentre.Budgets.Count} budget(s) and {costCentre.Transactions.Count} transaction(s) are still linked. " +
                    $"Call GET /cost-centre/{id}/delete-preview for the full impact summary.");
            }

            this.context.CostCentres.Remove(costCentre);
            int res = await this.context.SaveChangesAsync();

            if (res != 1)
            {
                throw new InternalErrorException($"Deleting CostCentre did not end as expected. Affected {res} records.");
            }
        }
    }
}
