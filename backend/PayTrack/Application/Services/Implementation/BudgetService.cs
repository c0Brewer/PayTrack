// <copyright file="BudgetService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class BudgetService(IBudgetRepository repo) : IBudgetService
    {
        /// <summary>
        /// Repository for Budgets.
        /// </summary>
        private readonly IBudgetRepository repo = repo;

        /// <inheritdoc/>
        public async Task<Budget> CreateBudgetAsync(
            string name,
            string? description,
            int teamId,
            int costCentreId,
            int seasonId,
            decimal targetAmount,
            DateTime periodStart,
            DateTime periodEnd)
        {
            ValidatePeriod(periodStart, periodEnd);

            return await this.repo.AddAsync(
                name,
                description,
                teamId,
                costCentreId,
                seasonId,
                targetAmount,
                DateTime.SpecifyKind(periodStart, DateTimeKind.Utc),
                DateTime.SpecifyKind(periodEnd, DateTimeKind.Utc));
        }

        /// <inheritdoc/>
        public async Task<Budget?> GetBudgetByIdAsync(int id)
        {
            return await this.repo.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<(List<Budget> budget, int totalCount)> GetBudgetsAsync(GetBudgetQuery? query = null)
        {
            return await this.repo.GetAllAsync(query);
        }

        /// <inheritdoc/>
        public async Task<Budget> UpdateBudgetAsync(
            int id,
            string? name = null,
            string? description = null,
            int? teamId = null,
            int? costCentreId = null,
            int? seasonId = null,
            decimal? targetAmount = null,
            DateTime? periodStart = null,
            DateTime? periodEnd = null)
        {
            if (periodStart.HasValue && periodEnd.HasValue)
            {
                ValidatePeriod(periodStart.Value, periodEnd.Value);
            }

            return await this.repo.UpdateAsync(
                id,
                name,
                description,
                teamId,
                costCentreId,
                seasonId,
                targetAmount,
                periodStart.HasValue ? DateTime.SpecifyKind(periodStart.Value, DateTimeKind.Utc) : null,
                periodEnd.HasValue ? DateTime.SpecifyKind(periodEnd.Value, DateTimeKind.Utc) : null);
        }

        /// <inheritdoc/>
        public async Task DeleteBudgetAsync(int id)
        {
            await this.repo.DeleteAsync(id);
        }

        private static void ValidatePeriod(DateTime periodStart, DateTime periodEnd)
        {
            if (periodEnd < periodStart)
            {
                throw new InvalidStateException("Budget period end must be after period start.");
            }
        }
    }
}
