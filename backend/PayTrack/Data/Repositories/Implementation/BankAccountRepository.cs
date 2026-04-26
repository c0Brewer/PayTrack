// <copyright file="BankAccountRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Exceptions;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class BankAccountRepository(AppDbContext context) : IBankAccountRepository
    {
        /// <summary>
        /// Database Context for accessing the DB.
        /// </summary>
        private readonly AppDbContext context = context;

        /// <inheritdoc/>
        public async Task<BankAccount> AddAsync(BankAccount bankAccount)
        {
            this.context.BankAccounts.Add(bankAccount);
            await this.context.SaveChangesAsync();
            return bankAccount;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByIdAsync(int userId, int bankAccountId)
        {
            var bankAccount = await this.context.BankAccounts
                .FirstOrDefaultAsync(existingBankAccount =>
                    existingBankAccount.UserId == userId &&
                    existingBankAccount.Id == bankAccountId);
            if (bankAccount is null)
            {
                return false;
            }

            this.context.BankAccounts.Remove(bankAccount);
            await this.context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<List<BankAccount>> GetByUserIdAsync(int userId)
        {
            return await this.context.BankAccounts
                .Where(bankAccount => bankAccount.UserId == userId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> UserExistsAsync(int userId)
        {
            return await this.context.User.AnyAsync(user => user.Id == userId);
        }

        /// <inheritdoc/>
        public async Task<BankAccount> UpdateAsync(
            int userId,
            int bankAccountId,
            string? accountHolder = null,
            string? iban = null,
            string? bic = null)
        {
            var bankAccount = await this.context.BankAccounts
                .FirstOrDefaultAsync(existingBankAccount =>
                    existingBankAccount.UserId == userId &&
                    existingBankAccount.Id == bankAccountId)
                ?? throw new NotFoundException("Bank account not found");

            if (accountHolder != null)
            {
                bankAccount.AccountHolder = accountHolder;
            }

            if (iban != null)
            {
                var ibanAlreadyUsedByAnotherAccount = await this.context.BankAccounts
                    .AnyAsync(existingBankAccount =>
                        existingBankAccount.UserId == userId &&
                        existingBankAccount.Id != bankAccountId &&
                        existingBankAccount.Iban == iban);
                if (ibanAlreadyUsedByAnotherAccount)
                {
                    throw new InvalidStateException("Bank account with the same IBAN already exists for this user");
                }

                bankAccount.Iban = iban;
            }

            if (bic != null)
            {
                bankAccount.Bic = bic;
            }

            await this.context.SaveChangesAsync();
            return bankAccount;
        }
    }
}
