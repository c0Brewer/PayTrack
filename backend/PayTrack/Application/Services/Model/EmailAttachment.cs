// <copyright file="EmailAttachment.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Represents a file attachment for an email.
    /// </summary>
    /// <param name="FileName">The file name shown to the email recipient.</param>
    /// <param name="Content">The file content.</param>
    /// <param name="ContentType">The MIME content type.</param>
    public sealed record EmailAttachment(string FileName, byte[] Content, string ContentType);
}
