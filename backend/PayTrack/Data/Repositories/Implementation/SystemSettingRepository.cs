// <copyright file="SystemSettingRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class SystemSettingRepository(AppDbContext context) : ISystemSettingRepository
    {
        private readonly AppDbContext context = context;

        /// <inheritdoc/>
        public async Task<SystemSetting?> GetByKeyAsync(string key)
        {
            return await this.context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == key);
        }

        /// <inheritdoc/>
        public async Task UpsertAsync(string key, string value, int lastModifiedByUserId)
        {
            var existing = await this.context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (existing is not null)
            {
                existing.Value = value;
                existing.LastModifiedAt = DateTime.UtcNow;
                existing.LastModifiedByUserId = lastModifiedByUserId;
            }
            else
            {
                this.context.SystemSettings.Add(new SystemSetting
                {
                    Key = key,
                    Value = value,
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedByUserId = lastModifiedByUserId,
                });
            }

            await this.context.SaveChangesAsync();
        }
    }
}
