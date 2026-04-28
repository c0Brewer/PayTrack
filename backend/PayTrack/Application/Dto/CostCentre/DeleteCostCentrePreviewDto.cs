// <copyright file="DeleteCostCentrePreviewDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Dto.CostCentre
{
    /// <summary>
    /// Dto returned before deletion to warn about linked entities.
    /// </summary>
    public sealed record class DeleteCostCentrePreviewDto(
        [property: Required]
        string CostCentreName,

        [property: Required]
        int BudgetCount,

        [property: Required]
        int TransactionCount,

        [property: Required]
        int AffectedUserCount,

        [property: Required]
        IList<string> AffectedTeamNames);
}
