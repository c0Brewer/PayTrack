// <copyright file="BankAccountService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class BankAccountService(IBankAccountRepository repo, IUserRepository userRepository) : IBankAccountService
    {
        private static readonly Regex BicRegex = new Regex("^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$", RegexOptions.Compiled);

        /// <summary>
        /// Repository for bank accounts.
        /// </summary>
        private readonly IBankAccountRepository repo = repo;

        /// <summary>
        /// Repository for users.
        /// </summary>
        private readonly IUserRepository userRepository = userRepository;

        /// <inheritdoc/>
        public async Task<BankAccount> CreateBankAccountAsync(int userId, string accountHolder, string iban, string bic)
        {
            var normalizedAccountHolder = this.NormalizeAccountHolder(accountHolder);
            var normalizedIban = this.Normalize(iban);
            var normalizedBic = this.Normalize(bic);

            this.ValidateBankAccountInput(normalizedAccountHolder, normalizedIban, normalizedBic);

            var bankAccounts = await this.GetUserBankAccountsOrThrowAsync(userId);

            if (bankAccounts.Any(existingBankAccount => existingBankAccount.Iban == normalizedIban))
            {
                throw new InvalidStateException("Bank account with the same IBAN already exists for this user");
            }

            var bankAccount = new BankAccount
            {
                UserId = userId,
                AccountHolder = normalizedAccountHolder,
                Iban = normalizedIban,
                Bic = normalizedBic,
            };

            return await this.repo.AddAsync(bankAccount);
        }

        /// <inheritdoc/>
        public async Task<BankAccount> CreateBankAccountOnboardingAsync(int userId, string accountHolder, string iban, string bic)
        {
            var bankAccount = await this.CreateBankAccountAsync(userId, accountHolder, iban, bic);
            await this.userRepository.UpdateBankInformationSkippedAsync(userId, false);
            return bankAccount;
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
            var normalizedAccountHolder = accountHolder is null ? null : this.NormalizeAccountHolder(accountHolder);
            var normalizedIban = iban is null ? null : this.Normalize(iban);
            var normalizedBic = bic is null ? null : this.Normalize(bic);

            if (normalizedAccountHolder is not null && string.IsNullOrWhiteSpace(normalizedAccountHolder))
            {
                throw new InvalidStateException("Account holder must not be empty.");
            }

            if (normalizedIban is not null && !this.IsValidIban(normalizedIban))
            {
                throw new InvalidStateException("IBAN is invalid.");
            }

            if (normalizedBic is not null && !BicRegex.IsMatch(normalizedBic))
            {
                throw new InvalidStateException("BIC is invalid.");
            }

            return await this.repo.UpdateAsync(userId, bankAccountId, normalizedAccountHolder, normalizedIban, normalizedBic);
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

        private void ValidateBankAccountInput(string accountHolder, string iban, string bic)
        {
            if (string.IsNullOrWhiteSpace(accountHolder))
            {
                throw new InvalidStateException("Account holder must not be empty.");
            }

            if (!this.IsValidIban(iban))
            {
                throw new InvalidStateException("IBAN is invalid.");
            }

            if (!BicRegex.IsMatch(bic))
            {
                throw new InvalidStateException("BIC is invalid.");
            }
        }

        private string Normalize(string value)
        {
            return value.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        }

        private string NormalizeAccountHolder(string value)
        {
            return value.Trim();
        }

        private bool IsValidIban(string iban)
        {
            if (iban.Length < 15 || iban.Length > 34 || !iban.All(char.IsLetterOrDigit))
            {
                return false;
            }

            var rearranged = iban[4..] + iban[..4];
            var remainder = 0;

            foreach (var character in rearranged)
            {
                if (char.IsDigit(character))
                {
                    remainder = ((remainder * 10) + (character - '0')) % 97;
                    continue;
                }

                if (character < 'A' || character > 'Z')
                {
                    return false;
                }

                var value = character - 'A' + 10;
                remainder = ((remainder * 100) + value) % 97;
            }

            return remainder == 1;
        }
    }
}
