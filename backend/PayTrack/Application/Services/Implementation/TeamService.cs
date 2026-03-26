// <copyright file="TeamService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

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
            string? displayColor)
        {
            var team = new Team
            {
                Name = name,
                Description = description,
                DisplayColor = displayColor,
            };

            return await this.repo.AddAsync(team);
        }

        /// <inheritdoc/>
        public async Task<Team?> GetTeamByIdAsync(int id)
        {
            return await this.repo.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<List<Team>> GetTeamsAsync()
        {
            return await this.repo.GetAllAsync();
        }
    }
}
