// <copyright file="BankStatementMatchResponseDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace PayTrack.Application.Dto.BankStatement
{
    /// <summary>
    /// DTO representing the full response for bank statement matching.
    /// </summary>
    public sealed record class BankStatementMatchResponseDto(

        // List of match results for each bank statement entry.
        [property: JsonPropertyName("results")]
        List<BankStatementMatchResultDto> Results
    );
}
