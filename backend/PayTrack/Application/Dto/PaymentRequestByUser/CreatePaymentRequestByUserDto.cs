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
    public sealed record class CreatePaymentRequestByUserDto(
        [property: Required]
        CreateTransactionDto Transaction,

        [property: Required]
        [property: MinLength(3)]
        string InvoiceNumber,

        [property: OptionalMinLength(3)]
        string? Comment,

        [property: Required]
        IFormFile Receipt,

        [property: Required]
        PayoutType PayoutType,

        [property: Required]
        int? BankAccountId);
}
