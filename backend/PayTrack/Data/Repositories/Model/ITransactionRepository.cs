// <copyright file="ITransactionRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.PaymentRequestByTeam;
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
        /// Gets all Transactions with optional filtering.
        /// </summary>
        /// <param name="query">Query information to include in search.</param>
        /// <returns>List of Transaction.</returns>
        Task<(List<PaymentRequestByTeam> transaction, int totalCount)> GetAllAsync(GetPaymentRequestByTeamQuery? query = null);

        /// <summary>
        /// Gets a lightweight dashboard section projection for a user's invoice overview.
        /// </summary>
        /// <param name="userId">Current user id.</param>
        /// <param name="recentItemsLimit">Maximum number of recent items to return.</param>
        /// <returns>Dashboard section projection.</returns>
        Task<HomeDashboardSectionProjection> GetHomeDashboardInvoiceSectionAsync(int userId, int recentItemsLimit);

        /// <summary>
        /// Gets a lightweight dashboard section projection for a user's payment-request overview.
        /// </summary>
        /// <param name="userId">Current user id.</param>
        /// <param name="recentItemsLimit">Maximum number of recent items to return.</param>
        /// <returns>Dashboard section projection.</returns>
        Task<HomeDashboardSectionProjection> GetHomeDashboardPaymentRequestSectionAsync(int userId, int recentItemsLimit);

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
        /// Gets a specific Transaction by their ID.
        /// </summary>
        /// <param name="id">id of Transaction to find.</param>
        /// <param name="query">Query information to include in search.</param>
        /// <returns>Transaction with given ID.</returns>
        Task<PaymentRequestByTeam?> GetByIdAsync(
            int id,
            GetPaymentRequestByTeamQueryById? query = null);

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
        /// Stores a Transaction to the Database.
        /// </summary>
        /// <param name="transaction">Transaction object to store.</param>
        /// <returns>Instance of created Transaction object.</returns>
        Task<PaymentRequestByTeam> AddAsync(PaymentRequestByTeam transaction);

        /// <summary>
        /// Gets candidate PaymentRequestByUser entries for weighted duplicate scoring.
        /// </summary>
        /// <param name="userId">Current user id.</param>
        /// <param name="teamId">Team id.</param>
        /// <param name="amount">Amount.</param>
        /// <param name="paidAt">Paid-at day.</param>
        /// <param name="invoiceNumber">Optional invoice number.</param>
        /// <param name="paymentRequestByUserId">Optional source payment request id. Dismissed pairs with this id are excluded.</param>
        /// <param name="includeOtherUsers">Whether candidates from other users may be returned.</param>
        /// <returns>List of potential duplicate candidates.</returns>
        Task<List<PaymentRequestByUser>> GetPotentialDuplicatesAsync(
            int userId,
            int teamId,
            decimal amount,
            DateTime paidAt,
            string? invoiceNumber = null,
            int? paymentRequestByUserId = null,
            bool includeOtherUsers = false);

        /// <summary>
        /// Updates a PaymentRequestByUser using the given input.
        /// </summary>
        /// <param name="transaction">Transaction object to update.</param>
        /// <returns>Instance of created PaymentRequestByUser object.</returns>
        Task<Transaction> UpdateAsync(Transaction transaction);

        /// <summary>
        /// Updates a PaymentRequestByUser using the given input.
        /// </summary>
        /// <param name="transaction">Transaction object to update.</param>
        /// <returns>Instance of created PaymentRequestByUser object.</returns>
        Task<PaymentRequestByUser> UpdateAsync(PaymentRequestByUser transaction);

        /// <summary>
        /// Updates a PaymentRequestByTeam using the given input.
        /// </summary>
        /// <param name="transaction">Transaction object to update.</param>
        /// <returns>Instance of created PaymentRequestByTeam object.</returns>
        Task<PaymentRequestByTeam> UpdateAsync(PaymentRequestByTeam transaction);

        /// <summary>
        /// Atomically updates a PaymentRequestByTeam and persists a status history entry in a single SaveChanges call.
        /// </summary>
        /// <param name="transaction">The transaction to update.</param>
        /// <param name="history">The status history entry to add.</param>
        /// <returns>The updated transaction.</returns>
        Task<PaymentRequestByTeam> UpdateAndAddStatusHistoryAsync(PaymentRequestByTeam transaction, TransactionStatusHistory history);

        /// <summary>
        /// Returns all PaymentRequestByTeam entries whose due date falls on <paramref name="dueDate"/>
        /// and whose status is not Paid or Declined. Includes the User navigation property.
        /// </summary>
        /// <param name="dueDate">The date to match against DueDate (time portion is ignored).</param>
        /// <returns>List of matching PaymentRequestByTeam entries.</returns>
        Task<List<PaymentRequestByTeam>> GetPaymentRequestsByTeamDueOnAsync(DateTime dueDate);

        /// <summary>
        /// Deletes a PaymentRequestByUser by id.
        /// </summary>
        /// <param name="id">Id of the PaymentRequestByUser to delete.</param>
        /// <returns><c>true</c> if an invoice was deleted; otherwise <c>false</c>.</returns>
        Task<bool> DeletePaymentRequestByUserAsync(int id);

        /// <summary>
        /// Deletes a PaymentRequestByTeam by id.
        /// </summary>
        /// <param name="id">Id of the PaymentRequestByTeam to delete.</param>
        /// <returns><c>true</c> if an entry was deleted; otherwise <c>false</c>.</returns>
        Task<bool> DeletePaymentRequestByTeamAsync(int id);

        /// <summary>
        /// Stores that a potential duplicate pair has been reviewed and dismissed.
        /// </summary>
        /// <param name="paymentRequestByUserId">First PaymentRequestByUser id.</param>
        /// <param name="duplicatePaymentRequestByUserId">Second PaymentRequestByUser id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DismissDuplicatePaymentRequestByUserAsync(int paymentRequestByUserId, int duplicatePaymentRequestByUserId);
    }
}
