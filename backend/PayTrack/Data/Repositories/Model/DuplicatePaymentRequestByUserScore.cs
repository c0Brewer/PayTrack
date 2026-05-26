// <copyright file="DuplicatePaymentRequestByUserScore.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Data.Repositories.Model
{
    /// <summary>
    /// Weighted score for a potential duplicate PaymentRequestByUser.
    /// </summary>
    /// <param name="Score">Weighted duplicate score.</param>
    /// <param name="IsAmountAndUserMatch">Whether amount, payday, and user match exactly.</param>
    /// <param name="IsInvoiceNumberMatch">Whether invoice number matches exactly after normalization.</param>
    /// <param name="IsAmountAndTeamMatch">Whether amount, payday, and team match exactly.</param>
    public sealed record class DuplicatePaymentRequestByUserScore(
        int Score,
        bool IsAmountAndUserMatch,
        bool IsInvoiceNumberMatch,
        bool IsAmountAndTeamMatch);
}
