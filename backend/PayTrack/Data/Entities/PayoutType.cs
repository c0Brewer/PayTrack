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
        /// The Money should get paid out to the issuer of the invoice.
        /// </summary>
        External,
    }
}
