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
        /// PaymentRequestByTeam requester id to query.
        /// </summary>
        public int? RequestById { get; init; }

        /// <summary>
        /// Cost centre of the assigned budget to query.
        /// </summary>
        public int? CostCentreId { get; init; }

        /// <summary>
        /// Whether to include only statuses visible in the payment requests table.
        /// </summary>
        public bool? VisibleStatusesOnly { get; init; }
    }
}
