// <copyright file="SeasonRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Exceptions;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class SeasonRepository(AppDbContext _context) : ISeasonRepository
    {
        /// <summary>
        /// Database Context for accessing the DB.
        /// </summary>
        private readonly AppDbContext context = _context;

        /// <inheritdoc/>
        public async Task<List<Season>> GetAllAsync()
        {
            return await this.context.Seasons
                .Include(s => s.Budgets)
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Season?> GetByIdAsync(int id)
        {
            return await this.context.Seasons
                .Include(s => s.Budgets)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Season> AddAsync(Season season)
        {
            this.context.Seasons.Add(season);
            int res = await this.context.SaveChangesAsync();

            if (res != 1)
            {
                throw new InternalErrorException($"Saving Season did not end as expected. Saved {res} records.");
            }

            return season;
        }

        /// <inheritdoc/>
        public async Task<Season> UpdateAsync(int id, string? name, bool? isActive)
        {
            var season = await this.context.Seasons
                .Include(s => s.Budgets)
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new NotFoundException($"Season with id {id} could not be found.");
            var hasChanges = false;

            if (name is not null && season.Name != name)
            {
                season.Name = name;
                hasChanges = true;
            }

            if (isActive is not null && season.IsActive != isActive.Value)
            {
                season.IsActive = isActive.Value;
                hasChanges = true;
            }

            if (!hasChanges)
            {
                return season;
            }

            int res = await this.context.SaveChangesAsync();

            if (res != 1)
            {
                throw new InternalErrorException($"Updating Season did not end as expected. Saved {res} records.");
            }

            return season;
        }

        /// <inheritdoc/>
        public async Task<Season?> DeleteAsync(int id)
        {
            var season = await this.context.Seasons
                .Include(s => s.Budgets)
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new NotFoundException($"Season with id {id} could not be found.");

            if (season.Budgets.Count > 0)
            {
                season.IsActive = false;
                int deactivationRes = await this.context.SaveChangesAsync();

                if (deactivationRes != 1)
                {
                    throw new InternalErrorException($"Deactivating Season did not end as expected. Affected {deactivationRes} records.");
                }

                return season;
            }

            this.context.Seasons.Remove(season);
            int res = await this.context.SaveChangesAsync();

            if (res != 1)
            {
                throw new InternalErrorException($"Deleting Season did not end as expected. Affected {res} records.");
            }

            return null;
        }
    }
}
