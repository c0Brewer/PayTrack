// <copyright file="MyInvoicesHandler.cs" company="PayTrack">
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
    /// Handler which gets called from Endpoint to manage requests for a user's own invoices.
    /// </summary>
    public static class MyInvoicesHandler
    {
        /// <summary>
        /// Returns all invoices belonging to the currently authenticated user.
        /// UserId and IncludeCostCentre are forced server-side and cannot be overridden by the client.
        /// </summary>
        /// <param name="query">Query object including filter and pagination options.</param>
        /// <param name="authService">Dependency-Injected Authentication Service.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected PaymentRequestByUser service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaginatedResponse<PaymentRequestByUserDto>>, BadRequest<ProblemDetails>, ProblemHttpResult>> GetMyInvoicesAsync(
            [AsParameters] GetPaymentRequestByUserQuery query,
            IAuthService authService,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var currentUser = await authService.GetCurrentUser() ?? throw new NotFoundException("Current user not found");

            var forcedQuery = new GetPaymentRequestByUserQuery
            {
                UserId = currentUser.Id,
                IncludeCostCentre = false,
                IncludeTeam = query.IncludeTeam ?? true,
                IncludeStatusHistory = query.IncludeStatusHistory,
                MinAmount = query.MinAmount,
                MaxAmount = query.MaxAmount,
                PurposeOfPayment = query.PurposeOfPayment,
                Status = query.Status,
                TeamId = query.TeamId,
                MinCreatedAt = query.MinCreatedAt,
                MaxCreatedAt = query.MaxCreatedAt,
                Limit = query.Limit,
                Offset = query.Offset,
                InvoiceNumber = query.InvoiceNumber,
                BankAccountId = query.BankAccountId,
                IncludeBankAccount = query.IncludeBankAccount,
            };

            forcedQuery.PayoutType = query.PayoutType;

            var (list, totalCount) = await paymentRequestByUserService.GetAllAsync(forcedQuery);
            var dtos = PaymentRequestByUserMapper.ListToDto(list);
            var response = new PaginatedResponse<PaymentRequestByUserDto>(dtos, totalCount, forcedQuery.Limit ?? -1, forcedQuery.Offset ?? 0);

            return TypedResults.Ok(response);
        }

        /// <summary>
        /// Returns a specific invoice by ID, only if it belongs to the currently authenticated user.
        /// </summary>
        /// <param name="id">Id of the invoice.</param>
        /// <param name="query">Query object including options for related data.</param>
        /// <param name="authService">Dependency-Injected Authentication Service.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaymentRequestByUserDto>, ForbidHttpResult, BadRequest<ProblemDetails>, NotFound<ProblemDetails>, ProblemHttpResult>> GetMyInvoiceByIdAsync(
            [FromRoute] int id,
            [AsParameters] GetPaymentRequestByUserQueryById query,
            IAuthService authService,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var currentUser = await authService.GetCurrentUser() ?? throw new NotFoundException("Current user not found");

            var invoice = await paymentRequestByUserService.GetPaymentRequestByUserByIdAsync(id, query)
                ?? throw new NotFoundException("Invoice not found");

            if (invoice.UserId != currentUser.Id)
            {
                return TypedResults.Forbid();
            }

            return TypedResults.Ok(PaymentRequestByUserMapper.ToDto(invoice));
        }

        /// <summary>
        /// Returns the receipt file for a specific invoice, only if it belongs to the currently authenticated user.
        /// </summary>
        /// <param name="id">Id of the invoice.</param>
        /// <param name="authService">Dependency-Injected Authentication Service.</param>
        /// <param name="paymentRequestByUserService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<FileContentHttpResult, ForbidHttpResult, BadRequest<ProblemDetails>, ProblemHttpResult>> GetMyInvoiceReceiptAsync(
            [FromRoute] int id,
            IAuthService authService,
            IPaymentRequestByUserService paymentRequestByUserService)
        {
            var currentUser = await authService.GetCurrentUser() ?? throw new NotFoundException("Current user not found");

            var invoice = await paymentRequestByUserService.GetPaymentRequestByUserByIdAsync(id, null)
                ?? throw new NotFoundException("Invoice not found");

            if (invoice.UserId != currentUser.Id)
            {
                return TypedResults.Forbid();
            }

            var file = await paymentRequestByUserService.GetReceiptForPaymentRequestByUserByIdAsync(id) ?? throw new NotFoundException("Could not load file");

            return TypedResults.File(file, "application/octet-stream");
        }
    }
}
