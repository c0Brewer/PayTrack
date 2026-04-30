// <copyright file="UpdateTeamDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.Team
{
    /// <summary>
    /// Dto containing necessary information for updating a team.
    /// </summary>
    public sealed record class UpdateTeamDto(
        string? Name,
        string? Description,
        string? DisplayColor,
        IList<UpsertTeamBudgetEntryDto>? BudgetsToUpsert,
        IList<int>? BudgetIdsToDelete
    );
}
