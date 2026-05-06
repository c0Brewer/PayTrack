// <copyright file="TransactionStatusHistoryDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.Transaction
{
    /// <summary>
    /// Dto containing necessary information about a Transaction status history.
    /// </summary>
    public sealed record class TransactionStatusHistoryDto(
            [property: Required]
            int ChangedById,

            [property: Required]
            string? Comment,

            [property: Required]
            TransactionStatus FromStatus,

            [property: Required]
            TransactionStatus ToStatus,

            [property: Required]
            DateTime ChangedAt);
}
