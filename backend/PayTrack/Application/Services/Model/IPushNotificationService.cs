// <copyright file="IPushNotificationService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Notification;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Manages browser push subscriptions and sends push notifications.
    /// </summary>
    public interface IPushNotificationService
    {
        /// <summary>
        /// Gets the current user's push configuration.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <param name="currentEndpoint">Current browser push endpoint, if available.</param>
        /// <returns>Push configuration.</returns>
        Task<PushNotificationConfigDto> GetConfigAsync(int userId, string? currentEndpoint = null);

        /// <summary>
        /// Saves a browser push subscription for a user.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <param name="subscription">Subscription request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SaveSubscriptionAsync(int userId, SavePushSubscriptionDto subscription);

        /// <summary>
        /// Disables a browser push subscription for a user.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <param name="endpoint">Browser push endpoint.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DisableSubscriptionAsync(int userId, string endpoint);

        /// <summary>
        /// Sends a workflow status notification to the target user.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <param name="title">Notification title.</param>
        /// <param name="body">Notification body.</param>
        /// <param name="url">Application URL to open.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendWorkflowStatusChangedAsync(int userId, string title, string body, string url);
    }
}
