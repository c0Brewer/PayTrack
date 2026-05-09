// <copyright file="PaymentRequestByUserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Dto.BankAccount;
using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Dto.Team;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Dto.User;
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
        UserDto? User,

        [property: Required]
        decimal Amount,

        [property: Required]
        string? PurposeOfPayment,

        [property: Required]
        string? PaymentReference,

        [property: Required]
        TransactionStatus Status,

        [property: Required]
        BudgetDto? Budget,

        [property: Required]
        TeamDto? Team,

        [property: Required]
        PaymentDirection? PaymentDirection,

        [property: Required]
        ICollection<TransactionStatusHistoryDto>? StatusHistory,

        [property: Required]
        DateTime? CreatedAt,

        [property: Required]
        DateTime? PaidAt,

        [property: Required]
        [property: MinLength(3)]
        string InvoiceNumber,

        [property: Required]
        [property: MinLength(3)]
        string? Comment,

        [property: Required]
        PayoutType PayoutType,

        [property: Required]
        BankAccountDto? BankAccount);
}
