// <copyright file="SlackSettings.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Settings
{
    /// <summary>
    /// Configuration for Slack API access.
    /// </summary>
    public sealed class SlackSettings
    {
        /// <summary>
        /// Gets or sets the Slack Bot Token used for API calls.
        /// </summary>
        public string BotToken { get; set; } = string.Empty;
    }
}
