// <copyright file="HomeDashboardService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Dashboard;
using PayTrack.Application.Dto.PaymentRequestByTeam;
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class HomeDashboardService(
        IPaymentRequestByUserService _paymentRequestByUserService,
        IPaymentRequestByTeamService _paymentRequestByTeamService) : IHomeDashboardService
    {
        private const int RecentItemsLimit = 5;

        private readonly IPaymentRequestByUserService paymentRequestByUserService = _paymentRequestByUserService;
        private readonly IPaymentRequestByTeamService paymentRequestByTeamService = _paymentRequestByTeamService;

        /// <inheritdoc/>
        public async Task<HomeDashboardDto> GetHomeDashboardAsync(User currentUser)
        {
            var invoiceQuery = new GetPaymentRequestByUserQuery
            {
                UserId = currentUser.Id,
                IncludeTeam = true,
            };

            var paymentRequestQuery = new GetPaymentRequestByTeamQuery
            {
                UserId = currentUser.Id,
                IncludeTeam = true,
            };

            var (invoices, _) = await this.paymentRequestByUserService.GetAllAsync(invoiceQuery);
            var (paymentRequests, _) = await this.paymentRequestByTeamService.GetAllAsync(paymentRequestQuery);
            var needsAttentionCount = invoices.Count(invoice => invoice.Status is TransactionStatus.ChangesRequested or TransactionStatus.Declined)
                + paymentRequests.Count(paymentRequest => paymentRequest.Status is TransactionStatus.ChangesRequested or TransactionStatus.Declined);

            HomeDashboardAdminDto? admin = null;
            if (currentUser.Role == Role.Admin)
            {
                var (submittedInvoices, submittedInvoicesCount) = await this.paymentRequestByUserService.GetAllAsync(new GetPaymentRequestByUserQuery
                {
                    Status = TransactionStatus.Submitted,
                });

                var (submittedPaymentRequests, submittedPaymentRequestsCount) = await this.paymentRequestByTeamService.GetAllAsync(new GetPaymentRequestByTeamQuery
                {
                    Status = TransactionStatus.Submitted,
                });

                admin = new HomeDashboardAdminDto(
                    SubmittedInvoicesAwaitingReview: submittedInvoicesCount > 0 ? submittedInvoicesCount : submittedInvoices.Count,
                    PaymentRequestsAwaitingReview: submittedPaymentRequestsCount > 0 ? submittedPaymentRequestsCount : submittedPaymentRequests.Count);
            }

            return new HomeDashboardDto(
                User: new HomeDashboardUserDto(currentUser.Id, currentUser.Name, currentUser.Role),
                Invoices: BuildInvoiceSection(invoices),
                PaymentRequests: BuildPaymentRequestSection(paymentRequests),
                Actions: new HomeDashboardActionsDto(
                    MissingBankAccount: currentUser.BankAccounts.Count == 0 && !currentUser.BankInformationSkipped,
                    BankInformationSkipped: currentUser.BankInformationSkipped,
                    PendingDuplicates: invoices.Count(invoice => invoice.HasPotentialDuplicate),
                    NeedsAttentionCount: needsAttentionCount),
                Admin: admin);
        }

        private static HomeDashboardSectionDto BuildInvoiceSection(List<PaymentRequestByUser> invoices)
        {
            var lastPaidAt = invoices
                .Where(invoice => invoice.Status == TransactionStatus.Paid && invoice.FinancePaidAt.HasValue)
                .MaxBy(invoice => invoice.FinancePaidAt)?
                .FinancePaidAt
                ?? invoices.Where(invoice => invoice.Status == TransactionStatus.Paid && invoice.PaidAt.HasValue).MaxBy(invoice => invoice.PaidAt)?.PaidAt;

            return new HomeDashboardSectionDto(
                OpenCount: invoices.Count(IsOpen),
                SubmittedCount: invoices.Count(invoice => invoice.Status == TransactionStatus.Submitted),
                PaidCount: invoices.Count(invoice => invoice.Status == TransactionStatus.Paid),
                OpenAmount: invoices.Where(IsOpen).Sum(invoice => invoice.Amount),
                LastPaidAt: lastPaidAt,
                Recent: invoices
                    .OrderByDescending(invoice => invoice.CreatedAt)
                    .Take(RecentItemsLimit)
                    .Select(invoice => new HomeDashboardRecentItemDto(
                        invoice.Id,
                        invoice.Amount,
                        invoice.Status,
                        invoice.CreatedAt,
                        invoice.FinancePaidAt ?? invoice.PaidAt,
                        invoice.InvoiceNumber,
                        invoice.PurposeOfPayment,
                        invoice.Team?.Name,
                        invoice.User?.Name))
                    .ToList());
        }

        private static HomeDashboardSectionDto BuildPaymentRequestSection(List<PaymentRequestByTeam> paymentRequests)
        {
            var lastPaidAt = paymentRequests
                .Where(paymentRequest => paymentRequest.Status == TransactionStatus.Paid && paymentRequest.PaidAt.HasValue)
                .MaxBy(paymentRequest => paymentRequest.PaidAt)?
                .PaidAt;

            return new HomeDashboardSectionDto(
                OpenCount: paymentRequests.Count(IsOpen),
                SubmittedCount: paymentRequests.Count(paymentRequest => paymentRequest.Status == TransactionStatus.Submitted),
                PaidCount: paymentRequests.Count(paymentRequest => paymentRequest.Status == TransactionStatus.Paid),
                OpenAmount: paymentRequests.Where(IsOpen).Sum(paymentRequest => paymentRequest.Amount),
                LastPaidAt: lastPaidAt,
                Recent: paymentRequests
                    .OrderByDescending(paymentRequest => paymentRequest.CreatedAt)
                    .Take(RecentItemsLimit)
                    .Select(paymentRequest => new HomeDashboardRecentItemDto(
                        paymentRequest.Id,
                        paymentRequest.Amount,
                        paymentRequest.Status,
                        paymentRequest.CreatedAt,
                        paymentRequest.PaidAt,
                        paymentRequest.PaymentReference,
                        paymentRequest.PurposeOfPayment,
                        paymentRequest.Team?.Name,
                        paymentRequest.User?.Name))
                    .ToList());
        }

        private static bool IsOpen(Transaction transaction)
        {
            return transaction.Status is not TransactionStatus.Paid and not TransactionStatus.Declined;
        }
    }
}
