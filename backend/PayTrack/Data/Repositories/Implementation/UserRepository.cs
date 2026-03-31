// <copyright file="UserRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Exceptions;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class UserRepository(AppDbContext _context) : IUserRepository
    {
        /// <summary>
        /// Database Context for accessing the DB.
        /// </summary>
        private readonly AppDbContext context = _context;

        /// <inheritdoc/>
        public async Task<List<User>> GetAllAsync(
            int? limit = null,
            int? offset = null)
        {
            IQueryable<User> query = this.context.User.AsQueryable();

            if (offset.HasValue)
            {
                query = query.Skip(offset.Value);
            }

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return await query.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<User?> GetByIdAsync(int id, bool includeTeam = false, bool includeBankAccounts = false)
        {
            IQueryable<User> query = this.context.User.AsQueryable();

            if (includeTeam)
            {
                query = query.Include(u => u.Team);
            }

            if (includeBankAccounts)
            {
                query = query.Include(u => u.BankAccounts);
            }

            return await query.FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <inheritdoc/>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await this.context.User.FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <inheritdoc/>
        public async Task<User> AddAsync(User user)
        {
            this.context.User.Add(user);
            int res = await this.context.SaveChangesAsync();

            if (res != 1)
            {
                throw new InternalErrorException($"Saving User did not end as expected. Saved {res} user.");
            }

            return user;
        }

        /// <inheritdoc/>
        public async Task<User> UpdateAsync(int id, bool? isActive = null, int? teamId = null, Role? role = null)
        {
            var user = await this.context.User.FirstOrDefaultAsync(u => u.Id == id) ?? throw new NotFoundException($"User with id {id} not found.");

            if (isActive.HasValue)
            {
                user.IsActive = isActive.Value;
            }

            if (teamId.HasValue)
            {
                user.TeamId = teamId.Value;
            }

            if (role.HasValue)
            {
                user.Role = role.Value;
            }

            int res = await this.context.SaveChangesAsync();

            if (res != 1)
            {
                throw new InternalErrorException($"Updating user failed. Updated {res} rows.");
            }

            return user;
        }
    }
}
