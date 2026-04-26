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
        /// Gets all bank accounts linked to a user.
        /// </summary>
        /// <param name="userId">Id of the user.</param>
        /// <returns>Bank accounts of the user.</returns>
        Task<List<BankAccount>> GetBankAccountsAsync(int userId);

        /// <summary>
        /// Creates a bank account for the user.
        /// </summary>
        /// <param name="userId">Id of the user.</param>
        /// <param name="accountHolder">Account holder name.</param>
        /// <param name="iban">IBAN value.</param>
        /// <param name="bic">BIC value.</param>
        /// <returns>Created bank account.</returns>
        Task<BankAccount> CreateBankAccountAsync(int userId, string accountHolder, string iban, string bic);

        /// <summary>
        /// Updates a bank account for the user.
        /// </summary>
        /// <param name="userId">Id of the user.</param>
        /// <param name="bankAccountId">Id of the bank account to update.</param>
        /// <param name="accountHolder">Optional account holder name.</param>
        /// <param name="iban">Optional IBAN value.</param>
        /// <param name="bic">Optional BIC value.</param>
        /// <returns>Updated bank account.</returns>
        Task<BankAccount> UpdateBankAccountAsync(
            int userId,
            int bankAccountId,
            string? accountHolder = null,
            string? iban = null,
            string? bic = null);

        /// <summary>
        /// Deletes a bank account for the user.
        /// </summary>
        /// <param name="userId">Id of the user.</param>
        /// <param name="bankAccountId">Id of the bank account to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteBankAccountAsync(int userId, int bankAccountId);
    }
}
