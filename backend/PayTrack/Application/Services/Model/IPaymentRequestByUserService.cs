// <copyright file="IPaymentRequestByUserService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service which handles PaymentRequestByUser-related requests.
    /// </summary>
    public interface IPaymentRequestByUserService
    {
        /// <summary>
        /// Gets all PaymentRequestByUser with an optional offset and limit.
        /// </summary>
        /// <param name="query">Query information for search.</param>
        /// <returns>List of PaymentRequestByUser.</returns>
        Task<(List<PaymentRequestByUser> paymentRequestByUser, int totalCount)> GetAllAsync(GetPaymentRequestByUserQuery? query = null);

        /// <summary>
        /// Gets a specific PaymentRequestByUser by their ID.
        /// </summary>
        /// <param name="id">id of PaymentRequestByUser to find.</param>
        /// <param name="query">Query information for search.</param>
        /// <returns>PaymentRequestByUser with given id.</returns>
        Task<PaymentRequestByUser?> GetPaymentRequestByUserByIdAsync(int id, GetPaymentRequestByUserQueryById? query = null);

        /// <summary>
        /// Creates a PaymentRequestByUser using the given input.
        /// </summary>
        /// <param name="userId">Id of user.</param>
        /// <param name="teamId">Id of team.</param>
        /// <param name="amount">Amount.</param>
        /// <param name="purposeOfPayment">Purpose.</param>
        /// <param name="PaidAt">When the invoice was paid at.</param>
        /// <param name="invoiceNumber">InvoiceNumber of PaymentRequestByUser.</param>
        /// <param name="comment">Optional comment.</param>
        /// <param name="payoutType">payout type (to user or to external).</param>
        /// <param name="bankAccountId">id of bank account to use.</param>
        /// <returns>Instance of created PaymentRequestByUser object.</returns>
        Task<PaymentRequestByUser> CreatePaymentRequestByUserAsync(
            int userId,
            int teamId,
            decimal amount,
            string purposeOfPayment,
            DateTime PaidAt,
            string invoiceNumber,
            string? comment,
            PayoutType payoutType,
            int bankAccountId);

        /// <summary>
        /// Update a PaymentRequestByUser using the given input.
        /// </summary>
        /// <param name="id">The id of the PaymentRequestByUser to update.</param>
        /// <param name="invoiceNumber">The new invoice number that should be set for the PaymentRequestByUser.</param>
        /// <param name="comment">The new comment that should be set for the PaymentRequestByUser.</param>
        /// <param name="payoutType">The new payout type to assign the PaymentRequestByUser to.</param>
        /// <param name="bankAccountId">The new bank account id that the PaymentRequestByUser should be assigned.</param>
        /// <returns>Instance of created PaymentRequestByUser object.</returns>
        Task<PaymentRequestByUser> UpdatePaymentRequestByUserAsync(int id, string? invoiceNumber, string? comment = null, PayoutType? payoutType = null, int? bankAccountId = null);
    }
}
