// <copyright file="PaymentDirection.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Direction of a Payment (from the Racing Teams View).
    /// </summary>
    public enum PaymentDirection
    {
        /// <summary>
        /// This is incoming money for the racing team (merch costs, etc.)
        /// </summary>
        In,

        /// <summary>
        /// This is outgoing money for the racing team (submitted invoice by user, etc.)
        /// </summary>
        Out,
    }
}
