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
        Submitted = 0,

        /// <summary>
        /// There have been changes requested by the Finance Team
        /// </summary>
        ChangesRequested = 1,

        /// <summary>
        /// The request has been approved
        /// </summary>
        Approved = 2,

        /// <summary>
        /// The money has been paid.
        /// </summary>
        Paid = 3,

        /// <summary>
        /// The request has been declined.
        /// </summary>
        Declined = 4,

        /// <summary>
        /// The request is back in review after requested changes were submitted.
        /// </summary>
        Review = 5,
    }
}
