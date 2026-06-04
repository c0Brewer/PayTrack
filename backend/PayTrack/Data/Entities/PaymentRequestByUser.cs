// <copyright file="PaymentRequestByUser.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Payment Request created from a regular User.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PaymentRequestByUser : Transaction
    {
        /// <summary>
        /// Number of Invoice.
        /// </summary>
        [MaxLength(100)]
        required public string InvoiceNumber { get; set; }

        /// <summary>
        /// Optional Comment.
        /// </summary>
        [MaxLength(1000)]
        public string? Comment { get; set; }

        /// <summary>
        /// URL / path to the stored receipt file.
        /// </summary>
        [MaxLength(2048)]
        public string? ReceiptUrl { get; set; }

        /// <summary>
        /// Type of Payout (Internal => to user, NotYetPaid => to invoice issuer, AlreadyPaid => only for documentation).
        /// </summary>
        [Required]
        public PayoutType PayoutType { get; set; }

        /// <summary>
        /// Name of the external creditor. Required when PayoutType is NotYetPaid.
        /// </summary>
        [MaxLength(255)]
        public string? CreditorName { get; set; }

        /// <summary>
        /// Required only when PayoutType is User or when an explicit bank account is chosen.
        /// </summary>
        public int? BankAccountId { get; set; }

        /// <summary>
        /// Reference to Bank Account.
        /// </summary>
        [ForeignKey(nameof(BankAccountId))]
        public BankAccount? BankAccount { get; set; }

        /// <summary>
        /// Indicates whether another payment request matches duplicate criteria.
        /// </summary>
        [NotMapped]
        public bool HasPotentialDuplicate { get; set; }
    }
}
