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
        public async Task<Team> AddAsync(Team team)
        {
            this.context.Teams.Add(team);
            var res = await this.context.SaveChangesAsync();

            return res != 1
                ? throw new InternalErrorException($"Saving Team did not end as expected. Saved {res} teams.")
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

            // Calculate total count
            var totalCount = await dbQuery.CountAsync();

            // Check if budget should be included
            if (query?.IncludeBudgets == true)
            {
                dbQuery = dbQuery.Include(t => t.Budgets);
            }

            // Check if members should be included
            if (query?.IncludeMembers == true)
            {
                dbQuery = dbQuery.Include(t => t.Members);
            }

            if (query?.Offset.HasValue == true)
            {
                dbQuery = dbQuery.Skip(query.Offset.Value);
            }

            if (query?.Limit.HasValue == true)
            {
                dbQuery = dbQuery.Take(query.Limit.Value);
            }

            var items = await dbQuery.OrderByDescending(t => t.Name).ToListAsync();
            return (items, totalCount);
        }

        /// <inheritdoc/>
        public async Task<Team?> GetByIdAsync(int id, GetTeamQueryById? query = null)
        {
            var dbQuery = this.context.Teams.AsQueryable();

            if (query?.IncludeBudgets.HasValue == true)
            {
                dbQuery = dbQuery.Include(t => t.Budgets);
            }

            if (query?.IncludeMembers.HasValue == true)
            {
                dbQuery = dbQuery.Include(t => t.Members);
            }

            return await dbQuery.FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
