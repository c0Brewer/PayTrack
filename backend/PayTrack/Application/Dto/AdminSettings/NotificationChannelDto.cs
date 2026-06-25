// <copyright file="NotificationChannelDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Dto.AdminSettings
{
    /// <summary>
    /// Channel toggle state for a single notification event.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record NotificationChannelDto(bool SendEmail, bool SendSlack, bool SendPush);
}
