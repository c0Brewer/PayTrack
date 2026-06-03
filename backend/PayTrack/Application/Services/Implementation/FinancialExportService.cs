//AI helped with PDF and CSV generation

// <copyright file="FinancialExportService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class FinancialExportService(ITransactionRepository _transactionRepository) : IFinancialExportService
    {
        private const string CsvContentType = "text/csv; charset=utf-8";
        private const string PdfContentType = "application/pdf";

        private readonly ITransactionRepository transactionRepository = _transactionRepository;

        /// <inheritdoc/>
        public async Task<FinancialExportResult> ExportFinancialDataAsync(GetTransactionQuery query)
        {
            var exportQuery = CreateExportQuery(query);
            var (transactions, _) = await this.transactionRepository.GetAllAsync(exportQuery);
            var rows = transactions
                .OrderBy(transaction => transaction.PaidAt ?? transaction.CreatedAt)
                .ThenBy(transaction => transaction.Id)
                .Select(ToExportRow)
                .ToList();

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var format = query.Format ?? FinancialExportFormat.Csv;

            return format switch
            {
                FinancialExportFormat.Csv => new FinancialExportResult(
                    Encoding.UTF8.GetBytes(BuildCsv(rows)),
                    CsvContentType,
                    $"financial-export-{timestamp}.csv"),

                FinancialExportFormat.Pdf => new FinancialExportResult(
                    BuildPdf(rows),
                    PdfContentType,
                    $"financial-export-{timestamp}.pdf"),

                _ => throw new InvalidStateException("Unsupported financial export format."),
            };
        }

        private static GetTransactionQuery CreateExportQuery(GetTransactionQuery query)
        {
            return new GetTransactionQuery
            {
                UserId = query.UserId,
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
                IncludeTeam = true,
                IncludeBudget = true,
            };
        }

        private static FinancialExportRow ToExportRow(Transaction transaction)
        {
            return new FinancialExportRow(
                transaction.Id,
                GetTransactionType(transaction),
                transaction.PaymentDirection.ToString(),
                transaction.Status.ToString(),
                transaction.Amount,
                transaction.PurposeOfPayment,
                transaction.PaymentReference,
                transaction.Team?.Name ?? transaction.TeamId.ToString(CultureInfo.InvariantCulture),
                transaction.Budget?.CostCentre.Name,
                transaction.Budget?.Name,
                transaction.User?.Email ?? transaction.UserId.ToString(CultureInfo.InvariantCulture),
                transaction.CreatedAt,
                transaction.PaidAt,
                transaction.FinancePaidAt,
                transaction.DueDate);
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

        private static string BuildCsv(IReadOnlyCollection<FinancialExportRow> rows)
        {
            var builder = new StringBuilder();

            AppendCsvLine(
                builder,
                "Id",
                "Type",
                "Direction",
                "Status",
                "Amount",
                "PurposeOfPayment",
                "PaymentReference",
                "Team",
                "CostCentre",
                "Budget",
                "User",
                "CreatedAt",
                "PaidAt",
                "FinancePaidAt",
                "DueDate");

            foreach (var row in rows)
            {
                AppendCsvLine(
                    builder,
                    row.Id.ToString(CultureInfo.InvariantCulture),
                    row.Type,
                    row.Direction,
                    row.Status,
                    row.Amount.ToString("F2", CultureInfo.InvariantCulture),
                    row.PurposeOfPayment,
                    row.PaymentReference,
                    row.Team,
                    row.CostCentre,
                    row.Budget,
                    row.User,
                    FormatDate(row.CreatedAt),
                    FormatDate(row.PaidAt),
                    FormatDate(row.FinancePaidAt),
                    FormatDate(row.DueDate));
            }

            AppendCsvLine(builder);
            AppendCsvLine(builder, "Summary");
            AppendCsvLine(builder, "Income", CalculateTotal(rows, PaymentDirection.In).ToString("F2", CultureInfo.InvariantCulture));
            AppendCsvLine(builder, "Expenses", CalculateTotal(rows, PaymentDirection.Out).ToString("F2", CultureInfo.InvariantCulture));
            AppendCsvLine(builder, "Net", CalculateNetTotal(rows).ToString("F2", CultureInfo.InvariantCulture));

            return builder.ToString();
        }

        private static byte[] BuildPdf(IReadOnlyCollection<FinancialExportRow> rows)
        {
            var lines = new List<string>
            {
                "PayTrack Financial Export",
                $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
                $"Rows: {rows.Count}",
                $"Income: {CalculateTotal(rows, PaymentDirection.In):F2}",
                $"Expenses: {CalculateTotal(rows, PaymentDirection.Out):F2}",
                $"Net: {CalculateNetTotal(rows):F2}",
                string.Empty,
                "Date       Type     Dir  Status             Amount       Team / Cost Centre",
            };

            lines.AddRange(rows.Select(row =>
                $"{FormatDate(row.PaidAt ?? row.CreatedAt),-10} {TrimForPdf(row.Type, 8),-8} {TrimForPdf(row.Direction, 3),-3} {TrimForPdf(row.Status, 18),-18} {row.Amount,10:F2}  {TrimForPdf($"{row.Team} / {row.CostCentre}", 42)}"));

            return SimplePdfBuilder.Build(lines);
        }

        private static decimal CalculateTotal(IEnumerable<FinancialExportRow> rows, PaymentDirection direction)
        {
            return rows
                .Where(row => string.Equals(row.Direction, direction.ToString(), StringComparison.Ordinal))
                .Sum(row => row.Amount);
        }

        private static decimal CalculateNetTotal(IReadOnlyCollection<FinancialExportRow> rows)
        {
            return CalculateTotal(rows, PaymentDirection.In) - CalculateTotal(rows, PaymentDirection.Out);
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

        private static string FormatDate(DateTime? value)
        {
            return value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static string TrimForPdf(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value ?? string.Empty;
            }

            return value[..(maxLength - 3)] + "...";
        }

        private sealed class FinancialExportRow
        {
            public FinancialExportRow(
                int id,
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
                DateTime? dueDate)
            {
                this.Id = id;
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
            }

            public int Id { get; }

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
                objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Courier >>");

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
                return value
                    .Replace("\\", "\\\\")
                    .Replace("(", "\\(")
                    .Replace(")", "\\)");
            }
        }
    }
}
