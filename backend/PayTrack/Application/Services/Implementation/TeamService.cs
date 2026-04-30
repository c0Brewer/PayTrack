// <copyright file="TeamService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Team;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class TeamService(ITeamRepository repo) : ITeamService
    {
        /// <summary>
        /// Repository for Teams.
        /// </summary>
        private readonly ITeamRepository repo = repo;

        /// <inheritdoc/>
        public async Task<Team> CreateTeamAsync(
            string name,
            string? description,
            string? displayColor,
            IList<CreateTeamBudgetEntryDto>? budgetEntries)
        {
            var team = new Team
            {
                Name = name,
                Description = description,
                DisplayColor = displayColor,
            };

            return await this.repo.AddAsync(team, budgetEntries);
        }

        /// <inheritdoc/>
        public async Task<Team?> GetTeamByIdAsync(int id, GetTeamQueryById? query = null)
        {
            return await this.repo.GetByIdAsync(id, query);
        }

        /// <inheritdoc/>
        public async Task<DeleteTeamImpactDto?> GetDeleteTeamImpactAsync(int id)
        {
            return await this.repo.GetDeleteTeamImpactAsync(id);
        }

        /// <inheritdoc/>
        public async Task<(List<Team> team, int totalCount)> GetTeamsAsync(GetTeamQuery? query = null)
        {
            return await this.repo.GetAllAsync(query);
        }

        /// <inheritdoc/>
        public async Task<Team> UpdateTeamAsync(
            int id,
            string? name,
            string? description,
            string? displayColor,
            IList<UpsertTeamBudgetEntryDto>? budgetsToUpsert,
            IList<int>? budgetIdsToDelete)
        {
            if (budgetsToUpsert is not null && budgetIdsToDelete is not null)
            {
                var upsertIds = budgetsToUpsert.Where(e => e.Id > 0).Select(e => e.Id!.Value).ToHashSet();
                if (upsertIds.Overlaps(budgetIdsToDelete))
                {
                    throw new InvalidStateException("A budget ID cannot appear in both BudgetsToUpsert and BudgetIdsToDelete.");
                }
            }

            return await this.repo.UpdateAsync(
                id,
                name,
                description,
                displayColor,
                budgetsToUpsert,
                budgetIdsToDelete);
        }

        /// <inheritdoc/>
        public async Task<Team> DeleteTeamAsync(int id)
        {
            return await this.repo.DeleteAsync(id);
        }
    }
}
