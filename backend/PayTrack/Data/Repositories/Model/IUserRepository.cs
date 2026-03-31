// <copyright file="IUserRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Data.Repositories.Model
{
    /// <summary>
    /// Repository for all User-related operations.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a specific User by their ID.
        /// </summary>
        /// <param name="email">id of User to find.</param>
        /// <returns>User with given ID.</returns>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Stores a User to the Database.
        /// </summary>
        /// <param name="user">User object to store.</param>
        /// <returns>Instance of created User object.</returns>
        Task<User> AddAsync(User user);
    }
}
