// <copyright file="DuplicatePaymentRequestByUserScoreResult.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Data.Helpers
{
    /// <summary>
    /// Result of weighted scoring for a potential duplicate PaymentRequestByUser.
    /// </summary>
    /// <param name="Score">Weighted duplicate score.</param>
    /// <param name="MatchedFields">Fields that contributed to the duplicate match.</param>
    public sealed record class DuplicatePaymentRequestByUserScoreResult(
        int Score,
        List<string> MatchedFields);
}
