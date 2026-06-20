// <copyright file="IFinancialExportService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Transaction;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service which handles financial data exports.
    /// </summary>
    public interface IFinancialExportService
    {
        /// <summary>
        /// Exports financial transactions using the supplied query filters.
        /// </summary>
        /// <param name="query">Export query including filters and output format.</param>
        /// <returns>Generated export file.</returns>
        Task<FinancialExportResult> ExportFinancialDataAsync(GetFinancialExportQuery query);
    }
}
