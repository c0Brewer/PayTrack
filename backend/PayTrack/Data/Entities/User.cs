// <copyright file="User.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// User in the system.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class User
    {
        /// <summary>
        /// Id of User.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Name of User.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Email of user.
        /// </summary>
        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Profile Picture URL of User.
        /// </summary>
        [MaxLength(255)]
        public string? ProfilePictureUrl { get; set; } = null!;

        /// <summary>
        /// Foreign Key on Team.
        /// </summary>
        public int? TeamId { get; set; }

        /// <summary>
        /// Reference to Team.
        /// </summary>
        [ForeignKey(nameof(TeamId))]
        public Team? Team { get; set; } = null!;

        /// <summary>
        /// Role of User.
        /// </summary>
        [Required]
        public Role Role { get; set; } = Role.RegularUser;

        /// <summary>
        /// Indicates if User is set active or inactive.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Timestamp the user was created at.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Reference to bank accounts.
        /// </summary>
        public ICollection<BankAccount> BankAccounts { get; set; } = [];

        /// <summary>
        /// Reference to Transactions.
        /// </summary>
        public ICollection<Transaction> Transactions { get; set; } = [];

        /// <summary>
        /// Reference to TransactionStatus changes.
        /// </summary>
        public ICollection<TransactionStatusHistory> StatusHistoryChanges { get; set; } = [];

        /// <summary>
        /// Reference to payment requests (which this user created).
        /// </summary>
        public ICollection<PaymentRequestByTeam> RequestedByTeamPayments { get; set; } = [];
    }
}
