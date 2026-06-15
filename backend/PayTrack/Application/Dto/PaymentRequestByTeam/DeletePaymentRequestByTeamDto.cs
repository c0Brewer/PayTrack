// <copyright file="DeletePaymentRequestByTeamDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.PaymentRequestByTeam
{
    /// <summary>
    /// Dto containing the optional reason when deleting a PaymentRequestByTeam.
    /// </summary>
    public sealed record class DeletePaymentRequestByTeamDto(
        [property: MaxLength(1000)] string? Reason);
}
