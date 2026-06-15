//AI helped with the test cases

using System.Text;
using FluentAssertions;
using Moq;
using PayTrack.Application.Dto.PaymentRequestByTeam;
using PayTrack.Application.Dto.PaymentRequestByUser;
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
        public async Task ExportFinancialDataAsync_ShouldUseSubmittedInvoicesSourceAndInvoiceFilters()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var capturedQuery = new GetPaymentRequestByUserQuery();
            var transactions = new List<PaymentRequestByUser>
            {
                new PaymentRequestByUser
                {
                    Id = 4,
                    InvoiceNumber = "INV-44",
                    Amount = 80,
                    PurposeOfPayment = "Tools",
                    PaymentDirection = PaymentDirection.Out,
                    Status = TransactionStatus.Paid,
                    TeamId = 2,
                    Team = new Team { Id = 2, Name = "Electronics" },
                    UserId = 5,
                    User = new User { Id = 5, Name = "Alice Admin", Email = "alice@paytrack.dev" },
                    CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                    PaidAt = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc),
                    PayoutType = PayoutType.AlreadyPaid,
                    Budget = new Budget
                    {
                        Id = 2,
                        Name = "Soldering",
                        CostCentre = new CostCentre { Id = 2, Name = "Lab" },
                    },
                },
            };

            repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetPaymentRequestByUserQuery>()))
                .Callback<GetPaymentRequestByUserQuery>(query => capturedQuery = query)
                .ReturnsAsync((transactions, transactions.Count));

            var service = new FinancialExportService(repoMock.Object);

            var result = await service.ExportFinancialDataAsync(new GetTransactionQuery
            {
                Source = FinancialExportSource.SubmittedInvoices,
                Format = FinancialExportFormat.Csv,
                InvoiceNumber = "INV-44",
                PayoutType = PayoutType.AlreadyPaid,
                BankAccountId = 12,
                TeamId = 2,
                UserId = 5,
                SortBy = "InvoiceNumber",
                SortDirection = "Asc",
                Limit = 10,
                Offset = 20,
            });

            result.FileName.Should().StartWith("submitted-invoices-export-");
            result.FileName.Should().EndWith(".csv");
            capturedQuery.InvoiceNumber.Should().Be("INV-44");
            capturedQuery.PayoutType.Should().Be(PayoutType.AlreadyPaid);
            capturedQuery.BankAccountId.Should().Be(12);
            capturedQuery.TeamId.Should().Be(2);
            capturedQuery.UserId.Should().Be(5);
            capturedQuery.SortBy.Should().Be("InvoiceNumber");
            capturedQuery.SortDirection.Should().Be("Asc");
            capturedQuery.IncludeTeam.Should().BeTrue();
            capturedQuery.IncludeBudget.Should().BeTrue();
            capturedQuery.IncludeBankAccount.Should().BeTrue();
            capturedQuery.Limit.Should().BeNull();
            capturedQuery.Offset.Should().BeNull();
            repoMock.Verify(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()), Times.Never);

            var csv = Encoding.UTF8.GetString(result.Content);
            csv.Should().Contain("Invoice Number,Submitted,Paid At,Amount,Purpose,Team/Cost Centre,Payout Type,Status,User");
            csv.Should().Contain("INV-44,01.02.2026,03.02.2026,80.00,Tools,Electronics / Lab,Already Paid,Paid,Alice Admin");

            var pdfResult = await service.ExportFinancialDataAsync(new GetTransactionQuery
            {
                Source = FinancialExportSource.SubmittedInvoices,
                Format = FinancialExportFormat.Pdf,
            });
            var pdf = Encoding.ASCII.GetString(pdfResult.Content);
            pdf.Should().Contain("Invoice Number");
            pdf.Should().Contain("Submitted");
            pdf.Should().Contain("Paid At");
            pdf.Should().Contain("Team/Cost Centre");
            pdf.Should().Contain("Payout Type");
            pdf.Should().Contain("Alice Admin");
        }

        [Fact]
        public async Task ExportFinancialDataAsync_ShouldUsePaymentRequestsSourceAndVisibleStatuses()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var capturedQuery = new GetPaymentRequestByTeamQuery();
            var transactions = new List<PaymentRequestByTeam>
            {
                new PaymentRequestByTeam
                {
                    Id = 1,
                    Amount = 40,
                    PaymentDirection = PaymentDirection.In,
                    Status = TransactionStatus.Paid,
                    TeamId = 1,
                    Team = new Team { Id = 1, Name = "Powertrain" },
                    UserId = 1,
                    User = new User { Id = 1, Name = "Member One", Email = "member1@paytrack.dev" },
                    PurposeOfPayment = "Workshop fee",
                    CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                    DueDate = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc),
                },
                new PaymentRequestByTeam
                {
                    Id = 2,
                    Amount = 100,
                    PaymentDirection = PaymentDirection.In,
                    Status = TransactionStatus.Submitted,
                    TeamId = 1,
                    Team = new Team { Id = 1, Name = "Powertrain" },
                    UserId = 1,
                    User = new User { Id = 1, Name = "Member One", Email = "member1@paytrack.dev" },
                    PurposeOfPayment = "Membership fee",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    DueDate = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc),
                    Budget = new Budget
                    {
                        Id = 1,
                        Name = "Income Budget",
                        CostCentre = new CostCentre { Id = 1, Name = "Membership" },
                    },
                },
                new PaymentRequestByTeam
                {
                    Id = 3,
                    Amount = 20,
                    PaymentDirection = PaymentDirection.In,
                    Status = TransactionStatus.Approved,
                    TeamId = 1,
                    Team = new Team { Id = 1, Name = "Powertrain" },
                    UserId = 1,
                    User = new User { Id = 1, Name = "Member One", Email = "member1@paytrack.dev" },
                    CreatedAt = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                },
            };

            repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetPaymentRequestByTeamQuery>()))
                .Callback<GetPaymentRequestByTeamQuery>(query => capturedQuery = query)
                .ReturnsAsync((transactions, transactions.Count));

            var service = new FinancialExportService(repoMock.Object);

            var result = await service.ExportFinancialDataAsync(new GetTransactionQuery
            {
                Source = FinancialExportSource.PaymentRequests,
                Format = FinancialExportFormat.Csv,
                RequestById = 8,
                TeamId = 1,
                SortBy = "DueDate",
                SortDirection = "Desc",
            });

            result.FileName.Should().StartWith("payment-requests-export-");
            result.FileName.Should().EndWith(".csv");
            capturedQuery.RequestById.Should().Be(8);
            capturedQuery.TeamId.Should().Be(1);
            capturedQuery.Status.Should().BeNull();
            capturedQuery.SortBy.Should().Be("DueDate");
            capturedQuery.SortDirection.Should().Be("Desc");
            capturedQuery.IncludeTeam.Should().BeTrue();
            capturedQuery.IncludeBudget.Should().BeTrue();
            capturedQuery.VisibleStatusesOnly.Should().BeTrue();

            var csv = Encoding.UTF8.GetString(result.Content);
            csv.Should().Contain("Amount,Due Date,Purpose,Team/Cost Centre,Status,User");
            csv.Should().Contain("40.00,15.02.2026,Workshop fee,Powertrain,Paid,Member One");
            csv.Should().Contain("100.00,31.01.2026,Membership fee,Powertrain / Membership,Submitted,Member One");
            csv.IndexOf("40.00,15.02.2026", StringComparison.Ordinal)
                .Should()
                .BeLessThan(csv.IndexOf("100.00,31.01.2026", StringComparison.Ordinal));
            csv.Should().NotContain("Approved,20.00");

            var pdfResult = await service.ExportFinancialDataAsync(new GetTransactionQuery
            {
                Source = FinancialExportSource.PaymentRequests,
                Format = FinancialExportFormat.Pdf,
                SortBy = "DueDate",
                SortDirection = "Desc",
            });
            var pdf = Encoding.ASCII.GetString(pdfResult.Content);
            pdf.Should().Contain("Amount");
            pdf.Should().Contain("Due Date");
            pdf.Should().Contain("Purpose");
            pdf.Should().Contain("Team/Cost Centre");
            pdf.Should().Contain("Status");
            pdf.Should().Contain("Member One");
            pdf.IndexOf("Workshop fee", StringComparison.Ordinal)
                .Should()
                .BeLessThan(pdf.IndexOf("Membership fee", StringComparison.Ordinal));
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
