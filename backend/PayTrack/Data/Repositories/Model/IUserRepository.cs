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
        /// Gets all Users with optional filtering.
        /// </summary>
        /// <param name="name">Name to include in search.</param>
        /// <param name="email">Email to include in search.</param>
        /// <param name="teamName">TeamName to include in search.</param>
        /// <param name="role">Role to include in search.</param>
        /// <param name="isActive">IsActive state to include in search.</param>
        /// <param name="includeTeam">Whether to include team in search.</param>
        /// <param name="limit">Limit to include in search.</param>
        /// <param name="offset">Offset to include in search.</param>
        /// <returns>User with given ID.</returns>
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
        /// <returns>User with given ID.</returns>
        Task<User?> GetByIdAsync(
            int id,
            bool? includeTeam = false,
            bool? includeBankAccounts = false);

        /// <summary>
        /// Gets a specific User by their Email.
        /// </summary>
        /// <param name="email">Email of User to find.</param>
        /// <returns>User with given Email.</returns>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Stores a User to the Database.
        /// </summary>
        /// <param name="name">Name to store.</param>
        /// <param name="email">email to store.</param>
        /// <param name="profilePictureUrl">url to profile picture.</param>
        /// <param name="isActive">isActive state.</param>
        /// <returns>Instance of created User object.</returns>
        Task<User> AddAsync(string name, string email, string? profilePictureUrl, bool isActive = true);

        /// <summary>
        /// Updates a User with optional values.
        /// </summary>
        /// <param name="id">Id of User to update.</param>
        /// <param name="name">Name to (optionally) set.</param>
        /// <param name="isActive">IsActive status to (optionally) set.</param>
        /// <param name="teamId">TeamId to (optionally) set.</param>
        /// <param name="role">Role to (optionally) set.</param>
        /// <returns>Instance of created User object.</returns>
        Task<User> UpdateAsync(int id, string? name, bool? isActive = null, int? teamId = null, Role? role = null);
    }
}
