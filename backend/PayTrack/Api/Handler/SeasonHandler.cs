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
            return Task.FromResult<Results<Ok, ProblemHttpResult>>(
                TypedResults.Problem(statusCode: StatusCodes.Status501NotImplemented, detail: "Season endpoint not implemented yet."));
        }

        /// <summary>
        /// Creates a season.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<Results<Ok, ProblemHttpResult>> CreateSeasonAsync()
        {
            return Task.FromResult<Results<Ok, ProblemHttpResult>>(
                TypedResults.Problem(statusCode: StatusCodes.Status501NotImplemented, detail: "Season endpoint not implemented yet."));
        }

        /// <summary>
        /// Updates a season.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<Results<Ok, ProblemHttpResult>> UpdateSeasonAsync()
        {
            return Task.FromResult<Results<Ok, ProblemHttpResult>>(
                TypedResults.Problem(statusCode: StatusCodes.Status501NotImplemented, detail: "Season endpoint not implemented yet."));
        }

        /// <summary>
        /// Deletes a season.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<Results<Ok, ProblemHttpResult>> DeleteSeasonAsync()
        {
            return Task.FromResult<Results<Ok, ProblemHttpResult>>(
                TypedResults.Problem(statusCode: StatusCodes.Status501NotImplemented, detail: "Season endpoint not implemented yet."));
        }
    }
}
