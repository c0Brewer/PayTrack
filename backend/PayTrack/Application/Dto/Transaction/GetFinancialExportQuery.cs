// <copyright file="GetFinancialExportQuery.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.Transaction
{
    /// <summary>
    /// Query options for financial exports.
    /// </summary>
    public class GetFinancialExportQuery : GetTransactionQuery
    {
        /// <summary>
        /// PaymentRequestByTeam requester id to query for payment request exports.
        /// </summary>
        public int? RequestById { get; init; }

        /// <summary>
        /// Invoice number to query for submitted invoice exports.
        /// </summary>
        public string? InvoiceNumber { get; init; }

        /// <summary>
        /// Payout type to query for submitted invoice exports.
        /// </summary>
        public PayoutType? PayoutType { get; set; }

        /// <summary>
        /// Bank account id to query for submitted invoice exports.
        /// </summary>
        public int? BankAccountId { get; init; }

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
