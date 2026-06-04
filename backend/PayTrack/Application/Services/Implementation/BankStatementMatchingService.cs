// <copyright file="BankStatementMatchingService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.BankAccount;
using PayTrack.Application.Dto.BankStatement;
using PayTrack.Application.Dto.PaymentRequestByUser;
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
        private const decimal NearAmountTolerance = 1.0m;
        private const int NearAmountMaxDays = 10;
        private readonly ITransactionRepository repo = repo;

        /// <inheritdoc/>
        public async Task<BankStatementMatchResponseDto> MatchBankStatementEntriesAsync(List<BankStatementEntryDto> entries)
        {
            var results = new List<BankStatementMatchResultDto>();

            // Only match against Approved transactions; always includes User via ApplyBasePostFilters
            var (transactions, _) = await this.repo.GetAllAsync(
                new GetTransactionQuery
                {
                    // Status = TransactionStatus.Approved,
                    IncludeTeam = true,
                });

            // Ignore already paid transactions
            transactions = [.. transactions.Where(t => t.Status != TransactionStatus.Paid)];

            // Load approved user payment requests with bank accounts for IBAN matching and display
            var (userRequests, _) = await this.repo.GetAllAsync(
                new GetPaymentRequestByUserQuery
                {
                    // Status = TransactionStatus.Approved,
                    IncludeBankAccount = true,
                });
            var ibanByTransactionId = userRequests
                .Where(r => r.BankAccount?.Iban != null)
                .ToDictionary(
                    r => r.Id,
                    r => NormalizeIban(r.BankAccount!.Iban));
            var userRequestById = userRequests.ToDictionary(r => r.Id);

            foreach (var entry in entries)
            {
                var bestMatch = this.FindBestMatch(entry, transactions, ibanByTransactionId);

                var matchedDto = bestMatch?.Transaction != null
                    ? MapToMatchedTransactionDto(bestMatch.Transaction, userRequestById)
                    : null;
                var result = new BankStatementMatchResultDto(
                    Entry: entry,
                    HasMatch: bestMatch != null,
                    MatchedTransaction: matchedDto,
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

        private static string NormalizeIban(string? iban) =>
            (iban ?? string.Empty).Replace(" ", string.Empty).ToUpperInvariant();

        private static BankStatementMatchedTransactionDto MapToMatchedTransactionDto(
            Transaction transaction,
            Dictionary<int, PaymentRequestByUser> userRequestById)
        {
            userRequestById.TryGetValue(transaction.Id, out var userReq);

            BankAccountDto? bankAccountDto = null;
            if (userReq?.BankAccount != null)
            {
                var ba = userReq.BankAccount;
                bankAccountDto = new BankAccountDto(ba.Id, ba.AccountHolder, ba.Iban, ba.Bic);
            }

            return new BankStatementMatchedTransactionDto
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                PurposeOfPayment = transaction.PurposeOfPayment,
                PaymentReference = transaction.PaymentReference,
                Status = transaction.Status,
                PaidAt = transaction.PaidAt,
                UserName = transaction.User?.Name,
                TeamName = transaction.Team?.Name,
                InvoiceNumber = userReq?.InvoiceNumber,
                BankAccount = bankAccountDto,
            };
        }

        private MatchCandidate? FindBestMatch(
            BankStatementEntryDto entry,
            List<Transaction> transactions,
            Dictionary<int, string> ibanByTransactionId)
        {
            MatchCandidate? bestMatch = null;
            int bestScore = 0;

            foreach (var transaction in transactions)
            {
                var score = this.CalculateMatchScore(entry, transaction, ibanByTransactionId);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatch = new MatchCandidate(transaction, score);
                }
            }

            // Only return matches that meet the threshold
            return bestScore >= PossibleMatchThreshold ? bestMatch : null;
        }

        private int CalculateMatchScore(
            BankStatementEntryDto entry,
            Transaction transaction,
            Dictionary<int, string> ibanByTransactionId)
        {
            int score = 0;

            // +3: Amount matches exactly
            bool exactAmount = entry.Amount.Value == transaction.Amount;
            if (exactAmount)
            {
                score += 3;
            }

            // +3: IBAN matches (PaymentRequestByUser only — partner account is the recipient's bank account)
            if (!string.IsNullOrEmpty(entry.PartnerAccount?.Iban) &&
                ibanByTransactionId.TryGetValue(transaction.Id, out var txIban) &&
                NormalizeIban(entry.PartnerAccount.Iban) == txIban)
            {
                score += 3;
            }

            // +2: Invoice number found in any reference field (PaymentRequestByUser only)
            var referenceFields = new[]
            {
                entry.Reference,
                entry.ReceiverReference,
                entry.PartnerName,
            };

            if (transaction is PaymentRequestByUser userTx && !string.IsNullOrEmpty(userTx.InvoiceNumber) &&
                    referenceFields.Any(field =>
                        !string.IsNullOrEmpty(field) &&
                        field.Contains(userTx.InvoiceNumber, StringComparison.OrdinalIgnoreCase)))
            {
                score += 2;
            }

            // +1: PaidAt date within ±3 days of booking date
            int? bookingDaysDiff = transaction.PaidAt.HasValue && entry.Booking != default
                ? Math.Abs((entry.Booking.Date - transaction.PaidAt.Value.Date).Days)
                : null;

            if (bookingDaysDiff <= 3)
            {
                score++;
            }

            // +1: Purpose/reference fuzzy match (>50% string similarity)
            if (!string.IsNullOrEmpty(entry.ReceiverReference) || !string.IsNullOrEmpty(entry.Reference))
            {
                var bankPurpose = entry.ReceiverReference ?? entry.Reference ?? string.Empty;
                var paymentPurpose = transaction.PurposeOfPayment ?? string.Empty;

                if (this.CalculateStringSimilarity(bankPurpose, paymentPurpose) > 0.5)
                {
                    score++;
                }
            }

            // +2: Partner name fuzzy match with user's name (>60% similarity)
            // User is always loaded by the repository — no extra query needed.
            if (!string.IsNullOrEmpty(entry.PartnerName) && !string.IsNullOrEmpty(transaction.User?.Name) && this.CalculateStringSimilarity(entry.PartnerName, transaction.User.Name) > 0.6)
            {
                score += 2;
            }

            // +1: Near-amount (within €1 tolerance) — only when NOT exact and booking within ±10 days.
            // The date gate prevents spurious near-matches across many historical transactions.
            if (!exactAmount &&
                Math.Abs(entry.Amount.Value - transaction.Amount) <= NearAmountTolerance &&
                bookingDaysDiff.HasValue && bookingDaysDiff.Value <= NearAmountMaxDays)
            {
                score++;
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
