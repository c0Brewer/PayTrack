// <copyright file="DuplicatePaymentRequestByUserScore.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Data.Repositories.Model
{
    /// <summary>
    /// Weighted score for a potential duplicate PaymentRequestByUser.
    /// </summary>
    /// <param name="Score">Weighted duplicate score.</param>
    /// <param name="IsAmountAndUserMatch">Whether amount, payday, and user match exactly.</param>
    /// <param name="IsInvoiceNumberMatch">Whether invoice number matches exactly after normalization.</param>
    /// <param name="IsAmountAndTeamMatch">Whether amount, payday, and team match exactly.</param>
    public sealed record class DuplicatePaymentRequestByUserScore(
        int Score,
        bool IsAmountAndUserMatch,
        bool IsInvoiceNumberMatch,
        bool IsAmountAndTeamMatch);

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
        private const int SameUserScore = 15;
        private const int SameTeamScore = 10;

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
        public static DuplicatePaymentRequestByUserScore Calculate(
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

            return new DuplicatePaymentRequestByUserScore(
                score,
                isSameAmount && isSamePayday && isSameUser,
                isInvoiceNumberMatch,
                isSameAmount && isSamePayday && isSameTeam);
        }

        private static bool IsSameInvoiceNumber(string candidateInvoiceNumber, string? invoiceNumber)
        {
            var normalizedCandidate = NormalizeInvoiceNumber(candidateInvoiceNumber);
            var normalizedInvoiceNumber = NormalizeInvoiceNumber(invoiceNumber);

            return normalizedCandidate.Length > 0
                && normalizedCandidate == normalizedInvoiceNumber;
        }

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
