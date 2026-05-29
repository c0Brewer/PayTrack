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
    public class CostCentreRepository(AppDbContext _context, IBudgetRepository _budgetRepository) : ICostCentreRepository
    {
        /// <summary>
        /// Database Context for accessing the DB.
        /// </summary>
        private readonly AppDbContext context = _context;
        private readonly IBudgetRepository budgetRepository = _budgetRepository;

        /// <inheritdoc/>
        public async Task<(List<CostCentre> Items, int TotalCount)> GetAllAsync(GetCostCentreQuery? query = null)
        {
            var today = DateTime.UtcNow;
            IQueryable<CostCentre> dbQuery = this.context.CostCentres.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query?.Name))
            {
                dbQuery = dbQuery.Where(c => EF.Functions.Like(c.Name, $"%{query.Name}%"));
            }

            if (!string.IsNullOrWhiteSpace(query?.Description))
            {
                dbQuery = dbQuery.Where(c => c.Description != null && EF.Functions.Like(c.Description, $"%{query.Description}%"));
            }

            if (query?.MinBudget.HasValue == true || query?.MaxBudget.HasValue == true)
            {
                dbQuery = dbQuery.Where(c => c.Budgets.Any(b =>
                    b.PeriodStart <= today && today <= b.PeriodEnd &&
                    (!query.MinBudget.HasValue || b.TargetAmount >= query.MinBudget.Value) &&
                    (!query.MaxBudget.HasValue || b.TargetAmount <= query.MaxBudget.Value)));
            }

            var totalCount = await dbQuery.CountAsync();

            if (query?.Offset.HasValue == true)
            {
                dbQuery = dbQuery.Skip(query.Offset.Value);
            }

            if (query?.Limit.HasValue == true)
            {
                dbQuery = dbQuery.Take(query.Limit.Value);
            }

            var items = await dbQuery
                .Include(c => c.Budgets)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <inheritdoc/>
        public async Task<CostCentre?> GetByIdAsync(int id)
        {
            return await this.context.CostCentres
                .Include(c => c.Budgets)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <inheritdoc/>
        public async Task<CostCentre> AddAsync(CostCentre costCentre, IList<CreateCostCentreBudgetEntryDto>? budgetEntries = null)
        {
            this.context.CostCentres.Add(costCentre);
            if (budgetEntries is not null)
            {
                await this.budgetRepository.AddRangeAsync(costCentre, budgetEntries);
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
        public async Task<CostCentre> UpdateAsync(int id, string? name = null, string? description = null, string? displayColor = null, IList<UpsertCostCentreBudgetEntryDto>? budgetsToUpsert = null, IList<int>? budgetIdsToDelete = null)
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

            if (budgetIdsToDelete is not null)
            {
                foreach (var budgetId in budgetIdsToDelete)
                {
                    var budget = costCentre.Budgets.FirstOrDefault(b => b.Id == budgetId)
                        ?? throw new NotFoundException($"Budget with id {budgetId} not found on CostCentre {id}.");
                    this.context.Budgets.Remove(budget);
                }
            }

            if (budgetsToUpsert is not null)
            {
                foreach (var entry in budgetsToUpsert)
                {
                    if (entry.Id is null or 0)
                    {
                        this.context.Budgets.Add(new Budget
                        {
                            Name = entry.Name,
                            Description = entry.Description,
                            CostCentre = costCentre,
                            TeamId = entry.TeamId,
                            SeasonId = entry.SeasonId,
                            TargetAmount = entry.TargetAmount,
                            PeriodStart = DateTime.SpecifyKind(entry.PeriodStart, DateTimeKind.Utc),
                            PeriodEnd = DateTime.SpecifyKind(entry.PeriodEnd, DateTimeKind.Utc),
                            Type = entry.Type,
                        });
                    }
                    else
                    {
                        var existing = costCentre.Budgets.FirstOrDefault(b => b.Id == entry.Id)
                            ?? throw new NotFoundException($"Budget with id {entry.Id} not found on CostCentre {id}.");
                        existing.Name = entry.Name;
                        existing.Description = entry.Description;
                        existing.TeamId = entry.TeamId;
                        existing.SeasonId = entry.SeasonId;
                        existing.TargetAmount = entry.TargetAmount;
                        existing.PeriodStart = DateTime.SpecifyKind(entry.PeriodStart, DateTimeKind.Utc);
                        existing.PeriodEnd = DateTime.SpecifyKind(entry.PeriodEnd, DateTimeKind.Utc);
                        existing.Type = entry.Type;
                    }
                }
            }

            bool hasChanges = name is not null || description is not null || displayColor is not null
                || (budgetIdsToDelete?.Count > 0) || (budgetsToUpsert?.Count > 0);

            if (!hasChanges)
            {
                return costCentre;
            }

            int res = await this.context.SaveChangesAsync();

            if (res < 1)
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
                .Include(c => c.Budgets)
                    .ThenInclude(b => b.Transactions)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException($"CostCentre with id {id} could not be found.");

            var affectedTeamNames = costCentre.Budgets
                .Select(b => b.Team.Name)
                .Distinct()
                .ToList();

            var transactions = costCentre.Budgets
                .SelectMany(b => b.Transactions)
                .ToList();

            var affectedUserCount = transactions
                .Select(t => t.UserId)
                .Distinct()
                .Count();

            return new DeleteCostCentrePreviewDto(
                costCentre.Name,
                costCentre.Budgets.Count,
                transactions.Count,
                affectedUserCount,
                affectedTeamNames);
        }

        /// <inheritdoc/>
        public async Task<CostCentre?> DeleteAsync(int id)
        {
            var costCentre = await this.context.CostCentres
                .Include(c => c.Budgets)
                    .ThenInclude(b => b.Transactions)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException($"CostCentre with id {id} could not be found.");

            if (costCentre.Budgets.Count > 0 || costCentre.Budgets.Any(b => b.Transactions.Count > 0))
            {
                costCentre.IsActive = false;
                await this.context.SaveChangesAsync();
                return costCentre;
            }

            this.context.CostCentres.Remove(costCentre);
            int res = await this.context.SaveChangesAsync();

            if (res != 1)
            {
                throw new InternalErrorException($"Deleting CostCentre did not end as expected. Affected {res} records.");
            }

            return null;
        }
    }
}
