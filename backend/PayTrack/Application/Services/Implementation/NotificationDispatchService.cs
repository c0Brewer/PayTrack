// <copyright file="NotificationDispatchService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PayTrack.Application.Services.Model;
using PayTrack.Application.Settings;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class NotificationDispatchService(
        IEmailSender emailSender,
        IOptions<SlackSettings> slackOptions,
        HttpClient httpClient) : INotificationDispatchService
    {
        private readonly IEmailSender emailSender = emailSender;
        private readonly SlackSettings slackSettings = slackOptions.Value;
        private readonly HttpClient httpClient = httpClient;

        /// <inheritdoc/>
        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            await this.emailSender.SendAsync(recipientEmail, subject, body);
        }

        /// <inheritdoc/>
        public async Task SendEmailAsync(string recipientEmail, string subject, string body, IReadOnlyCollection<EmailAttachment> attachments)
        {
            await this.emailSender.SendAsync(recipientEmail, subject, body, attachments);
        }

        /// <inheritdoc/>
        public async Task SendSlackAsync(string recipientEmail, string message)
        {
            var slackUserId = await this.LookupSlackUserIdAsync(recipientEmail);
            await this.PostSlackMessageAsync(slackUserId, message);
        }

        private async Task<string> LookupSlackUserIdAsync(string email)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://slack.com/api/users.lookupByEmail?email={Uri.EscapeDataString(email)}");
            request.Headers.Add("Authorization", $"Bearer {this.slackSettings.BotToken}");

            var response = await this.httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.GetProperty("ok").GetBoolean())
            {
                var error = root.TryGetProperty("error", out var errProp) ? errProp.GetString() : "unknown";
                throw new InvalidOperationException($"Slack users.lookupByEmail failed: {error}");
            }

            return root.GetProperty("user").GetProperty("id").GetString()
                ?? throw new InvalidOperationException("Slack returned a null user ID.");
        }

        private async Task PostSlackMessageAsync(string slackUserId, string message)
        {
            var payload = JsonSerializer.Serialize(new { channel = slackUserId, text = message });
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://slack.com/api/chat.postMessage")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            };
            request.Headers.Add("Authorization", $"Bearer {this.slackSettings.BotToken}");

            var response = await this.httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.GetProperty("ok").GetBoolean())
            {
                var error = root.TryGetProperty("error", out var errProp) ? errProp.GetString() : "unknown";
                throw new InvalidOperationException($"Slack chat.postMessage failed: {error}");
            }
        }
    }
}
