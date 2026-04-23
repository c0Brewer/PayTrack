// <copyright file="BankAccountMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.BankAccount;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Mapper
{
    /// <summary>
    /// Mapper for BankAccount.
    /// </summary>
    public static class BankAccountMapper
    {
        /// <summary>
        /// Turns a BankAccount object into a BankAccountResponseDto.
        /// </summary>
        /// <param name="bankAccount">Bank account to map.</param>
        /// <returns>BankAccountResponseDto instance.</returns>
        public static BankAccountResponseDto ToDto(BankAccount bankAccount)
        {
            return new BankAccountResponseDto(
                bankAccount.Id,
                bankAccount.AccountHolder,
                bankAccount.Iban,
                bankAccount.Bic);
        }

        /// <summary>
        /// Turns a User with linked BankAccounts into a BankAccountsResponseDto.
        /// </summary>
        /// <param name="user">User with bank accounts loaded.</param>
        /// <returns>BankAccountsResponseDto instance.</returns>
        public static BankAccountsResponseDto ToOverviewDto(User user)
        {
            return new BankAccountsResponseDto(
                user.BankAccounts
                    .Select(ToDto)
                    .ToList());
        }
    }
}
