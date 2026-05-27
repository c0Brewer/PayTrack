// <copyright file="IBudgetEntryDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.Budget
{
    /// <summary>
    /// Common contract for budget entry DTOs that can be validated by the application layer.
    /// </summary>
    public interface IBudgetEntryDto
    {
        /// <summary>
        /// Gets the target budget amount.
        /// </summary>
        decimal TargetAmount { get; }

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
