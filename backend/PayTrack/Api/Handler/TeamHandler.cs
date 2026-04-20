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
            var createdTeam = await teamService.CreateTeamAsync(teamDto.name, teamDto.description, teamDto.displayColor);

            var createdTeamDto = TeamMapper.ToDto(createdTeam);

            return TypedResults.Ok(createdTeamDto);
        }
    }
}
