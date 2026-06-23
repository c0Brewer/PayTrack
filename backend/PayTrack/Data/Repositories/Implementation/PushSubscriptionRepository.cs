// <copyright file="PushSubscriptionRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class PushSubscriptionRepository(AppDbContext _context) : IPushSubscriptionRepository
    {
        private readonly AppDbContext context = _context;

        /// <inheritdoc/>
        public async Task<bool> HasEnabledSubscriptionAsync(int userId)
        {
            return await this.context.PushSubscriptions.AnyAsync(s => s.UserId == userId && s.IsEnabled);
        }

        /// <inheritdoc/>
        public async Task<List<PushSubscription>> GetEnabledForUserAsync(int userId)
        {
            return await this.context.PushSubscriptions
                .Where(s => s.UserId == userId && s.IsEnabled)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<PushSubscription> UpsertAsync(PushSubscription subscription)
        {
            var existing = await this.context.PushSubscriptions
                .FirstOrDefaultAsync(s => s.Endpoint == subscription.Endpoint);

            if (existing is null)
            {
                this.context.PushSubscriptions.Add(subscription);
                await this.context.SaveChangesAsync();
                return subscription;
            }

            existing.UserId = subscription.UserId;
            existing.P256dh = subscription.P256dh;
            existing.Auth = subscription.Auth;
            existing.IsEnabled = true;
            existing.UpdatedAt = DateTime.UtcNow;

            await this.context.SaveChangesAsync();
            return existing;
        }

        /// <inheritdoc/>
        public async Task DisableAsync(int userId, string endpoint)
        {
            var subscription = await this.context.PushSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Endpoint == endpoint);

            if (subscription is null)
            {
                return;
            }

            subscription.IsEnabled = false;
            subscription.UpdatedAt = DateTime.UtcNow;
            await this.context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DisableByEndpointAsync(string endpoint)
        {
            var subscription = await this.context.PushSubscriptions
                .FirstOrDefaultAsync(s => s.Endpoint == endpoint);

            if (subscription is null)
            {
                return;
            }

            subscription.IsEnabled = false;
            subscription.UpdatedAt = DateTime.UtcNow;
            await this.context.SaveChangesAsync();
        }
    }
}
