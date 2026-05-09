// <copyright file="SeasonHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Api.Mapper;
using PayTrack.Application.Dto.Season;
using PayTrack.Application.Services.Model;

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
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<List<SeasonDto>>, ProblemHttpResult>> GetSeasonsAsync(
            ISeasonService service)
        {
            var seasons = await service.GetAllAsync();
            return TypedResults.Ok(SeasonMapper.ListToDto(seasons));
        }

        /// <summary>
        /// Creates a season.
        /// </summary>
        /// <param name="dto">Request body.</param>
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<SeasonDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> CreateSeasonAsync(
            [FromBody] CreateSeasonRequestDto dto,
            ISeasonService service)
        {
            var season = await service.CreateAsync(dto.Name);
            return TypedResults.Ok(SeasonMapper.ToDto(season));
        }

        /// <summary>
        /// Updates a season.
        /// </summary>
        /// <param name="id">Id from route.</param>
        /// <param name="dto">Request body.</param>
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<SeasonDto>, BadRequest<ProblemDetails>, NotFound<ProblemDetails>, ProblemHttpResult>> UpdateSeasonAsync(
            [FromRoute] int id,
            [FromBody] UpdateSeasonRequestDto dto,
            ISeasonService service)
        {
            var season = await service.UpdateAsync(id, dto.Name);
            return TypedResults.Ok(SeasonMapper.ToDto(season));
        }

        /// <summary>
        /// Deletes a season.
        /// </summary>
        /// <param name="id">Id from route.</param>
        /// <param name="service">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<NoContent, BadRequest<ProblemDetails>, NotFound<ProblemDetails>, ProblemHttpResult>> DeleteSeasonAsync(
            [FromRoute] int id,
            ISeasonService service)
        {
            await service.DeleteAsync(id);
            return TypedResults.NoContent();
        }
    }
}
