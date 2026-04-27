// <copyright file="BudgetMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>
using PayTrack.Application.Dto.Budget;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Mapper
{
    /// <summary>
    /// Mapper for Budget.
    /// </summary>
    public static class BudgetMapper
    {
        /// <summary>
        /// Turns Budget object into a BudgetDto.
        /// </summary>
        /// <param name="budget">Budget to map.</param>
        /// <returns>BudgetDto instance.</returns>
        public static BudgetDto ToDto(Budget budget)
        {
            return new BudgetDto(
                budget.Id,
                budget.TeamId,
                budget.CostCentreId,
                budget.TargetAmount,
                budget.PeriodStart,
                budget.PeriodEnd);
        }

        /// <summary>
        /// Turns a List of Budget objects into a List of BudgetDto objects.
        /// </summary>
        /// <param name="budget">List of Budget objects.</param>
        /// <returns>List of BudgetDto objects.</returns>
        public static List<BudgetDto> ListToDto(List<Budget> budget)
        {
            return budget.ConvertAll(ToDto);
        }
    }
}
