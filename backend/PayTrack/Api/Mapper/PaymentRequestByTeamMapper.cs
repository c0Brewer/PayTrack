// <copyright file="PaymentRequestByTeamMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.CostCentre;
using PayTrack.Application.Dto.PaymentRequestByTeam;
using PayTrack.Application.Dto.Team;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Dto.User;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Mapper
{
    /// <summary>
    /// Mapper for PaymentRequestByTeam.
    /// </summary>
    public static class PaymentRequestByTeamMapper
    {
        /// <summary>
        /// Turns PaymentRequestByTeam object into a PaymentRequestByTeamDto.
        /// </summary>
        /// <param name="paymentRequestByTeam">PaymentRequestByTeam to map.</param>
        /// <returns>PaymentRequestByTeamDto instance.</returns>
        public static PaymentRequestByTeamDto ToDto(PaymentRequestByTeam paymentRequestByTeam)
        {
            UserDto? user = null;
            if (paymentRequestByTeam.User != null)
            {
                user = UserMapper.ToDto(paymentRequestByTeam.User);
            }

            UserDto? createdByUser = null;
            if (paymentRequestByTeam.RequestedBy != null)
            {
                createdByUser = UserMapper.ToDto(paymentRequestByTeam.RequestedBy);
            }

            CostCentreDto? costCentre = null;
            if (paymentRequestByTeam.CostCentre != null)
            {
                costCentre = CostCentreMapper.ToDto(paymentRequestByTeam.CostCentre);
            }

            TeamDto? team = null;
            if (paymentRequestByTeam.Team != null)
            {
                team = TeamMapper.ToDto(paymentRequestByTeam.Team);
            }

            ICollection<TransactionStatusHistoryDto>? statusHistory = [];
            if (paymentRequestByTeam.StatusHistory != null)
            {
                statusHistory = TransactionStatusHistoryMapper.ListToDto([.. paymentRequestByTeam.StatusHistory]);
            }

            return new PaymentRequestByTeamDto(
                paymentRequestByTeam.Id,
                user,
                paymentRequestByTeam.Amount,
                paymentRequestByTeam.PurposeOfPayment,
                paymentRequestByTeam.PaymentReference,
                paymentRequestByTeam.Status,
                costCentre,
                team,
                paymentRequestByTeam.PaymentDirection,
                statusHistory,
                paymentRequestByTeam.CreatedAt,
                paymentRequestByTeam.PaidAt,
                createdByUser);
        }

        /// <summary>
        /// Turns a List of PaymentRequestByTeam objects into a List of PaymentRequestByTeamDto objects.
        /// </summary>
        /// <param name="PaymentRequestByTeam">List of PaymentRequestByTeam objects.</param>
        /// <returns>List of PaymentRequestByTeamDto objects.</returns>
        public static List<PaymentRequestByTeamDto> ListToDto(List<PaymentRequestByTeam> PaymentRequestByTeam)
        {
            return PaymentRequestByTeam.ConvertAll(ToDto);
        }
    }
}
