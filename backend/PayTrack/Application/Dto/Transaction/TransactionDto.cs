// <copyright file="TransactionDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.Transaction
{
    /// <summary>
    /// Dto containing necessary information for creating a PaymentRequestByUser.
    /// </summary>
    public sealed class TransactionDto
    {
        /// <summary>
        /// Foreign key on User.
        /// </summary>
        [Required]
        public int UserId { get; set; }

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
        public string? PurposeOfPayment { get; init; } = null!;

        /// <summary>
        /// Payment Reference. To be set by the finance team.
        /// </summary>
        [MaxLength(255)]
        public string? PaymentReference { get; set; }

        /// <summary>
        /// Status of Transaction.
        /// </summary>
        [Required]
        public TransactionStatus Status { get; set; }

        /// <summary>
        /// Foreign Key on Budget.
        /// </summary>
        public int? BudgetId { get; set; }

        /// <summary>
        /// Date the transaction was paid.
        /// </summary>
        [Required]
        public DateTime? PaidAt { get; init; }
    }
}
