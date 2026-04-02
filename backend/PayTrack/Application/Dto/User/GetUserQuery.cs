// <copyright file="GetUserQuery.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.User
{
    /// <summary>
    /// DTO representing all information a user can query on GET /user.
    /// </summary>
    public class GetUserQuery
    {
        /// <summary>
        /// Name to query.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Email to query.
        /// </summary>
        public string? Email { get; init; }

        /// <summary>
        /// Team Name to query.
        /// </summary>
        public string? TeamName { get; init; }

        /// <summary>
        /// Role to query.
        /// </summary>
        public Role? Role { get; init; }

        /// <summary>
        /// Active state to query.
        /// </summary>
        public bool? IsActive { get; init; }

        /// <summary>
        /// Whether to load the teams in the query.
        /// </summary>
        public bool? IncludeTeam { get; init; }

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
