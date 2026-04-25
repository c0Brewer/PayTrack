// <copyright file="TeamMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Team;
using PayTrack.Application.Dto.User;
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
            // Clear the navigation on copied users to avoid Team -> User -> Team recursion while mapping.
            var membersToMap = team.Members
                .Select(member => new User
                {
                    Id = member.Id,
                    Name = member.Name,
                    Email = member.Email,
                    ProfilePictureUrl = member.ProfilePictureUrl,
                    TeamId = member.TeamId,
                    Team = null!,
                    Role = member.Role,
                    IsActive = member.IsActive,
                    CreatedAt = member.CreatedAt,
                })
                .ToList();

            var members = UserMapper.ListToDto(membersToMap);
            var budgets = BudgetMapper.CollectionToDto(team.Budgets);

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
            return team.ConvertAll(ToDto);
        }
    }
}