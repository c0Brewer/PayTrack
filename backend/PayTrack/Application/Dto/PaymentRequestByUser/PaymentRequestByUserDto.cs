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

        UserDto? User,

        [property: Required]
        decimal Amount,

        string? PurposeOfPayment,

        string? PaymentReference,

        [property: Required]
        TransactionStatus Status,

        [property: Required]
        BudgetDto? Budget,

        TeamDto? Team,

        PaymentDirection? PaymentDirection,

        ICollection<TransactionStatusHistoryDto>? StatusHistory,

        DateTime? CreatedAt,

        DateTime? PaidAt,

        [property: Required]
        [property: MinLength(3)]
        string InvoiceNumber,

        [property: MinLength(3)]
        string? Comment,

        [property: Required]
        PayoutType PayoutType,

        bool HasPotentialDuplicate,

        BankAccountDto? BankAccount);
}
