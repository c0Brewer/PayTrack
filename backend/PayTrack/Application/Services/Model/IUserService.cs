// <copyright file="IUserService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service which handles User-related requests.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gets all User with an optional offset and limit.
        /// </summary>
        /// <param name="name">Name to include in search.</param>
        /// <param name="email">Email to include in search.</param>
        /// <param name="teamName">TeamName to include in search.</param>
        /// <param name="role">Role to include in search.</param>
        /// <param name="isActive">IsActive state to include in search.</param>
        /// <param name="includeTeam">Whether to include team in search.</param>
        /// <param name="limit">Limit to include in search.</param>
        /// <param name="offset">Offset to include in search.</param>
        /// <returns>List of User.</returns>
        Task<(List<User> user, int totalCount)> GetAllAsync(
            string? name = null,
            string? email = null,
            string? teamName = null,
            Role? role = null,
            bool? isActive = null,
            bool? includeTeam = null,
            int? limit = null,
            int? offset = null);

        /// <summary>
        /// Gets a specific User by their ID.
        /// </summary>
        /// <param name="id">id of User to find.</param>
        /// <param name="includeTeam">Indicate whether Teams should be loaded as well.</param>
        /// <param name="includeBankAccounts">Indicate whether Bank Accounts should be loaded as well.</param>
        /// <returns>User with given id.</returns>
        Task<User?> GetUserByIdAsync(int id, bool includeTeam = false, bool includeBankAccounts = false);

        /// <summary>
        /// Gets a specific User by their Email.
        /// </summary>
        /// <param name="email">email of User to find.</param>
        /// <returns>User with given email.</returns>
        Task<User?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Creates a User using the given input.
        /// </summary>
        /// <param name="name">name of user.</param>
        /// <param name="email">email of user.</param>
        /// <param name="profilePictureUrl">url to profile picture of user.</param>
        /// <param name="isActive">indicates whether a user is currently set to active or not.</param>
        /// <returns>Instance of created User object.</returns>
        Task<User> CreateUserAsync(string name, string email, string? profilePictureUrl, bool isActive = true);

        /// <summary>
        /// Update a User using the given input.
        /// </summary>
        /// <param name="id">The id of the user to update.</param>
        /// <param name="name">The new name that should be set for the user.</param>
        /// <param name="isActive">The new isActive status that should be set for the user.</param>
        /// <param name="teamId">The id of the new team to assign the User to.</param>
        /// <param name="role">The new role that the User should be assigned.</param>
        /// <returns>Instance of created User object.</returns>
        Task<User> UpdateUserAsync(int id, string? name, bool? isActive = null, int? teamId = null, Role? role = null);
    }
}
