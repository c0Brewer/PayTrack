// <copyright file="RequestChangesPaymentRequestByUserDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// Dto containing change request data supplied by finance.
    /// </summary>
    public sealed record class RequestChangesPaymentRequestByUserDto(
        [property: Required]
        [property: MinLength(3)]
        [property: MaxLength(1000)]
        string Reason);
}
