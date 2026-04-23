// <copyright file="BankAccountHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Api.Mapper;
using PayTrack.Application.Dto.BankAccount;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler which gets called from Endpoint to manage incoming BankAccount-related requests.
    /// </summary>
    public static class BankAccountHandler
    {
        /// <summary>
        /// Returns all bank accounts of the currently signed in user.
        /// </summary>
        /// <param name="authService">Service for resolving the currently signed in user.</param>
        /// <param name="bankAccountService">Service for reading persisted bank accounts.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<BankAccountsResponseDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> GetBankAccountsAsync(
            IAuthService authService,
            IBankAccountService bankAccountService)
        {
            var user = await authService.GetCurrentUser() ?? throw new NotFoundException("Current User not found");

            var userWithBankAccounts = await bankAccountService.GetUserWithBankAccountsAsync(user.Email);

            var responseDto = BankAccountMapper.ToOverviewDto(userWithBankAccounts);

            return TypedResults.Ok(responseDto);
        }

        /// <summary>
        /// Creates a new bank account for the currently signed in user.
        /// </summary>
        /// <param name="createDto">Request for bank account creation.</param>
        /// <param name="authService">Service for resolving the currently signed in user.</param>
        /// <param name="bankAccountService">Service for writing bank accounts.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<BankAccountResponseDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> CreateBankAccountAsync(
            [FromBody] BankAccountRequestDto createDto,
            IAuthService authService,
            IBankAccountService bankAccountService)
        {
            var user = await authService.GetCurrentUser() ?? throw new NotFoundException("Current User not found");

            var createdBankAccount = await bankAccountService.CreateBankAccountAsync(
                user.Email,
                createDto.accountHolder,
                createDto.iban,
                createDto.bic);

            var responseDto = BankAccountMapper.ToDto(createdBankAccount);

            return TypedResults.Ok(responseDto);
        }

        /// <summary>
        /// Updates a bank account of the currently signed in user.
        /// </summary>
        /// <param name="id">Id of the bank account to update.</param>
        /// <param name="updateDto">Request for bank account update.</param>
        /// <param name="authService">Service for resolving the currently signed in user.</param>
        /// <param name="bankAccountService">Service for writing bank accounts.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<BankAccountResponseDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> UpdateBankAccountAsync(
            [FromRoute] int id,
            [FromBody] BankAccountRequestDto updateDto,
            IAuthService authService,
            IBankAccountService bankAccountService)
        {
            var user = await authService.GetCurrentUser() ?? throw new NotFoundException("Current User not found");

            var updatedBankAccount = await bankAccountService.UpdateBankAccountAsync(
                user.Email,
                id,
                updateDto.accountHolder,
                updateDto.iban,
                updateDto.bic);

            var responseDto = BankAccountMapper.ToDto(updatedBankAccount);

            return TypedResults.Ok(responseDto);
        }

        /// <summary>
        /// Deletes a bank account of the currently signed in user.
        /// </summary>
        /// <param name="id">Id of the bank account to delete.</param>
        /// <param name="authService">Service for resolving the currently signed in user.</param>
        /// <param name="bankAccountService">Service for deleting bank accounts.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<NoContent, BadRequest<ProblemDetails>, ProblemHttpResult>> DeleteBankAccountAsync(
            [FromRoute] int id,
            IAuthService authService,
            IBankAccountService bankAccountService)
        {
            var user = await authService.GetCurrentUser() ?? throw new NotFoundException("Current User not found");

            await bankAccountService.DeleteBankAccountAsync(user.Email, id);

            return TypedResults.NoContent();
        }
    }
}
