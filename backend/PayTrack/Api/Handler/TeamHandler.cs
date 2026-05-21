// <copyright file="TeamHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Api.Mapper;
using PayTrack.Application.Dto.Pagination;
using PayTrack.Application.Dto.Team;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler which gets called from Endpoint to manage incoming Team-related requests.
    /// </summary>
    public static class TeamHandler
    {
        /// <summary>
        /// Returns all teams.
        /// </summary>
        /// <param name="query">Query object including all query options.</param>
        /// <param name="teamService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<PaginatedResponse<TeamDto>>, BadRequest<ProblemDetails>, ProblemHttpResult>> GetTeamsAsync(
            [AsParameters] GetTeamQuery query,
            ITeamService teamService)
        {
            var (teamList, totalCount) = await teamService.GetTeamsAsync(query);
            var teamListDto = TeamMapper.ListToDto(teamList);
            var paginatedResponse = new PaginatedResponse<TeamDto>(teamListDto, totalCount, query.Limit ?? -1, query.Offset ?? -1);
            return TypedResults.Ok(paginatedResponse);
        }

        /// <summary>
        /// Returns a Team by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="query">Query object including all query options.</param>
        /// <param name="teamService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<TeamDto>, BadRequest<ProblemDetails>, NotFound<ProblemDetails>, ProblemHttpResult>> GetTeamByIdAsync(
            [FromRoute] int id,
            [AsParameters] GetTeamQueryById query,
            ITeamService teamService)
        {
            var team = await teamService.GetTeamByIdAsync(id, query) ?? throw new NotFoundException("Team could not be found");
            var createdTeamDto = TeamMapper.ToDto(team);
            return TypedResults.Ok(createdTeamDto);
        }

        /// <summary>
        /// Creates a Team.
        /// </summary>
        /// <param name="teamDto">request for team creation.</param>
        /// <param name="teamService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<TeamDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> CreateTeamAsync(
            [FromBody] CreateTeamRequestDto teamDto,
            ITeamService teamService)
        {
            var createdTeam = await teamService.CreateTeamAsync(
                teamDto.Name,
                teamDto.Description,
                teamDto.DisplayColor,
                teamDto.Budgets);

            var createdTeamDto = TeamMapper.ToDto(createdTeam);

            return TypedResults.Ok(createdTeamDto);
        }

        /// <summary>
        /// Updates a Team.
        /// </summary>
        /// <param name="id">Id of the Team to update.</param>
        /// <param name="updateTeamRequestDto">request for team creation.</param>
        /// <param name="teamService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<TeamDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> UpdateTeamAsync(
            [FromRoute] int id,
            [FromBody] UpdateTeamRequestDto updateTeamRequestDto,
            ITeamService teamService)
        {
            var updatedTeam = await teamService.UpdateTeamAsync(
                id,
                updateTeamRequestDto.Name,
                updateTeamRequestDto.Description,
                updateTeamRequestDto.IsActive,
                updateTeamRequestDto.DisplayColor,
                updateTeamRequestDto.BudgetsToUpsert,
                updateTeamRequestDto.BudgetIdsToDelete);

            var updatedTeamDto = TeamMapper.ToDto(updatedTeam);
            return TypedResults.Ok(updatedTeamDto);
        }

        /// <summary>
        /// Deletes a Team.
        /// </summary>
        /// <param name="id">Id of the Team to delete.</param>
        /// <param name="teamService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<NoContent, Ok<TeamDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> DeleteTeamAsync(
            [FromRoute] int id,
            ITeamService teamService)
        {
            var deletedTeam = await teamService.DeleteTeamAsync(id);
            if (deletedTeam is null)
            {
                return TypedResults.NoContent();
            }

            var deletedTeamDto = TeamMapper.ToDto(deletedTeam);

            return TypedResults.Ok(deletedTeamDto);
        }

        /// <summary>
        /// Returns the impact of deleting a Team by ID.
        /// </summary>
        /// <param name="id">Id of the Team to inspect.</param>
        /// <param name="teamService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<DeleteTeamImpactDto>, BadRequest<ProblemDetails>, NotFound<ProblemDetails>, ProblemHttpResult>> GetDeleteTeamImpactAsync(
            [FromRoute] int id,
            ITeamService teamService)
        {
            var deleteImpact = await teamService.GetDeleteTeamImpactAsync(id) ?? throw new NotFoundException("Team could not be found");

            return TypedResults.Ok(deleteImpact);
        }
    }
}
