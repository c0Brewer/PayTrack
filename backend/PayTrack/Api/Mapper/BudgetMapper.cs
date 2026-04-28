// <copyright file="BudgetMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>
using PayTrack.Application.Dto.Budget;

using System.Linq;
using PayTrack.Application.Dto.Team;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Mapper
{
    /// <summary>
    /// Mapper for Budget.
    /// </summary>
    public static class BudgetMapper
    {
        /// <summary>
        /// Turns a list of Budget objects into a list of TeamBudgetDto objects.
        /// </summary>
        /// <param name="budget">List of Budget objects.</param>
        /// <returns>List of TeamBudgetDto objects.</returns>
        public static List<BudgetDto> CollectionToDto(ICollection<Budget> budget)
        {
            return budget.Select(ToDto).ToList();
        }

        /// <summary>
        /// Turns a Budget object into a TeamBudgetDto.
        /// </summary>
        /// <param name="budget">Budget to map.</param>
        /// <returns>TeamBudgetDto instance.</returns>
        private static BudgetDto ToDto(Budget budget)
        {
            return new BudgetDto(
                budget.Id,
                budget.CostCentreId,
                budget.TargetAmount,
                budget.PeriodStart,
                budget.PeriodEnd);
        }
    }
}
