// <copyright file="PaymentRequestByUserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Dto.BankAccount;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// Dto containing necessary information about a PaymentRequestByUser
    /// </summary>
    public sealed record class PaymentRequestByUserDto(
        [property: Required]
        int Id,

        [property: Required]
        [property: MinLength(3)]
        string InvoiceNumber,

        [property: Required]
        [property: MinLength(3)]
        string? Comment,

        [property: Required]
        [property: MinLength(3)]
        string? ReceiptUrl,

        [property: Required]
        PayoutType PayoutType,

        [property: Required]
        BankAccountDto? BankAccount);
}
