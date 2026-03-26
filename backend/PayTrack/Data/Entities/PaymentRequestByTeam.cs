// <copyright file="PaymentRequestByTeam.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Payment Requested created by the finance team. Requests money FROM a regular user.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PaymentRequestByTeam : Transaction
    {
        /// <summary>
        /// The finance/admin user who created this request on behalf of a team.
        /// </summary>
        [Required]
        public int RequestedById { get; set; }

        /// <summary>
        /// User who requested the Payment (A finance team user).
        /// </summary>
        [ForeignKey(nameof(RequestedById))]
        public User RequestedBy { get; set; } = null!;
    }
}
