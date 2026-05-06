// <copyright file="BudgetHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler for Budget-related requests.
    /// </summary>
    public static class BudgetHandler
    {
        /// <summary>
        /// Returns all budgets.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<Results<Ok, ProblemHttpResult>> GetBudgetsAsync()
        {
            return Task.FromResult<Results<Ok, ProblemHttpResult>>(
                TypedResults.Problem(statusCode: StatusCodes.Status501NotImplemented, detail: "Budget endpoint not implemented yet."));
        }

        /// <summary>
        /// Returns a budget by ID.
        /// </summary>
        /// <param name="id">Id from route.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<Results<Ok, ProblemHttpResult>> GetBudgetByIdAsync([FromRoute] int id)
        {
            return Task.FromResult<Results<Ok, ProblemHttpResult>>(
                TypedResults.Problem(statusCode: StatusCodes.Status501NotImplemented, detail: "Budget endpoint not implemented yet."));
        }

        /// <summary>
        /// Creates a budget.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<Results<Ok, ProblemHttpResult>> CreateBudgetAsync()
        {
            return Task.FromResult<Results<Ok, ProblemHttpResult>>(
                TypedResults.Problem(statusCode: StatusCodes.Status501NotImplemented, detail: "Budget endpoint not implemented yet."));
        }

        /// <summary>
        /// Updates a budget.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<Results<Ok, ProblemHttpResult>> UpdateBudgetAsync()
        {
            return Task.FromResult<Results<Ok, ProblemHttpResult>>(
                TypedResults.Problem(statusCode: StatusCodes.Status501NotImplemented, detail: "Budget endpoint not implemented yet."));
        }

        /// <summary>
        /// Deletes a budget.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<Results<Ok, ProblemHttpResult>> DeleteBudgetAsync()
        {
            return Task.FromResult<Results<Ok, ProblemHttpResult>>(
                TypedResults.Problem(statusCode: StatusCodes.Status501NotImplemented, detail: "Budget endpoint not implemented yet."));
        }
    }
}
