// <copyright file="BankAccountDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.UserSettings
{
    /// <summary>
    /// Data Transfer Object for Bank Account.
    /// </summary>
    public class BankAccountDto
    {
        /// <summary>
        /// Gets or sets the Bank Account Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the IBAN.
        /// </summary>
        public string Iban { get; set; } = null!;

        /// <summary>
        /// Gets or sets the BIC.
        /// </summary>
        public string Bic { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Account Holder name.
        /// </summary>
        public string AccountHolder { get; set; } = null!;
    }
}