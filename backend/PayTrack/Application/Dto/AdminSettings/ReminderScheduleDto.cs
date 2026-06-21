// <copyright file="ReminderScheduleDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Dto.AdminSettings
{
    /// <summary>
    /// Response DTO for admin-configurable reminder schedule settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record ReminderScheduleDto(int[] DaysBeforeDue, int RunAtHourUtc, int RunAtMinuteUtc, int EmailDelayMs);
}
