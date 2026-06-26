// <copyright file="ResubmitPaymentRequestByUserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Validation;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// Dto containing updated invoice data after finance requested changes.
    /// </summary>
    public sealed class ResubmitPaymentRequestByUserDto
    {
        /// <summary>
        /// Updated transaction data.
        /// </summary>
        [Required]
        required public CreateTransactionDto Transaction { get; init; }

        /// <summary>
        /// Updated invoice number.
        /// </summary>
        [Required]
        [MinLength(3)]
        required public string InvoiceNumber { get; init; }

        /// <summary>
        /// Optional updated comment.
        /// </summary>
        [OptionalMinLength(3)]
        public string? Comment { get; init; }

        /// <summary>
        /// Updated payout type.
        /// </summary>
        [Required]
        public PayoutType PayoutType { get; init; }

        /// <summary>
        /// Updated optional bank account id.
        /// </summary>
        public int? BankAccountId { get; init; }

        /// <summary>
        /// Updated optional creditor name.
        /// </summary>
        public string? CreditorName { get; init; }

        /// <summary>
        /// Updated optional due date.
        /// </summary>
        public DateTime? DueDate { get; init; }
    }
}
