// <copyright file="GetPaymentRequestByUserQuery.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Transaction;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// DTO representing all information a PaymentRequestByUser can query on GET /PaymentRequestByUser.
    /// </summary>
    public class GetPaymentRequestByUserQuery : GetTransactionQuery
    {
        /// <summary>
        /// Cost centre of the assigned budget to query.
        /// </summary>
        public int? CostCentreId { get; init; }

        /// <summary>
        /// Whether to load the bank account in the query.
        /// </summary>
        public bool? IncludeBankAccount { get; init; }
    }
}
