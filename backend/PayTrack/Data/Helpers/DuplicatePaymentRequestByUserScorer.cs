// <copyright file="DuplicatePaymentRequestByUserScorer.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Data.Helpers
{
    /// <summary>
    /// Calculates weighted scores for potential duplicate PaymentRequestByUser entries.
    /// </summary>
    public static class DuplicatePaymentRequestByUserScorer
    {
        /// <summary>
        /// Minimum score required for a candidate to be treated as a duplicate.
        /// </summary>
        public const int MatchThreshold = 60;

        private const int ExactInvoiceNumberScore = 80;
        private const int SimilarInvoiceNumberScore = 35;
        private const int SameAmountScore = 30;
        private const int SamePaydayScore = 20;
        private const int SameUserScore = 10;
        private const int SameTeamScore = 5;
        private const int AmountPaydayUserBonusScore = 10;
        private const int AmountPaydayTeamBonusScore = 5;

        /// <summary>
        /// Calculates a weighted duplicate score.
        /// </summary>
        /// <param name="candidate">Existing payment request candidate.</param>
        /// <param name="userId">Source user id.</param>
        /// <param name="teamId">Source team id.</param>
        /// <param name="amount">Source amount.</param>
        /// <param name="paidAt">Source payday.</param>
        /// <param name="invoiceNumber">Source invoice number.</param>
        /// <returns>Weighted score and exact match flags.</returns>
        public static DuplicatePaymentRequestByUserScoreResult Calculate(
            PaymentRequestByUser candidate,
            int userId,
            int teamId,
            decimal amount,
            DateTime paidAt,
            string? invoiceNumber)
        {
            bool isSameAmount = candidate.Amount == amount;
            bool isSamePayday = candidate.PaidAt?.Date == paidAt.Date;
            bool isSameUser = candidate.UserId == userId;
            bool isSameTeam = candidate.TeamId == teamId;
            bool isInvoiceNumberMatch = IsSameInvoiceNumber(candidate.InvoiceNumber, invoiceNumber);
            bool isSimilarInvoiceNumber = !isInvoiceNumberMatch && IsSimilarInvoiceNumber(candidate.InvoiceNumber, invoiceNumber);
            bool isAmountAndUserMatch = isSameAmount && isSamePayday && isSameUser;
            bool isAmountAndTeamMatch = isSameAmount && isSamePayday && isSameTeam;

            int score = 0;

            if (isInvoiceNumberMatch)
            {
                score += ExactInvoiceNumberScore;
            }
            else if (isSimilarInvoiceNumber)
            {
                score += SimilarInvoiceNumberScore;
            }

            if (isSameAmount)
            {
                score += SameAmountScore;
            }

            if (isSamePayday)
            {
                score += SamePaydayScore;
            }

            if (isSameUser)
            {
                score += SameUserScore;
            }

            if (isSameTeam)
            {
                score += SameTeamScore;
            }

            if (isAmountAndUserMatch)
            {
                score += AmountPaydayUserBonusScore;
            }

            if (isAmountAndTeamMatch)
            {
                score += AmountPaydayTeamBonusScore;
            }

            return new DuplicatePaymentRequestByUserScoreResult(
                score,
                isAmountAndUserMatch,
                isInvoiceNumberMatch,
                isAmountAndTeamMatch);
        }

        private static bool IsSameInvoiceNumber(string candidateInvoiceNumber, string? invoiceNumber)
        {
            var normalizedCandidate = NormalizeInvoiceNumber(candidateInvoiceNumber);
            var normalizedInvoiceNumber = NormalizeInvoiceNumber(invoiceNumber);

            return normalizedCandidate.Length > 0
                && normalizedCandidate == normalizedInvoiceNumber;
        }

        // If one invoice number contains the other, and their lengths differ by at most 2, it counts as similar.
        private static bool IsSimilarInvoiceNumber(string candidateInvoiceNumber, string? invoiceNumber)
        {
            var normalizedCandidate = NormalizeInvoiceNumber(candidateInvoiceNumber);
            var normalizedInvoiceNumber = NormalizeInvoiceNumber(invoiceNumber);

            if (normalizedCandidate.Length < 4 || normalizedInvoiceNumber.Length < 4)
            {
                return false;
            }

            var shorterLength = Math.Min(normalizedCandidate.Length, normalizedInvoiceNumber.Length);
            var longerLength = Math.Max(normalizedCandidate.Length, normalizedInvoiceNumber.Length);

            if (shorterLength >= 5
                && longerLength - shorterLength <= 2
                && (normalizedCandidate.Contains(normalizedInvoiceNumber, StringComparison.Ordinal)
                    || normalizedInvoiceNumber.Contains(normalizedCandidate, StringComparison.Ordinal)))
            {
                return true;
            }

            var maxDistance = Math.Max(1, longerLength / 5);
            return GetLevenshteinDistance(normalizedCandidate, normalizedInvoiceNumber) <= maxDistance;
        }

        private static string NormalizeInvoiceNumber(string? invoiceNumber)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
            {
                return string.Empty;
            }

            return new string(invoiceNumber
                .Trim()
                .ToUpperInvariant()
                .Where(char.IsLetterOrDigit)
                .ToArray());
        }

        // This counts how many single-character edits are needed to turn one string into the other.
        // AI generated this function
        private static int GetLevenshteinDistance(string left, string right)
        {
            if (left.Length == 0)
            {
                return right.Length;
            }

            if (right.Length == 0)
            {
                return left.Length;
            }

            var previous = Enumerable.Range(0, right.Length + 1).ToArray();
            var current = new int[right.Length + 1];

            for (var leftIndex = 0; leftIndex < left.Length; leftIndex++)
            {
                current[0] = leftIndex + 1;

                for (var rightIndex = 0; rightIndex < right.Length; rightIndex++)
                {
                    var cost = left[leftIndex] == right[rightIndex] ? 0 : 1;

                    current[rightIndex + 1] = Math.Min(
                        Math.Min(current[rightIndex] + 1, previous[rightIndex + 1] + 1),
                        previous[rightIndex] + cost);
                }

                (previous, current) = (current, previous);
            }

            return previous[right.Length];
        }
    }
}
