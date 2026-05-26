// <copyright file="DuplicatePaymentRequestByUserMatch.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Internal model containing duplicate match details for a PaymentRequestByUser.
    /// </summary>
    public sealed record class DuplicatePaymentRequestByUserMatch(
        PaymentRequestByUser PaymentRequestByUser,
        int Score,
        bool IsAmountAndUserMatch,
        bool IsInvoiceNumberMatch,
        bool IsAmountAndTeamMatch);
}
