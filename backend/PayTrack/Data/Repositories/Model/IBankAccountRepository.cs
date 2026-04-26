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
        /// Gets all bank accounts linked to a user.
        /// </summary>
        /// <param name="userId">Id of the user.</param>
        /// <returns>Bank accounts of the user.</returns>
        Task<List<BankAccount>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Checks whether a user exists.
        /// </summary>
        /// <param name="userId">Id of the user.</param>
        /// <returns><c>true</c> if the user exists; otherwise <c>false</c>.</returns>
        Task<bool> UserExistsAsync(int userId);

        /// <summary>
        /// Stores a bank account to the database.
        /// </summary>
        /// <param name="bankAccount">Bank account object to store.</param>
        /// <returns>Created bank account instance.</returns>
        Task<BankAccount> AddAsync(BankAccount bankAccount);

        /// <summary>
        /// Persists updates of a bank account.
        /// </summary>
        /// <param name="userId">Id of the user.</param>
        /// <param name="bankAccountId">Id of the bank account to update.</param>
        /// <param name="accountHolder">Optional account holder name.</param>
        /// <param name="iban">Optional IBAN value.</param>
        /// <param name="bic">Optional BIC value.</param>
        /// <returns>Updated bank account instance.</returns>
        Task<BankAccount> UpdateAsync(
            int userId,
            int bankAccountId,
            string? accountHolder = null,
            string? iban = null,
            string? bic = null);

        /// <summary>
        /// Deletes a bank account by id for a given user.
        /// </summary>
        /// <param name="userId">Id of the user.</param>
        /// <param name="bankAccountId">Id of the bank account to delete.</param>
        /// <returns><c>true</c> if a bank account was deleted; otherwise <c>false</c>.</returns>
        Task<bool> DeleteByIdAsync(int userId, int bankAccountId);
    }
}
