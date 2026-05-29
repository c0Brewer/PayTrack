// <copyright file="SmtpEmailSender.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using PayTrack.Application.Services.Model;
using PayTrack.Application.Settings;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class SmtpEmailSender(IOptions<EmailSettings> options) : IEmailSender
    {
        private readonly EmailSettings settings = options.Value;

        /// <inheritdoc/>
        public async Task SendAsync(string to, string subject, string body)
        {
            await this.SendAsync(to, subject, body, []);
        }

        /// <inheritdoc/>
        public async Task SendAsync(string to, string subject, string body, IReadOnlyCollection<EmailAttachment> attachments)
        {
            using var client = new SmtpClient(this.settings.SmtpHost, this.settings.SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(this.settings.SmtpUser, this.settings.SmtpPassword),
            };

            using var message = new MailMessage(this.settings.FromAddress, to, subject, body);

            foreach (var attachment in attachments)
            {
                message.Attachments.Add(new Attachment(
                    new MemoryStream(attachment.Content),
                    attachment.FileName,
                    attachment.ContentType));
            }

            await client.SendMailAsync(message);
        }
    }
}
