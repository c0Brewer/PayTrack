// <copyright file="UpdateReminderScheduleRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Dto.AdminSettings
{
    /// <summary>
    /// Request DTO for updating the reminder schedule settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record UpdateReminderScheduleRequestDto(
        [Required] int[] DaysBeforeDue,
        [Required][Range(0, 23)] int RunAtHourUtc,
        [Required][Range(0, 59)] int RunAtMinuteUtc,
        [Required][Range(0, int.MaxValue)] int EmailDelayMs);
}
