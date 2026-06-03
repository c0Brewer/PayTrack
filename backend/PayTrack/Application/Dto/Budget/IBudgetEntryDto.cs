// <copyright file="IBudgetEntryDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.Budget
{
    /// <summary>
    /// Common contract for budget entry DTOs that can be validated by the application layer.
    /// </summary>
    public interface IBudgetEntryDto
    {
        /// <summary>
        /// Gets the target budget amount. Required for Expense budgets; must be null for Income budgets.
        /// </summary>
        decimal? TargetAmount { get; }

        /// <summary>
        /// Gets the budget type.
        /// </summary>
        BudgetType Type { get; }

        /// <summary>
        /// Gets the budget period start.
        /// </summary>
        DateTime PeriodStart { get; }

        /// <summary>
        /// Gets the budget period end.
        /// </summary>
        DateTime PeriodEnd { get; }
    }
}
