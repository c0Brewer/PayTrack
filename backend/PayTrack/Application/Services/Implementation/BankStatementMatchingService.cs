// <copyright file="BankStatementMatchingService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Api.Mapper;
using PayTrack.Application.Dto.BankStatement;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class BankStatementMatchingService(ITransactionRepository repo) : IBankStatementMatchingService
    {
        private const int PossibleMatchThreshold = 2;
        private readonly ITransactionRepository repo = repo;

        /// <inheritdoc/>
        public async Task<BankStatementMatchResponseDto> MatchBankStatementEntriesAsync(List<BankStatementEntryDto> entries)
        {
            var results = new List<BankStatementMatchResultDto>();

            // Get all Approved transactions that could potentially be matched
            var (approvedTransactions, _) = await this.repo.GetAllAsync(
                new GetTransactionQuery());

            foreach (var entry in entries)
            {
                var bestMatch = this.FindBestMatch(entry, approvedTransactions);

                var result = new BankStatementMatchResultDto(
                    Entry: entry,
                    HasMatch: bestMatch != null,
                    MatchedTransaction: bestMatch?.Transaction != null ? TransactionMapper.ToDto(bestMatch.Transaction) : null,
                    MatchScore: bestMatch?.Score ?? 0);

                results.Add(result);
            }

            return new BankStatementMatchResponseDto(results);
        }

        /// <inheritdoc/>
        public async Task<List<Transaction>> UpdateBankStatementMatchesAsync(
            List<BankStatementUpdateRequestDto> updates, int changedById)
        {
            var updatedTransactions = new List<Transaction>();

            foreach (var update in updates)
            {
                if (update.Skipped)
                {
                    continue;
                }

                if (update.MatchedTransactionId.HasValue)
                {
                    var transaction = await this.repo.GetByIdAsync(
                        update.MatchedTransactionId.Value,
                        new GetTransactionQueryById { IncludeStatusHistory = true });

                    if (transaction == null)
                    {
                        throw new NotFoundException($"Transaction {update.MatchedTransactionId.Value} not found");
                    }

                    // Track status change
                    var statusHistory = new TransactionStatusHistory
                    {
                        TransactionId = transaction.Id,
                        ChangedById = changedById,
                        FromStatus = transaction.Status,
                        ToStatus = TransactionStatus.Paid,
                        ChangedAt = DateTime.UtcNow,
                    };

                    transaction.StatusHistory.Add(statusHistory);
                    transaction.Status = TransactionStatus.Paid;

                    var updated = await this.repo.UpdateAsync(transaction);
                    updatedTransactions.Add(updated);
                }
            }

            return updatedTransactions;
        }

        private MatchCandidate? FindBestMatch(BankStatementEntryDto entry, List<Transaction> transactions)
        {
            MatchCandidate? bestMatch = null;
            int bestScore = 0;

            foreach (var transaction in transactions)
            {
                var score = this.CalculateMatchScore(entry, transaction);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatch = new MatchCandidate(transaction, score);
                }
            }

            // Only return matches that meet the threshold
            return bestScore >= PossibleMatchThreshold ? bestMatch : null;
        }

        private int CalculateMatchScore(BankStatementEntryDto entry, Transaction transaction)
        {
            int score = 0;

            // +3: Amount matches exactly
            if (entry.Amount.Value == transaction.Amount)
            {
                score += 3;
            }

            // +2: Invoice number found in reference field
            var referenceFields = new[]
            {
                entry.Reference,
                entry.ReceiverReference,
                entry.PartnerName,
            };

            // Check invoice number from transaction if transaction is a paymentRequestByUser
            if (transaction is PaymentRequestByUser userTx && !string.IsNullOrEmpty(userTx.InvoiceNumber) &&
                    referenceFields.Any(field =>
                        !string.IsNullOrEmpty(field) &&
                        field.Contains(userTx.InvoiceNumber, StringComparison.OrdinalIgnoreCase)))
            {
                score += 2;
            }

            // +1: PaidAt date within ±3 days of booking date
            if (transaction.PaidAt.HasValue && entry.Booking != default)
            {
                var daysDifference = Math.Abs((entry.Booking.Date - transaction.PaidAt.Value.Date).Days);
                if (daysDifference <= 3)
                {
                    score++;
                }
            }

            // +1: Purpose/reference fuzzy match on receiver reference
            if (!string.IsNullOrEmpty(entry.ReceiverReference) || !string.IsNullOrEmpty(entry.Reference))
            {
                var bankPurpose = entry.ReceiverReference ?? entry.Reference ?? string.Empty;
                var paymentPurpose = transaction.PurposeOfPayment ?? string.Empty;

                if (this.CalculateStringSimilarity(bankPurpose, paymentPurpose) > 0.5)
                {
                    score++;
                }
            }

            return score;
        }

        private double CalculateStringSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            {
                return 0;
            }

            var longer = s1.Length > s2.Length ? s1 : s2;
            var shorter = s1.Length > s2.Length ? s2 : s1;

            if (longer.Length == 0)
            {
                return 1.0;
            }

            var editDistance = this.CalculateLevenshteinDistance(longer, shorter);
            return (longer.Length - editDistance) / (double)longer.Length;
        }

        private int CalculateLevenshteinDistance(string s1, string s2)
        {
            var n = s1.Length;
            var m = s2.Length;
            var d = new int[n + 1, m + 1];

            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            for (int i = 0; i <= n; i++)
            {
                d[i, 0] = i;
            }

            for (int j = 0; j <= m; j++)
            {
                d[0, j] = j;
            }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        private class MatchCandidate(Transaction transaction, int score)
        {
            public Transaction Transaction { get; } = transaction;
            public int Score { get; } = score;
        }
    }
}
