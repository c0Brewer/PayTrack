// <copyright file="PaymentRequestByUserService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class PaymentRequestByUserService(ITransactionRepository repo, ITeamService _teamService, IFileRepository _fileRepo, IBankAccountService _bankAccountService) : IPaymentRequestByUserService
    {
        private const int DuplicateMatchThreshold = 1;
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
                CostCentreId = null, // Cost centre will be set later by the finance team
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
            string invoiceNumber)
        {
            var normalizedInvoiceNumber = invoiceNumber.Trim();

            var duplicateCandidates = await this.repo.GetPotentialDuplicatesAsync(userId, teamId, amount, normalizedInvoiceNumber);

            return duplicateCandidates
                .Select(paymentRequestByUser => this.CreateDuplicateMatch(paymentRequestByUser, userId, teamId, amount, normalizedInvoiceNumber))
                .Where(duplicateMatch => duplicateMatch.Score >= DuplicateMatchThreshold)
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
            var transaction = await this.repo.GetByIdAsync(id, new())
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
                // TODO: Retrieve bank and set correct id like with team above. This should be implemented as soon as the bankAccountService is available!

                /*
                // var bankAccount = await this.bankAccountService.GetByIdAsync(bankAccountId.Value)
                //     ?? throw new NotFoundException("Bank account not found");
                */

                transaction.BankAccountId = bankAccountId;
            }

            return await this.repo.UpdateAsync(transaction);
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetReceiptForPaymentRequestByUserByIdAsync(int id)
        {
            var paymentRequest = await this.GetPaymentRequestByUserByIdAsync(id);

            if (paymentRequest?.ReceiptUrl == null)
            {
                throw new InvalidStateException("Receipt URL is null although it should not be.");
            }

            return await this.fileRepo.GetByPath(paymentRequest.ReceiptUrl);
        }

        private DuplicatePaymentRequestByUserMatch CreateDuplicateMatch(
            PaymentRequestByUser paymentRequestByUser,
            int userId,
            int teamId,
            decimal amount,
            string normalizedInvoiceNumber)
        {
            bool isAmountAndUserMatch = paymentRequestByUser.UserId == userId && paymentRequestByUser.Amount == amount;
            bool isAmountAndTeamMatch = paymentRequestByUser.TeamId == teamId && paymentRequestByUser.Amount == amount;
            bool isInvoiceNumberMatch = string.Equals(paymentRequestByUser.InvoiceNumber, normalizedInvoiceNumber, StringComparison.Ordinal);

            int score = 0;

            if (isAmountAndUserMatch)
            {
                score++;
            }

            if (isAmountAndTeamMatch)
            {
                score++;
            }

            if (isInvoiceNumberMatch)
            {
                score++;
            }

            return new DuplicatePaymentRequestByUserMatch(
                paymentRequestByUser,
                score,
                isAmountAndUserMatch,
                isAmountAndTeamMatch,
                isInvoiceNumberMatch);
        }
    }
}
