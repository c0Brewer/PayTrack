// <copyright file="MarkPaymentRequestByUserAsPaidDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// Dto containing payment completion data supplied by finance.
    /// </summary>
    public sealed record class MarkPaymentRequestByUserAsPaidDto(
        [property: Required]
        [property: MinLength(3)]
        [property: MaxLength(255)]
        string PaymentReference,

        [property: Required]
        [property: MinLength(3)]
        [property: MaxLength(500)]
        string PurposeOfPayment,

        [property: Required]
        [property: Range(typeof(DateTime), "1900-01-01", "9999-12-31")]
        DateTime? PaymentDate);
}
