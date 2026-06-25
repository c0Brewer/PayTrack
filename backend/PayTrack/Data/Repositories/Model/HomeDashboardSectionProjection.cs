// <copyright file="HomeDashboardSectionProjection.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Data.Repositories.Model
{
    /// <summary>
    /// Lightweight dashboard projection for a transaction section.
    /// </summary>
    public sealed record class HomeDashboardSectionProjection(
        int OpenCount,
        int SubmittedCount,
        int PaidCount,
        decimal OpenAmount,
        DateTime? LastPaidAt,
        int TotalRecentCount,
        int NeedsAttentionCount,
        IReadOnlyList<HomeDashboardRecentItemProjection> Recent);

    /// <summary>
    /// Lightweight dashboard projection for a recent transaction item.
    /// </summary>
    public sealed record class HomeDashboardRecentItemProjection(
        int Id,
        decimal Amount,
        TransactionStatus Status,
        DateTime? CreatedAt,
        DateTime? PaidAt,
        string? Reference,
        string? PurposeOfPayment,
        string? TeamName,
        string? UserName);
}
