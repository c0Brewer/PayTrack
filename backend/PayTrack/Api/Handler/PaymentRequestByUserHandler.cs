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

            var comment = string.IsNullOrWhiteSpace(createPaymentRequestByUserDto.Comment)
                ? null
                : createPaymentRequestByUserDto.Comment.Trim();

            var createdPaymentRequestByUser = await paymentRequestByUserService.CreatePaymentRequestByUserAsync(
                    user.Id,
                    createPaymentRequestByUserDto.Transaction.TeamId,
                    createPaymentRequestByUserDto.Transaction.Amount,
                    createPaymentRequestByUserDto.Transaction.PurposeOfPayment,
                    createPaymentRequestByUserDto.Receipt,
                    createPaymentRequestByUserDto.Transaction.PaidAt,
                    createPaymentRequestByUserDto.InvoiceNumber,
                    comment,
                    createPaymentRequestByUserDto.PayoutType,
                    createPaymentRequestByUserDto.BankAccountId,
                    createPaymentRequestByUserDto.CreditorName);

            var createdPaymentRequestByUserDto = PaymentRequestByUserMapper.ToDto(createdPaymentRequestByUser);

            return TypedResults.Ok(createdPaymentRequestByUserDto);
        }

        /// <summary>
        /// Extracts invoice data from a receipt without persisting the uploaded file.
        /// </summary>
        /// <param name="receipt">Receipt to inspect.</param>
        /// <param name="receiptExtractionService">Dependency-injected receipt extraction service.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The extracted invoice fields and their confidence values.</returns>
        public static async Task<Ok<ReceiptExtractionDto>> ExtractReceiptAsync(
            [FromForm] IFormFile receipt,
            IReceiptExtractionService receiptExtractionService,
            CancellationToken cancellationToken)
        {
            var result = await receiptExtractionService.ExtractAsync(receipt, cancellationToken);
            return TypedResults.Ok(result);
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

            if (getDuplicatePaymentRequestsByUserDto.PaymentRequestByUserId.HasValue)
            {
                var sourcePaymentRequest = await paymentRequestByUserService.GetPaymentRequestByUserByIdAsync(
                        getDuplicatePaymentRequestsByUserDto.PaymentRequestByUserId.Value,
                        null)
                    ?? throw new NotFoundException("PaymentRequestByUser could not be found");

                if (!paymentRequestByUserService.ValidateAccessToInvoice(sourcePaymentRequest, user))
                {
                    throw new ForbiddenException("You do not have permission to access this invoice.");
                }
            }

            var duplicatePaymentRequests = await paymentRequestByUserService.GetDuplicatePaymentRequestsByUserAsync(
                    user.Id,
                    getDuplicatePaymentRequestsByUserDto.TeamId,
                    getDuplicatePaymentRequestsByUserDto.Amount,
                    getDuplicatePaymentRequestsByUserDto.PaidAt,
                    getDuplicatePaymentRequestsByUserDto.InvoiceNumber,
                    getDuplicatePaymentRequestsByUserDto.PaymentRequestByUserId);

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
        /// Deletes a PaymentRequestByUser.
        /// </summary>
        /// <param name="id">Id of the PaymentRequestByUser to delete.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<NoContent, BadRequest<ProblemDetails>, NotFound<ProblemDetails>, ProblemHttpResult>> DeletePaymentRequestByUserAsync(
            [FromRoute] int id,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            await paymentRequestByUserService.DeletePaymentRequestByUserAsync(id);

            return TypedResults.NoContent();
        }

        /// <summary>
        /// Dismisses a duplicate warning between two PaymentRequestByUser entries.
        /// </summary>
        /// <param name="id">Source PaymentRequestByUser id.</param>
        /// <param name="duplicateId">Potential duplicate PaymentRequestByUser id.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<NoContent, BadRequest<ProblemDetails>, NotFound<ProblemDetails>, ProblemHttpResult>> DismissDuplicatePaymentRequestByUserAsync(
            [FromRoute] int id,
            [FromRoute] int duplicateId,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            await paymentRequestByUserService.DismissDuplicatePaymentRequestByUserAsync(id, duplicateId);

            return TypedResults.NoContent();
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
