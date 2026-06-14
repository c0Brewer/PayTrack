using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Implementation;

namespace PayTrack.Tests.UnitTests.Services
{
    public class ReceiptExtractionServiceTests
    {
        [Fact]
        public async Task ExtractAsync_ExtractsFieldsFromTextBasedPdf()
        {
            var filePath = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "../../../../PayTrack/uploads/presentation-invoices/invoice-consulting-2026.pdf"));
            await using var stream = File.OpenRead(filePath);
            var formFile = new FormFile(stream, 0, stream.Length, "receipt", Path.GetFileName(filePath))
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf",
            };
            var service = new ReceiptExtractionService(
                new ReceiptParser(),
                NullLogger<ReceiptExtractionService>.Instance);

            var result = await service.ExtractAsync(formFile);

            result.ExtractionSucceeded.Should().BeTrue();
            result.Amount.Value.Should().NotBeNull();
            result.InvoiceDate.Value.Should().NotBeNull();
            result.InvoiceNumber.Value.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task ExtractAsync_RejectsUnsupportedFileType()
        {
            await using var stream = new MemoryStream([1, 2, 3]);
            var formFile = new FormFile(stream, 0, stream.Length, "receipt", "invoice.txt");
            var service = new ReceiptExtractionService(
                new ReceiptParser(),
                NullLogger<ReceiptExtractionService>.Instance);

            var act = () => service.ExtractAsync(formFile);

            await act.Should().ThrowAsync<InvalidFileException>()
                .WithMessage("*PDF, JPG, JPEG, and PNG*");
        }
    }
}
