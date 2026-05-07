// <copyright file="PaymentRequestByUserMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.BankAccount;
using PayTrack.Application.Dto.CostCentre;
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Dto.Team;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Dto.User;
using PayTrack.Application.Services.Model;
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

        /// <summary>
        /// Turns DuplicatePaymentRequestByUserMatch object into a DuplicatePaymentRequestByUserDto.
        /// </summary>
        /// <param name="duplicatePaymentRequestByUser">DuplicatePaymentRequestByUserMatch to map.</param>
        /// <returns>DuplicatePaymentRequestByUserDto instance.</returns>
        public static DuplicatePaymentRequestByUserDto DuplicateToDto(DuplicatePaymentRequestByUserMatch duplicatePaymentRequestByUser)
        {
            return new DuplicatePaymentRequestByUserDto(
                ToDto(duplicatePaymentRequestByUser.PaymentRequestByUser),
                duplicatePaymentRequestByUser.Score,
                duplicatePaymentRequestByUser.IsAmountAndUserMatch,
                duplicatePaymentRequestByUser.IsAmountAndTeamMatch,
                duplicatePaymentRequestByUser.IsInvoiceNumberMatch);
        }

        /// <summary>
        /// Turns a List of DuplicatePaymentRequestByUserMatch objects into a List of DuplicatePaymentRequestByUserDto objects.
        /// </summary>
        /// <param name="duplicatePaymentRequestByUser">List of DuplicatePaymentRequestByUserMatch objects.</param>
        /// <returns>List of DuplicatePaymentRequestByUserDto objects.</returns>
        public static List<DuplicatePaymentRequestByUserDto> DuplicateListToDto(List<DuplicatePaymentRequestByUserMatch> duplicatePaymentRequestByUser)
        {
            return duplicatePaymentRequestByUser.ConvertAll(DuplicateToDto);
        }
    }
}
