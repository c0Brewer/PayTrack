// <copyright file="BankAccountsResponseDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.BankAccount
{
    /// <summary>
    /// DTO for returning all bank accounts of the current user.
    /// </summary>
    public sealed record class BankAccountsResponseDto(
        List<BankAccountResponseDto> bankAccounts);
}
