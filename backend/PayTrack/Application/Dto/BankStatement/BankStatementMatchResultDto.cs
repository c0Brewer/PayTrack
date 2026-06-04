// <copyright file="BankStatementMatchResultDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace PayTrack.Application.Dto.BankStatement
{
    /// <summary>
    /// DTO representing the match result for a single bank statement entry.
    /// </summary>
    public sealed record class BankStatementMatchResultDto(

        // The original bank statement entry being matched.
        [property: JsonPropertyName("entry")]
        BankStatementEntryDto Entry,

        // Whether a confident match was found.
        [property: JsonPropertyName("hasMatch")]
        bool HasMatch,

        // The matched payment request (if any).
        [property: JsonPropertyName("matchedTransaction")]
        BankStatementMatchedTransactionDto? MatchedTransaction,

        // Match score (0–11, higher is better).
        [property: JsonPropertyName("matchScore")]
        int MatchScore);
}
