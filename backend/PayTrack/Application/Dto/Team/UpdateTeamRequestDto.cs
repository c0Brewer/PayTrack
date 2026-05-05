// <copyright file="UpdateTeamRequestDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using PayTrack.Application.Dto.Budget;

namespace PayTrack.Application.Dto.Team
{
    /// <summary>
    /// Dto containing necessary information for updating a team.
    /// </summary>
    public sealed record class UpdateTeamRequestDto(
        [property: MinLength(3)]
        string? Name,

        [property: MinLength(3)]
        string? Description,

        [property: MaxLength(7)]
        [property: RegularExpression("^#[0-9A-Fa-f]{6}$")]
        string? DisplayColor,

        IList<UpsertTeamBudgetEntryDto>? BudgetsToUpsert,

        IList<int>? BudgetIdsToDelete
    );
}
