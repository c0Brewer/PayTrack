// <copyright file="IReceiptExtractionService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.PaymentRequestByUser;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Extracts invoice data from uploaded receipts without persisting them.
    /// </summary>
    public interface IReceiptExtractionService
    {
        /// <summary>
        /// Extracts invoice data from a PDF or image receipt.
        /// </summary>
        /// <param name="receipt">Uploaded receipt.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Extracted fields and confidence values.</returns>
        Task<ReceiptExtractionDto> ExtractAsync(IFormFile receipt, CancellationToken cancellationToken = default);
    }
}
