// <copyright file="BankStatementUpdateRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace PayTrack.Application.Dto.BankStatement
{
    /// <summary>
    /// DTO for saving confirmed bank statement matches.
    /// </summary>
    public sealed record class BankStatementUpdateRequestDto(

        // Unique identifier for the entry in the frontend.
        [property: JsonPropertyName("entryId")]
        string EntryId,

        // The ID of the matched payment request (if any).
        [property: JsonPropertyName("matchedTransactionId")]
        int? MatchedTransactionId,

        // Whether to mark this entry as skipped.
        [property: JsonPropertyName("skipped")]
        bool Skipped
    );
}
