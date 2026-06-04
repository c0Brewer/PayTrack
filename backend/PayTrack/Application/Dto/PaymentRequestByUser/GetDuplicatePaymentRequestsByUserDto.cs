// <copyright file="GetDuplicatePaymentRequestsByUserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// Dto containing the required input to check duplicate PaymentRequestByUser entries.
    /// </summary>
    public sealed class GetDuplicatePaymentRequestsByUserDto
    {
        /// <summary>
        /// Team id to compare against.
        /// </summary>
        [Required]
        required public int TeamId { get; init; }

        /// <summary>
        /// Amount to compare against.
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue)]
        required public decimal Amount { get; init; }

        /// <summary>
        /// Paid-at day to compare against.
        /// </summary>
        [Required]
        required public DateTime PaidAt { get; init; }

        /// <summary>
        /// Invoice number to compare against.
        /// </summary>
        public string? InvoiceNumber { get; init; }

        /// <summary>
        /// Existing payment request id to compare from. Used to hide dismissed duplicate pairs.
        /// </summary>
        public int? PaymentRequestByUserId { get; init; }
    }
}
