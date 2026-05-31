// <copyright file="TransactionMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Transaction;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Mapper
{
    /// <summary>
    /// Mapper for Transaction.
    /// </summary>
    public static class TransactionMapper
    {
        /// <summary>
        /// Turns Transaction object into a TransactionDto.
        /// </summary>
        /// <param name="transaction">Transaction to map.</param>
        /// <returns>TransactionDto instance.</returns>
        public static TransactionDto ToDto(Transaction transaction)
        {
            return new TransactionDto
            {
                UserId = transaction.Id,
                TeamId = transaction.TeamId,
                Amount = transaction.Amount,
                PurposeOfPayment = transaction.PurposeOfPayment,
                PaymentReference = transaction.PaymentReference,
                Status = transaction.Status,
                BudgetId = transaction.BudgetId,
                PaidAt = transaction.PaidAt,
            };
        }

        /// <summary>
        /// Turns a List of Transaction objects into a List of TransactionDto objects.
        /// </summary>
        /// <param name="transactions">List of Transaction objects.</param>
        /// <returns>List of TransactionDto objects.</returns>
        public static List<TransactionDto> ListToDto(List<Transaction> transactions)
        {
            return transactions.ConvertAll(ToDto);
        }
    }
}
