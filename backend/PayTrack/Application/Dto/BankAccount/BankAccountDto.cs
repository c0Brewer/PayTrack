// <copyright file="BankAccountDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.BankAccount
{
    /// <summary>
    /// DTO containing bank account information.
    /// </summary>
    public sealed record class BankAccountDto(
        [property: Required]
        int Id,

        [property: Required]
        string AccountHolder,

        [property: Required]
        string Iban,

        [property: Required]
        string Bic);
}
