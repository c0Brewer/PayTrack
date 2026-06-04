// <copyright file="BankStatementAccountDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace PayTrack.Application.Dto.BankStatement
{
    /// <summary>
    /// DTO for bank account details within a bank statement entry.
    /// </summary>
    public class BankStatementAccountDto
    {
        /// <summary>
        /// Gets or sets the IBAN of the account.
        /// </summary>
        [JsonPropertyName("iban")]
        public string? Iban { get; set; }

        /// <summary>
        /// Gets or sets the BIC of the account.
        /// </summary>
        [JsonPropertyName("bic")]
        public string? Bic { get; set; }
    }
}
