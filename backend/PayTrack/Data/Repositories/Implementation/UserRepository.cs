// <copyright file="UserRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        /// <summary>
        /// Database Context for accessing the DB.
        /// </summary>
        private readonly AppDbContext context = context;

        /// <inheritdoc/>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return this.context.User.FirstOrDefault(u => u.Email == email);
        }

        /// <inheritdoc/>
        public async Task<User> AddAsync(User user)
        {
            this.context.User.Add(user);
            await this.context.SaveChangesAsync(); // TODO: Check return value if succesful?
            return user;
        }
    }
}
