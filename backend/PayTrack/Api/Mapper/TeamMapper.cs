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
        /// <param name="includeMembers">Whether to include mapped team members.</param>
        /// <param name="includeBudgets">Whether to include mapped team budgets.</param>
        /// <returns>TeamDto instance.</returns>
        public static TeamDto ToDto(Team team, bool includeMembers = false, bool includeBudgets = false)
        {
            // By default, do not include members; map them only if includeMembers is true
            List<TeamMemberDto>? members = null;
            List<TeamBudgetDto>? budgets = null;
            if (includeMembers)
            {
                members = TeamMemberMapper.ListToDto(team.Members);
            }

            if (includeBudgets)
            {
                budgets = TeamBudgetMapper.ListToDto(team.Budgets);
            }

            return new TeamDto(
                team.Id,
                team.Name,
                team.Description,
                team.DisplayColor,
                members,
                budgets);
        }

        /// <summary>
        /// Turns a List of Team objects into a List of TeamDto objects.
        /// </summary>
        /// <param name="team">List of Team objects.</param>
        /// <param name="includeMembers">Whether to include mapped team members.</param>
        /// <param name="includeBudgets">Whether to include mapped team budgets.</param>
        /// <returns>List of TeamDto objects.</returns>
        public static List<TeamDto> ListToDto(List<Team> team, bool includeMembers = false, bool includeBudgets = false)
        {
            return team.ConvertAll(t => ToDto(t, includeMembers, includeBudgets));
        }
    }
}
