// <copyright file="IBankStatementMatchingService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.BankStatement;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service for matching bank statement entries against payment requests.
    /// </summary>
    public interface IBankStatementMatchingService
    {
        /// <summary>
        /// Matches bank statement entries against existing payment requests.
        /// </summary>
        /// <param name="entries">Bank statement entries to match.</param>
        /// <returns>Match results for each entry.</returns>
        Task<BankStatementMatchResponseDto> MatchBankStatementEntriesAsync(List<BankStatementEntryDto> entries);

        /// <summary>
        /// Updates matched transactions with status changes.
        /// </summary>
        /// <param name="updates">List of updates to apply.</param>
        /// <param name="changedById">Id of user who changed it.</param>
        /// <returns>List of updated transactions.</returns>
        Task<List<Transaction>> UpdateBankStatementMatchesAsync(List<BankStatementUpdateRequestDto> updates, int changedById);
    }
}
