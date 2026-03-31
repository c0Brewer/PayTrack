// <copyright file="TeamRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Exceptions;
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
            int res = await this.context.SaveChangesAsync();

            if (res != 1)
            {
                throw new InternalErrorException($"Saving Team did not end as expected. Saved {res} teams.");
            }

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
