// <copyright file="GetPaymentRequestByTeamQuery.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Transaction;

namespace PayTrack.Application.Dto.PaymentRequestByTeam
{
    /// <summary>
    /// DTO representing all information a PaymentRequestByTeam can query on GET /PaymentRequestByTeam.
    /// </summary>
    public class GetPaymentRequestByTeamQuery : GetTransactionQuery
    {
        /// <summary>
        /// Whether to include only statuses visible in the payment requests table.
        /// </summary>
        public bool? VisibleStatusesOnly { get; init; }
    }
}
