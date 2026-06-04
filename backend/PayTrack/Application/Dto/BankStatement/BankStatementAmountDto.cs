// <copyright file="BankStatementAmountDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace PayTrack.Application.Dto.BankStatement
{
    /// <summary>
    /// DTO for the amount details within a bank statement entry.
    /// </summary>
    public class BankStatementAmountDto
    {
        /// <summary>
        /// Gets or sets the value of the amount.
        /// </summary>
        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the currency of the amount.
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = null!;
    }
}
