// <copyright file="UserMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Team;
using PayTrack.Application.Dto.User;
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
            TeamDto? teamDto = null;
            if (user.Team != null)
            {
                teamDto = TeamMapper.ToDto(user.Team);
            }

            var bankAccounts = user.BankAccounts
                .Select(BankAccountMapper.ToDto)
                .ToList();

            return new UserDto(
                user.Id,
                user.Name,
                user.Email,
                user.ProfilePictureUrl,
                user.Role,
                user.IsActive,
                teamDto,
                user.BankInformationSkipped,
                bankAccounts.Count > 0,
                bankAccounts);
        }

        /// <summary>
        /// Turns a List of User objects into a List of UserDto objects.
        /// </summary>
        /// <param name="user">List of User objects.</param>
        /// <returns>List of UserDto objects.</returns>
        public static List<UserDto> ListToDto(List<User> user)
        {
            return user.ConvertAll(ToDto);
        }
    }
}
