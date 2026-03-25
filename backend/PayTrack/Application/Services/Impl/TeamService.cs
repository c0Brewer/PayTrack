// <copyright file="TeamService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;
using PayTrack.Data.Repositories;

namespace PayTrack.Application.Services.Impl
{
    /// <inheritdoc/>
    public class TeamService(ITeamRepository repo) : ITeamService
    {
        /// <summary>
        /// Repository for Teams.
        /// </summary>
        private readonly ITeamRepository repo = repo;

        /// <inheritdoc/>
        public async Task<Team> CreateTeamAsync(string name)
        {
            var team = new Team
            {
                Name = name,
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
