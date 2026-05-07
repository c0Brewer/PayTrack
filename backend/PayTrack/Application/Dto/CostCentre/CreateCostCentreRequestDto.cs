// <copyright file="CreateCostCentreRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Dto.Budget;

namespace PayTrack.Application.Dto.CostCentre
{
    /// <summary>
    /// Dto containing necessary information for creating a cost center.
    /// </summary>
    public sealed record class CreateCostCentreRequestDto(
        [property: Required]
        [property: MinLength(3)]
        string Name,

        [property: MinLength(3)]
        string? Description,

        [property: MinLength(3)]
        string? DisplayColor,

        IList<CreateCostCentreBudgetEntryDto>? Budgets);
}
