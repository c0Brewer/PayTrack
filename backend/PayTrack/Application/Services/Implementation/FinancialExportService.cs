// <copyright file="FinancialExportService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using PayTrack.Application.Dto.PaymentRequestByTeam;
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

// AI helped with PDF and CSV generation.
namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class FinancialExportService(ITransactionRepository _transactionRepository) : IFinancialExportService
    {
        private const string CsvContentType = "text/csv; charset=utf-8";
        private const string PdfContentType = "application/pdf";

        private readonly ITransactionRepository transactionRepository = _transactionRepository;

        /// <inheritdoc/>
        public async Task<FinancialExportResult> ExportFinancialDataAsync(GetFinancialExportQuery query)
        {
            var source = GetRequiredSource(query.Source);
            var transactions = await this.GetTransactionsForExportAsync(query, source);
            var rows = transactions
                .Select(ToExportRow)
                .ToList();

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var format = query.Format ?? FinancialExportFormat.Csv;
            var filePrefix = GetFilePrefix(source);
            var title = GetTitle(source);

            return format switch
            {
                FinancialExportFormat.Csv => new FinancialExportResult(
                    Encoding.UTF8.GetBytes(BuildCsv(rows, source)),
                    CsvContentType,
                    $"{filePrefix}-{timestamp}.csv"),

                FinancialExportFormat.Pdf => new FinancialExportResult(
                    BuildPdf(rows, title, source),
                    PdfContentType,
                    $"{filePrefix}-{timestamp}.pdf"),

                _ => throw new InvalidStateException("Unsupported financial export format."),
            };
        }

        private static FinancialExportSource GetRequiredSource(FinancialExportSource? source)
        {
            if (!source.HasValue)
            {
                throw new InvalidStateException("Financial export source is required.");
            }

            return source.Value;
        }

        private static GetPaymentRequestByUserQuery CreateSubmittedInvoicesExportQuery(GetFinancialExportQuery query)
        {
            return new GetPaymentRequestByUserQuery
            {
                UserId = query.UserId,
                MinAmount = query.MinAmount,
                MaxAmount = query.MaxAmount,
                PurposeOfPayment = query.PurposeOfPayment,
                PaymentReference = query.PaymentReference,
                InvoiceNumber = query.InvoiceNumber,
                PayoutType = query.PayoutType,
                BankAccountId = query.BankAccountId,
                Status = query.Status,
                TeamId = query.TeamId,
                CostCentreId = query.CostCentreId,
                PaymentDirection = query.PaymentDirection,
                MinCreatedAt = query.MinCreatedAt,
                MaxCreatedAt = query.MaxCreatedAt,
                MinPaidAt = query.MinPaidAt,
                MaxPaidAt = query.MaxPaidAt,
                MinDueDate = query.MinDueDate,
                MaxDueDate = query.MaxDueDate,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection,
                IncludeTeam = true,
                IncludeBudget = true,
                IncludeBankAccount = true,
            };
        }

        private static GetPaymentRequestByTeamQuery CreatePaymentRequestsExportQuery(GetFinancialExportQuery query)
        {
            return new GetPaymentRequestByTeamQuery
            {
                UserId = query.UserId,
                RequestById = query.RequestById,
                MinAmount = query.MinAmount,
                MaxAmount = query.MaxAmount,
                PurposeOfPayment = query.PurposeOfPayment,
                PaymentReference = query.PaymentReference,
                Status = query.Status,
                TeamId = query.TeamId,
                CostCentreId = query.CostCentreId,
                PaymentDirection = query.PaymentDirection,
                MinCreatedAt = query.MinCreatedAt,
                MaxCreatedAt = query.MaxCreatedAt,
                MinPaidAt = query.MinPaidAt,
                MaxPaidAt = query.MaxPaidAt,
                MinDueDate = query.MinDueDate,
                MaxDueDate = query.MaxDueDate,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection,
                IncludeTeam = true,
                IncludeBudget = true,
                VisibleStatusesOnly = true,
            };
        }

        private static FinancialExportRow ToExportRow(Transaction transaction)
        {
            var invoiceNumber = transaction is PaymentRequestByUser paymentRequestByUser
                ? paymentRequestByUser.InvoiceNumber
                : null;
            var payoutType = transaction is PaymentRequestByUser userPaymentRequest
                ? GetPayoutTypeLabel(userPaymentRequest.PayoutType)
                : null;

            return new FinancialExportRow(
                transaction.Id,
                invoiceNumber,
                GetTransactionType(transaction),
                transaction.PaymentDirection.ToString(),
                GetTransactionStatusLabel(transaction.Status),
                transaction.Amount,
                transaction.PurposeOfPayment,
                transaction.PaymentReference,
                transaction.Team?.Name ?? transaction.TeamId.ToString(CultureInfo.InvariantCulture),
                transaction.Budget?.CostCentre.Name,
                transaction.Budget?.Name,
                transaction.User?.Name ?? transaction.User?.Email ?? transaction.UserId.ToString(CultureInfo.InvariantCulture),
                transaction.CreatedAt,
                transaction.PaidAt,
                transaction.FinancePaidAt,
                transaction.DueDate,
                payoutType);
        }

        private static string GetTransactionType(Transaction transaction)
        {
            return transaction switch
            {
                PaymentRequestByUser => "Expense",
                PaymentRequestByTeam => "Income",
                _ => "Transaction",
            };
        }

        private static string BuildCsv(
            IReadOnlyCollection<FinancialExportRow> rows,
            FinancialExportSource source)
        {
            return source switch
            {
                FinancialExportSource.SubmittedInvoices => BuildSubmittedInvoicesCsv(rows),
                FinancialExportSource.PaymentRequests => BuildPaymentRequestsCsv(rows),
                _ => throw new InvalidStateException("Unsupported financial export source."),
            };
        }

        private static string BuildSubmittedInvoicesCsv(IReadOnlyCollection<FinancialExportRow> rows)
        {
            var builder = new StringBuilder();

            AppendCsvLine(
                builder,
                "Invoice Number",
                "Submitted",
                "Paid At",
                "Amount",
                "Purpose",
                "Team/Cost Centre",
                "Payout Type",
                "Status",
                "User");

            foreach (var row in rows)
            {
                AppendCsvLine(
                    builder,
                    row.InvoiceNumber,
                    FormatDisplayDate(row.CreatedAt),
                    FormatDisplayDate(row.PaidAt),
                    row.Amount.ToString("F2", CultureInfo.InvariantCulture),
                    row.PurposeOfPayment,
                    FormatTeamCostCentre(row),
                    row.PayoutType,
                    row.Status,
                    row.User);
            }

            return builder.ToString();
        }

        private static string BuildPaymentRequestsCsv(IReadOnlyCollection<FinancialExportRow> rows)
        {
            var builder = new StringBuilder();

            AppendCsvLine(
                builder,
                "Amount",
                "Due Date",
                "Purpose",
                "Team/Cost Centre",
                "Status",
                "User");

            foreach (var row in rows)
            {
                AppendCsvLine(
                    builder,
                    row.Amount.ToString("F2", CultureInfo.InvariantCulture),
                    FormatDisplayDate(row.DueDate),
                    row.PurposeOfPayment,
                    FormatTeamCostCentre(row),
                    row.Status,
                    row.User);
            }

            return builder.ToString();
        }

        private static byte[] BuildPdf(
            IReadOnlyCollection<FinancialExportRow> rows,
            string title,
            FinancialExportSource source)
        {
            return source switch
            {
                FinancialExportSource.SubmittedInvoices => BuildSubmittedInvoicesPdf(rows, title),
                FinancialExportSource.PaymentRequests => BuildPaymentRequestsPdf(rows, title),
                _ => throw new InvalidStateException("Unsupported financial export source."),
            };
        }

        private static byte[] BuildSubmittedInvoicesPdf(IReadOnlyCollection<FinancialExportRow> rows, string title)
        {
            var lines = new List<string>
            {
                title,
                $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
                $"Invoices: {rows.Count}",
                string.Empty,
                "Invoice Number  Submitted   Paid At     Amount      Purpose              Team/Cost Centre        Payout Type       Status             User",
            };

            foreach (var row in rows)
            {
                AddWrappedPdfRow(
                    lines,
                    (row.InvoiceNumber, 15),
                    (FormatDisplayDate(row.CreatedAt), 11),
                    (FormatDisplayDate(row.PaidAt), 10),
                    (row.Amount.ToString("F2", CultureInfo.InvariantCulture), 10),
                    (row.PurposeOfPayment, 20),
                    (FormatTeamCostCentre(row), 23),
                    (row.PayoutType, 17),
                    (row.Status, 18),
                    (row.User, 24));
            }

            return SimplePdfBuilder.Build(lines);
        }

        private static byte[] BuildPaymentRequestsPdf(IReadOnlyCollection<FinancialExportRow> rows, string title)
        {
            var lines = new List<string>
            {
                title,
                $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
                $"Payments: {rows.Count}",
                string.Empty,
                "Amount      Due Date    Purpose                         Team/Cost Centre              Status             User",
            };

            foreach (var row in rows)
            {
                AddWrappedPdfRow(
                    lines,
                    (row.Amount.ToString("F2", CultureInfo.InvariantCulture), 10),
                    (FormatDisplayDate(row.DueDate), 10),
                    (row.PurposeOfPayment, 30),
                    (FormatTeamCostCentre(row), 29),
                    (row.Status, 18),
                    (row.User, 28));
            }

            return SimplePdfBuilder.Build(lines);
        }

        private static string GetFilePrefix(FinancialExportSource source)
        {
            return source switch
            {
                FinancialExportSource.SubmittedInvoices => "submitted-invoices-export",
                FinancialExportSource.PaymentRequests => "payment-requests-export",
                _ => throw new InvalidStateException("Unsupported financial export source."),
            };
        }

        private static string GetTitle(FinancialExportSource source)
        {
            return source switch
            {
                FinancialExportSource.SubmittedInvoices => "PayTrack Submitted Invoices Export",
                FinancialExportSource.PaymentRequests => "PayTrack Payment Requests Export",
                _ => throw new InvalidStateException("Unsupported financial export source."),
            };
        }

        private static void AppendCsvLine(StringBuilder builder, params string?[] values)
        {
            builder.AppendLine(string.Join(",", values.Select(EscapeCsvValue)));
        }

        private static string EscapeCsvValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var mustQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            var escaped = value.Replace("\"", "\"\"");

            return mustQuote ? $"\"{escaped}\"" : escaped;
        }

        private static string FormatDisplayDate(DateTime? value)
        {
            return value?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static string FormatTeamCostCentre(FinancialExportRow row)
        {
            if (string.IsNullOrWhiteSpace(row.CostCentre))
            {
                return row.Team;
            }

            return $"{row.Team} / {row.CostCentre}";
        }

        private static string GetPayoutTypeLabel(PayoutType payoutType)
        {
            return payoutType switch
            {
                PayoutType.User => "Pay to User",
                PayoutType.NotYetPaid => "Pay to Supplier",
                PayoutType.AlreadyPaid => "Already Paid",
                _ => payoutType.ToString(),
            };
        }

        private static string GetTransactionStatusLabel(TransactionStatus status)
        {
            return status switch
            {
                TransactionStatus.ChangesRequested => "Changes Requested",
                _ => status.ToString(),
            };
        }

        private static void AddWrappedPdfRow(
            ICollection<string> lines,
            params (string? Value, int Width)[] columns)
        {
            var wrappedColumns = columns
                .Select(column => WrapForPdf(column.Value, column.Width))
                .ToList();
            var rowLineCount = wrappedColumns.Max(column => column.Count);

            for (var lineIndex = 0; lineIndex < rowLineCount; lineIndex++)
            {
                var builder = new StringBuilder();

                for (var columnIndex = 0; columnIndex < columns.Length; columnIndex++)
                {
                    var column = columns[columnIndex];
                    var value = lineIndex < wrappedColumns[columnIndex].Count
                        ? wrappedColumns[columnIndex][lineIndex]
                        : string.Empty;

                    builder.Append(value.PadRight(column.Width));

                    if (columnIndex < columns.Length - 1)
                    {
                        builder.Append(' ');
                    }
                }

                lines.Add(builder.ToString().TrimEnd());
            }
        }

        private static IReadOnlyList<string> WrapForPdf(string? value, int maxLength)
        {
            if (maxLength <= 0)
            {
                return [string.Empty];
            }

            var normalizedValue = value?
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n') ?? string.Empty;
            var result = new List<string>();

            foreach (var paragraph in normalizedValue.Split('\n'))
            {
                AddWrappedParagraph(result, paragraph, maxLength);
            }

            return result.Count == 0 ? [string.Empty] : result;
        }

        private static void AddWrappedParagraph(ICollection<string> result, string paragraph, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(paragraph))
            {
                result.Add(string.Empty);
                return;
            }

            var currentLine = string.Empty;

            foreach (var originalWord in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var word = originalWord;

                while (word.Length > maxLength)
                {
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        result.Add(currentLine);
                        currentLine = string.Empty;
                    }

                    result.Add(word[..maxLength]);
                    word = word[maxLength..];
                }

                if (string.IsNullOrEmpty(currentLine))
                {
                    currentLine = word;
                    continue;
                }

                if (currentLine.Length + 1 + word.Length <= maxLength)
                {
                    currentLine += $" {word}";
                    continue;
                }

                result.Add(currentLine);
                currentLine = word;
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                result.Add(currentLine);
            }
        }

        private async Task<IReadOnlyCollection<Transaction>> GetTransactionsForExportAsync(
            GetFinancialExportQuery query,
            FinancialExportSource source)
        {
            return source switch
            {
                FinancialExportSource.SubmittedInvoices => await this.GetSubmittedInvoicesForExportAsync(query),
                FinancialExportSource.PaymentRequests => await this.GetPaymentRequestsForExportAsync(query),
                _ => throw new InvalidStateException("Unsupported financial export source."),
            };
        }

        private async Task<IReadOnlyCollection<Transaction>> GetSubmittedInvoicesForExportAsync(GetFinancialExportQuery query)
        {
            var (transactions, _) = await this.transactionRepository.GetAllAsync(CreateSubmittedInvoicesExportQuery(query));
            return transactions.Cast<Transaction>().ToList();
        }

        private async Task<IReadOnlyCollection<Transaction>> GetPaymentRequestsForExportAsync(GetFinancialExportQuery query)
        {
            var (transactions, _) = await this.transactionRepository.GetAllAsync(CreatePaymentRequestsExportQuery(query));
            return transactions
                .Where(transaction => query.Status.HasValue || transaction.Status is TransactionStatus.Submitted or TransactionStatus.Paid)
                .Cast<Transaction>()
                .ToList();
        }

        private sealed class FinancialExportRow
        {
            public FinancialExportRow(
                int id,
                string? invoiceNumber,
                string type,
                string direction,
                string status,
                decimal amount,
                string? purposeOfPayment,
                string? paymentReference,
                string team,
                string? costCentre,
                string? budget,
                string user,
                DateTime createdAt,
                DateTime? paidAt,
                DateTime? financePaidAt,
                DateTime? dueDate,
                string? payoutType)
            {
                this.Id = id;
                this.InvoiceNumber = invoiceNumber;
                this.Type = type;
                this.Direction = direction;
                this.Status = status;
                this.Amount = amount;
                this.PurposeOfPayment = purposeOfPayment;
                this.PaymentReference = paymentReference;
                this.Team = team;
                this.CostCentre = costCentre;
                this.Budget = budget;
                this.User = user;
                this.CreatedAt = createdAt;
                this.PaidAt = paidAt;
                this.FinancePaidAt = financePaidAt;
                this.DueDate = dueDate;
                this.PayoutType = payoutType;
            }

            public int Id { get; }

            public string? InvoiceNumber { get; }

            public string Type { get; }

            public string Direction { get; }

            public string Status { get; }

            public decimal Amount { get; }

            public string? PurposeOfPayment { get; }

            public string? PaymentReference { get; }

            public string Team { get; }

            public string? CostCentre { get; }

            public string? Budget { get; }

            public string User { get; }

            public DateTime CreatedAt { get; }

            public DateTime? PaidAt { get; }

            public DateTime? FinancePaidAt { get; }

            public DateTime? DueDate { get; }

            public string? PayoutType { get; }
        }

        private sealed class SimplePdfBuilder
        {
            private const int LinesPerPage = 42;

            public static byte[] Build(IReadOnlyList<string> lines)
            {
                var pages = lines.Count == 0
                    ? [new List<string>()]
                    : lines
                        .Select((line, index) => new { line, index })
                        .GroupBy(item => item.index / LinesPerPage)
                        .Select(group => group.Select(item => item.line).ToList())
                        .ToList();

                var objects = new List<string>();
                var pageObjectIds = new List<int>();
                var fontObjectId = 3;

                objects.Add("<< /Type /Catalog /Pages 2 0 R >>");
                objects.Add(string.Empty);
                objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Courier /Encoding /WinAnsiEncoding >>");

                foreach (var page in pages)
                {
                    var pageObjectId = objects.Count + 1;
                    var contentObjectId = pageObjectId + 1;
                    pageObjectIds.Add(pageObjectId);

                    objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 842 595] /Resources << /Font << /F1 {fontObjectId} 0 R >> >> /Contents {contentObjectId} 0 R >>");

                    var content = BuildPageContent(page);
                    objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream");
                }

                objects[1] = $"<< /Type /Pages /Kids [{string.Join(" ", pageObjectIds.Select(id => $"{id} 0 R"))}] /Count {pageObjectIds.Count} >>";

                return WritePdf(objects);
            }

            private static string BuildPageContent(IEnumerable<string> lines)
            {
                var builder = new StringBuilder();
                builder.AppendLine("BT");
                builder.AppendLine("/F1 9 Tf");
                builder.AppendLine("40 555 Td");

                foreach (var line in lines)
                {
                    builder.AppendLine($"({EscapePdfText(line)}) Tj");
                    builder.AppendLine("0 -12 Td");
                }

                builder.AppendLine("ET");
                return builder.ToString();
            }

            private static byte[] WritePdf(IReadOnlyList<string> objects)
            {
                var builder = new StringBuilder();
                var offsets = new List<int> { 0 };

                builder.AppendLine("%PDF-1.4");

                for (var index = 0; index < objects.Count; index++)
                {
                    offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
                    builder.AppendLine($"{index + 1} 0 obj");
                    builder.AppendLine(objects[index]);
                    builder.AppendLine("endobj");
                }

                var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
                builder.AppendLine("xref");
                builder.AppendLine($"0 {objects.Count + 1}");
                builder.AppendLine("0000000000 65535 f ");

                foreach (var offset in offsets.Skip(1))
                {
                    builder.AppendLine($"{offset:0000000000} 00000 n ");
                }

                builder.AppendLine("trailer");
                builder.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
                builder.AppendLine("startxref");
                builder.AppendLine(xrefOffset.ToString(CultureInfo.InvariantCulture));
                builder.AppendLine("%%EOF");

                return Encoding.ASCII.GetBytes(builder.ToString());
            }

            private static string EscapePdfText(string value)
            {
                var builder = new StringBuilder();

                foreach (var character in value)
                {
                    var encoded = ToWinAnsiByte(character);

                    if (encoded is (byte)'\\' or (byte)'(' or (byte)')')
                    {
                        builder.Append('\\');
                        builder.Append((char)encoded);
                        continue;
                    }

                    if (encoded is < 32 or > 126)
                    {
                        builder.Append('\\');
                        builder.Append(Convert.ToString(encoded, 8).PadLeft(3, '0'));
                        continue;
                    }

                    builder.Append((char)encoded);
                }

                return builder.ToString();
            }

            private static byte ToWinAnsiByte(char character)
            {
                if (character <= 0x7F)
                {
                    return (byte)character;
                }

                if (character is >= '\u00A0' and <= '\u00FF')
                {
                    return (byte)character;
                }

                return character switch
                {
                    '\u2010' or '\u2011' or '\u2212' => (byte)'-',
                    '\u20AC' => 0x80,
                    '\u201A' => 0x82,
                    '\u0192' => 0x83,
                    '\u201E' => 0x84,
                    '\u2026' => 0x85,
                    '\u2020' => 0x86,
                    '\u2021' => 0x87,
                    '\u02C6' => 0x88,
                    '\u2030' => 0x89,
                    '\u0160' => 0x8A,
                    '\u2039' => 0x8B,
                    '\u0152' => 0x8C,
                    '\u017D' => 0x8E,
                    '\u2018' => 0x91,
                    '\u2019' => 0x92,
                    '\u201C' => 0x93,
                    '\u201D' => 0x94,
                    '\u2022' => 0x95,
                    '\u2013' => 0x96,
                    '\u2014' => 0x97,
                    '\u02DC' => 0x98,
                    '\u2122' => 0x99,
                    '\u0161' => 0x9A,
                    '\u203A' => 0x9B,
                    '\u0153' => 0x9C,
                    '\u017E' => 0x9E,
                    '\u0178' => 0x9F,
                    _ => (byte)'?',
                };
            }
        }
    }
}
