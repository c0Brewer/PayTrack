// <copyright file="TransactionHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Application.Dto.Transaction;

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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <param name="query">Query object including all export filter options.</param>
        public static Task<StatusCodeHttpResult> ExportFinancialDataAsync(
            [AsParameters] GetTransactionQuery query)
        {
            return Task.FromResult(TypedResults.StatusCode(StatusCodes.Status501NotImplemented));
        }
    }
}
