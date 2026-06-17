// <copyright file="UpdateCsvColumnSettingsRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Dto.AdminSettings
{
    /// <summary>
    /// Request DTO for updating admin-configurable CSV column name settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record UpdateCsvColumnSettingsRequestDto(
        [Required][MaxLength(255)] string NameColumn,
        [Required][MaxLength(255)] string SummeColumn);
}
