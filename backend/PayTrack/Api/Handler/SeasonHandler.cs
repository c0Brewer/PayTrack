// <copyright file="SeasonHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler for Season-related requests.
    /// </summary>
    public static class SeasonHandler
    {
        /// <summary>
        /// Returns all seasons.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<Results<Ok, ProblemHttpResult>> GetSeasonsAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a season.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<Results<Ok, ProblemHttpResult>> CreateSeasonAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates a season.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<Results<Ok, ProblemHttpResult>> UpdateSeasonAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes a season.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<Results<Ok, ProblemHttpResult>> DeleteSeasonAsync()
        {
            throw new NotImplementedException();
        }
    }
}
