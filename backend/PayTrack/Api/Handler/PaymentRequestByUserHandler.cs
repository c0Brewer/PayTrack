// <copyright file="PaymentRequestByUserHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Api.Mapper;
using PayTrack.Application.Dto.Pagination;
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler which gets called from Endpoint to manage incoming PaymentRequestByUser-related requests.
    /// </summary>
    public static class PaymentRequestByUserHandler
    {
        /// <summary>
        /// Returns all PaymentRequestByUsers.
        /// </summary>
        /// <param name="query">Query object including all query options.</param>
        /// <param name="paymentRequestByUserService">Dependency injected PaymentRequestByUser service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaginatedResponse<PaymentRequestByUserDto>>, BadRequest<ProblemDetails>, ProblemHttpResult>> GetPaymentRequestByUsersAsync(
            [AsParameters] GetPaymentRequestByUserQuery query,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var (paymentRequestByUserList, totalCount) = await paymentRequestByUserService.GetAllAsync(query);

            var paymentRequestByUserListDto = PaymentRequestByUserMapper.ListToDto(paymentRequestByUserList);

            var paginatedResponse = new PaginatedResponse<PaymentRequestByUserDto>(paymentRequestByUserListDto, totalCount, query.Limit ?? -1, query.Offset ?? 0);

            return TypedResults.Ok(paginatedResponse);
        }

        /// <summary>
        /// Returns a PaymentRequestByUser by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="query">Query object including all query options.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaymentRequestByUserDto>, BadRequest<ProblemDetails>, NotFound<ProblemDetails>, ProblemHttpResult>> GetPaymentRequestByUserByIdAsync(
            [FromRoute] int id,
            [AsParameters] GetPaymentRequestByUserQueryById query,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var paymentRequestByUser = await paymentRequestByUserService.GetPaymentRequestByUserByIdAsync(id, query) ?? throw new NotFoundException("PaymentRequestByUser could not be found");

            var paymentRequestByUserDto = PaymentRequestByUserMapper.ToDto(paymentRequestByUser);

            return TypedResults.Ok(paymentRequestByUserDto);
        }

        /// <summary>
        /// Creates a PaymentRequestByUser.
        /// </summary>
        /// <param name="createPaymentRequestByUserDto">request for PaymentRequestByUser creation.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaymentRequestByUserDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> CreatePaymentRequestByUserAsync(
            [FromBody] CreatePaymentRequestByUserDto createPaymentRequestByUserDto,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var updatedPaymentRequestByUser = await paymentRequestByUserService.CreatePaymentRequestByUserAsync(
                    createPaymentRequestByUserDto.Transaction.UserId,
                    createPaymentRequestByUserDto.Transaction.TeamId,
                    createPaymentRequestByUserDto.Transaction.Amount,
                    createPaymentRequestByUserDto.Transaction.PurposeOfPayment,
                    createPaymentRequestByUserDto.Transaction.PaidAt,
                    createPaymentRequestByUserDto.InvoiceNumber,
                    createPaymentRequestByUserDto.Comment,
                    createPaymentRequestByUserDto.PayoutType,
                    createPaymentRequestByUserDto.BankAccountId);

            var updatedPaymentRequestByUserDto = PaymentRequestByUserMapper.ToDto(updatedPaymentRequestByUser);

            // TODO: return created
            return TypedResults.Ok(updatedPaymentRequestByUserDto);
        }

        /// <summary>
        /// Updates a PaymentRequestByUser.
        /// </summary>
        /// <param name="id">Id of the PaymentRequestByUser to update.</param>
        /// <param name="updatePaymentRequestByUserDto">request for PaymentRequestByUser creation.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaymentRequestByUserDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> UpdatePaymentRequestByUserAsync(
            [FromRoute] int id,
            [FromBody] UpdatePaymentRequestByUserDto updatePaymentRequestByUserDto,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            // TODO: Add missing parameters for transaction update here as well
            var updatedPaymentRequestByUser = await paymentRequestByUserService.UpdatePaymentRequestByUserAsync(
                    id,
                    updatePaymentRequestByUserDto.InvoiceNumber,
                    updatePaymentRequestByUserDto.Comment,
                    updatePaymentRequestByUserDto.PayoutType,
                    updatePaymentRequestByUserDto.BankAccountId);

            var updatedPaymentRequestByUserDto = PaymentRequestByUserMapper.ToDto(updatedPaymentRequestByUser);

            // TODO: return updated
            return TypedResults.Ok(updatedPaymentRequestByUserDto);
        }
    }
}
