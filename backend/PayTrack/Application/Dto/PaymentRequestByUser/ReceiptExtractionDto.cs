// <copyright file="ReceiptExtractionDto.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.PaymentRequestByUser
{
    /// <summary>
    /// Result of a non-persistent receipt extraction attempt.
    /// </summary>
    /// <param name="ExtractionSucceeded">Whether text could be extracted and parsed.</param>
    /// <param name="Message">Optional information about a failed or partial extraction.</param>
    /// <param name="Amount">Extracted invoice total.</param>
    /// <param name="InvoiceDate">Extracted invoice date.</param>
    /// <param name="InvoiceNumber">Extracted invoice number.</param>
    public sealed record ReceiptExtractionDto(
        bool ExtractionSucceeded,
        string? Message,
        ExtractedReceiptFieldDto<decimal?> Amount,
        ExtractedReceiptFieldDto<DateTime?> InvoiceDate,
        ExtractedReceiptFieldDto<string?> InvoiceNumber)
    {
        /// <summary>
        /// Creates an extraction result containing no extracted values.
        /// </summary>
        /// <param name="message">Reason why extraction did not produce a result.</param>
        /// <returns>An empty extraction result.</returns>
        public static ReceiptExtractionDto Failed(string message)
        {
            return new ReceiptExtractionDto(
                false,
                message,
                new ExtractedReceiptFieldDto<decimal?>(null, 0),
                new ExtractedReceiptFieldDto<DateTime?>(null, 0),
                new ExtractedReceiptFieldDto<string?>(null, 0));
        }
    }

    /// <summary>
    /// An extracted receipt value together with a confidence between zero and one.
    /// </summary>
    /// <typeparam name="T">Type of the extracted value.</typeparam>
    /// <param name="Value">Extracted value, or null if no value was found.</param>
    /// <param name="Confidence">Rule-based confidence between zero and one.</param>
    public sealed record ExtractedReceiptFieldDto<T>(T Value, decimal Confidence);
}
