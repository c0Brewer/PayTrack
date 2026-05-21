// <copyright file="PaymentRequestByTeamDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Dto.CostCentre;
using PayTrack.Application.Dto.Team;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Dto.User;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.PaymentRequestByTeam
{
    /// <summary>
    /// Dto containing necessary information about a PaymentRequestByTeam
    /// </summary>
    public sealed record class PaymentRequestByTeamDto(
        [property: Required]
        int Id,

        UserDto? User,

        [property: Required]
        decimal Amount,

        string? PurposeOfPayment,

        string? PaymentReference,

        [property: Required]
        TransactionStatus Status,

        CostCentreDto? CostCentre,

        TeamDto? Team,

        PaymentDirection? PaymentDirection,

        ICollection<TransactionStatusHistoryDto>? StatusHistory,

        DateTime? CreatedAt,

        DateTime? PaidAt,

        DateTime? DueDate,

        UserDto? CreatedByUser);
}
