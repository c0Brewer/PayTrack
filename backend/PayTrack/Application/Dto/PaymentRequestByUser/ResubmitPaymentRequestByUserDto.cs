// <copyright file="ResubmitPaymentRequestByUserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// Dto containing updated invoice data after finance requested changes.
    /// </summary>
    public sealed record class ResubmitPaymentRequestByUserDto(
        [property: Required]
        CreateTransactionDto Transaction,

        [property: Required]
        [property: MinLength(3)]
        string InvoiceNumber,

        [property: MinLength(3)]
        string? Comment,

        [property: Required]
        PayoutType PayoutType,

        int? BankAccountId);
}
