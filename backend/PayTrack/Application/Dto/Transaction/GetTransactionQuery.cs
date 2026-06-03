// <copyright file="GetTransactionQuery.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.Transaction
{
    /// <summary>
    /// DTO representing all information a Transaction can query on GET /Transaction.
    /// </summary>
    public class GetTransactionQuery
    {
        /// <summary>
        /// UserId to query.
        /// </summary>
        public int? UserId { get; init; }

        /// <summary>
        /// Min Amount to query.
        /// </summary>
        public decimal? MinAmount { get; init; }

        /// <summary>
        /// Max Amount to query.
        /// </summary>
        public decimal? MaxAmount { get; init; }

        /// <summary>
        /// Purpose of Payment to query.
        /// </summary>
        public string? PurposeOfPayment { get; init; }

        /// <summary>
        /// Payment Reference to query.
        /// </summary>
        public string? PaymentReference { get; init; }

        /// <summary>
        /// Transaction Status to query.
        /// </summary>
        public TransactionStatus? Status { get; init; }

        /// <summary>
        /// Team to query.
        /// </summary>
        public int? TeamId { get; init; }

        /// <summary>
        /// Cost centre to query.
        /// </summary>
        public int? CostCentreId { get; init; }

        /// <summary>
        /// PaymentDirection to query.
        /// </summary>
        public PaymentDirection? PaymentDirection { get; init; }

        /// <summary>
        /// MinCreatedAt to query.
        /// </summary>
        public DateTime? MinCreatedAt { get; init; }

        /// <summary>
        /// MaxCreatedAt to query.
        /// </summary>
        public DateTime? MaxCreatedAt { get; init; }

        /// <summary>
        /// MinPaidAt to query.
        /// </summary>
        public DateTime? MinPaidAt { get; init; }

        /// <summary>
        /// MaxPaidAt to query.
        /// </summary>
        public DateTime? MaxPaidAt { get; init; }

        /// <summary>
        /// MinDueDate to query.
        /// </summary>
        public DateTime? MinDueDate { get; init; }

        /// <summary>
        /// MaxDueDate to query.
        /// </summary>
        public DateTime? MaxDueDate { get; init; }

        /// <summary>
        /// Limit of query.
        /// </summary>
        public int? Limit { get; init; }

        /// <summary>
        /// Offset of query.
        /// </summary>
        public int? Offset { get; init; }

        /// <summary>
        /// Whether to include the team in the query.
        /// </summary>
        public bool? IncludeTeam { get; init; }

        /// <summary>
        /// Whether to load the status history in the query.
        /// </summary>
        public bool? IncludeStatusHistory { get; init; }

        /// <summary>
        /// Whether to include the budget and cost centre in the query.
        /// </summary>
        public bool? IncludeBudget { get; init; }

        /// <summary>
        /// Requested financial export format.
        /// </summary>
        public FinancialExportFormat? Format { get; init; }
    }
}
