// <copyright file="PaymentRequestByUserMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.BankAccount;
using PayTrack.Application.Dto.PaymentRequestByUser;
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

            return new PaymentRequestByUserDto(
                paymentRequestByUser.Id,
                paymentRequestByUser.InvoiceNumber,
                paymentRequestByUser.Comment,
                paymentRequestByUser.ReceiptUrl,
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
