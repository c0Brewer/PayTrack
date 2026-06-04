// <copyright file="BankStatementMatchedTransactionDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using PayTrack.Application.Dto.BankAccount;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.BankStatement
{
    /// <summary>
    /// Flat DTO representing a matched transaction for bank statement review.
    /// Contains all fields needed for the side-by-side comparison UI.
    /// Fields specific to PaymentRequestByUser are null for team transactions.
    /// </summary>
    public class BankStatementMatchedTransactionDto
    {
        /// <summary>Gets or sets the transaction id.</summary>
        [JsonPropertyName("id")]
        public int Id { get; init; }

        /// <summary>Gets or sets the transaction amount.</summary>
        [JsonPropertyName("amount")]
        public decimal Amount { get; init; }

        /// <summary>Gets or sets the purpose of payment.</summary>
        [JsonPropertyName("purposeOfPayment")]
        public string? PurposeOfPayment { get; init; }

        /// <summary>Gets or sets the payment reference.</summary>
        [JsonPropertyName("paymentReference")]
        public string? PaymentReference { get; init; }

        /// <summary>Gets or sets the current transaction status.</summary>
        [JsonPropertyName("status")]
        public TransactionStatus Status { get; init; }

        /// <summary>Gets or sets the date the transaction was paid.</summary>
        [JsonPropertyName("paidAt")]
        public DateTime? PaidAt { get; init; }

        /// <summary>Gets or sets the name of the submitting user.</summary>
        [JsonPropertyName("userName")]
        public string? UserName { get; init; }

        /// <summary>Gets or sets the name of the team the transaction belongs to.</summary>
        [JsonPropertyName("teamName")]
        public string? TeamName { get; init; }

        /// <summary>Gets or sets the invoice number (PaymentRequestByUser only).</summary>
        [JsonPropertyName("invoiceNumber")]
        public string? InvoiceNumber { get; init; }

        /// <summary>Gets or sets the bank account (PaymentRequestByUser only).</summary>
        [JsonPropertyName("bankAccount")]
        public BankAccountDto? BankAccount { get; init; }
    }
}
