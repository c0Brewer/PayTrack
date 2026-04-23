// <copyright file="IBankAccountRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Data.Repositories.Model
{
    /// <summary>
    /// Repository for all BankAccount-related operations.
    /// </summary>
    public interface IBankAccountRepository
    {
        /// <summary>
        /// Gets a user with all linked bank accounts by email.
        /// </summary>
        /// <param name="email">Email of the user.</param>
        /// <returns>User with loaded bank accounts if found.</returns>
        Task<User?> GetUserWithBankAccountsByEmailAsync(string email);

        /// <summary>
        /// Stores a bank account to the database.
        /// </summary>
        /// <param name="bankAccount">Bank account object to store.</param>
        /// <returns>Created bank account instance.</returns>
        Task<BankAccount> AddAsync(BankAccount bankAccount);

        /// <summary>
        /// Persists updates of a bank account.
        /// </summary>
        /// <param name="bankAccount">Bank account object to update.</param>
        /// <returns>Updated bank account instance.</returns>
        Task<BankAccount> UpdateAsync(BankAccount bankAccount);

        /// <summary>
        /// Deletes a bank account.
        /// </summary>
        /// <param name="bankAccount">Bank account object to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(BankAccount bankAccount);
    }
}
