// <copyright file="CostCentreHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Api.Mapper;
using PayTrack.Application.Dto.CostCentre;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler for CostCentre-related requests.
    /// </summary>
    public static class CostCentreHandler
    {
        /// <summary>
        /// Returns all cost centers.
        /// </summary>
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<List<CostCentreDto>>, ProblemHttpResult>> GetAllAsync(
            ICostCentreService service)
        {
            var costCentres = await service.GetAllAsync();
            return TypedResults.Ok(CostCentreMapper.ListToDto(costCentres));
        }

        /// <summary>
        /// Returns a cost center by ID.
        /// </summary>
        /// <param name="id">Id from route.</param>
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<CostCentreDto>, NotFound<ProblemDetails>, ProblemHttpResult>> GetByIdAsync(
            [FromRoute] int id,
            ICostCentreService service)
        {
            var costCentre = await service.GetByIdAsync(id) ?? throw new NotFoundException("CostCentre could not be found.");
            return TypedResults.Ok(CostCentreMapper.ToDto(costCentre));
        }

        /// <summary>
        /// Creates a new cost center.
        /// </summary>
        /// <param name="dto">Request body.</param>
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<CostCentreDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> CreateAsync(
            [FromBody] CreateCostCentreRequestDto dto,
            ICostCentreService service)
        {
            var costCentre = await service.CreateAsync(dto.Name, dto.Description, dto.DisplayColor, dto.Budgets);
            return TypedResults.Ok(CostCentreMapper.ToDto(costCentre));
        }

        /// <summary>
        /// Partially updates a cost center.
        /// </summary>
        /// <param name="id">Id from route.</param>
        /// <param name="dto">Request body.</param>
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<CostCentreDto>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>, ProblemHttpResult>> UpdateAsync(
            [FromRoute] int id,
            [FromBody] UpdateCostCentreRequestDto dto,
            ICostCentreService service)
        {
            var costCentre = await service.UpdateAsync(id, dto.Name, dto.Description, dto.DisplayColor);
            return TypedResults.Ok(CostCentreMapper.ToDto(costCentre));
        }

        /// <summary>
        /// Returns a preview of entities that would be affected by deleting the given cost center.
        /// </summary>
        /// <param name="id">Id from route.</param>
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<DeleteCostCentrePreviewDto>, NotFound<ProblemDetails>, ProblemHttpResult>> GetDeletePreviewAsync(
            [FromRoute] int id,
            ICostCentreService service)
        {
            var preview = await service.GetDeletePreviewAsync(id);
            return TypedResults.Ok(preview);
        }

        /// <summary>
        /// Deletes a cost center. Returns 400 if linked budgets or transactions exist.
        /// </summary>
        /// <param name="id">Id from route.</param>
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<NoContent, NotFound<ProblemDetails>, BadRequest<ProblemDetails>, ProblemHttpResult>> DeleteAsync(
            [FromRoute] int id,
            ICostCentreService service)
        {
            await service.DeleteAsync(id);
            return TypedResults.NoContent();
        }
    }
}
