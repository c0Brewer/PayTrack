// <copyright file="BankAccountResponseDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.BankAccount
{
    /// <summary>
    /// DTO for returning bank account data.
    /// </summary>
    public sealed record class BankAccountResponseDto(
        int id,
        string accountHolder,
        string iban,
        string bic);
}
