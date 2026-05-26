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
        /// id of user who made the request to query.
        /// </summary>
        public int? RequestById { get; init; }
    }
}
