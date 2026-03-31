// <copyright file="UserHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Api.Mapper;
using PayTrack.Application.Dto.User;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler which gets called from Endpoint to manage incoming User-related requests.
    /// </summary>
    public static class UserHandler
    {
        /// <summary>
        /// Returns all Users.
        /// </summary>
        /// <param name="limit">Limit to query.</param>
        /// <param name="offset">Offset to query.</param>
        /// <param name="userService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<List<UserDto>>, BadRequest<ProblemDetails>, ProblemHttpResult>> GetUsersAsync(
            [FromQuery] int? limit,
            [FromQuery] int? offset,
            IUserService userService)
        {
            var userList = await userService.GetAllAsync(limit, offset);

            var userListDto = UserMapper.ListToDto(userList);

            return TypedResults.Ok(userListDto);
        }

        /// <summary>
        /// Returns a User by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="includeTeam">Whether to load Team as well.</param>
        /// <param name="includeBankAccounts">Whether to load Bank Accounts as well.</param>
        /// <param name="userService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<UserDto>, BadRequest<ProblemDetails>, NotFound<ProblemDetails>, ProblemHttpResult>> GetUserByIdAsync(
            [FromRoute] int id,
            [FromQuery] bool includeTeam,
            [FromQuery] bool includeBankAccounts,
            IUserService userService)
        {
            var user = await userService.GetUserByIdAsync(id, includeTeam, includeBankAccounts) ?? throw new NotFoundException("User could not be found");

            var userDto = UserMapper.ToDto(user);

            return TypedResults.Ok(userDto);
        }

        /// <summary>
        /// Updates a User.
        /// </summary>
        /// <param name="id">Id of the User to update.</param>
        /// <param name="updateUserDto">request for User creation.</param>
        /// <param name="userService">Dependency-Injected Service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<UserDto>, BadRequest<ProblemDetails>, ProblemHttpResult>> UpdateUserAsync(
            [FromRoute] int id,
            [FromBody] UpdateUserDto updateUserDto,
            IUserService userService)
        {
            var updatedUser = await userService.UpdateUserAsync(
                    id,
                    updateUserDto.IsActive,
                    updateUserDto.TeamId,
                    updateUserDto.Role);

            var updatedUserDto = UserMapper.ToDto(updatedUser);

            return TypedResults.Ok(updatedUserDto);
        }
    }
}
