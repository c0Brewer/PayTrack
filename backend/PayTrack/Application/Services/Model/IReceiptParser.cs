// <copyright file="IReceiptParser.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.PaymentRequestByUser;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Parses invoice fields from already extracted receipt text.
    /// </summary>
    public interface IReceiptParser
    {
        /// <summary>
        /// Applies deterministic extraction rules to receipt text.
        /// </summary>
        /// <param name="text">Text extracted from a receipt.</param>
        /// <returns>Extracted fields and confidence values.</returns>
        ReceiptExtractionDto Parse(string text);
    }
}
