// <copyright file="GetCostCentreQuery.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.CostCentre
{
    /// <summary>
    /// DTO representing all information a user can query on GET /cost-centre.
    /// </summary>
    public class GetCostCentreQuery
    {
        /// <summary>
        /// Name to query.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Description to query.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Minimum active budget amount. Only cost centres with an active budget (PeriodStart &lt;= today &lt;= PeriodEnd) of at least this amount are returned.
        /// </summary>
        public decimal? MinBudget { get; init; }

        /// <summary>
        /// Maximum active budget amount. Only cost centres with an active budget (PeriodStart &lt;= today &lt;= PeriodEnd) of at most this amount are returned.
        /// </summary>
        public decimal? MaxBudget { get; init; }

        /// <summary>
        /// Limit of query.
        /// </summary>
        public int? Limit { get; init; }

        /// <summary>
        /// Offset of query.
        /// </summary>
        public int? Offset { get; init; }
    }
}
