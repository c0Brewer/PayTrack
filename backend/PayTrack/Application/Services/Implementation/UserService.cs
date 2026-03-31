// <copyright file="UserService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class UserService(IUserRepository repo) : IUserService
    {
        /// <summary>
        /// Repository for Users.
        /// </summary>
        private readonly IUserRepository repo = repo;

        /// <inheritdoc/>
        public async Task<List<User>> GetUserAsync(int? limit = null, int? offset = null)
        {
            return await this.repo.GetUserAsync(limit, offset);
        }

        /// <inheritdoc/>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await this.repo.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await this.repo.GetByEmailAsync(email);
        }

        /// <inheritdoc/>
        public async Task<User> CreateUserAsync(
            string name,
            string email,
            string? profilePictureUrl,
            bool isActive = true)
        {
            var team = new User
            {
                Name = name,
                Email = email,
                ProfilePictureUrl = profilePictureUrl,
                IsActive = isActive,
            };

            return await this.repo.AddAsync(team);
        }

        /// <inheritdoc/>
        public async Task<User> UpdateUserAsync(int id, bool? isActive = null, int? teamId = null, Role? role = null)
        {
            return await this.repo.UpdateAsync(id, isActive, teamId, role);
        }
    }
}
