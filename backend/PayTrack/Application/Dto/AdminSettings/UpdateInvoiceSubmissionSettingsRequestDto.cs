// <copyright file="UpdateInvoiceSubmissionSettingsRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Dto.AdminSettings
{
    /// <summary>
    /// Request DTO for updating invoice submission settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record UpdateInvoiceSubmissionSettingsRequestDto(
        [Required] bool ReceiptExtractionEnabled);
}
