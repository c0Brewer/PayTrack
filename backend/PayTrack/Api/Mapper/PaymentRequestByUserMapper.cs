// <copyright file="PaymentRequestByUserMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.BankAccount;
using PayTrack.Application.Dto.CostCentre;
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Dto.Team;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Dto.User;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Mapper
{
    /// <summary>
    /// Mapper for PaymentRequestByUser.
    /// </summary>
    public static class PaymentRequestByUserMapper
    {
        /// <summary>
        /// Turns PaymentRequestByUser object into a PaymentRequestByUserDto.
        /// </summary>
        /// <param name="paymentRequestByUser">PaymentRequestByUser to map.</param>
        /// <returns>PaymentRequestByUserDto instance.</returns>
        public static PaymentRequestByUserDto ToDto(PaymentRequestByUser paymentRequestByUser)
        {
            BankAccountDto? bankAccountDto = null;
            if (paymentRequestByUser.BankAccount != null)
            {
                bankAccountDto = BankAccountMapper.ToDto(paymentRequestByUser.BankAccount);
            }

            UserDto? user = null;
            if (paymentRequestByUser.User != null)
            {
                user = UserMapper.ToDto(paymentRequestByUser.User);
            }

            CostCentreDto? costCentre = null;
            if (paymentRequestByUser.CostCentre != null)
            {
                costCentre = CostCentreMapper.ToDto(paymentRequestByUser.CostCentre);
            }

            TeamDto? team = null;
            if (paymentRequestByUser.Team != null)
            {
                team = TeamMapper.ToDto(paymentRequestByUser.Team);
            }

            ICollection<TransactionStatusHistoryDto>? statusHistory = [];
            if (paymentRequestByUser.StatusHistory != null)
            {
                statusHistory = TransactionStatusHistoryMapper.ListToDto([.. paymentRequestByUser.StatusHistory]);
            }

            return new PaymentRequestByUserDto(
                paymentRequestByUser.Id,
                user,
                paymentRequestByUser.Amount,
                paymentRequestByUser.PurposeOfPayment,
                paymentRequestByUser.PaymentReference,
                paymentRequestByUser.Status,
                costCentre,
                team,
                paymentRequestByUser.PaymentDirection,
                statusHistory,
                paymentRequestByUser.CreatedAt,
                paymentRequestByUser.PaidAt,
                paymentRequestByUser.InvoiceNumber,
                paymentRequestByUser.Comment,
                paymentRequestByUser.PayoutType,
                bankAccountDto);
        }

        /// <summary>
        /// Turns a List of PaymentRequestByUser objects into a List of PaymentRequestByUserDto objects.
        /// </summary>
        /// <param name="PaymentRequestByUser">List of PaymentRequestByUser objects.</param>
        /// <returns>List of PaymentRequestByUserDto objects.</returns>
        public static List<PaymentRequestByUserDto> ListToDto(List<PaymentRequestByUser> PaymentRequestByUser)
        {
            return PaymentRequestByUser.ConvertAll(ToDto);
        }
    }
}
