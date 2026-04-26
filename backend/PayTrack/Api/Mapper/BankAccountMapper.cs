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
        /// Turns a BankAccount object into a BankAccountDto.
        /// </summary>
        /// <param name="bankAccount">Bank account to map.</param>
        /// <returns>BankAccountDto instance.</returns>
        public static BankAccountDto ToDto(BankAccount bankAccount)
        {
            return new BankAccountDto(
                bankAccount.Id,
                bankAccount.AccountHolder,
                bankAccount.Iban,
                bankAccount.Bic);
        }

        /// <summary>
        /// Turns a list of bank account objects into a list of BankAccountDto objects.
        /// </summary>
        /// <param name="bankAccounts">List of bank account objects.</param>
        /// <returns>List of BankAccountDto objects.</returns>
        public static List<BankAccountDto> ListToDto(List<BankAccount> bankAccounts)
        {
            return bankAccounts.ConvertAll(ToDto);
        }
    }
}
