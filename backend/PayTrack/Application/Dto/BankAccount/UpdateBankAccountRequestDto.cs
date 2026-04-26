// <copyright file="UpdateBankAccountRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.BankAccount
{
    /// <summary>
    /// DTO for updating a bank account.
    /// </summary>
    public sealed record class UpdateBankAccountRequestDto(
        [property: MinLength(3)]
        [property: MaxLength(255)]
        string? accountHolder,

        [property: MinLength(15)]
        [property: MaxLength(31)]
        string? iban,

        [property: MinLength(8)]
        [property: MaxLength(11)]
        string? bic);
}
