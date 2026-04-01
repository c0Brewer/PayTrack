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
            IQueryable<User> query = this.context.User.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(u => EF.Functions.Like(u.Name, $"%{name}%"));
            }

            // Filter by email
            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(u => EF.Functions.Like(u.Email, $"%{email}%"));
            }

            // Filter by team name (need Include if navigation property)
            if (!string.IsNullOrWhiteSpace(teamName))
            {
                query = query.Include(u => u.Team)
                             .Where(u => u.Team != null && EF.Functions.Like(u.Team.Name, $"%{teamName}%"));
            }

            // Filter by role
            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            // Filter by active status
            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            // Calculate total count before limit / offset
            var totalCount = await query.CountAsync();

            if (offset.HasValue)
            {
                query = query.Skip(offset.Value);
            }

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            if (includeTeam.HasValue)
            {
                query = query.Include(u => u.Team);
            }

            // Could potentially add other ordering logic here as well
            var items = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

            return (items, totalCount);
        }

        /// <inheritdoc/>
        public async Task<User?> GetByIdAsync(
            int id,
            bool? includeTeam = false,
            bool? includeBankAccounts = false)
        {
            IQueryable<User> query = this.context.User.AsQueryable();

            if (includeTeam.HasValue)
            {
                query = query.Include(u => u.Team);
            }

            if (includeBankAccounts.HasValue)
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
        public async Task<User> AddAsync(string name, string email, string? profilePictureUrl, bool isActive = true)
        {
            var count = await this.context.User.CountAsync();

            var user = new User
            {
                Name = name,
                Email = email,
                ProfilePictureUrl = profilePictureUrl,
                IsActive = isActive,

                // Set the first user in the system as Admin
                Role = count == 0 ? Role.Admin : Role.RegularUser,
            };

            this.context.User.Add(user);
            int res = await this.context.SaveChangesAsync();

            if (res != 1)
            {
                throw new InternalErrorException($"Saving User did not end as expected. Saved {res} user.");
            }

            return user;
        }

        /// <inheritdoc/>
        public async Task<User> UpdateAsync(int id, string? name, bool? isActive = null, int? teamId = null, Role? role = null)
        {
            var user = await this.context.User.FirstOrDefaultAsync(u => u.Id == id) ?? throw new NotFoundException($"User with id {id} not found.");

            if (name != null)
            {
                user.Name = name;
            }

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
