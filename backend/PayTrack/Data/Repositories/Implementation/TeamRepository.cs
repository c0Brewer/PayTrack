// <copyright file="TeamRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Dto.Team;
using PayTrack.Application.Exceptions;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class TeamRepository(AppDbContext _context) : ITeamRepository
    {
        /// <summary>
        /// Database Context for accessing the DB.
        /// </summary>
        private readonly AppDbContext context = _context;

        /// <inheritdoc/>
        public async Task<Team> AddAsync(Team team, IList<CreateTeamBudgetEntryDto>? budgetEntries = null)
        {
            if (await this.context.Teams.AnyAsync(t => t.Name == team.Name))
            {
                throw new InvalidStateException($"A team with the name '{team.Name}' already exists.");
            }

            this.context.Teams.Add(team);
            if (budgetEntries is not null)
            {
                foreach (var entry in budgetEntries)
                {
                    this.context.Budgets.Add(new Budget
                    {
                        Team = team,
                        CostCentreId = entry.CostCentreId,
                        TargetAmount = entry.TargetAmount,
                        PeriodStart = DateTime.SpecifyKind(entry.PeriodStart, DateTimeKind.Utc),
                        PeriodEnd = DateTime.SpecifyKind(entry.PeriodEnd, DateTimeKind.Utc),
                    });
                }
            }

            var res = await this.context.SaveChangesAsync();
            var expectedCount = 1 + (budgetEntries?.Count ?? 0);

            return res != expectedCount
                ? throw new InternalErrorException($"Saving Team did not end as expected. Saved {res} records.")
                    : team;
        }

        /// <inheritdoc/>
        public async Task<Team> DeleteAsync(int id)
        {
            var deleteImpact = await this.GetDeleteTeamImpactAsync(id) ?? throw new NotFoundException($"Team with id {id} not found.");

            if (!deleteImpact.CanDelete)
            {
                throw new InvalidStateException(deleteImpact.WarningMessage);
            }

            var team = await this.context.Teams.FirstAsync(t => t.Id == id);
            this.context.Teams.Remove(team);

            var res = await this.context.SaveChangesAsync();

            return res != 1
                ? throw new InternalErrorException($"Deleting Team did not end as expected. Deleted {res} teams.")
                : team;
        }

        /// <inheritdoc/>
        public async Task<(List<Team> team, int totalCount)> GetAllAsync(GetTeamQuery? query = null)
        {
            var dbQuery = this.context.Teams.AsQueryable();

            // Filter by Name
            if (!string.IsNullOrWhiteSpace(query?.Name))
            {
                dbQuery = dbQuery.Where(t => EF.Functions.Like(t.Name, $"%{query.Name}%"));
            }

            // Filter by Description
            if (!string.IsNullOrWhiteSpace(query?.Description))
            {
                dbQuery = dbQuery.Where(t => t.Description != null && t.Description.Contains(query.Description));
            }

            // Check if budget should be included
            if (query?.IncludeBudgets == true)
            {
                dbQuery = dbQuery.Include(t => t.Budgets);
            }

            // Filter by Budget
            if (query?.MinBudget.HasValue == true || query?.MaxBudget.HasValue == true)
            {
                var currentDate = DateTime.UtcNow.Date;
                dbQuery = dbQuery.Where(t => t.Budgets.Any(b =>
                    b.PeriodStart <= currentDate &&
                    b.PeriodEnd >= currentDate &&
                    (!query.MinBudget.HasValue || b.TargetAmount >= query.MinBudget.Value) &&
                    (!query.MaxBudget.HasValue || b.TargetAmount <= query.MaxBudget.Value)));
            }

            // Calculate total count after filters but before pagination.
            var totalCount = await dbQuery.CountAsync();

            // Check if members should be included
            if (query?.IncludeMembers == true)
            {
                dbQuery = dbQuery.Include(t => t.Members);
            }

            dbQuery = dbQuery.OrderBy(t => t.Name);

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
        public async Task<DeleteTeamImpactDto?> GetDeleteTeamImpactAsync(int id)
        {
            var team = await this.context.Teams
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    TeamId = t.Id,
                    TeamName = t.Name,
                })
                .FirstOrDefaultAsync();

            if (team is null)
            {
                return null;
            }

            var affectedUserCount = await this.context.User.CountAsync(u => u.TeamId == id);
            var blockingBudgetCount = await this.context.Budgets.CountAsync(b => b.TeamId == id);
            var blockingTransactionCount = await this.context.Transactions.CountAsync(t => t.TeamId == id);
            var invoiceCount = await this.context.PaymentRequestsByUser.CountAsync(p => p.TeamId == id);
            var canDelete = blockingBudgetCount == 0 && blockingTransactionCount == 0;

            return new DeleteTeamImpactDto(
                team.TeamId,
                team.TeamName,
                canDelete,
                affectedUserCount,
                blockingBudgetCount,
                blockingTransactionCount,
                invoiceCount,
                BuildDeleteWarningMessage(
                    canDelete,
                    affectedUserCount,
                    blockingBudgetCount,
                    blockingTransactionCount,
                    invoiceCount));
        }

        /// <inheritdoc/>
        public async Task<Team?> GetByIdAsync(int id, GetTeamQueryById? query = null)
        {
            var dbQuery = this.context.Teams.AsQueryable();

            if (query?.IncludeBudgets == true)
            {
                dbQuery = dbQuery.Include(t => t.Budgets);
            }

            if (query?.IncludeMembers == true)
            {
                dbQuery = dbQuery.Include(t => t.Members);
            }

            return await dbQuery.FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Team> UpdateAsync(
            int id,
            string? name,
            string? description,
            string? displayColor,
            IList<UpsertTeamBudgetEntryDto>? budgetsToUpsert,
            IList<int>? budgetIdsToDelete)
        {
            var team = await this.context.Teams
                .Include(t => t.Budgets)
                .FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new NotFoundException($"Team with id {id} not found.");
            var hasChanges = false;

            if (name is not null && team.Name != name)
            {
                var duplicateNameExists = await this.context.Teams.AnyAsync(t => t.Id != id && t.Name == name);
                if (duplicateNameExists)
                {
                    throw new InvalidStateException($"A team with the name '{name}' already exists.");
                }

                team.Name = name;
                hasChanges = true;
            }

            if (description is not null && team.Description != description)
            {
                team.Description = description;
                hasChanges = true;
            }

            if (displayColor is not null && team.DisplayColor != displayColor)
            {
                team.DisplayColor = displayColor;
                hasChanges = true;
            }

            if (budgetIdsToDelete is not null)
            {
                foreach (var budgetId in budgetIdsToDelete)
                {
                    var budget = team.Budgets.FirstOrDefault(b => b.Id == budgetId)
                        ?? throw new NotFoundException($"Budget with id {budgetId} not found on Team {id}.");
                    this.context.Budgets.Remove(budget);
                    hasChanges = true;
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
                            Team = team,
                            CostCentreId = entry.CostCentreId,
                            TargetAmount = entry.TargetAmount,
                            PeriodStart = DateTime.SpecifyKind(entry.PeriodStart, DateTimeKind.Utc),
                            PeriodEnd = DateTime.SpecifyKind(entry.PeriodEnd, DateTimeKind.Utc),
                        });
                    }
                    else
                    {
                        var existing = team.Budgets.FirstOrDefault(b => b.Id == entry.Id)
                            ?? throw new NotFoundException($"Budget with id {entry.Id} not found on Team {id}.");
                        existing.CostCentreId = entry.CostCentreId;
                        existing.TargetAmount = entry.TargetAmount;
                        existing.PeriodStart = DateTime.SpecifyKind(entry.PeriodStart, DateTimeKind.Utc);
                        existing.PeriodEnd = DateTime.SpecifyKind(entry.PeriodEnd, DateTimeKind.Utc);
                    }

                    hasChanges = true;
                }
            }

            if (!hasChanges)
            {
                return team;
            }

            var res = await this.context.SaveChangesAsync();

            return res < 1
                ? throw new InternalErrorException($"Updating Team did not end as expected. Updated {res} records.")
                : team;
        }

        private static string BuildDeleteWarningMessage(
            bool canDelete,
            int affectedUserCount,
            int blockingBudgetCount,
            int blockingTransactionCount,
            int invoiceCount)
        {
            var impactMessages = new List<string>();

            if (affectedUserCount > 0)
            {
                impactMessages.Add($"{affectedUserCount} user(s) will lose their team assignment");
            }

            if (blockingBudgetCount > 0)
            {
                impactMessages.Add($"{blockingBudgetCount} budget(s) block deletion");
            }

            if (blockingTransactionCount > 0)
            {
                impactMessages.Add($"{blockingTransactionCount} transaction(s) block deletion");
            }

            if (invoiceCount > 0)
            {
                impactMessages.Add($"{invoiceCount} invoice(s) are part of those transactions");
            }

            if (impactMessages.Count == 0)
            {
                return "Deleting this team has no related users, budgets, or transactions.";
            }

            var prefix = canDelete
                ? "Deleting this team affects related records: "
                : "Deleting this team is currently blocked: ";

            return prefix + string.Join("; ", impactMessages) + ".";
        }
    }
}
