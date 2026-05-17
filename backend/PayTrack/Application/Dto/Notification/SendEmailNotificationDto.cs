// <copyright file="SendEmailNotificationDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.Notification
{
    /// <summary>
    /// DTO for sending an email notification to a given recipient.
    /// </summary>
    public sealed record class SendEmailNotificationDto(
        [property: Required, EmailAddress]
        string RecipientEmail,

        [property: Required]
        string Subject,

        [property: Required]
        string Body);
}
