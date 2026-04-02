// <copyright file="GetUserQueryById.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.User
{
    /// <summary>
    /// DTO representing all information a user can query on GET /user.
    /// </summary>
    public class GetUserQueryById
    {
        /// <summary>
        /// Whether to load the teams in the query.
        /// </summary>
        public bool? IncludeTeam { get; init; }

        /// <summary>
        /// Whether to include the bank accounts in the query.
        /// </summary>
        public bool? IncludeBankAccounts { get; init; }
    }
}
