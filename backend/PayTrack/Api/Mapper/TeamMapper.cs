// <copyright file="TeamMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Team;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Mapper
{
    /// <summary>
    /// Mapper for Team.
    /// </summary>
    public static class TeamMapper
    {
        /// <summary>
        /// Turns Team object into a TeamDto.
        /// </summary>
        /// <param name="team">Team to map.</param>
        /// <returns>TeamDto instance.</returns>
        public static TeamDto ToDto(Team team)
        {
            return new TeamDto(
                team.Id,
                team.Name);
        }

        /// <summary>
        /// Turns a List of Team objects into a List of TeamDto objects.
        /// </summary>
        /// <param name="team">List of Team objects.</param>
        /// <returns>List of TeamDto objects.</returns>
        public static List<TeamDto> ListToDto(List<Team> team)
        {
            return team.ConvertAll(ToDto);
        }
    }
}
