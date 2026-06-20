// <copyright file="CsvColumnSettingsDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Dto.AdminSettings
{
    /// <summary>
    /// Response DTO for the admin-configurable CSV column name settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record CsvColumnSettingsDto(string NameColumn, string SummeColumn);
}
