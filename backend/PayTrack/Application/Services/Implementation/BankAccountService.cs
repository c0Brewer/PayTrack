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
        public async Task<BankAccount> CreateBankAccountAsync(string email, string accountHolder, string iban, string bic)
        {
            var user = await this.GetUserWithBankAccountsAsync(email);

            if (user.BankAccounts.Any(existingBankAccount => existingBankAccount.Iban == iban))
            {
                throw new InvalidStateException("Bank account with the same IBAN already exists for this user");
            }

            var bankAccount = new BankAccount
            {
                UserId = user.Id,
                AccountHolder = accountHolder,
                Iban = iban,
                Bic = bic,
            };

            return await this.repo.AddAsync(bankAccount);
        }

        /// <inheritdoc/>
        public async Task DeleteBankAccountAsync(string email, int bankAccountId)
        {
            var user = await this.GetUserWithBankAccountsAsync(email);

            var bankAccount = user.BankAccounts
                .FirstOrDefault(existingBankAccount => existingBankAccount.Id == bankAccountId)
                ?? throw new NotFoundException("Bank account not found");

            await this.repo.DeleteAsync(bankAccount);
        }

        /// <inheritdoc/>
        public async Task<User> GetUserWithBankAccountsAsync(string email)
        {
            return await this.repo.GetUserWithBankAccountsByEmailAsync(email)
                   ?? throw new NotFoundException("User not found");
        }

        /// <inheritdoc/>
        public async Task<BankAccount> UpdateBankAccountAsync(string email, int bankAccountId, string accountHolder, string iban, string bic)
        {
            var user = await this.GetUserWithBankAccountsAsync(email);

            var bankAccount = user.BankAccounts
                .FirstOrDefault(existingBankAccount => existingBankAccount.Id == bankAccountId)
                ?? throw new NotFoundException("Bank account not found");

            var ibanAlreadyUsedByAnotherAccount = user.BankAccounts.Any(existingBankAccount =>
                existingBankAccount.Id != bankAccountId && existingBankAccount.Iban == iban);
            if (ibanAlreadyUsedByAnotherAccount)
            {
                throw new InvalidStateException("Bank account with the same IBAN already exists for this user");
            }

            bankAccount.AccountHolder = accountHolder;
            bankAccount.Iban = iban;
            bankAccount.Bic = bic;

            return await this.repo.UpdateAsync(bankAccount);
        }
    }
}
