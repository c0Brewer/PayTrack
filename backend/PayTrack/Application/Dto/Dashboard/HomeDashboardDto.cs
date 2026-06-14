// <copyright file="HomeDashboardDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Application.Dto.Dashboard
{
    /// <summary>
    /// Dashboard payload for the authenticated user's home page.
    /// </summary>
    public sealed record class HomeDashboardDto(
        HomeDashboardUserDto User,
        HomeDashboardSectionDto Invoices,
        HomeDashboardSectionDto PaymentRequests,
        HomeDashboardActionsDto Actions);

    /// <summary>
    /// Minimal user information shown on the home dashboard.
    /// </summary>
    public sealed record class HomeDashboardUserDto(
        int Id,
        string Name,
        Role Role);

    /// <summary>
    /// Shared dashboard summary for a transaction section.
    /// </summary>
    public sealed record class HomeDashboardSectionDto(
        int OpenCount,
        int SubmittedCount,
        int PaidCount,
        decimal OpenAmount,
        DateTime? LastPaidAt,
        IReadOnlyList<HomeDashboardRecentItemDto> Recent);

    /// <summary>
    /// Recent item preview for the home dashboard.
    /// </summary>
    public sealed record class HomeDashboardRecentItemDto(
        int Id,
        decimal Amount,
        TransactionStatus Status,
        DateTime? CreatedAt,
        DateTime? PaidAt,
        string? Reference,
        string? PurposeOfPayment,
        string? TeamName,
        string? UserName);

    /// <summary>
    /// Action and warning information for the home dashboard.
    /// </summary>
    public sealed record class HomeDashboardActionsDto(
        bool MissingBankAccount,
        bool BankInformationSkipped,
        int NeedsAttentionCount);
}
