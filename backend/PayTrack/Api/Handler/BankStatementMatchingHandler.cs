// <copyright file="BankStatementMatchingHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Api.Mapper;
using PayTrack.Application.Dto.BankStatement;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler which gets called from Endpoint to manage incoming PaymentRequestByUser-related requests.
    /// </summary>
    public static class BankStatementMatchingHandler
    {
        /// <summary>
        /// Returns the matches for a list of bank statements.
        /// </summary>
        /// <param name="bankStatements">list of bankstatements from json.</param>
        /// <param name="bankStatementMatchingService">bank statement matching service via DI.</param>
        /// <returns>list of match results.</returns>
        public static async Task<Results<Ok<BankStatementMatchResponseDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> GetBankStatementMatches(
            [FromBody] List<BankStatementEntryDto> bankStatements,
            IBankStatementMatchingService bankStatementMatchingService)
        {
            var bankStatementMatches = await bankStatementMatchingService.MatchBankStatementEntriesAsync(bankStatements);

            return TypedResults.Ok(bankStatementMatches);
        }

        /// <summary>
        /// Updates the matches for a list of bank statements.
        /// </summary>
        /// <param name="updatedBankStatements">list of bankstatements including changes from json.</param>
        /// <param name="authService">auth service via DI.</param>
        /// <param name="bankStatementMatchingService">bank statement matching service via DI.</param>
        /// <returns>list of match results.</returns>
        public static async Task<Results<Ok<List<TransactionDto>>, BadRequest<ProblemDetails>, ProblemHttpResult>> UpdateBankStatementMatches(
            [FromBody] List<BankStatementUpdateRequestDto> updatedBankStatements,
            IAuthService authService,
            IBankStatementMatchingService bankStatementMatchingService)
        {
            var user = await authService.GetCurrentUser() ?? throw new NotFoundException("Current User not found");

            var bankStatementMatches = await bankStatementMatchingService.UpdateBankStatementMatchesAsync(updatedBankStatements, user.Id);

            var updatedTransactions = TransactionMapper.ListToDto(bankStatementMatches);

            return TypedResults.Ok(updatedTransactions);
        }
    }
}
