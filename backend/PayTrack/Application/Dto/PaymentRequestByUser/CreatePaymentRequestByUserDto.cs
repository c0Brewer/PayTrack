// <copyright file="CreatePaymentRequestByUserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Validation;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// Dto containing necessary information for creating a PaymentRequestByUser.
    /// </summary>
    public sealed class CreatePaymentRequestByUserDto
    {
        /// <summary>
        /// Gets the base transaction data.
        /// </summary>
        [Required]
        public CreateTransactionDto Transaction { get; init; } = null!;

        /// <summary>
        /// Gets the invoice number.
        /// </summary>
        [Required]
        [MinLength(3)]
        public string InvoiceNumber { get; init; } = string.Empty;

        /// <summary>
        /// Gets an optional comment for the invoice.
        /// </summary>
        [OptionalMinLength(3)]
        public string? Comment { get; init; } = string.Empty;

        /// <summary>
        /// Gets the uploaded receipt file.
        /// </summary>
        [Required]
        public IFormFile Receipt { get; init; } = null!;

        /// <summary>
        /// Gets the payout type.
        /// </summary>
        [Required]
        public PayoutType PayoutType { get; init; }

        /// <summary>
        /// Gets the name of the external creditor. Required when PayoutType is NotYetPaid.
        /// </summary>
        public string? CreditorName { get; init; }

        /// <summary>
        /// Gets the due date of the invoice. Required when PayoutType is NotYetPaid.
        /// </summary>
        public DateTime? DueDate { get; init; }

        /// <summary>
        /// Gets the bank account id, required only for user payouts.
        /// </summary>
        public int? BankAccountId { get; init; }
    }
}
