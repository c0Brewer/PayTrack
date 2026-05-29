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
        /// Id of the team the transaction belongs to.
        /// </summary>
        [Required]
        public int TeamId { get; init; }

        /// <summary>
        /// Amount of the transaction.
        /// </summary>
        [Required]
        public decimal Amount { get; init; }

        /// <summary>
        /// Purpose of payment.
        /// </summary>
        [Required]
        [MinLength(3)]
        public string PurposeOfPayment { get; init; } = null!;

        /// <summary>
        /// Date the transaction was paid.
        /// </summary>
        [Required]
        public DateTime PaidAt { get; init; }

        /// <summary>
        /// Optional budget id assigned to the transaction.
        /// </summary>
        public int? BudgetId { get; init; }
    }
}
