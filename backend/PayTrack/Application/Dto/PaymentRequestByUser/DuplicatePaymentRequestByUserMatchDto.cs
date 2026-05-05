// <copyright file="DuplicatePaymentRequestByUserMatchDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PaymentRequestByUserEntity = PayTrack.Data.Entities.PaymentRequestByUser;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// Internal dto containing duplicate match details for a PaymentRequestByUser.
    /// </summary>
    public sealed record class DuplicatePaymentRequestByUserMatchDto(
        [property: Required]
        PaymentRequestByUserEntity PaymentRequestByUser,

        [property: Required]
        int Score,

        [property: Required]
        bool IsAmountAndUserMatch,

        [property: Required]
        bool IsAmountAndTeamMatch,

        [property: Required]
        bool IsInvoiceNumberMatch);
}
