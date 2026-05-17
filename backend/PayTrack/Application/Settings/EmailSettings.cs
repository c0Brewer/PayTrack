// <copyright file="EmailSettings.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Settings
{
    /// <summary>
    /// Configuration for SMTP email sending.
    /// </summary>
    public sealed class EmailSettings
    {
        /// <summary>
        /// Gets or sets the SMTP host name.
        /// </summary>
        public string SmtpHost { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SMTP port (default 587).
        /// </summary>
        public int SmtpPort { get; set; } = 587;

        /// <summary>
        /// Gets or sets the SMTP login user name.
        /// </summary>
        public string SmtpUser { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SMTP login password.
        /// </summary>
        public string SmtpPassword { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the From address shown on outgoing emails.
        /// </summary>
        public string FromAddress { get; set; } = string.Empty;
    }
}
