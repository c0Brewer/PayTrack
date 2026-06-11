// <copyright file="SeasonService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Season;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class SeasonService(ISeasonRepository repo) : ISeasonService
    {
        /// <summary>
        /// Repository for Seasons.
        /// </summary>
        private readonly ISeasonRepository repo = repo;

        /// <inheritdoc/>
        public async Task<List<Season>> GetAllAsync(GetSeasonQuery? query = null)
        {
            return await this.repo.GetAllAsync(query);
        }

        /// <inheritdoc/>
        public async Task<Season?> GetByIdAsync(int id)
        {
            return await this.repo.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<Season> CreateAsync(string name)
        {
            return await this.repo.AddAsync(new Season { Name = name });
        }

        /// <inheritdoc/>
        public async Task<Season> UpdateAsync(int id, string? name, bool? isActive)
        {
            return await this.repo.UpdateAsync(id, name, isActive);
        }

        /// <inheritdoc/>
        public async Task<Season?> DeleteAsync(int id)
        {
            return await this.repo.DeleteAsync(id);
        }
    }
}
