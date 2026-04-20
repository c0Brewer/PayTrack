// <copyright file="UpdateCostCentreRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.CostCentre
{
    /// <summary>
    /// Dto for partial update of a cost center. All fields are optional.
    /// </summary>
    public sealed record class UpdateCostCentreRequestDto(
        [property: MinLength(3)]
        string? Name,

        [property: MinLength(3)]
        string? Description,

        [property: MinLength(3)]
        string? DisplayColor);
}
