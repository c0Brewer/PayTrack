// <copyright file="SendSlackNotificationDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Notification
{
    /// <summary>
    /// DTO for sending a Slack DM notification. The recipient's Slack user is resolved via their email address.
    /// </summary>
    public sealed record class SendSlackNotificationDto(
        [property: Required, EmailAddress]
        string RecipientEmail,

        [property: Required]
        string Message);
}
