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
        public async Task<User> CreateUserAsync(
            string name,
            string email,
            string? profilePictureUrl)
        {
            var team = new User
            {
                Name = name,
                Email = email,
                ProfilePictureUrl = profilePictureUrl,
            };

            return await this.repo.AddAsync(team);
        }

        /// <inheritdoc/>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await this.repo.GetByEmailAsync(email);
        }
    }
}
