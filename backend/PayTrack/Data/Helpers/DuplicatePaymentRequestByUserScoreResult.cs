// <copyright file="DuplicatePaymentRequestByUserScoreResult.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Data.Helpers
{
    /// <summary>
    /// Result of weighted scoring for a potential duplicate PaymentRequestByUser.
    /// </summary>
    /// <param name="Score">Weighted duplicate score.</param>
    /// <param name="IsAmountAndUserMatch">Whether amount, payday, and user match exactly.</param>
    /// <param name="IsInvoiceNumberMatch">Whether invoice number matches exactly after normalization.</param>
    /// <param name="IsAmountAndTeamMatch">Whether amount, payday, and team match exactly.</param>
    public sealed record class DuplicatePaymentRequestByUserScoreResult(
        int Score,
        bool IsAmountAndUserMatch,
        bool IsInvoiceNumberMatch,
        bool IsAmountAndTeamMatch);
}
