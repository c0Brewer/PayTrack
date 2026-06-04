// <copyright file="DismissedDuplicatePaymentRequestByUser.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Stores invoice pairs that an admin reviewed and marked as not duplicates.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DismissedDuplicatePaymentRequestByUser
    {
        /// <summary>
        /// Id of the dismissed duplicate pair.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Lower payment request id in the dismissed pair.
        /// </summary>
        public int FirstPaymentRequestByUserId { get; set; }

        /// <summary>
        /// Reference to the first payment request.
        /// </summary>
        [ForeignKey(nameof(FirstPaymentRequestByUserId))]
        public PaymentRequestByUser FirstPaymentRequestByUser { get; set; } = null!;

        /// <summary>
        /// Higher payment request id in the dismissed pair.
        /// </summary>
        public int SecondPaymentRequestByUserId { get; set; }

        /// <summary>
        /// Reference to the second payment request.
        /// </summary>
        [ForeignKey(nameof(SecondPaymentRequestByUserId))]
        public PaymentRequestByUser SecondPaymentRequestByUser { get; set; } = null!;

        /// <summary>
        /// Timestamp of when the duplicate warning was dismissed.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
