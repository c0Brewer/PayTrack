// <copyright file="PaymentManual.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Manual Payment from the Finance Team.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PaymentManual : Transaction
    {
        /// <summary>
        /// Source of the Payment.
        /// </summary>
        [MaxLength(255)]
        public string? PaymentSource { get; set; }
    }
}
