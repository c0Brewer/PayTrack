// <copyright file="FinancialExportResult.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Represents a generated financial export file.
    /// </summary>
    public class FinancialExportResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialExportResult"/> class.
        /// </summary>
        /// <param name="content">Export file content.</param>
        /// <param name="contentType">MIME type of the export file.</param>
        /// <param name="fileName">Suggested export file name.</param>
        public FinancialExportResult(byte[] content, string contentType, string fileName)
        {
            this.Content = content;
            this.ContentType = contentType;
            this.FileName = fileName;
        }

        /// <summary>
        /// Gets export file content.
        /// </summary>
        public byte[] Content { get; }

        /// <summary>
        /// Gets MIME type of the export file.
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Gets suggested export file name.
        /// </summary>
        public string FileName { get; }
    }
}
