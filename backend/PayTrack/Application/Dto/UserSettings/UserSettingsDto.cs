// <copyright file="UserSettingsDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace PayTrack.Application.Dto.UserSettings
{
    /// <summary>
    /// Data Transfer Object for User Settings.
    /// </summary>
    public class UserSettingsDto
    {
        /// <summary>
        /// Gets or sets the Name of the user.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Email of the user.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Gets or sets the preferred Bank Account Id.
        /// </summary>
        public int? PreferredBankAccountId { get; set; }

        /// <summary>
        /// Gets or sets the list of Bank Accounts.
        /// </summary>
        public List<BankAccountDto> BankAccounts { get; set; } = new List<BankAccountDto>();
    }
}