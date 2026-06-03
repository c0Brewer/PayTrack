using System.Text;
using FluentAssertions;
using Moq;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Implementation;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Tests.UnitTests.Services
{
    public class FinancialExportServiceTests
    {
        [Fact]
        public async Task ExportFinancialDataAsync_ShouldReturnCsvWithRowsAndSummary()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var capturedQuery = new GetTransactionQuery();
            var transactions = new List<Transaction>
            {
                new PaymentRequestByUser
                {
                    Id = 1,
                    InvoiceNumber = "INV-1",
                    Amount = 50,
                    PurposeOfPayment = "Engine, \"repair\"",
                    PaymentDirection = PaymentDirection.Out,
                    Status = TransactionStatus.Paid,
                    TeamId = 1,
                    Team = new Team { Id = 1, Name = "Powertrain" },
                    UserId = 1,
                    User = new User { Id = 1, Email = "user@paytrack.dev", Name = "User" },
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    PaidAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                    Budget = new Budget
                    {
                        Id = 1,
                        Name = "Engine Budget",
                        CostCentre = new CostCentre { Id = 1, Name = "Engine" },
                    },
                },
                new PaymentRequestByTeam
                {
                    Id = 2,
                    Amount = 200,
                    PurposeOfPayment = "Membership fee",
                    PaymentDirection = PaymentDirection.In,
                    Status = TransactionStatus.Submitted,
                    TeamId = 1,
                    Team = new Team { Id = 1, Name = "Powertrain" },
                    UserId = 2,
                    User = new User { Id = 2, Email = "member@paytrack.dev", Name = "Member" },
                    CreatedAt = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                },
            };

            repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()))
                .Callback<GetTransactionQuery>(query => capturedQuery = query)
                .ReturnsAsync((transactions, transactions.Count));

            var service = new FinancialExportService(repoMock.Object);

            var result = await service.ExportFinancialDataAsync(new GetTransactionQuery
            {
                Format = FinancialExportFormat.Csv,
                TeamId = 1,
                CostCentreId = 1,
                Limit = 10,
                Offset = 20,
            });

            result.ContentType.Should().Be("text/csv; charset=utf-8");
            result.FileName.Should().EndWith(".csv");
            capturedQuery.TeamId.Should().Be(1);
            capturedQuery.CostCentreId.Should().Be(1);
            capturedQuery.IncludeTeam.Should().BeTrue();
            capturedQuery.IncludeBudget.Should().BeTrue();
            capturedQuery.Limit.Should().BeNull();
            capturedQuery.Offset.Should().BeNull();

            var csv = Encoding.UTF8.GetString(result.Content);
            csv.Should().Contain("Id,Type,Direction,Status,Amount");
            csv.Should().Contain("Expense,Out,Paid,50.00,\"Engine, \"\"repair\"\"\"");
            csv.Should().Contain("Income,In,Submitted,200.00");
            csv.Should().Contain("Income,200.00");
            csv.Should().Contain("Expenses,50.00");
            csv.Should().Contain("Net,150.00");
        }

        [Fact]
        public async Task ExportFinancialDataAsync_ShouldReturnPdf()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var transactions = new List<Transaction>
            {
                new PaymentRequestByTeam
                {
                    Id = 1,
                    Amount = 100,
                    PaymentDirection = PaymentDirection.In,
                    Status = TransactionStatus.Submitted,
                    TeamId = 1,
                    Team = new Team { Id = 1, Name = "Electronics" },
                    UserId = 1,
                    User = new User { Id = 1, Email = "user@paytrack.dev", Name = "User" },
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
            };

            repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()))
                .ReturnsAsync((transactions, transactions.Count));

            var service = new FinancialExportService(repoMock.Object);

            var result = await service.ExportFinancialDataAsync(new GetTransactionQuery
            {
                Format = FinancialExportFormat.Pdf,
            });

            result.ContentType.Should().Be("application/pdf");
            result.FileName.Should().EndWith(".pdf");

            var pdf = Encoding.ASCII.GetString(result.Content);
            pdf.Should().StartWith("%PDF-1.4");
            pdf.Should().Contain("PayTrack Financial Export");
            pdf.Should().Contain("Rows: 1");
            pdf.Should().Contain("%%EOF");
        }

        [Fact]
        public async Task ExportFinancialDataAsync_ShouldThrow_WhenFormatIsUnsupported()
        {
            var repoMock = new Mock<ITransactionRepository>();
            repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()))
                .ReturnsAsync((new List<Transaction>(), 0));
            var service = new FinancialExportService(repoMock.Object);

            Func<Task> act = async () =>
                await service.ExportFinancialDataAsync(new GetTransactionQuery
                {
                    Format = (FinancialExportFormat)999,
                });

            await act.Should()
                .ThrowAsync<InvalidStateException>()
                .WithMessage("Unsupported financial export format.");
        }
    }
}
