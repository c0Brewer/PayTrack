// <copyright file="DuplicatePaymentRequestByUserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// Dto containing the scored duplicate match for a PaymentRequestByUser.
    /// </summary>
    public sealed record class DuplicatePaymentRequestByUserDto(
        [property: Required]
        PaymentRequestByUserDto PaymentRequestByUser,

        [property: Required]
        int Score,

        [property: Required]
        bool IsAmountAndUserMatch,

        [property: Required]
        bool IsAmountAndTeamMatch,

        [property: Required]
        bool IsInvoiceNumberMatch);
}
