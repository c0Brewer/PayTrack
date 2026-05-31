// <copyright file="TransactionStatusHistoryMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Transaction;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Mapper
{
    /// <summary>
    /// Mapper for TransactionStatusHistory.
    /// </summary>
    public static class TransactionStatusHistoryMapper
    {
        /// <summary>
        /// Turns TransactionStatusHistory object into a TransactionStatusHistoryDto.
        /// </summary>
        /// <param name="transactionStatusHistory">TransactionStatusHistory to map.</param>
        /// <returns>TransactionStatusHistoryDto instance.</returns>
        public static TransactionStatusHistoryDto ToDto(TransactionStatusHistory transactionStatusHistory)
        {
            return new TransactionStatusHistoryDto(
                transactionStatusHistory.ChangedById,
                transactionStatusHistory.ChangedBy != null ? UserMapper.ToDto(transactionStatusHistory.ChangedBy) : null,
                transactionStatusHistory.Comment,
                transactionStatusHistory.FromStatus,
                transactionStatusHistory.ToStatus,
                transactionStatusHistory.ChangedAt);
        }

        /// <summary>
        /// Turns a List of TransactionStatusHistory objects into a List of TransactionStatusHistoryDto objects.
        /// </summary>
        /// <param name="transactionStatusHistory">List of TransactionStatusHistory objects.</param>
        /// <returns>List of TransactionStatusHistoryDto objects.</returns>
        public static List<TransactionStatusHistoryDto> ListToDto(List<TransactionStatusHistory> transactionStatusHistory)
        {
            return transactionStatusHistory.ConvertAll(ToDto);
        }
    }
}
