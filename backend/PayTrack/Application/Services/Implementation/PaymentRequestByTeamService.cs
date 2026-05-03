// <copyright file="PaymentRequestByTeamService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.PaymentRequestByTeam;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class PaymentRequestByTeamService(ITransactionRepository repo, ITeamService _teamService) : IPaymentRequestByTeamService
    {
        /// <summary>
        /// Repository for PaymentRequestByTeams.
        /// </summary>
        private readonly ITransactionRepository repo = repo;
        private readonly ITeamService teamService = _teamService;

        /// <inheritdoc/>
        public async Task<(List<PaymentRequestByTeam> paymentRequestByTeam, int totalCount)> GetAllAsync(
            GetPaymentRequestByTeamQuery? query = null)
        {
            return await this.repo.GetAllAsync(query);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByTeam?> GetPaymentRequestByTeamByIdAsync(int id, GetPaymentRequestByTeamQueryById? query = null)
        {
            return await this.repo.GetByIdAsync(id, query);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByTeam> CreatePaymentRequestByTeamAsync(
            int userToAssignToId,
            int creatingUserId,
            int teamId,
            decimal amount,
            string purposeOfPayment)
        {
            var team = await this.teamService.GetTeamByIdAsync(teamId) ?? throw new NotFoundException("Team could not be found");

            var paymentRequest = new PaymentRequestByTeam
            {
                // Transaction settings
                UserId = userToAssignToId,
                Amount = amount,
                PurposeOfPayment = purposeOfPayment,
                PaymentReference = string.Empty, // Payment reference will be set later by the finance team
                Status = TransactionStatus.Submitted,
                CostCentreId = null, // Cost centre will be set later by the finance team
                TeamId = team.Id,
                PaymentDirection = PaymentDirection.In, // Payment direction is in for payment requests to user

                // Created at is set automatically
                RequestedById = creatingUserId,
            };

            return await this.repo.AddAsync(paymentRequest);
        }

        /// <inheritdoc/>
        public async Task<PaymentRequestByTeam> UpdatePaymentRequestByTeamAsync(
            int id,
            int? teamId = null,
            decimal? amount = null,
            string? purposeOfPayment = null,
            DateTime? paidAt = null)
        {
            var transaction = await this.repo.GetByIdAsync(id, new GetPaymentRequestByTeamQueryById())
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

            return await this.repo.UpdateAsync(transaction);
        }
    }
}
