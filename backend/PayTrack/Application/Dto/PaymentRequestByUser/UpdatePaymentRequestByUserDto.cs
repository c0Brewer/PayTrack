// <copyright file="UpdatePaymentRequestByUserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// Dto containing necessary information for updating a PaymentRequestByUser.
    /// </summary>
    public sealed record class UpdatePaymentRequestByUserDto(

        // Optional:
        string? InvoiceNumber,

        // Optional:
        string? Comment,

        // Optional:
        PayoutType? PayoutType,

        // Optional:
        int? BankAccountId);
}
