// <copyright file="UserSettingsService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Dto.UserSettings;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Implementation
{
    /// <summary>
    /// Service for managing user settings.
    /// </summary>
    public class UserSettingsService : IUserSettingsService
    {
        private readonly AppDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSettingsService"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public UserSettingsService(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<UserSettingsDto> GetUserSettingsAsync(int userId)
        {
            var user = await this.dbContext.User
                .Include(u => u.BankAccounts)
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("User not found.");

            return new UserSettingsDto
            {
                Name = user.Name,
                Email = user.Email,
                PreferredBankAccountId = user.PreferredBankAccountId,
                BankAccounts = user.BankAccounts.Select(b => new BankAccountDto
                {
                    Id = b.Id,
                    Iban = b.Iban,
                    Bic = b.Bic,
                    AccountHolder = b.AccountHolder,
                }).ToList(),
            };
        }

        /// <inheritdoc/>
        public async Task UpdateUserSettingsAsync(int userId, UserSettingsDto settingsDto)
        {
            var user = await this.dbContext.User
                .Include(u => u.BankAccounts)
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("User not found.");

            user.Name = settingsDto.Name;
            user.Email = settingsDto.Email;

            // 1. Identify IDs of incoming bank accounts
            var incomingAccountIds = settingsDto.BankAccounts
                .Where(b => b.Id != 0)
                .Select(b => b.Id)
                .ToList();

            // 2. Remove bank accounts that the user deleted
            var accountsToRemove = user.BankAccounts
                .Where(b => !incomingAccountIds.Contains(b.Id))
                .ToList();

            foreach (var account in accountsToRemove)
            {
                this.dbContext.BankAccounts.Remove(account);
            }

            // 3. Add new or Update existing bank accounts
            foreach (var accountDto in settingsDto.BankAccounts)
            {
                if (accountDto.Id == 0)
                {
                    // It's a newly added account
                    user.BankAccounts.Add(new BankAccount
                    {
                        UserId = userId,
                        Iban = accountDto.Iban,
                        Bic = accountDto.Bic,
                        AccountHolder = accountDto.AccountHolder,
                    });
                }
                else
                {
                    // Update existing account
                    var existingAccount = user.BankAccounts.FirstOrDefault(b => b.Id == accountDto.Id);
                    if (existingAccount != null)
                    {
                        existingAccount.Iban = accountDto.Iban;
                        existingAccount.Bic = accountDto.Bic;
                        existingAccount.AccountHolder = accountDto.AccountHolder;
                    }
                }
            }

            // Save first so any brand-new bank accounts get assigned an ID by the database
            await this.dbContext.SaveChangesAsync();

            // 4. Update the preferred bank account (needs to happen after new accounts get an ID)
            user.PreferredBankAccountId = settingsDto.PreferredBankAccountId;

            await this.dbContext.SaveChangesAsync();
        }
    }
}