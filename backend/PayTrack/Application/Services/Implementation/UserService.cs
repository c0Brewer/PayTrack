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
        public async Task<(List<User> user, int totalCount)> GetAllAsync(
            string? name = null,
            string? email = null,
            string? teamName = null,
            Role? role = null,
            bool? isActive = null,
            bool? includeTeam = null,
            int? limit = null,
            int? offset = null)
        {
            return await this.repo.GetAllAsync(
                name: name,
                email: email,
                teamName: teamName,
                role: role,
                isActive: isActive,
                includeTeam: includeTeam,
                limit: limit,
                offset: offset);
        }

        /// <inheritdoc/>
        public async Task<User?> GetUserByIdAsync(int id, bool includeTeam = false, bool includeBankAccounts = false)
        {
            return await this.repo.GetByIdAsync(id, includeTeam, includeBankAccounts);
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
            return await this.repo.AddAsync(name, email, profilePictureUrl, isActive);
        }

        /// <inheritdoc/>
        public async Task<User> UpdateUserAsync(int id, string? name, bool? isActive = null, int? teamId = null, Role? role = null)
        {
            return await this.repo.UpdateAsync(id, name, isActive, teamId, role);
        }
    }
}
