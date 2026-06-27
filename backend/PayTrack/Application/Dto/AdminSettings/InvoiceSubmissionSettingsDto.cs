// <copyright file="InvoiceSubmissionSettingsDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Dto.AdminSettings
{
    /// <summary>
    /// Response DTO for admin-configurable invoice submission settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record InvoiceSubmissionSettingsDto(bool ReceiptExtractionEnabled);
}
