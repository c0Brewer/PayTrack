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
        /// <param name="budgetId">Optional budget to assign.</param>
        /// <returns>Instance of created PaymentRequestByTeam object.</returns>
        Task<PaymentRequestByTeam> CreatePaymentRequestByTeamAsync(
            int userToAssignToId,
            int creatingUserId,
            int teamId,
            decimal amount,
            string purposeOfPayment,
            DateTime dueDate,
            int? budgetId = null);

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

        /// <summary>
        /// Marks a PaymentRequestByTeam as Paid and records the status change in history.
        /// </summary>
        /// <param name="id">The id of the PaymentRequestByTeam to mark as paid.</param>
        /// <param name="adminUserId">The id of the admin user performing the action.</param>
        /// <param name="comment">An optional comment to store with the status history entry.</param>
        /// <returns>The updated PaymentRequestByTeam.</returns>
        Task<PaymentRequestByTeam> MarkAsPaidAsync(int id, int adminUserId, string? comment);

        /// <summary>
        /// Validates that the supplied query parameters are permissible for the current user's role.
        /// </summary>
        /// <param name="query">The query submitted by the client.</param>
        /// <param name="currentUser">The currently authenticated user.</param>
        /// <returns><c>true</c> if the query is valid for the user's role; <c>false</c> otherwise.</returns>
        bool ValidateQuery(GetPaymentRequestByTeamQuery query, User currentUser);
    }
}
