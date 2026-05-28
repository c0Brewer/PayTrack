// <copyright file="CreateTransactionDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Transaction
{
    /// <summary>
    /// Dto containing necessary information for creating a PaymentRequestByUser.
    /// </summary>
    public sealed class CreateTransactionDto
    {
        /// <summary>
        /// Gets the id of the team this transaction belongs to.
        /// </summary>
        [Required]
        public int TeamId { get; init; }

        /// <summary>
        /// Gets the transaction amount.
        /// </summary>
        [Required]
        public decimal Amount { get; init; }

        /// <summary>
        /// Gets the purpose of payment.
        /// </summary>
        [Required]
        [MinLength(3)]
        public string PurposeOfPayment { get; init; } = string.Empty;

        /// <summary>
        /// Gets the date when the transaction was paid.
        /// </summary>
        [Required]
        public DateTime PaidAt { get; init; }

        /// <summary>
        /// Gets the optional budget id.
        /// </summary>
        public int? BudgetId { get; init; }
    }
}
