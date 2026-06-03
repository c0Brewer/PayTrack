// <copyright file="FinancialExportResult.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Represents a generated financial export file.
    /// </summary>
    /// <param name="Content">Export file content.</param>
    /// <param name="ContentType">MIME type of the export file.</param>
    /// <param name="FileName">Suggested export file name.</param>
    public record FinancialExportResult(byte[] Content, string ContentType, string FileName);
}
