// <copyright file="GetTeamQuery.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.Team
{
    /// <summary>
    /// Data Transfer Object (DTO) representing all information a team can query on GET /team.
    /// </summary>
    public class GetTeamQuery
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
        /// Minimum budget to query.
        /// </summary>
        public decimal? MinBudget { get; init; }

        /// <summary>
        /// Maximum budget to query.
        /// </summary>
        public decimal? MaxBudget { get; init; }

        /// <summary>
        /// Whether to include team members.
        /// </summary>
        public bool? IncludeMembers { get; init; }

        /// <summary>
        /// Whether to include budgets.
        /// </summary>
        public bool? IncludeBudgets { get; init; }

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