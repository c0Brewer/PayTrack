// <copyright file="UserRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Dto.User;
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
        public async Task<(List<User> user, int totalCount)> GetAllAsync(GetUserQuery? query = null)
        {
            IQueryable<User> dbQuery = this.context.User.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query?.Name))
            {
                dbQuery = dbQuery.Where(u => EF.Functions.Like(u.Name, $"%{query.Name}%"));
            }

            // Filter by email
            if (!string.IsNullOrWhiteSpace(query?.Email))
            {
                dbQuery = dbQuery.Where(u => EF.Functions.Like(u.Email, $"%{query.Email}%"));
            }

            // Filter by team name (need Include if navigation property)
            if (!string.IsNullOrWhiteSpace(query?.TeamName))
            {
                dbQuery = dbQuery.Include(u => u.Team)
                             .Where(u => u.Team != null && EF.Functions.Like(u.Team.Name, $"%{query.TeamName}%"));
            }

            // Filter by role
            if (query?.Role.HasValue == true)
            {
                dbQuery = dbQuery.Where(u => u.Role == query.Role.Value);
            }

            // Filter by active status
            if (query?.IsActive.HasValue == true)
            {
                dbQuery = dbQuery.Where(u => u.IsActive == query.IsActive.Value);
            }

            // Calculate total count before limit / offset
            var totalCount = await dbQuery.CountAsync();

            if (query?.Offset.HasValue == true)
            {
                dbQuery = dbQuery.Skip(query.Offset.Value);
            }

            if (query?.Limit.HasValue == true)
            {
                dbQuery = dbQuery.Take(query.Limit.Value);
            }

            if (query?.IncludeTeam.HasValue == true)
            {
                dbQuery = dbQuery.Include(u => u.Team);
            }

            // Could potentially add other ordering logic here as well
            var items = await dbQuery.OrderByDescending(u => u.CreatedAt).ToListAsync();

            return (items, totalCount);
        }

        /// <inheritdoc/>
        public async Task<User?> GetByIdAsync(
            int id,
            GetUserQueryById? query = null)
        {
            IQueryable<User> dbQuery = this.context.User.AsQueryable();

            if (query?.IncludeTeam.HasValue == true)
            {
                dbQuery = dbQuery.Include(u => u.Team);
            }

            if (query?.IncludeBankAccounts.HasValue == true)
            {
                dbQuery = dbQuery.Include(u => u.BankAccounts);
            }

            return await dbQuery.FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <inheritdoc/>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await this.context.User
                .Include(u => u.BankAccounts)
                .FirstOrDefaultAsync(u => u.Email == email);
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

        /// <inheritdoc/>
        public async Task<User> UpdateBankInformationSkippedAsync(int userId, bool bankInformationSkipped)
        {
            var user = await this.context.User
                .Include(u => u.BankAccounts)
                .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new NotFoundException($"User with id {userId} not found.");

            user.BankInformationSkipped = bankInformationSkipped;

            var res = await this.context.SaveChangesAsync();

            if (res != 1)
            {
                throw new InternalErrorException($"Updating bank information state failed. Updated {res} rows.");
            }

            return user;
        }
    }
}
