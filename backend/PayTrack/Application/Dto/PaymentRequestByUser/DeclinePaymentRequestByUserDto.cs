// <copyright file="DeclinePaymentRequestByUserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// Dto containing decline data supplied by finance.
    /// </summary>
    public sealed record class DeclinePaymentRequestByUserDto(
        [property: Required]
        [property: MinLength(3)]
        [property: MaxLength(1000)]
        string Reason);
}
