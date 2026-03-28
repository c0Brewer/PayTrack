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
        /// Gets a specific User by their ID.
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
    }
}
