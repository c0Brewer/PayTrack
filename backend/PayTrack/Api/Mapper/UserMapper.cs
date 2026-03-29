// <copyright file="UserMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Team;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Mapper
{
    /// <summary>
    /// Mapper for User.
    /// </summary>
    public static class UserMapper
    {
        /// <summary>
        /// Turns User object into a UserDto.
        /// </summary>
        /// <param name="user">User to map.</param>
        /// <returns>UserDto instance.</returns>
        public static UserDto ToDto(User user)
        {
            return new UserDto(
                user.Id,
                user.Name,
                user.Email,
                user.ProfilePictureUrl,
                user.Role,
                user.IsActive);
        }
    }
}
