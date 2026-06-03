// <copyright file="BudgetType.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Classifies the direction of a Budget.
    /// </summary>
    public enum BudgetType
    {
        /// <summary>
        /// An outgoing budget for planned expenses (default).
        /// </summary>
        Expense = 0,

        /// <summary>
        /// An incoming budget for expected revenue, such as merchandise sales.
        /// </summary>
        Income = 1,
    }
}
