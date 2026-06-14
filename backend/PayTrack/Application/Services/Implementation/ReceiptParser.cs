// <copyright file="ReceiptParser.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.RegularExpressions;
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Services.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <summary>
    /// Deterministic, rule-based parser for common invoice layouts.
    /// </summary>
    public sealed partial class ReceiptParser : IReceiptParser
    {
        private static readonly CultureInfo[] DateCultures =
        [
            CultureInfo.GetCultureInfo("de-AT"),
            CultureInfo.GetCultureInfo("de-DE"),
            CultureInfo.GetCultureInfo("en-US"),
            CultureInfo.GetCultureInfo("en-GB"),
        ];

        private static readonly string[] DateFormats =
        [
            "d.M.yyyy", "dd.MM.yyyy", "d/M/yyyy", "dd/MM/yyyy", "M/d/yyyy", "MM/dd/yyyy",
            "yyyy-MM-dd", "d-M-yyyy", "dd-MM-yyyy", "MMM d, yyyy", "MMMM d, yyyy",
        ];

        /// <inheritdoc/>
        public ReceiptExtractionDto Parse(string text)
        {
            var lines = text
                .Replace('\r', '\n')
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(NormalizeWhitespace)
                .Where(line => line.Length > 0)
                .ToArray();

            if (lines.Length == 0)
            {
                return ReceiptExtractionDto.Failed("No readable text was found in the receipt.");
            }

            var amount = ExtractAmount(lines);
            var invoiceDate = ExtractDate(lines);
            var invoiceNumber = ExtractInvoiceNumber(lines);
            var foundAny = amount.Value.HasValue ||
                           invoiceDate.Value.HasValue ||
                           invoiceNumber.Value is not null;

            return new ReceiptExtractionDto(
                foundAny,
                foundAny ? null : "Text was read, but no supported invoice fields were recognized.",
                amount,
                invoiceDate,
                invoiceNumber);
        }

        private static ExtractedReceiptFieldDto<decimal?> ExtractAmount(string[] lines)
        {
            foreach (var line in lines.Reverse())
            {
                if (!TotalLabelRegex().IsMatch(line) || ExcludedAmountLabelRegex().IsMatch(line))
                {
                    continue;
                }

                var amounts = FindAmounts(line);
                if (amounts.Count > 0)
                {
                    return new ExtractedReceiptFieldDto<decimal?>(amounts[^1], 0.95m);
                }
            }

            var candidates = lines
                .Where(line => !ExcludedAmountLabelRegex().IsMatch(line))
                .SelectMany(FindAmounts)
                .Where(value => value > 0 && value < 10000000)
                .ToArray();

            return candidates.Length == 0
                ? new ExtractedReceiptFieldDto<decimal?>(null, 0)
                : new ExtractedReceiptFieldDto<decimal?>(candidates.Max(), 0.45m);
        }

        private static ExtractedReceiptFieldDto<DateTime?> ExtractDate(string[] lines)
        {
            foreach (var line in lines.Where(line => DateLabelRegex().IsMatch(line)))
            {
                if (TryFindDate(line, out var date))
                {
                    return new ExtractedReceiptFieldDto<DateTime?>(date, 0.92m);
                }
            }

            foreach (var line in lines.Take(20))
            {
                if (TryFindDate(line, out var date))
                {
                    return new ExtractedReceiptFieldDto<DateTime?>(date, 0.55m);
                }
            }

            return new ExtractedReceiptFieldDto<DateTime?>(null, 0);
        }

        private static ExtractedReceiptFieldDto<string?> ExtractInvoiceNumber(string[] lines)
        {
            foreach (var line in lines)
            {
                var match = InvoiceNumberRegex().Match(line);
                if (!match.Success)
                {
                    continue;
                }

                var value = match.Groups["value"].Value.Trim().TrimEnd('.', ',', ';');
                if (value.Length >= 3)
                {
                    return new ExtractedReceiptFieldDto<string?>(value, 0.95m);
                }
            }

            return new ExtractedReceiptFieldDto<string?>(null, 0);
        }

        private static List<decimal> FindAmounts(string line)
        {
            var results = new List<decimal>();

            foreach (Match match in AmountRegex().Matches(line))
            {
                if (TryParseAmount(match.Groups["amount"].Value, out var amount))
                {
                    results.Add(amount);
                }
            }

            return results;
        }

        private static bool TryParseAmount(string rawValue, out decimal amount)
        {
            var value = rawValue.Replace(" ", string.Empty, StringComparison.Ordinal);
            var lastComma = value.LastIndexOf(',');
            var lastDot = value.LastIndexOf('.');

            if (lastComma >= 0 && lastDot >= 0)
            {
                var decimalSeparator = lastComma > lastDot ? ',' : '.';
                var thousandsSeparator = decimalSeparator == ',' ? "." : ",";
                value = value.Replace(thousandsSeparator, string.Empty, StringComparison.Ordinal)
                    .Replace(decimalSeparator, '.');
            }
            else if (lastComma >= 0)
            {
                value = value.Replace(',', '.');
            }

            return decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount);
        }

        private static bool TryFindDate(string line, out DateTime date)
        {
            foreach (Match match in DateValueRegex().Matches(line))
            {
                foreach (var culture in DateCultures)
                {
                    if (DateTime.TryParseExact(
                        match.Value,
                        DateFormats,
                        culture,
                        DateTimeStyles.AllowWhiteSpaces,
                        out date))
                    {
                        date = date.Date;
                        return true;
                    }
                }
            }

            date = default;
            return false;
        }

        private static string NormalizeWhitespace(string value)
        {
            return WhitespaceRegex().Replace(value, " ").Trim();
        }

        [GeneratedRegex(@"(?ix)\b(?:grand\s+total|invoice\s+total|amount\s+due|balance\s+due|total\s+due|gesamtbetrag|rechnungsbetrag|zahlbetrag|zu\s+zahlen|summe|total)\b")]
        private static partial Regex TotalLabelRegex();

        [GeneratedRegex(@"(?ix)\b(?:subtotal|zwischensumme|netto|vat|mwst|ust|tax|steuer|tip|discount|rabatt)\b")]
        private static partial Regex ExcludedAmountLabelRegex();

        [GeneratedRegex(@"(?ix)\b(?:invoice\s+date|receipt\s+date|date\s+of\s+issue|issued|rechnungsdatum|belegdatum|ausstellungsdatum|datum)\b")]
        private static partial Regex DateLabelRegex();

        [GeneratedRegex(@"(?ix)\b(?:(?:invoice|receipt)\s*(?:number|no\.?|\#)|(?:rechnung|beleg)\s*(?:snummer|nummer|nr\.?|\#)|quittung\s*(?:nummer|nr\.?|\#))\s*[:\-]?\s*(?<value>[A-Z0-9][A-Z0-9._\-/]{2,})")]
        private static partial Regex InvoiceNumberRegex();

        [GeneratedRegex(@"(?ix)(?<!\d)(?:EUR|USD|GBP|CHF|\$|€|£)?\s*(?<amount>\d{1,3}(?:[.,\s]\d{3})*(?:[.,]\d{2})|\d+[.,]\d{2})\s*(?:EUR|USD|GBP|CHF|\$|€|£)?(?!\d)")]
        private static partial Regex AmountRegex();

        [GeneratedRegex(@"(?ix)(?<!\d)(?:\d{1,2}[./-]\d{1,2}[./-]\d{4}|\d{4}-\d{1,2}-\d{1,2}|[A-Z]{3,9}\s+\d{1,2},\s+\d{4})(?!\d)")]
        private static partial Regex DateValueRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRegex();
    }
}
