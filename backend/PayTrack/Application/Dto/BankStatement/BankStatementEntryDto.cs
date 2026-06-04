// <copyright file="BankStatementEntryDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace PayTrack.Application.Dto.BankStatement
{
    /// <summary>
    /// DTO for a single bank statement entry.
    /// </summary>
    public class BankStatementEntryDto
    {
        /// <summary>
        /// Gets or sets the booking date of the transaction.
        /// </summary>
        [JsonPropertyName("booking")]
        public DateTime Booking { get; set; }

        /// <summary>
        /// Gets or sets the name of the partner (beneficiary/sender).
        /// </summary>
        [JsonPropertyName("partnerName")]
        public string? PartnerName { get; set; }

        /// <summary>
        /// Gets or sets the account details of the partner.
        /// </summary>
        [JsonPropertyName("partnerAccount")]
        public BankStatementAccountDto? PartnerAccount { get; set; }

        /// <summary>
        /// Gets or sets the amount of the transaction.
        /// </summary>
        [JsonPropertyName("amount")]
        public BankStatementAmountDto Amount { get; set; } = null!;

        /// <summary>
        /// Gets or sets the receiver reference.
        /// </summary>
        [JsonPropertyName("receiverReference")]
        public string? ReceiverReference { get; set; }

        /// <summary>
        /// Gets or sets the general reference.
        /// </summary>
        [JsonPropertyName("reference")]
        public string? Reference { get; set; }
    }
}
