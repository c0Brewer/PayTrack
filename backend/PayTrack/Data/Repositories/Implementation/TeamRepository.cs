// <copyright file="TeamRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class TeamRepository(AppDbContext context) : ITeamRepository
    {
        /// <summary>
        /// Database Context for accessing the DB.
        /// </summary>
        private readonly AppDbContext context = context;

        /// <inheritdoc/>
        public async Task<Team> AddAsync(Team team)
        {
            this.context.Teams.Add(team);
            await this.context.SaveChangesAsync(); // TODO: Check return value if succesful?
            return team;
        }

        /// <inheritdoc/>
        public async Task<List<Team>> GetAllAsync()
        {
            return await this.context.Teams.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Team?> GetByIdAsync(int id)
        {
            return await this.context.Teams.FindAsync(id);
        }
    }
}
