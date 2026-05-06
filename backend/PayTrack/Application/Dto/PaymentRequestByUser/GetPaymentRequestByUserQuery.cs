// <copyright file="GetPaymentRequestByUserQuery.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Transaction;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// DTO representing all information a PaymentRequestByUser can query on GET /PaymentRequestByUser.
    /// </summary>
    public class GetPaymentRequestByUserQuery : GetTransactionQuery
    {
        /// <summary>
        /// Invoice Number to query.
        /// </summary>
        public string? InvoiceNumber { get; init; }

        /// <summary>
        /// Payout Type to query.
        /// </summary>
        public PayoutType? PayoutType { get; set; }

        /// <summary>
        /// Bank account id to query.
        /// </summary>
        public int? BankAccountId { get; init; }

        /// <summary>
        /// Whether to load the bank account in the query.
        /// </summary>
        public bool? IncludeBankAccount { get; init; }
    }
}
