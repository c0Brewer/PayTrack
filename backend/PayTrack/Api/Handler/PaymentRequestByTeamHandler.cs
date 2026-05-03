// <copyright file="PaymentRequestByTeamHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Api.Mapper;
using PayTrack.Application.Dto.Pagination;
using PayTrack.Application.Dto.PaymentRequestByTeam;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler which gets called from Endpoint to manage incoming PaymentRequestByTeam-related requests.
    /// </summary>
    public static class PaymentRequestByTeamHandler
    {
        /// <summary>
        /// Returns all PaymentRequestByTeams.
        /// </summary>
        /// <param name="query">Query object including all query options.</param>
        /// <param name="paymentRequestByTeamService">Dependency injected PaymentRequestByTeam service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaginatedResponse<PaymentRequestByTeamDto>>, BadRequest<ProblemDetails>, ProblemHttpResult>> GetPaymentRequestByTeamsAsync(
            [AsParameters] GetPaymentRequestByTeamQuery query,
            IPaymentRequestByTeamService paymentRequestByTeamService)
        {
            var (paymentRequestByTeamList, totalCount) = await paymentRequestByTeamService.GetAllAsync(query);

            var paymentRequestByTeamListDto = PaymentRequestByTeamMapper.ListToDto(paymentRequestByTeamList);

            var paginatedResponse = new PaginatedResponse<PaymentRequestByTeamDto>(paymentRequestByTeamListDto, totalCount, query.Limit ?? -1, query.Offset ?? 0);

            return TypedResults.Ok(paginatedResponse);
        }

        /// <summary>
        /// Returns a PaymentRequestByTeam by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="query">Query object including all query options.</param>
        /// <param name="paymentRequestByTeamService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaymentRequestByTeamDto>, BadRequest<ProblemDetails>, NotFound<ProblemDetails>, ProblemHttpResult>> GetPaymentRequestByTeamByIdAsync(
            [FromRoute] int id,
            [AsParameters] GetPaymentRequestByTeamQueryById query,
            IPaymentRequestByTeamService paymentRequestByTeamService)
        {
            var paymentRequestByTeam = await paymentRequestByTeamService.GetPaymentRequestByTeamByIdAsync(id, query) ?? throw new NotFoundException("PaymentRequestByTeam could not be found");

            var paymentRequestByTeamDto = PaymentRequestByTeamMapper.ToDto(paymentRequestByTeam);

            return TypedResults.Ok(paymentRequestByTeamDto);
        }

        /// <summary>
        /// Creates a PaymentRequestByTeam.
        /// </summary>
        /// <param name="createPaymentRequestByTeamDto">request for PaymentRequestByTeam creation.</param>
        /// <param name="authService">Dependency-Injected Authentication Service..</param>
        /// <param name="paymentRequestByTeamService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaymentRequestByTeamDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> CreatePaymentRequestByTeamAsync(
            [FromBody] CreatePaymentRequestByTeamDto createPaymentRequestByTeamDto,
            IAuthService authService,
            IPaymentRequestByTeamService paymentRequestByTeamService)
        {
            var user = await authService.GetCurrentUser() ?? throw new NotFoundException("Current User not found");

            var createdPaymentRequestByTeam = await paymentRequestByTeamService.CreatePaymentRequestByTeamAsync(
                    createPaymentRequestByTeamDto.UserToAssignToId,
                    user.Id,
                    createPaymentRequestByTeamDto.Transaction.TeamId,
                    createPaymentRequestByTeamDto.Transaction.Amount,
                    createPaymentRequestByTeamDto.Transaction.PurposeOfPayment);

            var createdPaymentRequestByTeamDto = PaymentRequestByTeamMapper.ToDto(createdPaymentRequestByTeam);

            return TypedResults.Ok(createdPaymentRequestByTeamDto);
        }

        /// <summary>
        /// Updates a PaymentRequestByTeam.
        /// </summary>
        /// <param name="id">Id of the PaymentRequestByTeam to update.</param>
        /// <param name="updatePaymentRequestByTeamDto">request for PaymentRequestByTeam creation.</param>
        /// <param name="paymentRequestByTeamService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaymentRequestByTeamDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> UpdatePaymentRequestByTeamAsync(
            [FromRoute] int id,
            [FromBody] UpdatePaymentRequestByTeamDto updatePaymentRequestByTeamDto,
            IPaymentRequestByTeamService paymentRequestByTeamService)
        {
            var updatedPaymentRequestByTeam = await paymentRequestByTeamService.UpdatePaymentRequestByTeamAsync(
                    id,
                    updatePaymentRequestByTeamDto.Transaction.TeamId,
                    updatePaymentRequestByTeamDto.Transaction.Amount,
                    updatePaymentRequestByTeamDto.Transaction.PurposeOfPayment,
                    updatePaymentRequestByTeamDto.Transaction.PaidAt);

            var updatedPaymentRequestByTeamDto = PaymentRequestByTeamMapper.ToDto(updatedPaymentRequestByTeam);

            return TypedResults.Ok(updatedPaymentRequestByTeamDto);
        }
    }
}
