// <copyright file="MarkAsPaidPaymentRequestByTeamDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.PaymentRequestByTeam
{
    /// <summary>
    /// Dto containing the optional comment when marking a PaymentRequestByTeam as paid.
    /// </summary>
    public sealed record class MarkAsPaidPaymentRequestByTeamDto(
        [property: MaxLength(1000)] string? Comment);
}
