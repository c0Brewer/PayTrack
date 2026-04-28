// <copyright file="BankAccountService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class BankAccountService(IBankAccountRepository repo) : IBankAccountService
    {
        /// <summary>
        /// Repository for bank accounts.
        /// </summary>
        private readonly IBankAccountRepository repo = repo;

        /// <inheritdoc/>
        public async Task<BankAccount> CreateBankAccountAsync(int userId, string accountHolder, string iban, string bic)
        {
            var bankAccounts = await this.GetUserBankAccountsOrThrowAsync(userId);

            if (bankAccounts.Any(existingBankAccount => existingBankAccount.Iban == iban))
            {
                throw new InvalidStateException("Bank account with the same IBAN already exists for this user");
            }

            var bankAccount = new BankAccount
            {
                UserId = userId,
                AccountHolder = accountHolder,
                Iban = iban,
                Bic = bic,
            };

            return await this.repo.AddAsync(bankAccount);
        }

        /// <inheritdoc/>
        public async Task DeleteBankAccountAsync(int userId, int bankAccountId)
        {
            var wasDeleted = await this.repo.DeleteByIdAsync(userId, bankAccountId);
            if (!wasDeleted)
            {
                throw new NotFoundException("Bank account not found");
            }
        }

        /// <inheritdoc/>
        public async Task<List<BankAccount>> GetBankAccountsAsync(int userId)
        {
            return await this.GetUserBankAccountsOrThrowAsync(userId);
        }

        /// <inheritdoc/>
        public async Task<BankAccount> UpdateBankAccountAsync(
            int userId,
            int bankAccountId,
            string? accountHolder = null,
            string? iban = null,
            string? bic = null)
        {
            return await this.repo.UpdateAsync(userId, bankAccountId, accountHolder, iban, bic);
        }

        /// <summary>
        /// Gets all bank accounts of a user and ensures the user exists.
        /// </summary>
        /// <param name="userId">Id of the user.</param>
        /// <returns>Bank accounts of the user.</returns>
        private async Task<List<BankAccount>> GetUserBankAccountsOrThrowAsync(int userId)
        {
            var bankAccounts = await this.repo.GetByUserIdAsync(userId);

            if (bankAccounts.Count > 0 || await this.repo.UserExistsAsync(userId))
            {
                return bankAccounts;
            }

            throw new NotFoundException("User not found");
        }
    }
}
