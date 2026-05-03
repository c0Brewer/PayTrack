// <copyright file="UpdatePaymentRequestByTeamDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Dto.Transaction;

namespace PayTrack.Application.Dto.PaymentRequestByTeam
{
    /// <summary>
    /// Dto containing necessary information for updating a PaymentRequestByTeam.
    /// </summary>
    public sealed record class UpdatePaymentRequestByTeamDto(
        [property: Required]
        UpdateTransactionDto Transaction);
}
