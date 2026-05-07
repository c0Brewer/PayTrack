// <copyright file="Transaction.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Abstract base class for all transaction types.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class Transaction
    {
        /// <summary>
        /// Id of Transaction.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key on User.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Reference to User.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        /// <summary>
        /// Amount of Transaction.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Purpose of Payment.
        /// </summary>
        [MaxLength(500)]
        public string? PurposeOfPayment { get; set; }

        /// <summary>
        /// Payment Reference. To be set by the finance team.
        /// </summary>
        [MaxLength(255)]
        public string? PaymentReference { get; set; }

        /// <summary>
        /// Status of Transaction.
        /// </summary>
        [Required]
        public TransactionStatus Status { get; set; } = TransactionStatus.Submitted;

        /// <summary>
        /// Foreign Key on Budget.
        /// </summary>
        public int? BudgetId { get; set; }

        /// <summary>
        /// Budget Reference.
        /// </summary>
        [ForeignKey(nameof(BudgetId))]
        public Budget? Budget { get; set; }

        /// <summary>
        /// Foreign Key on Team.
        /// </summary>
        [Required]
        public int TeamId { get; set; }

        /// <summary>
        /// Team Reference.
        /// </summary>
        [ForeignKey(nameof(TeamId))]
        public Team Team { get; set; } = null!;

        /// <summary>
        /// Direction of Payment.
        /// </summary>
        [Required]
        public PaymentDirection PaymentDirection { get; set; }

        /// <summary>
        /// Timestamp of when the invoice was created in our system.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp of when the payment was actually done.
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Reference for Status History.
        /// </summary>
        public ICollection<TransactionStatusHistory> StatusHistory { get; set; } = [];
    }
}
