// <copyright file="BudgetMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Budget;
using PayTrack.Data.Entities;
using static PayTrack.Data.Entities.PaymentDirection;
using static PayTrack.Data.Entities.TransactionStatus;

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
            return [.. budget.Select(ToDto)];
        }

        /// <summary>
        /// Turns a Budget object into a TeamBudgetDto.
        /// </summary>
        /// <param name="budget">Budget to map.</param>
        /// <returns>TeamBudgetDto instance.</returns>
        public static BudgetDto ToDto(Budget budget)
        {
            var paidTransactions = budget.Transactions.Where(t => t.Status == Paid);
            var approvedTransactions = budget.Transactions.Where(t => t.Status == Approved);
            decimal paidAmount = 0;
            decimal approvedAmount = 0;

            if (budget.Type == BudgetType.Expense)
            {
                paidAmount = paidTransactions.Where(t => t.PaymentDirection == Out).Sum(t => t.Amount)
                               - paidTransactions.Where(t => t.PaymentDirection == In).Sum(t => t.Amount);
                approvedAmount = approvedTransactions.Where(t => t.PaymentDirection == Out).Sum(t => t.Amount)
                                   - approvedTransactions.Where(t => t.PaymentDirection == In).Sum(t => t.Amount);
            }
            else if (budget.Type == BudgetType.Income)
            {
                paidAmount = paidTransactions.Where(t => t.PaymentDirection == In).Sum(t => t.Amount)
                               - paidTransactions.Where(t => t.PaymentDirection == Out).Sum(t => t.Amount);
                approvedAmount = approvedTransactions.Where(t => t.PaymentDirection == In).Sum(t => t.Amount)
                                   - approvedTransactions.Where(t => t.PaymentDirection == Out).Sum(t => t.Amount);
            }

            return new BudgetDto(
                budget.Id,
                budget.Name,
                budget.Description,
                budget.TeamId,
                budget.CostCentreId,
                budget.SeasonId,
                budget.TargetAmount,
                budget.PeriodStart,
                budget.PeriodEnd,
                budget.Type,
                [.. budget.Transactions.Select(t => t.Id)],
                paidAmount,
                approvedAmount);
        }
    }
}
