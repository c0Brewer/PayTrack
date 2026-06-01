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
        /// Gets potential duplicate PaymentRequestByUser entries based on exact duplicate criteria.
        /// </summary>
        /// <param name="userId">Current user id.</param>
        /// <param name="teamId">Team id.</param>
        /// <param name="amount">Amount.</param>
        /// <returns>List of potential duplicates.</returns>
        Task<List<PaymentRequestByUser>> GetPotentialDuplicatesAsync(int userId, int teamId, decimal amount);

        /// <summary>
        /// Updates a PaymentRequestByUser using the given input.
        /// </summary>
        /// <param name="transaction">Transaction object to update.</param>
        /// <returns>Instance of created PaymentRequestByUser object.</returns>
        Task<PaymentRequestByUser> UpdateAsync(PaymentRequestByUser transaction);

        /// <summary>
        /// Updates a PaymentRequestByUser using the given input.
        /// </summary>
        /// <param name="transaction">Transaction object to update.</param>
        /// <returns>Instance of created PaymentRequestByUser object.</returns>
        Task<PaymentRequestByTeam> UpdateAsync(PaymentRequestByTeam transaction);

        /// <summary>
        /// Persists a new TransactionStatusHistory entry.
        /// </summary>
        /// <param name="history">The history entry to persist.</param>
        /// <returns>The persisted TransactionStatusHistory entry.</returns>
        Task<TransactionStatusHistory> AddStatusHistoryAsync(TransactionStatusHistory history);

        /// <summary>
        /// Atomically updates a PaymentRequestByTeam and persists a status history entry in a single SaveChanges call.
        /// </summary>
        /// <param name="transaction">The transaction to update.</param>
        /// <param name="history">The status history entry to add.</param>
        /// <returns>The updated transaction.</returns>
        Task<PaymentRequestByTeam> UpdateAndAddStatusHistoryAsync(PaymentRequestByTeam transaction, TransactionStatusHistory history);
    }
}
