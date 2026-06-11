// <copyright file="GetSeasonQuery.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.Season
{
    /// <summary>
    /// Data Transfer Object (DTO) representing query options for GET /season.
    /// </summary>
    public class GetSeasonQuery
    {
        /// <summary>
        /// Whether inactive seasons should be included.
        /// </summary>
        public bool? IncludeInactive { get; init; }
    }
}
