// <copyright file="GetBudgetQuery.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.Budget
{
    /// <summary>
    /// DTO representing all information a user can query on GET /user.
    /// </summary>
    public class GetBudgetQuery
    {
        /// <summary>
        /// Name to query.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// TeamId to query.
        /// </summary>
        public int? TeamId { get; init; }

        /// <summary>
        /// CostCentreId to query.
        /// </summary>
        public int? CostCentreId { get; init; }

        /// <summary>
        /// SeasonId to query.
        /// </summary>
        public int? SeasonId { get; init; }

        /// <summary>
        /// TargetAmount to query.
        /// </summary>
        public decimal? TargetAmount { get; init; }

        /// <summary>
        /// PeriodStart to query.
        /// </summary>
        public DateTime? PeriodStart { get; init; }

        /// <summary>
        /// PeriodEnd to query.
        /// </summary>
        public DateTime? PeriodEnd { get; init; }

        /// <summary>
        /// Limit of query.
        /// </summary>
        public int? Limit { get; init; }

        /// <summary>
        /// Offset of query.
        /// </summary>
        public int? Offset { get; init; }

        /// <summary>
        /// Type of budget to query.
        /// </summary>
        public BudgetType? Type { get; init; }
    }
}
