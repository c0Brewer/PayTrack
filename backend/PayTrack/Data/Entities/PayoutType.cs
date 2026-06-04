// <copyright file="PayoutType.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Payout Target Type.
    /// </summary>
    public enum PayoutType
    {
        /// <summary>
        /// The Money should get paid out to the User who submitted the invoice.
        /// </summary>
        User,

        /// <summary>
        /// The invoice has not been paid yet and should be paid directly to the external creditor.
        /// </summary>
        NotYetPaid,

        /// <summary>
        /// The invoice has already been paid outside the system and is submitted for documentation purposes only.
        /// Status is automatically set to Paid on creation.
        /// </summary>
        AlreadyPaid,
    }
}
