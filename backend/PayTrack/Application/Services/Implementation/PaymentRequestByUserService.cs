// <copyright file="PaymentRequestByUserService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Helpers;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class PaymentRequestByUserService(ITransactionRepository repo, ITeamService _teamService, IFileRepository _fileRepo, IBankAccountService _bankAccountService) : IPaymentRequestByUserService
    {
        private const int MaxDuplicateResults = 10;

        /// <summary>
        /// Repository for PaymentRequestByUsers.
        /// </summary>
        private readonly ITransactionRepository repo = repo;
        private readonly IFileRepository fileRepo = _fileRepo;
        private readonly ITeamService teamService = _teamService;
        private readonly IBankAccountService bankAccountService = _bankAccountService;

        /// <inheritdoc/>
        public async Task<(List<PaymentRequestByUser> paymentRequestByUser, int totalCount)> GetAllAsync(
            GetPaymentRequestByUserQuery? query = null)
        {
            return await this.repo.GetAllAsync(query);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser?> GetPaymentRequestByUserByIdAsync(int id, GetPaymentRequestByUserQueryById? query = null)
        {
            return await this.repo.GetByIdAsync(id, query);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> CreatePaymentRequestByUserAsync(
            int userId,
            int teamId,
            decimal amount,
            string purposeOfPayment,
            IFormFile receipt,
            DateTime PaidAt,
            string invoiceNumber,
            string? comment,
            PayoutType payoutType,
            int? bankAccountId)
        {
            var team = await this.teamService.GetTeamByIdAsync(teamId) ?? throw new NotFoundException("Team could not be found");

            if (PaidAt.Date > DateTime.Today)
            {
                throw new InvalidStateException("Paid at cannot be in the future!");
            }

            // Ensure bank account id is set if payout type is user
            if (payoutType == PayoutType.User)
            {
                if (!bankAccountId.HasValue)
                {
                    throw new InvalidStateException("If the money should be paid out to you, you must specify a bankAccount");
                }

                var bankAccounts = await this.bankAccountService.GetBankAccountsAsync(userId) ?? throw new NotFoundException("Bank Accounts could not be found");

                if (!bankAccounts.Any(b => b.Id == bankAccountId.Value))
                {
                    throw new InvalidStateException("Could not find specified bank account");
                }
            }
            else
            {
                // Ensure bank account is null if payout type is external
                bankAccountId = null;
            }

            var paymentRequest = new PaymentRequestByUser
            {
                // Transaction settings
                UserId = userId,
                Amount = amount,
                PurposeOfPayment = purposeOfPayment,
                PaymentReference = string.Empty, // Payment reference will be set later by the finance team
                Status = TransactionStatus.Submitted,
                BudgetId = null, // Budget will be set later by the finance team
                TeamId = team.Id,
                PaymentDirection = PaymentDirection.Out, // Payment direction is out for payment requests by user

                // Created at is set automatically
                PaidAt = PaidAt.ToUniversalTime(),

                // Payment request specific settings
                InvoiceNumber = invoiceNumber,
                Comment = comment,
                ReceiptUrl = string.Empty, // will be set in the repo later
                PayoutType = payoutType,
                BankAccountId = bankAccountId,
            };

            return await this.repo.AddAsync(paymentRequest, receipt);
        }

        /// <inheritdoc/>
        public async Task<List<DuplicatePaymentRequestByUserMatch>> GetDuplicatePaymentRequestsByUserAsync(
            int userId,
            int teamId,
            decimal amount,
            DateTime paidAt,
            string? invoiceNumber = null,
            int? paymentRequestByUserId = null)
        {
            var matchUserId = userId;
            var matchTeamId = teamId;
            var matchAmount = amount;
            var matchPaidAt = paidAt;
            var matchInvoiceNumber = invoiceNumber;

            if (paymentRequestByUserId.HasValue)
            {
                var sourcePaymentRequest = await this.repo.GetByIdAsync(paymentRequestByUserId.Value, new GetPaymentRequestByUserQueryById())
                    ?? throw new NotFoundException("PaymentRequestByUser could not be found");

                if (!sourcePaymentRequest.PaidAt.HasValue)
                {
                    throw new InvalidStateException("Duplicate lookup is missing paid date.");
                }

                matchUserId = sourcePaymentRequest.UserId;
                matchTeamId = sourcePaymentRequest.TeamId;
                matchAmount = sourcePaymentRequest.Amount;
                matchPaidAt = sourcePaymentRequest.PaidAt.Value;
                matchInvoiceNumber = sourcePaymentRequest.InvoiceNumber;
            }

            var duplicateCandidates = await this.repo.GetPotentialDuplicatesAsync(
                matchUserId,
                matchTeamId,
                matchAmount,
                matchPaidAt,
                matchInvoiceNumber,
                paymentRequestByUserId);

            return duplicateCandidates
                .Select(paymentRequestByUser => this.CreateDuplicateMatch(paymentRequestByUser, matchUserId, matchTeamId, matchAmount, matchPaidAt, matchInvoiceNumber))
                .Where(duplicateMatch => duplicateMatch.Score >= DuplicatePaymentRequestByUserScorer.MatchThreshold)
                .OrderByDescending(duplicateMatch => duplicateMatch.Score)
                .ThenByDescending(duplicateMatch => duplicateMatch.PaymentRequestByUser.CreatedAt)
                .Take(MaxDuplicateResults)
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByUser> UpdatePaymentRequestByUserAsync(
            int id,
            int? teamId = null,
            decimal? amount = null,
            string? purposeOfPayment = null,
            DateTime? paidAt = null,
            string? invoiceNumber = null,
            string? comment = null,
            PayoutType? payoutType = null,
            int? bankAccountId = null)
        {
            var transaction = await this.repo.GetByIdAsync(id, new GetPaymentRequestByUserQueryById())
                ?? throw new NotFoundException("Transaction not found");

            if (teamId.HasValue)
            {
                var team = await this.teamService.GetTeamByIdAsync(teamId.Value)
                    ?? throw new NotFoundException("Team not found");

                transaction.TeamId = team.Id;
            }

            if (amount.HasValue)
            {
                transaction.Amount = amount.Value;
            }

            if (purposeOfPayment != null)
            {
                transaction.PurposeOfPayment = purposeOfPayment;
            }

            if (paidAt.HasValue)
            {
                transaction.PaidAt = paidAt.Value;
            }

            if (invoiceNumber != null)
            {
                transaction.InvoiceNumber = invoiceNumber;
            }

            if (comment != null)
            {
                transaction.Comment = comment;
            }

            if (payoutType.HasValue)
            {
                transaction.PayoutType = payoutType.Value;
            }

            if (bankAccountId.HasValue)
            {
                var bankAccounts = await this.bankAccountService.GetBankAccountsAsync(transaction.UserId) ?? throw new NotFoundException("Bank Accounts could not be found");

                if (!bankAccounts.Any(b => b.Id == bankAccountId.Value))
                {
                    throw new InvalidStateException("Could not find specified bank account");
                }

                transaction.BankAccountId = bankAccountId;
            }

            return await this.repo.UpdateAsync(transaction);
        }

        /// <inheritdoc/>
        public async Task DeletePaymentRequestByUserAsync(int id)
        {
            var wasDeleted = await this.repo.DeletePaymentRequestByUserAsync(id);

            if (!wasDeleted)
            {
                throw new NotFoundException("PaymentRequestByUser could not be found");
            }
        }

        /// <inheritdoc/>
        public async Task DismissDuplicatePaymentRequestByUserAsync(int paymentRequestByUserId, int duplicatePaymentRequestByUserId)
        {
            if (paymentRequestByUserId == duplicatePaymentRequestByUserId)
            {
                throw new InvalidStateException("A payment request cannot be a duplicate of itself.");
            }

            await this.repo.DismissDuplicatePaymentRequestByUserAsync(paymentRequestByUserId, duplicatePaymentRequestByUserId);
        }

        /// <inheritdoc/>
        public async Task<(byte[] content, string contentType)> GetReceiptForPaymentRequestByUserByIdAsync(int id)
        {
            var paymentRequest = await this.GetPaymentRequestByUserByIdAsync(id);

            if (string.IsNullOrEmpty(paymentRequest?.ReceiptUrl))
            {
                throw new InvalidStateException("Receipt URL is null although it should not be.");
            }

            var content = await this.fileRepo.GetByPath(paymentRequest.ReceiptUrl);
            var contentType = GetContentTypeFromPath(paymentRequest.ReceiptUrl);
            return (content, contentType);
        }

        /// <inheritdoc/>
        public bool ValidateQuery(GetPaymentRequestByUserQuery query, User currentUser)
        {
            return currentUser.Role switch
            {
                Role.RegularUser => query.UserId == currentUser.Id,

                Role.TeamLead => currentUser.TeamId.HasValue
                                  && query.TeamId == currentUser.TeamId,

                Role.Admin => true,

                _ => false
            };
        }

        /// <inheritdoc/>
        public bool ValidateAccessToInvoice(PaymentRequestByUser invoice, User currentUser)
        {
            return currentUser.Role switch
            {
                Role.RegularUser => invoice.UserId == currentUser.Id,

                Role.TeamLead => currentUser.TeamId.HasValue
                                && invoice.TeamId == currentUser.TeamId,

                Role.Admin => true,

                _ => false
            };
        }

        private static string GetContentTypeFromPath(string filePath)
        {
            return Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream",
            };
        }

        private DuplicatePaymentRequestByUserMatch CreateDuplicateMatch(
            PaymentRequestByUser paymentRequestByUser,
            int userId,
            int teamId,
            decimal amount,
            DateTime paidAt,
            string? invoiceNumber)
        {
            var duplicateScore = DuplicatePaymentRequestByUserScorer.Calculate(
                paymentRequestByUser,
                userId,
                teamId,
                amount,
                paidAt,
                invoiceNumber);

            return new DuplicatePaymentRequestByUserMatch(
                paymentRequestByUser,
                duplicateScore.Score,
                duplicateScore.MatchedFields);
        }
    }
}
