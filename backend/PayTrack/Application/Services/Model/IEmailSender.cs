// <copyright file="IEmailSender.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Abstraction for sending emails.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends a plain-text email.
        /// </summary>
        /// <param name="to">Recipient email address.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">Email body (plain text).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendAsync(string to, string subject, string body);

        /// <summary>
        /// Sends a plain-text email with attachments.
        /// </summary>
        /// <param name="to">Recipient email address.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">Email body (plain text).</param>
        /// <param name="attachments">Files to attach to the email.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendAsync(string to, string subject, string body, IReadOnlyCollection<EmailAttachment> attachments);
    }
}
