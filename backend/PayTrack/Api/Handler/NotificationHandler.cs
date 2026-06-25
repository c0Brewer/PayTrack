// <copyright file="NotificationHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Application.Dto.Notification;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler for notification dispatch endpoints.
    /// </summary>
    public static class NotificationHandler
    {
        /// <summary>
        /// Sends an email notification to the given recipient.
        /// </summary>
        /// <param name="dto">Request body containing recipient email, subject, and body.</param>
        /// <param name="notificationDispatchService">Dependency-injected notification service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok, BadRequest<ProblemDetails>, ProblemHttpResult>> SendEmailNotificationAsync(
            [FromBody] SendEmailNotificationDto dto,
            INotificationDispatchService notificationDispatchService)
        {
            await notificationDispatchService.SendEmailAsync(dto.RecipientEmail, dto.Subject, dto.Body);
            return TypedResults.Ok();
        }

        /// <summary>
        /// Sends a Slack DM notification to the user whose Slack account matches the given email.
        /// </summary>
        /// <param name="dto">Request body containing recipient email and message.</param>
        /// <param name="notificationDispatchService">Dependency-injected notification service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok, BadRequest<ProblemDetails>, ProblemHttpResult>> SendSlackNotificationAsync(
            [FromBody] SendSlackNotificationDto dto,
            INotificationDispatchService notificationDispatchService)
        {
            await notificationDispatchService.SendSlackAsync(dto.RecipientEmail, dto.Message);
            return TypedResults.Ok();
        }

        /// <summary>
        /// Returns the current user's push notification configuration.
        /// </summary>
        /// <param name="authService">Dependency-injected auth service.</param>
        /// <param name="pushNotificationService">Dependency-injected push notification service.</param>
        /// <returns>Push notification configuration.</returns>
        public static async Task<Results<Ok<PushNotificationConfigDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> GetPushNotificationConfigAsync(
            IAuthService authService,
            IPushNotificationService pushNotificationService)
        {
            var user = await authService.GetCurrentUser() ?? throw new NotFoundException("Current user not found");
            var config = await pushNotificationService.GetConfigAsync(user.Id);
            return TypedResults.Ok(config);
        }

        /// <summary>
        /// Registers a browser push subscription for the current user.
        /// </summary>
        /// <param name="dto">Subscription data.</param>
        /// <param name="authService">Dependency-injected auth service.</param>
        /// <param name="pushNotificationService">Dependency-injected push notification service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok, BadRequest<ProblemDetails>, ProblemHttpResult>> SubscribeToPushNotificationsAsync(
            [FromBody] SavePushSubscriptionDto dto,
            IAuthService authService,
            IPushNotificationService pushNotificationService)
        {
            var user = await authService.GetCurrentUser() ?? throw new NotFoundException("Current user not found");
            await pushNotificationService.SaveSubscriptionAsync(user.Id, dto);
            return TypedResults.Ok();
        }

        /// <summary>
        /// Disables a browser push subscription for the current user.
        /// </summary>
        /// <param name="dto">Subscription endpoint.</param>
        /// <param name="authService">Dependency-injected auth service.</param>
        /// <param name="pushNotificationService">Dependency-injected push notification service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok, BadRequest<ProblemDetails>, ProblemHttpResult>> UnsubscribeFromPushNotificationsAsync(
            [FromBody] UnsubscribePushNotificationDto dto,
            IAuthService authService,
            IPushNotificationService pushNotificationService)
        {
            var user = await authService.GetCurrentUser() ?? throw new NotFoundException("Current user not found");
            await pushNotificationService.DisableSubscriptionAsync(user.Id, dto.Endpoint);
            return TypedResults.Ok();
        }
    }
}
