// <copyright file="UpdatePaymentRequestByUserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// Dto containing necessary information for updating a PaymentRequestByUser.
    /// </summary>
    public sealed record class UpdatePaymentRequestByUserDto(
        [property: Required]
        UpdateTransactionDto Transaction,

        // Optional:
        string? InvoiceNumber,

        // Optional:
        string? Comment,

        // Optional:
        PayoutType? PayoutType,

        // Optional:
        int? BankAccountId);
}
