// <copyright file="ApprovePaymentRequestByUserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// Dto containing approval data supplied by finance.
    /// </summary>
    public sealed record class ApprovePaymentRequestByUserDto(
        [property: Required]
        int CostCentreId,

        [property: MaxLength(1000)]
        string? Reason);
}
