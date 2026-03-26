// <copyright file="Bankaccount.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Bank Account of a particular User.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class BankAccount
    {
        /// <summary>
        /// Id of Bank Account.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Foreign Key of User.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Reference to User.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        /// <summary>
        /// Iban of Bank Account.
        /// </summary>
        [Required]
        [MaxLength(34)] // Max IBAN length per ISO 13616
        public string Iban { get; set; } = null!;

        /// <summary>
        /// BIC/Swift of Bankaccount.
        /// </summary>
        [Required]
        [MaxLength(11)] // BIC is 8 or 11 chars per ISO 9362
        public string Bic { get; set; } = null!;

        /// <summary>
        /// Account Holder of Bankaccount.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string AccountHolder { get; set; } = null!;
    }
}
