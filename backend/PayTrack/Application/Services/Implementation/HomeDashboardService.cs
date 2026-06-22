// <copyright file="HomeDashboardService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Dashboard;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class HomeDashboardService(
        ITransactionRepository _transactionRepository) : IHomeDashboardService
    {
        private const int RecentItemsLimit = 5;

        private readonly ITransactionRepository transactionRepository = _transactionRepository;

        /// <inheritdoc/>
        public async Task<HomeDashboardDto> GetHomeDashboardAsync(User currentUser)
        {
            var invoiceSection = await this.transactionRepository.GetHomeDashboardInvoiceSectionAsync(currentUser.Id, RecentItemsLimit);
            var paymentRequestSection = await this.transactionRepository.GetHomeDashboardPaymentRequestSectionAsync(currentUser.Id, RecentItemsLimit);
            var needsAttentionCount = invoiceSection.NeedsAttentionCount + paymentRequestSection.NeedsAttentionCount;

            return new HomeDashboardDto(
                User: new HomeDashboardUserDto(currentUser.Id, currentUser.Name, currentUser.Role),
                Invoices: BuildSection(invoiceSection),
                PaymentRequests: BuildSection(paymentRequestSection),
                Actions: new HomeDashboardActionsDto(
                    MissingBankAccount: currentUser.BankAccounts.Count == 0,
                    BankInformationSkipped: currentUser.BankInformationSkipped,
                    NeedsAttentionCount: needsAttentionCount));
        }

        private static HomeDashboardSectionDto BuildSection(HomeDashboardSectionProjection section)
        {
            return new HomeDashboardSectionDto(
                OpenCount: section.OpenCount,
                SubmittedCount: section.SubmittedCount,
                PaidCount: section.PaidCount,
                OpenAmount: section.OpenAmount,
                LastPaidAt: section.LastPaidAt,
                TotalRecentCount: section.TotalRecentCount,
                Recent: section.Recent
                    .Select(item => new HomeDashboardRecentItemDto(
                        item.Id,
                        item.Amount,
                        item.Status,
                        item.CreatedAt,
                        item.PaidAt,
                        item.Reference,
                        item.PurposeOfPayment,
                        item.TeamName,
                        item.UserName))
                    .ToList());
        }
    }
}
