// <copyright file="FinancialExportFormat.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.Transaction
{
    /// <summary>
    /// Supported file formats for financial exports.
    /// </summary>
    public enum FinancialExportFormat
    {
        /// <summary>
        /// Comma-separated values export.
        /// </summary>
        Csv = 1,

        /// <summary>
        /// Portable document format export.
        /// </summary>
        Pdf = 2,
    }
}
