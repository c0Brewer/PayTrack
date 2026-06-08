// <copyright file="BudgetRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Implementation;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class BudgetRepository(AppDbContext _context) : IBudgetRepository
    {
        /// <summary>
        /// Database Context for accessing the DB.
        /// </summary>
        private readonly AppDbContext context = _context;

        /// <inheritdoc/>
        public async Task<(List<Budget> budget, int totalCount)> GetAllAsync(GetBudgetQuery? query = null)
        {
            IQueryable<Budget> dbQuery = this.context.Budgets
                .Include(b => b.Transactions);

            if (!string.IsNullOrWhiteSpace(query?.Name))
            {
                dbQuery = dbQuery.Where(b => b.Name.Contains(query.Name, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by TeamId
            if (query?.TeamId.HasValue == true)
            {
                dbQuery = dbQuery.Where(b => b.TeamId == query.TeamId.Value);
            }

            // Filter by CostCentreId
            if (query?.CostCentreId.HasValue == true)
            {
                dbQuery = dbQuery.Where(b => b.CostCentreId == query.CostCentreId.Value);
            }

            if (query?.SeasonId.HasValue == true)
            {
                dbQuery = dbQuery.Where(b => b.SeasonId == query.SeasonId.Value);
            }

            // Filter by TargetAmount
            if (query?.TargetAmount.HasValue == true)
            {
                dbQuery = dbQuery.Where(b => b.TargetAmount == query.TargetAmount.Value);
            }

            // Filter by PeriodStart (>=)
            if (query?.PeriodStart.HasValue == true)
            {
                dbQuery = dbQuery.Where(b => b.PeriodStart >= query.PeriodStart.Value);
            }

            // Filter by PeriodEnd (<=)
            if (query?.PeriodEnd.HasValue == true)
            {
                dbQuery = dbQuery.Where(b => b.PeriodEnd <= query.PeriodEnd.Value);
            }

            // Filter by Type
            if (query?.Type.HasValue == true)
            {
                dbQuery = dbQuery.Where(b => b.Type == query.Type.Value);
            }

            var totalCount = await dbQuery.CountAsync();

            dbQuery = dbQuery.OrderByDescending(b => b.PeriodStart);

            if (query?.Offset.HasValue == true)
            {
                dbQuery = dbQuery.Skip(query.Offset.Value);
            }

            if (query?.Limit.HasValue == true)
            {
                dbQuery = dbQuery.Take(query.Limit.Value);
            }

            var items = await dbQuery.ToListAsync();

            return (items, totalCount);
        }

        /// <inheritdoc/>
        public async Task<Budget?> GetByIdAsync(int id)
        {
            IQueryable<Budget> dbQuery = this.context.Budgets
                .Include(b => b.Transactions);

            return await dbQuery.FirstOrDefaultAsync(b => b.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Budget> AddAsync(string name, string? description, int teamId, int costCentreId, int seasonId, decimal? targetAmount, DateTime periodStart, DateTime periodEnd, BudgetType type = BudgetType.Expense)
        {
            var budget = new Budget
            {
                Name = name,
                Description = description,
                TeamId = teamId,
                CostCentreId = costCentreId,
                SeasonId = seasonId,
                TargetAmount = targetAmount,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                Type = type,
            };

            this.context.Budgets.Add(budget);
            int res = await this.context.SaveChangesAsync();
            if (res != 1)
            {
                throw new InternalErrorException($"Saving Budget did not end as expected. Saved {res} budget.");
            }

            return budget;
        }

        /// <inheritdoc/>
        public Task AddRangeAsync(CostCentre costCentre, IList<CreateCostCentreBudgetEntryDto> entries)
        {
            foreach (var entry in entries)
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

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<Budget> UpdateAsync(int id, string? name = null, string? description = null, int? teamId = null, int? costCentreId = null, int? seasonId = null, decimal? targetAmount = null, DateTime? periodStart = null, DateTime? periodEnd = null, BudgetType? type = null)
        {
            var budget = await this.context.Budgets
                .Include(b => b.Transactions)
                .FirstOrDefaultAsync(b => b.Id == id)
                ?? throw new NotFoundException($"Budget with id {id} not found.");

            if (name is not null)
            {
                budget.Name = name;
            }

            if (description is not null)
            {
                budget.Description = description;
            }

            if (teamId.HasValue)
            {
                budget.TeamId = teamId.Value;
            }

            if (costCentreId.HasValue)
            {
                budget.CostCentreId = costCentreId.Value;
            }

            if (seasonId.HasValue)
            {
                budget.SeasonId = seasonId.Value;
            }

            if (targetAmount.HasValue)
            {
                budget.TargetAmount = targetAmount.Value;
            }

            if (periodStart.HasValue)
            {
                budget.PeriodStart = periodStart.Value;
            }

            if (periodEnd.HasValue)
            {
                budget.PeriodEnd = periodEnd.Value;
            }

            if (type.HasValue)
            {
                budget.Type = type.Value;
            }

            BudgetEntryValidation.EnsureValid(budget.TargetAmount, budget.Type, budget.PeriodStart, budget.PeriodEnd);

            int res = await this.context.SaveChangesAsync();
            if (res != 1)
            {
                throw new InternalErrorException($"Updating Budget failed. Updated {res} rows.");
            }

            return budget;
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            var budget = await this.context.Budgets
                .Include(b => b.Transactions)
                .FirstOrDefaultAsync(b => b.Id == id)
                ?? throw new NotFoundException($"Budget with id {id} not found.");

            if (budget.Transactions.Count > 0)
            {
                throw new InvalidStateException("Budget cannot be deleted because it is assigned to transactions.");
            }

            this.context.Budgets.Remove(budget);

            int res = await this.context.SaveChangesAsync();
            if (res != 1)
            {
                throw new InternalErrorException($"Deleting Budget failed. Deleted {res} rows.");
            }
        }
    }
}
