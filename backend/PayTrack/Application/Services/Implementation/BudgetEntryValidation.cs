// <copyright file="BudgetEntryValidation.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Exceptions;

namespace PayTrack.Application.Services.Implementation
{
    /// <summary>
    /// Shared validation for budget entries handled by services.
    /// </summary>
    internal static class BudgetEntryValidation
    {
        private const string TargetAmountMustBeNonNegative = "Target amount must be non-negative.";

        private const string PeriodEndMustNotBeBeforePeriodStart = "Period end must not be before period start.";

        /// <summary>
        /// Ensures a budget entry satisfies service-level business rules.
        /// </summary>
        /// <param name="targetAmount">Target budget amount.</param>
        /// <param name="periodStart">Budget period start.</param>
        /// <param name="periodEnd">Budget period end.</param>
        public static void EnsureValid(decimal targetAmount, DateTime periodStart, DateTime periodEnd)
        {
            if (targetAmount < 0)
            {
                throw new InvalidStateException(TargetAmountMustBeNonNegative);
            }

            if (periodEnd < periodStart)
            {
                throw new InvalidStateException(PeriodEndMustNotBeBeforePeriodStart);
            }
        }

        /// <summary>
        /// Ensures all supplied budget entries satisfy service-level business rules.
        /// </summary>
        /// <typeparam name="T">Budget entry DTO type.</typeparam>
        /// <param name="budgetEntries">Optional budget entries to validate.</param>
        public static void EnsureValidEntries<T>(IEnumerable<T>? budgetEntries)
            where T : IBudgetEntryDto
        {
            if (budgetEntries is null)
            {
                return;
            }

            foreach (var entry in budgetEntries)
            {
                EnsureValid(entry.TargetAmount, entry.PeriodStart, entry.PeriodEnd);
            }
        }
    }
}
