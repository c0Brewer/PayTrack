// <copyright file="TransactionStatus.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Status of a Transaction.
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>
        /// The request has been submitted but not yet processed in any way.
        /// </summary>
        Submitted,

        /// <summary>
        /// There have been changes requested by the Finance Team
        /// </summary>
        ChangesRequested,

        /// <summary>
        /// The request has been approved
        /// </summary>
        Approved,

        /// <summary>
        /// The money has been paid.
        /// </summary>
        Paid,

        /// <summary>
        /// The request has been declined.
        /// </summary>
        Declined,
    }
}
