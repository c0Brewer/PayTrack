// <copyright file="TransactionHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Services.Model;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler which gets called from Endpoint to manage incoming Transaction-related requests.
    /// </summary>
    public static class TransactionHandler
    {
        /// <summary>
        /// Exports financial transaction data.
        /// </summary>
        /// <param name="query">Query object including all export filter options.</param>
        /// <param name="financialExportService">Dependency-Injected financial export service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<FileContentHttpResult, BadRequest<ProblemDetails>, ProblemHttpResult>> ExportFinancialDataAsync(
            [AsParameters] GetTransactionQuery query,
            IFinancialExportService financialExportService)
        {
            var exportResult = await financialExportService.ExportFinancialDataAsync(query);

            return TypedResults.File(
                exportResult.Content,
                exportResult.ContentType,
                exportResult.FileName);
        }
    }
}
