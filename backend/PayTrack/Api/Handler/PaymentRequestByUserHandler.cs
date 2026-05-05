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
        /// <param name="authService">Dependency-Injected Authentication Service..</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaymentRequestByUserDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> CreatePaymentRequestByUserAsync(
            [FromForm] CreatePaymentRequestByUserDto createPaymentRequestByUserDto,
            IAuthService authService,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var user = await authService.GetCurrentUser() ?? throw new NotFoundException("Current User not found");

            var createdPaymentRequestByUser = await paymentRequestByUserService.CreatePaymentRequestByUserAsync(
                    user.Id,
                    createPaymentRequestByUserDto.Transaction.TeamId,
                    createPaymentRequestByUserDto.Transaction.Amount,
                    createPaymentRequestByUserDto.Transaction.PurposeOfPayment,
                    createPaymentRequestByUserDto.Receipt,
                    createPaymentRequestByUserDto.Transaction.PaidAt,
                    createPaymentRequestByUserDto.InvoiceNumber,
                    createPaymentRequestByUserDto.Comment,
                    createPaymentRequestByUserDto.PayoutType,
                    createPaymentRequestByUserDto.BankAccountId);

            var createdPaymentRequestByUserDto = PaymentRequestByUserMapper.ToDto(createdPaymentRequestByUser);

            return TypedResults.Ok(createdPaymentRequestByUserDto);
        }

        /// <summary>
        /// Checks possible duplicates for a PaymentRequestByUser.
        /// </summary>
        /// <param name="getDuplicatePaymentRequestsByUserDto">Data for duplicate check.</param>
        /// <param name="authService">Dependency-Injected Authentication Service.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<List<DuplicatePaymentRequestByUserDto>>, BadRequest<ProblemDetails>, ProblemHttpResult>> GetDuplicatePaymentRequestsByUserAsync(
            [AsParameters] GetDuplicatePaymentRequestsByUserDto getDuplicatePaymentRequestsByUserDto,
            IAuthService authService,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var user = await authService.GetCurrentUser() ?? throw new NotFoundException("Current User not found");

            var duplicatePaymentRequests = await paymentRequestByUserService.GetDuplicatePaymentRequestsByUserAsync(
                    user.Id,
                    getDuplicatePaymentRequestsByUserDto.TeamId,
                    getDuplicatePaymentRequestsByUserDto.Amount,
                    getDuplicatePaymentRequestsByUserDto.InvoiceNumber);

            var duplicatePaymentRequestsDto = PaymentRequestByUserMapper.DuplicateListToDto(duplicatePaymentRequests);

            return TypedResults.Ok(duplicatePaymentRequestsDto);
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
            var updatedPaymentRequestByUser = await paymentRequestByUserService.UpdatePaymentRequestByUserAsync(
                    id,
                    updatePaymentRequestByUserDto.Transaction.TeamId,
                    updatePaymentRequestByUserDto.Transaction.Amount,
                    updatePaymentRequestByUserDto.Transaction.PurposeOfPayment,
                    updatePaymentRequestByUserDto.Transaction.PaidAt,
                    updatePaymentRequestByUserDto.InvoiceNumber,
                    updatePaymentRequestByUserDto.Comment,
                    updatePaymentRequestByUserDto.PayoutType,
                    updatePaymentRequestByUserDto.BankAccountId);

            var updatedPaymentRequestByUserDto = PaymentRequestByUserMapper.ToDto(updatedPaymentRequestByUser);

            return TypedResults.Ok(updatedPaymentRequestByUserDto);
        }

        /// <summary>
        /// Returns a PaymentRequestByUser by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<FileContentHttpResult, BadRequest<ProblemDetails>, ProblemHttpResult>> GetPaymentRequestByUserByIdReceiptAsync(
            [FromRoute] int id,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var file = await paymentRequestByUserService.GetReceiptForPaymentRequestByUserByIdAsync(id) ?? throw new NotFoundException("Could not load file");

            return TypedResults.File(file, "application/octet-stream");
        }
    }
}
