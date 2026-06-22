// <copyright file="NotificationChannelGroupsDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Dto.AdminSettings
{
    /// <summary>
    /// Response DTO grouping channel toggles for all notification event types.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record NotificationChannelGroupsDto(
        NotificationChannelDto Creation,
        NotificationChannelDto Confirmation,
        NotificationChannelDto Reminders,
        NotificationChannelDto Deletion,
        NotificationChannelDto InvoiceApproval,
        NotificationChannelDto InvoiceRejection,
        NotificationChannelDto InvoiceChangesRequested,
        NotificationChannelDto InvoicePaymentCompleted);
}
