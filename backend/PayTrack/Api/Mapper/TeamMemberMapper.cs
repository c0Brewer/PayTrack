// <copyright file="TeamMemberMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Linq;
using PayTrack.Application.Dto.Team;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Mapper
{
    /// <summary>
    /// Mapper for TeamMember.
    /// </summary>
    public static class TeamMemberMapper
    {
        /// <summary>
        /// Turns a list of User objects into a list of TeamMemberDto objects.
        /// </summary>
        /// <param name="user">List of User objects.</param>
        /// <returns>List of TeamMemberDto objects.</returns>
        public static List<TeamMemberDto> ListToDto(ICollection<User> user)
        {
            return user.Select(ToDto).ToList();
        }

        /// <summary>
        /// Turns a User object into a TeamMemberDto.
        /// </summary>
        /// <param name="user">User to map.</param>
        /// <returns>TeamMemberDto instance.</returns>
        private static TeamMemberDto ToDto(User user)
        {
            return new TeamMemberDto(
                user.Id,
                user.Name,
                user.Email,
                user.ProfilePictureUrl,
                user.Role,
                user.IsActive);
        }
    }
}
