// <copyright file="UpdateTransactionDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Transaction
{
    /// <summary>
    /// Dto containing necessary information for updating a PaymentRequestByUser.
    /// </summary>
    public sealed record class UpdateTransactionDto(

        // Optional:
        int? TeamId,

        // Optional:
        decimal? Amount,

        // Optional:
        [property: MinLength(3)]
        string? PurposeOfPayment,

        // Optional:
        DateTime? PaidAt,

        // Optional:
        int? BudgetId = null);
}
