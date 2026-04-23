// <copyright file="IBankAccountService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service which handles BankAccount-related requests.
    /// </summary>
    public interface IBankAccountService
    {
        /// <summary>
        /// Gets a user with linked bank accounts by email.
        /// </summary>
        /// <param name="email">Email of the user.</param>
        /// <returns>User with loaded bank accounts.</returns>
        Task<User> GetUserWithBankAccountsAsync(string email);

        /// <summary>
        /// Creates a bank account for the user.
        /// </summary>
        /// <param name="email">Email of the user.</param>
        /// <param name="accountHolder">Account holder name.</param>
        /// <param name="iban">IBAN value.</param>
        /// <param name="bic">BIC value.</param>
        /// <returns>Created bank account.</returns>
        Task<BankAccount> CreateBankAccountAsync(string email, string accountHolder, string iban, string bic);

        /// <summary>
        /// Updates a bank account for the user.
        /// </summary>
        /// <param name="email">Email of the user.</param>
        /// <param name="bankAccountId">Id of the bank account to update.</param>
        /// <param name="accountHolder">Account holder name.</param>
        /// <param name="iban">IBAN value.</param>
        /// <param name="bic">BIC value.</param>
        /// <returns>Updated bank account.</returns>
        Task<BankAccount> UpdateBankAccountAsync(string email, int bankAccountId, string accountHolder, string iban, string bic);

        /// <summary>
        /// Deletes a bank account for the user.
        /// </summary>
        /// <param name="email">Email of the user.</param>
        /// <param name="bankAccountId">Id of the bank account to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteBankAccountAsync(string email, int bankAccountId);
    }
}
