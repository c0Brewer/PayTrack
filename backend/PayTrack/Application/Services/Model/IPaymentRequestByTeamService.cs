// <copyright file="IPaymentRequestByTeamService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.PaymentRequestByTeam;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service which handles PaymentRequestByTeam-related requests.
    /// </summary>
    public interface IPaymentRequestByTeamService
    {
        /// <summary>
        /// Gets all PaymentRequestByTeam with an optional offset and limit.
        /// </summary>
        /// <param name="query">Query information for search.</param>
        /// <returns>List of PaymentRequestByTeam.</returns>
        Task<(List<PaymentRequestByTeam> paymentRequestByTeam, int totalCount)> GetAllAsync(GetPaymentRequestByTeamQuery? query = null);

        /// <summary>
        /// Gets a specific PaymentRequestByTeam by their ID.
        /// </summary>
        /// <param name="id">id of PaymentRequestByTeam to find.</param>
        /// <param name="query">Query information for search.</param>
        /// <returns>PaymentRequestByTeam with given id.</returns>
        Task<PaymentRequestByTeam?> GetPaymentRequestByTeamByIdAsync(int id, GetPaymentRequestByTeamQueryById? query = null);

        /// <summary>
        /// Creates a PaymentRequestByTeam using the given input.
        /// </summary>
        /// <param name="userToAssignToId">Id of user who the payment gets assigned to.</param>
        /// <param name="creatingUserId">id of user who creates the payment.</param>
        /// <param name="teamId">Id of team.</param>
        /// <param name="amount">Amount.</param>
        /// <param name="purposeOfPayment">Purpose.</param>
        /// <param name="dueDate">Due date requested by the finance team.</param>
        /// <param name="costCentreId">Optional cost centre to assign.</param>
        /// <returns>Instance of created PaymentRequestByTeam object.</returns>
        Task<PaymentRequestByTeam> CreatePaymentRequestByTeamAsync(
            int userToAssignToId,
            int creatingUserId,
            int teamId,
            decimal amount,
            string purposeOfPayment,
            DateTime dueDate,
            int? costCentreId = null);

        /// <summary>
        /// Update a PaymentRequestByTeam using the given input.
        /// </summary>
        /// <param name="id">The id of the PaymentRequestByTeam to update.</param>
        /// <param name="teamId">The teamid of the PaymentRequestByTeam to update.</param>
        /// <param name="amount">The amount of the PaymentRequestByTeam to update.</param>
        /// <param name="purposeOfPayment">The purposeOfPayment of the PaymentRequestByTeam to update.</param>
        /// <param name="paidAt">The paidat of the PaymentRequestByTeam to update.</param>
        /// <returns>Instance of created PaymentRequestByTeam object.</returns>
        Task<PaymentRequestByTeam> UpdatePaymentRequestByTeamAsync(
            int id,
            int? teamId = null,
            decimal? amount = null,
            string? purposeOfPayment = null,
            DateTime? paidAt = null);
    }
}
