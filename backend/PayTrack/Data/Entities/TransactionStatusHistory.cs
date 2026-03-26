// <copyright file="TransactionStatusHistory.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Tracks History of the Status changes of a transaction.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TransactionStatusHistory
    {
        /// <summary>
        /// Id of transaction change.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key of transaction it referencecs.
        /// </summary>
        [Required]
        public int TransactionId { get; set; }

        /// <summary>
        /// Transaction Reference.
        /// </summary>
        [ForeignKey(nameof(TransactionId))]
        public Transaction Transaction { get; set; } = null!;

        /// <summary>
        /// Id of user who made the change.
        /// </summary>
        [Required]
        public int ChangedById { get; set; }

        /// <summary>
        /// User who made the change.
        /// </summary>
        [ForeignKey(nameof(ChangedById))]
        public User ChangedBy { get; set; } = null!;

        /// <summary>
        /// Optional comment.
        /// </summary>
        [MaxLength(1000)]
        public string? Comment { get; set; }

        /// <summary>
        /// Status it was changed from.
        /// </summary>
        [Required]
        public TransactionStatus FromStatus { get; set; }

        /// <summary>
        /// Status it was changed to.
        /// </summary>
        [Required]
        public TransactionStatus ToStatus { get; set; }

        /// <summary>
        /// Timestamp of when the transaction was changed at.
        /// </summary>
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
