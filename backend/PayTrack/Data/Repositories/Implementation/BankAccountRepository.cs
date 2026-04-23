// <copyright file="BankAccountRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
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
        public async Task DeleteAsync(BankAccount bankAccount)
        {
            this.context.BankAccounts.Remove(bankAccount);
            await this.context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<User?> GetUserWithBankAccountsByEmailAsync(string email)
        {
            return await this.context.User
                .Include(u => u.BankAccounts)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <inheritdoc/>
        public async Task<BankAccount> UpdateAsync(BankAccount bankAccount)
        {
            await this.context.SaveChangesAsync();
            return bankAccount;
        }
    }
}
