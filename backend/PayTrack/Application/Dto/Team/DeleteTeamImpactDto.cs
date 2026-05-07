// <copyright file="DeleteTeamImpactDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.Team
{
    /// <summary>
    /// Dto containing necessary information about the impact of deleting a Team.
    /// </summary>
    public record DeleteTeamImpactDto(
        int TeamId,
        string TeamName,
        bool CanDelete,
        int AffectedUserCount,
        int BlockingBudgetCount,
        int BlockingTransactionCount,
        int InvoiceCount,
        string WarningMessage
        );
}
