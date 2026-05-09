// <copyright file="TransactionRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Exceptions;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class TransactionRepository(AppDbContext _context, IFileRepository _fileRepository) : ITransactionRepository
    {
        /// <summary>
        /// Database Context for accessing the DB.
        /// </summary>
        private readonly AppDbContext context = _context;
        private readonly IFileRepository fileRepository = _fileRepository;

        /// <inheritdoc/>
        public async Task<(List<Transaction> transaction, int totalCount)> GetAllAsync(GetTransactionQuery? query = null)
        {
            IQueryable<Transaction> dbQuery = this.context.Transactions.AsQueryable();

            dbQuery = ApplyBasePreFilters(dbQuery, query);

            // Calculate total count before limit / offset
            var totalCount = await dbQuery.CountAsync();

            dbQuery = ApplyBasePostFilters(dbQuery, query);

            // Could potentially add other ordering logic here as well
            var items = await dbQuery.OrderByDescending(t => t.CreatedAt).ToListAsync();

            return (items, totalCount);
        }

        /// <inheritdoc/>
        public async Task<(List<PaymentRequestByUser> transaction, int totalCount)> GetAllAsync(GetPaymentRequestByUserQuery? query = null)
        {
            IQueryable<PaymentRequestByUser> dbQuery = this.context.PaymentRequestsByUser.AsQueryable();

            dbQuery = ApplyBasePreFilters(dbQuery, query);

            if (!string.IsNullOrWhiteSpace(query?.InvoiceNumber))
            {
                dbQuery = dbQuery.Where(t => EF.Functions.Like(t.InvoiceNumber, $"%{query.InvoiceNumber}%"));
            }

            if (query?.PayoutType.HasValue == true)
            {
                dbQuery = dbQuery.Where(t => t.PayoutType == query.PayoutType.Value);
            }

            if (query?.BankAccountId.HasValue == true)
            {
                dbQuery = dbQuery.Where(t => t.BankAccountId == query.BankAccountId.Value);
            }

            // Calculate total count before limit / offset
            var totalCount = await dbQuery.CountAsync();

            dbQuery = ApplyBasePostFilters(dbQuery, query);

            if (query?.IncludeBankAccount.HasValue == true)
            {
                dbQuery = dbQuery.Include(t => t.BankAccount);
            }

            // Could potentially add other ordering logic here as well
            var items = await dbQuery.OrderByDescending(t => t.CreatedAt).ToListAsync();

            return (items, totalCount);
        }

        /// <inheritdoc/>
        public Task<Transaction?> GetByIdAsync(int id, GetTransactionQueryById? query = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser?> GetByIdAsync(int id, GetPaymentRequestByUserQueryById? query = null)
        {
            IQueryable<PaymentRequestByUser> dbQuery = this.context.PaymentRequestsByUser.AsQueryable();

            if (query?.IncludeCostCentre == true)
            {
                dbQuery = dbQuery.Include(t => t.CostCentre);
            }

            if (query?.IncludeUser == true)
            {
                dbQuery = dbQuery.Include(t => t.User);
            }

            if (query?.IncludeTeam == true)
            {
                dbQuery = dbQuery.Include(t => t.Team);
            }

            if (query?.IncludeStatusHistory == true)
            {
                dbQuery = dbQuery.Include(t => t.StatusHistory)
                    .ThenInclude(h => h.ChangedBy);
            }

            if (query?.IncludeBankAccount == true)
            {
                dbQuery = dbQuery.Include(t => t.BankAccount);
            }

            return await dbQuery.FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Transaction> AddAsync(Transaction transaction)
        {
            this.context.Transactions.Add(transaction);
            int res = await this.context.SaveChangesAsync();

            if (res != 1)
            {
                throw new InternalErrorException($"Saving Transaction did not end as expected. Saved {res} teams.");
            }

            return transaction;
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> AddAsync(PaymentRequestByUser transaction, IFormFile receipt)
        {
            transaction.ReceiptUrl = await this.fileRepository.SaveFile(
                receipt,
                $"invoice_{transaction.InvoiceNumber}_{transaction.CreatedAt:yyyyMMdd_HHmmss}");

            this.context.PaymentRequestsByUser.Add(transaction);
            int res = await this.context.SaveChangesAsync();

            if (res != 1)
            {
                throw new InternalErrorException($"Saving Transaction did not end as expected. Saved {res} teams.");
            }

            return transaction;
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> UpdateAsync(PaymentRequestByUser transaction)
        {
            this.context.PaymentRequestsByUser.Update(transaction);
            int res = await this.context.SaveChangesAsync();

            if (res < 1)
            {
                throw new InternalErrorException($"Updating Transaction did not end as expected. Saved {res} teams.");
            }

            return transaction;
        }

        private static IQueryable<T> ApplyBasePreFilters<T>(IQueryable<T> dbQuery, GetTransactionQuery? query)
            where T : Transaction
        {
            if (query?.UserId.HasValue == true)
            {
                dbQuery = dbQuery.Where(t => t.UserId == query.UserId.Value);
            }

            if (query?.MinAmount.HasValue == true)
            {
                dbQuery = dbQuery.Where(t => t.Amount >= query.MinAmount.Value);
            }

            if (query?.MaxAmount.HasValue == true)
            {
                dbQuery = dbQuery.Where(t => t.Amount <= query.MaxAmount.Value);
            }

            if (!string.IsNullOrWhiteSpace(query?.PurposeOfPayment))
            {
                dbQuery = dbQuery.Where(t => EF.Functions.Like(t.PurposeOfPayment, $"%{query.PurposeOfPayment}%"));
            }

            if (!string.IsNullOrWhiteSpace(query?.PaymentReference))
            {
                dbQuery = dbQuery.Where(t => EF.Functions.Like(t.PaymentReference, $"%{query.PaymentReference}%"));
            }

            if (query?.Status.HasValue == true)
            {
                dbQuery = dbQuery.Where(t => t.Status == query.Status.Value);
            }

            if (query?.CostCentreId.HasValue == true)
            {
                dbQuery = dbQuery.Where(t => t.CostCentreId == query.CostCentreId.Value);
            }

            if (query?.TeamId.HasValue == true)
            {
                dbQuery = dbQuery.Where(t => t.TeamId == query.TeamId.Value);
            }

            if (query?.PaymentDirection.HasValue == true)
            {
                dbQuery = dbQuery.Where(t => t.PaymentDirection == query.PaymentDirection.Value);
            }

            if (query?.MinCreatedAt.HasValue == true)
            {
                var minCreatedAt = DateTime.SpecifyKind(query.MinCreatedAt.Value, DateTimeKind.Utc);
                dbQuery = dbQuery.Where(t => t.CreatedAt >= minCreatedAt);
            }

            if (query?.MaxCreatedAt.HasValue == true)
            {
                var maxCreatedAt = DateTime.SpecifyKind(query.MaxCreatedAt.Value.Date.AddDays(1), DateTimeKind.Utc);
                dbQuery = dbQuery.Where(t => t.CreatedAt < maxCreatedAt);
            }

            if (query?.MinPaidAt.HasValue == true)
            {
                var minPaidAt = DateTime.SpecifyKind(query.MinPaidAt.Value, DateTimeKind.Utc);
                dbQuery = dbQuery.Where(t => t.PaidAt >= minPaidAt);
            }

            if (query?.MaxPaidAt.HasValue == true)
            {
                var maxPaidAt = DateTime.SpecifyKind(query.MaxPaidAt.Value.Date.AddDays(1), DateTimeKind.Utc);
                dbQuery = dbQuery.Where(t => t.PaidAt < maxPaidAt);
            }

            return dbQuery;
        }

        private static IQueryable<T> ApplyBasePostFilters<T>(IQueryable<T> dbQuery, GetTransactionQuery? query)
            where T : Transaction
        {
            if (query?.Offset.HasValue == true)
            {
                dbQuery = dbQuery.Skip(query.Offset.Value);
            }

            if (query?.Limit.HasValue == true)
            {
                dbQuery = dbQuery.Take(query.Limit.Value);
            }

            if (query?.IncludeCostCentre.HasValue == true)
            {
                dbQuery = dbQuery.Include(t => t.CostCentre);
            }

            if (query?.IncludeTeam.HasValue == true)
            {
                dbQuery = dbQuery.Include(t => t.Team);
            }

            if (query?.IncludeStatusHistory.HasValue == true)
            {
                dbQuery = dbQuery.Include(t => t.StatusHistory)
                    .ThenInclude(h => h.ChangedBy);
            }

            dbQuery = dbQuery.Include(t => t.User);

            return dbQuery;
        }
    }
}
