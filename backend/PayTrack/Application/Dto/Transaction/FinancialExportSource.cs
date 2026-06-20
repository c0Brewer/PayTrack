// <copyright file="FinancialExportSource.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.Transaction
{
    /// <summary>
    /// Defines which transaction view a financial export is based on.
    /// </summary>
    public enum FinancialExportSource
    {
        /// <summary>
        /// Export the submitted invoices view.
        /// </summary>
        SubmittedInvoices = 1,

        /// <summary>
        /// Export the payment requests view.
        /// </summary>
        PaymentRequests = 2,
    }
}
