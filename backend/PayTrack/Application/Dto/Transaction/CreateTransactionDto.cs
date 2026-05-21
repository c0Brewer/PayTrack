// <copyright file="CreateTransactionDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Transaction
{
    /// <summary>
    /// Dto containing necessary information for creating a PaymentRequestByUser.
    /// </summary>
    public sealed record class CreateTransactionDto(
        [property: Required]
        int TeamId,

        [property: Required]
        decimal Amount,

        [property: Required]
        [property: MinLength(3)]
        string PurposeOfPayment,

        [property: Required]
        DateTime PaidAt,

        int? BudgetId = null);
}
