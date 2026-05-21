// <copyright file="INotificationDispatchService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Dispatches notifications to recipients via email or Slack.
    /// </summary>
    public interface INotificationDispatchService
    {
        /// <summary>
        /// Sends an email to the given recipient.
        /// </summary>
        /// <param name="recipientEmail">Email address of the recipient.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">Email body (plain text).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendEmailAsync(string recipientEmail, string subject, string body);

        /// <summary>
        /// Sends a Slack DM to the user whose Slack account is associated with <paramref name="recipientEmail"/>.
        /// </summary>
        /// <param name="recipientEmail">Email address used to resolve the Slack user.</param>
        /// <param name="message">Message text to deliver.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendSlackAsync(string recipientEmail, string message);
    }
}
