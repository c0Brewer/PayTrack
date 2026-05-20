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
        /// Returns all PaymentRequestByUsers visible to the currently authenticated user.
        /// </summary>
        /// <param name="query">Query object including all query options.</param>
        /// <param name="authService">Dependency-Injected Authentication Service.</param>
        /// <param name="paymentRequestByUserService">Dependency injected PaymentRequestByUser service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaginatedResponse<PaymentRequestByUserDto>>, BadRequest<ProblemDetails>, ProblemHttpResult>> GetPaymentRequestByUsersAsync(
            [AsParameters] GetPaymentRequestByUserQuery query,
            IAuthService authService,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var currentUser = await authService.GetCurrentUser() ?? throw new NotFoundException("Current user not found");

            if (!paymentRequestByUserService.ValidateQuery(query, currentUser))
            {
                throw new ForbiddenException("You do not have permission to access these invoices.");
            }

            var (paymentRequestByUserList, totalCount) = await paymentRequestByUserService.GetAllAsync(query);

            var paymentRequestByUserListDto = PaymentRequestByUserMapper.ListToDto(paymentRequestByUserList);

            var paginatedResponse = new PaginatedResponse<PaymentRequestByUserDto>(paymentRequestByUserListDto, totalCount, query.Limit ?? -1, query.Offset ?? 0);

            return TypedResults.Ok(paginatedResponse);
        }

        /// <summary>
        /// Returns a PaymentRequestByUser by ID, only if the current user has access to it.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="query">Query object including all query options.</param>
        /// <param name="authService">Dependency-Injected Authentication Service.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaymentRequestByUserDto>, BadRequest<ProblemDetails>, NotFound<ProblemDetails>, ProblemHttpResult>> GetPaymentRequestByUserByIdAsync(
            [FromRoute] int id,
            [AsParameters] GetPaymentRequestByUserQueryById query,
            IAuthService authService,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var currentUser = await authService.GetCurrentUser() ?? throw new NotFoundException("Current user not found");

            var paymentRequestByUser = await paymentRequestByUserService.GetPaymentRequestByUserByIdAsync(id, query) ?? throw new NotFoundException("PaymentRequestByUser could not be found");

            if (!paymentRequestByUserService.ValidateAccessToInvoice(paymentRequestByUser, currentUser))
            {
                throw new ForbiddenException("You do not have permission to access this invoice.");
            }

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
        /// Marks a PaymentRequestByUser as paid.
        /// </summary>
        /// <param name="id">Id of the PaymentRequestByUser to update.</param>
        /// <param name="markPaidDto">Payment completion data supplied by finance.</param>
        /// <param name="authService">Dependency-Injected Authentication Service.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaymentRequestByUserDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> MarkPaymentRequestByUserAsPaidAsync(
            [FromRoute] int id,
            [FromBody] MarkPaymentRequestByUserAsPaidDto markPaidDto,
            IAuthService authService,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var currentUser = await authService.GetCurrentUser() ?? throw new NotFoundException("Current user not found");
            var paymentDate = markPaidDto.PaymentDate ?? throw new InvalidStateException("Payment date is required");

            var updatedPaymentRequestByUser = await paymentRequestByUserService.MarkPaymentRequestByUserAsPaidAsync(
                id,
                currentUser.Id,
                markPaidDto.PaymentReference,
                markPaidDto.PurposeOfPayment,
                paymentDate);

            var updatedPaymentRequestByUserDto = PaymentRequestByUserMapper.ToDto(updatedPaymentRequestByUser);

            return TypedResults.Ok(updatedPaymentRequestByUserDto);
        }

        /// <summary>
        /// Approves a PaymentRequestByUser.
        /// </summary>
        /// <param name="id">Id of the PaymentRequestByUser to update.</param>
        /// <param name="approveDto">Approval data supplied by finance.</param>
        /// <param name="authService">Dependency-Injected Authentication Service.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaymentRequestByUserDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> ApprovePaymentRequestByUserAsync(
            [FromRoute] int id,
            [FromBody] ApprovePaymentRequestByUserDto approveDto,
            IAuthService authService,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var currentUser = await authService.GetCurrentUser() ?? throw new NotFoundException("Current user not found");

            var updatedPaymentRequestByUser = await paymentRequestByUserService.ApprovePaymentRequestByUserAsync(
                id,
                currentUser.Id,
                approveDto.CostCentreId,
                approveDto.Reason);

            var updatedPaymentRequestByUserDto = PaymentRequestByUserMapper.ToDto(updatedPaymentRequestByUser);

            return TypedResults.Ok(updatedPaymentRequestByUserDto);
        }

        /// <summary>
        /// Declines a PaymentRequestByUser.
        /// </summary>
        /// <param name="id">Id of the PaymentRequestByUser to update.</param>
        /// <param name="declineDto">Decline data supplied by finance.</param>
        /// <param name="authService">Dependency-Injected Authentication Service.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaymentRequestByUserDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> DeclinePaymentRequestByUserAsync(
            [FromRoute] int id,
            [FromBody] DeclinePaymentRequestByUserDto declineDto,
            IAuthService authService,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var currentUser = await authService.GetCurrentUser() ?? throw new NotFoundException("Current user not found");

            var updatedPaymentRequestByUser = await paymentRequestByUserService.DeclinePaymentRequestByUserAsync(
                id,
                currentUser.Id,
                declineDto.Reason);

            var updatedPaymentRequestByUserDto = PaymentRequestByUserMapper.ToDto(updatedPaymentRequestByUser);

            return TypedResults.Ok(updatedPaymentRequestByUserDto);
        }

        /// <summary>
        /// Requests changes for a PaymentRequestByUser.
        /// </summary>
        /// <param name="id">Id of the PaymentRequestByUser to update.</param>
        /// <param name="requestChangesDto">Change request data supplied by finance.</param>
        /// <param name="authService">Dependency-Injected Authentication Service.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaymentRequestByUserDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> RequestChangesPaymentRequestByUserAsync(
            [FromRoute] int id,
            [FromBody] RequestChangesPaymentRequestByUserDto requestChangesDto,
            IAuthService authService,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var currentUser = await authService.GetCurrentUser() ?? throw new NotFoundException("Current user not found");

            var updatedPaymentRequestByUser = await paymentRequestByUserService.RequestChangesPaymentRequestByUserAsync(
                id,
                currentUser.Id,
                requestChangesDto.Reason);

            var updatedPaymentRequestByUserDto = PaymentRequestByUserMapper.ToDto(updatedPaymentRequestByUser);

            return TypedResults.Ok(updatedPaymentRequestByUserDto);
        }

        /// <summary>
        /// Returns the receipt file for a PaymentRequestByUser, only if the current user has access to it.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="authService">Dependency-Injected Authentication Service.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<FileContentHttpResult, BadRequest<ProblemDetails>, ProblemHttpResult>> GetPaymentRequestByUserByIdReceiptAsync(
            [FromRoute] int id,
            IAuthService authService,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var currentUser = await authService.GetCurrentUser() ?? throw new NotFoundException("Current user not found");

            var invoice = await paymentRequestByUserService.GetPaymentRequestByUserByIdAsync(id, null) ?? throw new NotFoundException("PaymentRequestByUser could not be found");

            if (!paymentRequestByUserService.ValidateAccessToInvoice(invoice, currentUser))
            {
                throw new ForbiddenException("You do not have permission to access this invoice.");
            }

            var (file, contentType) = await paymentRequestByUserService.GetReceiptForPaymentRequestByUserByIdAsync(id);

            return TypedResults.File(file, contentType);
        }
    }
}
