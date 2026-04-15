// <copyright file="BankAccountDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.BankAccount
{
    /// <summary>
    /// Dto containing necessary information about a Bank account
    /// </summary>
    public sealed record class BankAccountDto(
        [property: Required]
        int Id,

        [property: Required]
        string IBAN,

        [property: Required]
        string BIC,

        [property: Required]
        string AccountHolder);
}
