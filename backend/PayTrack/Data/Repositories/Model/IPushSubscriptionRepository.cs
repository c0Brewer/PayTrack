// <copyright file="IPushSubscriptionRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Data.Repositories.Model
{
    /// <summary>
    /// Repository for browser push subscriptions.
    /// </summary>
    public interface IPushSubscriptionRepository
    {
        /// <summary>
        /// Returns whether a user has any enabled push subscription.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>True if at least one subscription is enabled.</returns>
        Task<bool> HasEnabledSubscriptionAsync(int userId);

        /// <summary>
        /// Gets all enabled subscriptions for a user.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>Enabled subscriptions.</returns>
        Task<List<PushSubscription>> GetEnabledForUserAsync(int userId);

        /// <summary>
        /// Creates or updates a subscription.
        /// </summary>
        /// <param name="subscription">Subscription data.</param>
        /// <returns>The stored subscription.</returns>
        Task<PushSubscription> UpsertAsync(PushSubscription subscription);

        /// <summary>
        /// Disables one subscription for a user.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <param name="endpoint">Browser push endpoint.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DisableAsync(int userId, string endpoint);

        /// <summary>
        /// Disables a subscription by endpoint.
        /// </summary>
        /// <param name="endpoint">Browser push endpoint.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DisableByEndpointAsync(string endpoint);
    }
}
