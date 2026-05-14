// <copyright file="CreatePaymentRequestByTeamDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Dto.Transaction;

namespace PayTrack.Application.Dto.PaymentRequestByTeam
{
    /// <summary>
    /// Dto containing necessary information for creating a PaymentRequestByTeam.
    /// </summary>
    public sealed record class CreatePaymentRequestByTeamDto(
        [property: Required]
        CreateTransactionDto Transaction,

        [property: Required]
        int UserToAssignToId,

        [property: Required]
        DateTime DueDate,

        int? CostCentreId);
}
