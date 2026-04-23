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
    public class PaymentRequestByUserService(ITransactionRepository repo, ITeamService _teamService, IFileRepository _fileRepo) : IPaymentRequestByUserService
    {
        /// <summary>
        /// Repository for PaymentRequestByUsers.
        /// </summary>
        private readonly ITransactionRepository repo = repo;
        private readonly IFileRepository fileRepo = _fileRepo;
        private readonly ITeamService teamService = _teamService;

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
            int bankAccountId)
        {
            // TODO: Check here if bank account valid when bank account service is ready
            var team = await this.teamService.GetTeamByIdAsync(teamId) ?? throw new NotFoundException("Team could not be found");

            // TODO: Check paid at < todays date. Allow paid at to be null when PayoutType is not User
            if (PaidAt > DateTime.Today)
            {
                throw new InvalidStateException("Paid at cannot be in the future!");
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
            var transaction = await this.repo.GetByIdAsync(id, new ())
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
    }
}
