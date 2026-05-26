// <copyright file="ITransactionRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Data.Entities;

namespace PayTrack.Data.Repositories.Model
{
    /// <summary>
    /// Repository for all Transaction-related operations.
    /// </summary>
    public interface ITransactionRepository
    {
        /// <summary>
        /// Gets all Transactions with optional filtering.
        /// </summary>
        /// <param name="query">Query information to include in search.</param>
        /// <returns>List of Transaction.</returns>
        Task<(List<Transaction> transaction, int totalCount)> GetAllAsync(GetTransactionQuery? query = null);

        /// <summary>
        /// Gets all Transactions with optional filtering.
        /// </summary>
        /// <param name="query">Query information to include in search.</param>
        /// <returns>List of Transaction.</returns>
        Task<(List<PaymentRequestByUser> transaction, int totalCount)> GetAllAsync(GetPaymentRequestByUserQuery? query = null);

        /// <summary>
        /// Gets a specific Transaction by their ID.
        /// </summary>
        /// <param name="id">id of Transaction to find.</param>
        /// <param name="query">Query information to include in search.</param>
        /// <returns>Transaction with given ID.</returns>
        Task<Transaction?> GetByIdAsync(
            int id,
            GetTransactionQueryById? query = null);

        /// <summary>
        /// Gets a specific Transaction by their ID.
        /// </summary>
        /// <param name="id">id of Transaction to find.</param>
        /// <param name="query">Query information to include in search.</param>
        /// <returns>Transaction with given ID.</returns>
        Task<PaymentRequestByUser?> GetByIdAsync(
            int id,
            GetPaymentRequestByUserQueryById? query = null);

        /// <summary>
        /// Stores a Transaction to the Database.
        /// </summary>
        /// <param name="transaction">Transaction object to store.</param>
        /// <returns>Instance of created Transaction object.</returns>
        Task<Transaction> AddAsync(Transaction transaction);

        /// <summary>
        /// Stores a Transaction to the Database.
        /// </summary>
        /// <param name="transaction">Transaction object to store.</param>
        /// <param name="receipt">Receipt to store with transaction.</param>
        /// <returns>Instance of created Transaction object.</returns>
        Task<PaymentRequestByUser> AddAsync(PaymentRequestByUser transaction, IFormFile receipt);

        /// <summary>
        /// Gets candidate PaymentRequestByUser entries for weighted duplicate scoring.
        /// </summary>
        /// <param name="userId">Current user id.</param>
        /// <param name="teamId">Team id.</param>
        /// <param name="amount">Amount.</param>
        /// <param name="paidAt">Paid-at day.</param>
        /// <param name="invoiceNumber">Optional invoice number.</param>
        /// <param name="paymentRequestByUserId">Optional source payment request id. Dismissed pairs with this id are excluded.</param>
        /// <returns>List of potential duplicate candidates.</returns>
        Task<List<PaymentRequestByUser>> GetPotentialDuplicatesAsync(
            int userId,
            int teamId,
            decimal amount,
            DateTime paidAt,
            string? invoiceNumber = null,
            int? paymentRequestByUserId = null);

        /// <summary>
        /// Updates a PaymentRequestByUser using the given input.
        /// </summary>
        /// <param name="transaction">Transaction object to update.</param>
        /// <returns>Instance of created PaymentRequestByUser object.</returns>
        Task<PaymentRequestByUser> UpdateAsync(PaymentRequestByUser transaction);

        /// <summary>
        /// Deletes a PaymentRequestByUser by id.
        /// </summary>
        /// <param name="id">Id of the PaymentRequestByUser to delete.</param>
        /// <returns><c>true</c> if an invoice was deleted; otherwise <c>false</c>.</returns>
        Task<bool> DeletePaymentRequestByUserAsync(int id);

        /// <summary>
        /// Stores that a potential duplicate pair has been reviewed and dismissed.
        /// </summary>
        /// <param name="paymentRequestByUserId">First PaymentRequestByUser id.</param>
        /// <param name="duplicatePaymentRequestByUserId">Second PaymentRequestByUser id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DismissDuplicatePaymentRequestByUserAsync(int paymentRequestByUserId, int duplicatePaymentRequestByUserId);
    }
}
