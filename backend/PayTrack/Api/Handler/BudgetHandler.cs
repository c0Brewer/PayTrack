// <copyright file="BudgetHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Api.Mapper;
using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Dto.Pagination;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

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
        /// <param name="query">Query object including all filter and pagination options.</param>
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaginatedResponse<BudgetDto>>, BadRequest<ProblemDetails>, ProblemHttpResult>> GetBudgetsAsync(
            [AsParameters] GetBudgetQuery query,
            IBudgetService service)
        {
            var (budgetList, totalCount) = await service.GetBudgetsAsync(query);
            var budgetListDto = BudgetMapper.CollectionToDto(budgetList);
            var paginatedResponse = new PaginatedResponse<BudgetDto>(budgetListDto, totalCount, query.Limit ?? -1, query.Offset ?? 0);
            return TypedResults.Ok(paginatedResponse);
        }

        /// <summary>
        /// Returns a budget by ID.
        /// </summary>
        /// <param name="id">Id from route.</param>
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<BudgetDto>, NotFound<ProblemDetails>, ProblemHttpResult>> GetBudgetByIdAsync(
            [FromRoute] int id,
            IBudgetService service)
        {
            var budget = await service.GetBudgetByIdAsync(id) ?? throw new NotFoundException("Budget could not be found.");
            return TypedResults.Ok(BudgetMapper.ToDto(budget));
        }

        /// <summary>
        /// Creates a budget.
        /// </summary>
        /// <param name="dto">Request body.</param>
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<BudgetDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> CreateBudgetAsync(
            [FromBody] CreateBudgetRequestDto dto,
            IBudgetService service)
        {
            var budget = await service.CreateBudgetAsync(
                dto.Name,
                dto.Description,
                dto.TeamId,
                dto.CostCentreId,
                dto.SeasonId,
                dto.TargetAmount,
                dto.PeriodStart,
                dto.PeriodEnd);

            return TypedResults.Ok(BudgetMapper.ToDto(budget));
        }

        /// <summary>
        /// Updates a budget.
        /// </summary>
        /// <param name="id">Id from route.</param>
        /// <param name="dto">Request body.</param>
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<BudgetDto>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>, ProblemHttpResult>> UpdateBudgetAsync(
            [FromRoute] int id,
            [FromBody] UpdateBudgetRequestDto dto,
            IBudgetService service)
        {
            var budget = await service.UpdateBudgetAsync(
                id,
                dto.Name,
                dto.Description,
                dto.TeamId,
                dto.CostCentreId,
                dto.SeasonId,
                dto.TargetAmount,
                dto.PeriodStart,
                dto.PeriodEnd);

            return TypedResults.Ok(BudgetMapper.ToDto(budget));
        }

        /// <summary>
        /// Deletes a budget.
        /// </summary>
        /// <param name="id">Id from route.</param>
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<NoContent, NotFound<ProblemDetails>, BadRequest<ProblemDetails>, ProblemHttpResult>> DeleteBudgetAsync(
            [FromRoute] int id,
            IBudgetService service)
        {
            await service.DeleteBudgetAsync(id);
            return TypedResults.NoContent();
        }
    }
}
