// <copyright file="GetTransactionQueryById.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.Transaction
{
    /// <summary>
    /// DTO representing all information a transaction can query on GET /transaction.
    /// </summary>
    public class GetTransactionQueryById
    {
        /// <summary>
        /// Whether to include the cost centre in the query.
        /// </summary>
        public bool? IncludeCostCentre { get; init; }

        /// <summary>
        /// Whether to include the cost centre in the query.
        /// </summary>
        public bool? IncludeUser { get; init; }

        /// <summary>
        /// Whether to include the team in the query.
        /// </summary>
        public bool? IncludeTeam { get; init; }

        /// <summary>
        /// Whether to load the status history in the query.
        /// </summary>
        public bool? IncludeStatusHistory { get; init; }
    }
}
