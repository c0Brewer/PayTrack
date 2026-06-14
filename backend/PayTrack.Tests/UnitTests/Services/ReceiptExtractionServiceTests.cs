using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Implementation;

namespace PayTrack.Tests.UnitTests.Services
{
    public class ReceiptExtractionServiceTests
    {
        private readonly ReceiptExtractionService service = new(
            new ReceiptParser(),
            NullLogger<ReceiptExtractionService>.Instance);

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
            var result = await this.service.ExtractAsync(formFile);

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

            var act = () => this.service.ExtractAsync(formFile);

            await act.Should().ThrowAsync<InvalidFileException>()
                .WithMessage("*PDF, JPG, JPEG, and PNG*");
        }

        [Fact]
        public async Task ExtractAsync_RejectsEmptyFile()
        {
            await using var stream = new MemoryStream();
            var formFile = new FormFile(stream, 0, 0, "receipt", "invoice.pdf");

            var act = () => this.service.ExtractAsync(formFile);

            await act.Should().ThrowAsync<InvalidFileException>()
                .WithMessage("*empty*");
        }

        [Fact]
        public async Task ExtractAsync_RejectsFileLargerThanTenMegabytes()
        {
            await using var stream = new MemoryStream([1]);
            var formFile = new FormFile(stream, 0, (10 * 1024 * 1024) + 1, "receipt", "invoice.pdf");

            var act = () => this.service.ExtractAsync(formFile);

            await act.Should().ThrowAsync<InvalidFileException>()
                .WithMessage("*10 MB*");
        }

        [Theory]
        [InlineData("invoice.pdf")]
        [InlineData("invoice.PNG")]
        public async Task ExtractAsync_ReturnsFailureWhenSupportedFileCannotBeRead(string fileName)
        {
            await using var stream = new MemoryStream([1, 2, 3]);
            var formFile = new FormFile(stream, 0, stream.Length, "receipt", fileName);

            var result = await this.service.ExtractAsync(formFile);

            result.ExtractionSucceeded.Should().BeFalse();
            result.Message.Should().Contain("manually");
            result.Amount.Value.Should().BeNull();
            result.InvoiceDate.Value.Should().BeNull();
            result.InvoiceNumber.Value.Should().BeNull();
        }
    }
}
