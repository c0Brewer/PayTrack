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
    public class PaymentRequestByTeamService(ITransactionRepository repo, ITeamService _teamService, IUserService _userService, ICostCentreService _costCentreService) : IPaymentRequestByTeamService
    {
        /// <summary>
        /// Repository for PaymentRequestByTeams.
        /// </summary>
        private readonly ITransactionRepository repo = repo;
        private readonly ITeamService teamService = _teamService;
        private readonly IUserService userService = _userService;
        private readonly ICostCentreService costCentreService = _costCentreService;

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
            string purposeOfPayment,
            DateTime dueDate,
            int? costCentreId = null)
        {
            var team = await this.teamService.GetTeamByIdAsync(teamId) ?? throw new NotFoundException("Team could not be found");
            var userToAssignTo = await this.userService.GetUserByIdAsync(userToAssignToId) ?? throw new NotFoundException("Assigned user could not be found");
            var creatingUser = await this.userService.GetUserByIdAsync(creatingUserId) ?? throw new NotFoundException("Creating user could not be found");
            if (costCentreId.HasValue)
            {
                _ = await this.costCentreService.GetByIdAsync(costCentreId.Value) ?? throw new NotFoundException("Cost centre could not be found");
            }

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0");
            }

            if (dueDate.Date < DateTime.Today)
            {
                throw new ArgumentException("Due date cannot be in the past");
            }

            var paymentRequest = new PaymentRequestByTeam
            {
                // Transaction settings
                UserId = userToAssignTo.Id,
                Amount = amount,
                PurposeOfPayment = purposeOfPayment,
                PaymentReference = string.Empty, // Payment reference will be set later by the finance team
                Status = TransactionStatus.Submitted,
                CostCentreId = costCentreId,
                TeamId = team.Id,
                PaymentDirection = PaymentDirection.In, // Payment direction is in for payment requests to user
                DueDate = dueDate,

                // Created at is set automatically
                RequestedById = creatingUser.Id,
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

        /// <inheritdoc/>
        public bool ValidateQuery(GetPaymentRequestByTeamQuery query, User currentUser)
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
    }
}
