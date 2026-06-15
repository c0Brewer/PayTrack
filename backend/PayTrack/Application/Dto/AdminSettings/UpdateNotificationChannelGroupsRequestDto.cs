// <copyright file="UpdateNotificationChannelGroupsRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Dto.AdminSettings
{
    /// <summary>
    /// Request DTO for updating notification channel toggles for all event types.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record UpdateNotificationChannelGroupsRequestDto(
        [Required] NotificationChannelDto Creation,
        [Required] NotificationChannelDto Confirmation,
        [Required] NotificationChannelDto Reminders,
        [Required] NotificationChannelDto Deletion);
}
