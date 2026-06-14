// <copyright file="ReceiptExtractionService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Text;
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace PayTrack.Application.Services.Implementation
{
    /// <summary>
    /// Extracts receipt text with PdfPig or Tesseract and delegates field selection to the rule-based parser.
    /// </summary>
    public sealed class ReceiptExtractionService(
        IReceiptParser receiptParser,
        ILogger<ReceiptExtractionService> logger) : IReceiptExtractionService
    {
        private const long MaximumFileSize = 10 * 1024 * 1024;
        private const int MinimumEmbeddedPdfTextLength = 40; // Below this threshold, treat the PDF as scanned and use OCR.
        private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".jpg", ".jpeg", ".png",
        };

        private readonly IReceiptParser receiptParser = receiptParser;
        private readonly ILogger<ReceiptExtractionService> logger = logger;

        /// <inheritdoc/>
        public async Task<ReceiptExtractionDto> ExtractAsync(
            IFormFile receipt,
            CancellationToken cancellationToken = default)
        {
            Validate(receipt);

            var extension = Path.GetExtension(receipt.FileName);
            var tempDirectory = Path.Combine(Path.GetTempPath(), $"paytrack-receipt-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDirectory);
            var inputPath = Path.Combine(tempDirectory, $"input{extension.ToLowerInvariant()}");

            try
            {
                await using (var input = File.Create(inputPath))
                {
                    await receipt.CopyToAsync(input, cancellationToken);
                }

                string text;
                if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    text = ExtractPdfText(inputPath);
                    if (CountUsefulCharacters(text) < MinimumEmbeddedPdfTextLength)
                    {
                        text = await ExtractScannedPdfTextAsync(inputPath, tempDirectory, cancellationToken);
                    }
                }
                else
                {
                    text = await RunTesseractAsync(inputPath, cancellationToken);
                }

                Console.WriteLine(text);
                return this.receiptParser.Parse(text);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Receipt extraction failed for {FileName}", receipt.FileName);
                return ReceiptExtractionDto.Failed(
                    "The receipt could not be read automatically. You can still enter the invoice data manually.");
            }
            finally
            {
                try
                {
                    Directory.Delete(tempDirectory, true);
                }
                catch (Exception exception)
                {
                    this.logger.LogDebug(exception, "Could not remove temporary receipt extraction directory.");
                }
            }
        }

        /// <summary>
        /// Validates that the uploaded receipt is non-empty, within the size limit, and has a supported extension.
        /// </summary>
        /// <param name="receipt">Receipt file to validate.</param>
        /// <exception cref="InvalidFileException">Thrown when the receipt does not meet the upload requirements.</exception>
        private static void Validate(IFormFile receipt)
        {
            if (receipt.Length <= 0)
            {
                throw new InvalidFileException("The receipt is empty.");
            }

            if (receipt.Length > MaximumFileSize)
            {
                throw new InvalidFileException("The receipt must not exceed 10 MB.");
            }

            if (!SupportedExtensions.Contains(Path.GetExtension(receipt.FileName)))
            {
                throw new InvalidFileException("Only PDF, JPG, JPEG, and PNG receipts are supported.");
            }
        }

        /// <summary>
        /// Extracts embedded text from every page of a text-based PDF using PdfPig.
        /// </summary>
        /// <param name="inputPath">Path to the temporary PDF file.</param>
        /// <returns>The combined text of all PDF pages.</returns>
        private static string ExtractPdfText(string inputPath)
        {
            var builder = new StringBuilder();
            using var document = PdfDocument.Open(inputPath);

            foreach (var page in document.GetPages())
            {
                builder.AppendLine(ContentOrderTextExtractor.GetText(page));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Converts a scanned PDF into page images and extracts their text with Tesseract.
        /// </summary>
        /// <param name="inputPath">Path to the temporary PDF file.</param>
        /// <param name="tempDirectory">Directory used for the generated page images.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The combined OCR text of all PDF pages.</returns>
        private static async Task<string> ExtractScannedPdfTextAsync(
            string inputPath,
            string tempDirectory,
            CancellationToken cancellationToken)
        {
            var outputPrefix = Path.Combine(tempDirectory, "page");
            await RunProcessAsync(
                "pdftoppm",
                ["-png", "-r", "300", inputPath, outputPrefix],
                cancellationToken);

            var builder = new StringBuilder();
            foreach (var pagePath in Directory.EnumerateFiles(tempDirectory, "page-*.png").Order())
            {
                builder.AppendLine(await RunTesseractAsync(pagePath, cancellationToken));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Runs Tesseract OCR with English and German language data for an image.
        /// </summary>
        /// <param name="inputPath">Path to the image to process.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The text recognized by Tesseract.</returns>
        private static Task<string> RunTesseractAsync(string inputPath, CancellationToken cancellationToken)
        {
            return RunProcessAsync(
                "tesseract",
                [inputPath, "stdout", "-l", "eng+deu", "--psm", "1"],
                cancellationToken);
        }

        /// <summary>
        /// Executes an external process and returns its standard output.
        /// </summary>
        /// <param name="fileName">Name or path of the executable.</param>
        /// <param name="arguments">Arguments passed to the executable.</param>
        /// <param name="cancellationToken">Cancellation token used to terminate the process.</param>
        /// <returns>The standard output produced by the process.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the process cannot start or exits unsuccessfully.</exception>
        private static async Task<string> RunProcessAsync(
            string fileName,
            IReadOnlyCollection<string> arguments,
            CancellationToken cancellationToken)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            foreach (var argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException($"Could not start {fileName}.");

            var standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var standardError = process.StandardError.ReadToEndAsync(cancellationToken);
            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }

                throw;
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"{fileName} failed with exit code {process.ExitCode}: {await standardError}");
            }

            return await standardOutput;
        }

        /// <summary>
        /// Counts letters and digits to determine whether embedded PDF text is sufficient for parsing.
        /// </summary>
        /// <param name="text">Extracted text to inspect.</param>
        /// <returns>The number of alphanumeric characters.</returns>
        private static int CountUsefulCharacters(string text)
        {
            return text.Count(char.IsLetterOrDigit);
        }
    }
}
