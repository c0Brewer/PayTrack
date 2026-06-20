// <copyright file="GetFinancialExportQuery.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.Transaction
{
    /// <summary>
    /// Query options for financial exports.
    /// </summary>
    public class GetFinancialExportQuery : GetTransactionQuery
    {
        /// <summary>
        /// Cost centre of the assigned budget to query for payment request exports.
        /// </summary>
        public int? CostCentreId { get; init; }

        /// <summary>
        /// Requested financial export format.
        /// </summary>
        public FinancialExportFormat? Format { get; init; }

        /// <summary>
        /// Requested financial export source view.
        /// </summary>
        public FinancialExportSource? Source { get; init; }
    }
}
