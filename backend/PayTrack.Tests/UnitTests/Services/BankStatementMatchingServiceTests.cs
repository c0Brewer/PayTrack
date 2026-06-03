using FluentAssertions;
using Moq;
using PayTrack.Application.Dto.BankStatement;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Implementation;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Tests.UnitTests.Services
{
    public class BankStatementMatchingServiceTests
    {
        private readonly Mock<ITransactionRepository> repoMock;
        private readonly BankStatementMatchingService service;

        public BankStatementMatchingServiceTests()
        {
            this.repoMock = new Mock<ITransactionRepository>();
            this.service = new BankStatementMatchingService(this.repoMock.Object);
        }

        // ── MatchBankStatementEntriesAsync ────────────────────────────────────

        [Fact]
        public async Task MatchBankStatementEntriesAsync_WithEmptyEntryList_ReturnsEmptyResults()
        {
            // Arrange
            this.repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()))
                .ReturnsAsync((new List<Transaction>(), 0));

            // Act
            var response = await this.service.MatchBankStatementEntriesAsync([]);

            // Assert
            response.Results.Should().BeEmpty();
        }

        [Fact]
        public async Task MatchBankStatementEntriesAsync_WhenNoTransactionsExist_ReturnsNoMatchForEntry()
        {
            // Arrange
            var entry = BuildEntry(amount: 100m, booking: DateTime.UtcNow);

            this.repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()))
                .ReturnsAsync((new List<Transaction>(), 0));

            // Act
            var response = await this.service.MatchBankStatementEntriesAsync([entry]);

            // Assert
            response.Results.Should().HaveCount(1);
            response.Results![0].HasMatch.Should().BeFalse();
            response.Results![0].MatchedTransaction.Should().BeNull();
        }

        [Fact]
        public async Task MatchBankStatementEntriesAsync_WhenAmountMatchesExactly_ShouldReturnMatch()
        {
            // Arrange — amount match alone scores 3 pts, above the threshold of 2
            var entry = BuildEntry(amount: 100m, booking: DateTime.UtcNow.AddDays(-30));
            var transaction = BuildTransaction(id: 1, amount: 100m, paidAt: DateTime.UtcNow);

            this.repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()))
                .ReturnsAsync((new List<Transaction> { transaction }, 1));

            // Act
            var response = await this.service.MatchBankStatementEntriesAsync([entry]);

            // Assert
            response.Results.Should().HaveCount(1);
            response.Results![0].HasMatch.Should().BeTrue();
            response.Results![0].MatchedTransaction!.Id.Should().Be(1);
            response.Results![0].MatchScore.Should().BeGreaterThanOrEqualTo(3);
        }

        [Fact]
        public async Task MatchBankStatementEntriesAsync_WhenOnlyDateMatches_ShouldReturnNoMatch()
        {
            // Arrange — date match scores only 1 pt, below the threshold of 2
            var booking = new DateTime(2026, 5, 21, 0, 0, 0, DateTimeKind.Utc);
            var entry = BuildEntry(amount: 100m, booking: booking);
            var transaction = BuildTransaction(id: 1, amount: 999m, paidAt: booking);

            this.repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()))
                .ReturnsAsync((new List<Transaction> { transaction }, 1));

            // Act
            var response = await this.service.MatchBankStatementEntriesAsync([entry]);

            // Assert
            response.Results![0].HasMatch.Should().BeFalse();
        }

        [Fact]
        public async Task MatchBankStatementEntriesAsync_WhenAmountAndDateMatch_ShouldScoreAtLeast4Points()
        {
            // Arrange — amount (3pts) + date within 3 days (1pt) = 4pts
            var booking = new DateTime(2026, 5, 21, 0, 0, 0, DateTimeKind.Utc);
            var entry = BuildEntry(amount: 250m, booking: booking);
            var transaction = BuildTransaction(id: 2, amount: 250m, paidAt: booking.AddDays(1));

            this.repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()))
                .ReturnsAsync((new List<Transaction> { transaction }, 1));

            // Act
            var response = await this.service.MatchBankStatementEntriesAsync([entry]);

            // Assert
            response.Results![0].HasMatch.Should().BeTrue();
            response.Results![0].MatchScore.Should().BeGreaterThanOrEqualTo(4);
        }

        [Fact]
        public async Task MatchBankStatementEntriesAsync_WhenDateExceeds3DayWindow_ShouldNotScoreDatePoint()
        {
            // Arrange — amount (3pts) only; date is 10 days apart
            var entry = BuildEntry(amount: 100m, booking: new DateTime(2026, 5, 21, 0, 0, 0, DateTimeKind.Utc));
            var transaction = BuildTransaction(id: 1, amount: 100m, paidAt: new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc));

            this.repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()))
                .ReturnsAsync((new List<Transaction> { transaction }, 1));

            // Act
            var response = await this.service.MatchBankStatementEntriesAsync([entry]);

            // Assert — match still found (amount = 3pts) but score is exactly 3
            response.Results![0].HasMatch.Should().BeTrue();
            response.Results![0].MatchScore.Should().Be(3);
        }

        [Fact]
        public async Task MatchBankStatementEntriesAsync_WhenInvoiceNumberInReference_ShouldAddInvoicePoints()
        {
            // Arrange — amount (3pts) + invoice number match (2pts) = 5pts
            var entry = BuildEntry(amount: 75m, booking: DateTime.UtcNow, receiverReference: "INV-2026-007");
            var transaction = BuildPaymentRequestByUser(id: 3, amount: 75m, invoiceNumber: "INV-2026-007");

            this.repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()))
                .ReturnsAsync((new List<Transaction> { transaction }, 1));

            // Act
            var response = await this.service.MatchBankStatementEntriesAsync([entry]);

            // Assert
            response.Results![0].HasMatch.Should().BeTrue();
            response.Results![0].MatchScore.Should().BeGreaterThanOrEqualTo(5);
        }

        [Fact]
        public async Task MatchBankStatementEntriesAsync_WithMultipleTransactions_SelectsBestMatch()
        {
            // Arrange — transaction 1 matches only on date (1pt), transaction 2 matches on amount (3pts)
            var booking = new DateTime(2026, 5, 21, 0, 0, 0, DateTimeKind.Utc);
            var entry = BuildEntry(amount: 200m, booking: booking);
            var weakMatch = BuildTransaction(id: 10, amount: 999m, paidAt: booking);
            var strongMatch = BuildTransaction(id: 11, amount: 200m, paidAt: DateTime.UtcNow.AddDays(-60));

            this.repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()))
                .ReturnsAsync((new List<Transaction> { weakMatch, strongMatch }, 2));

            // Act
            var response = await this.service.MatchBankStatementEntriesAsync([entry]);

            // Assert
            response.Results![0].HasMatch.Should().BeTrue();
            response.Results![0].MatchedTransaction!.Id.Should().Be(11);
        }

        [Fact]
        public async Task MatchBankStatementEntriesAsync_WithMultipleEntries_MatchesEachIndependently()
        {
            // Arrange
            var entryA = BuildEntry(amount: 50m, booking: DateTime.UtcNow);
            var entryB = BuildEntry(amount: 200m, booking: DateTime.UtcNow);
            var txA = BuildTransaction(id: 1, amount: 50m);
            var txB = BuildTransaction(id: 2, amount: 200m);

            this.repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()))
                .ReturnsAsync((new List<Transaction> { txA, txB }, 2));

            // Act
            var response = await this.service.MatchBankStatementEntriesAsync([entryA, entryB]);

            // Assert
            response.Results.Should().HaveCount(2);
            response.Results![0].MatchedTransaction!.Id.Should().Be(1);
            response.Results![1].MatchedTransaction!.Id.Should().Be(2);
        }

        // ── UpdateBankStatementMatchesAsync ───────────────────────────────────

        [Fact]
        public async Task UpdateBankStatementMatchesAsync_ShouldSetTransactionStatusToPaid()
        {
            // Arrange
            var transaction = BuildTransaction(id: 5, amount: 100m);
            var update = new BankStatementUpdateRequestDto("entry-0", MatchedTransactionId: 5, Skipped: false);

            this.repoMock
                .Setup(r => r.GetByIdAsync(5, It.IsAny<GetTransactionQueryById>()))
                .ReturnsAsync(transaction);
            this.repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Transaction>()))
                .ReturnsAsync((Transaction t) => t);

            // Act
            var result = await this.service.UpdateBankStatementMatchesAsync([update], changedById: 1);

            // Assert
            result.Should().HaveCount(1);
            transaction.Status.Should().Be(TransactionStatus.Paid);
        }

        [Fact]
        public async Task UpdateBankStatementMatchesAsync_ShouldAddStatusHistoryRecord()
        {
            // Arrange
            var transaction = BuildTransaction(id: 5, amount: 100m, status: TransactionStatus.Approved);
            var update = new BankStatementUpdateRequestDto("entry-0", MatchedTransactionId: 5, Skipped: false);

            this.repoMock
                .Setup(r => r.GetByIdAsync(5, It.IsAny<GetTransactionQueryById>()))
                .ReturnsAsync(transaction);
            this.repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Transaction>()))
                .ReturnsAsync((Transaction t) => t);

            // Act
            await this.service.UpdateBankStatementMatchesAsync([update], changedById: 99);

            // Assert
            transaction.StatusHistory.Should().HaveCount(1);
            var historyEntry = transaction.StatusHistory.First();
            historyEntry.FromStatus.Should().Be(TransactionStatus.Approved);
            historyEntry.ToStatus.Should().Be(TransactionStatus.Paid);
            historyEntry.ChangedById.Should().Be(99);
        }

        [Fact]
        public async Task UpdateBankStatementMatchesAsync_WithSkippedTrue_ShouldNotUpdateTransaction()
        {
            // Arrange
            var update = new BankStatementUpdateRequestDto("entry-0", MatchedTransactionId: 5, Skipped: true);

            // Act
            var result = await this.service.UpdateBankStatementMatchesAsync([update], changedById: 1);

            // Assert
            result.Should().BeEmpty();
            this.repoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<GetTransactionQueryById>()), Times.Never);
        }

        [Fact]
        public async Task UpdateBankStatementMatchesAsync_WithNullMatchedTransactionId_ShouldNotUpdateTransaction()
        {
            // Arrange
            var update = new BankStatementUpdateRequestDto("entry-0", MatchedTransactionId: null, Skipped: false);

            // Act
            var result = await this.service.UpdateBankStatementMatchesAsync([update], changedById: 1);

            // Assert
            result.Should().BeEmpty();
            this.repoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<GetTransactionQueryById>()), Times.Never);
        }

        [Fact]
        public async Task UpdateBankStatementMatchesAsync_WhenTransactionNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var update = new BankStatementUpdateRequestDto("entry-0", MatchedTransactionId: 999, Skipped: false);

            this.repoMock
                .Setup(r => r.GetByIdAsync(999, It.IsAny<GetTransactionQueryById>()))
                .ReturnsAsync((Transaction?)null);

            // Act
            var act = async () => await this.service.UpdateBankStatementMatchesAsync([update], changedById: 1);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*999*");
        }

        private static BankStatementEntryDto BuildEntry(
            decimal amount,
            DateTime booking,
            string? receiverReference = null,
            string? reference = null,
            string? partnerName = null) =>
            new()
            {
                Booking = booking,
                PartnerName = partnerName,
                Amount = new BankStatementAmountDto { Value = amount, Currency = "EUR" },
                ReceiverReference = receiverReference,
                Reference = reference,
            };

        private static PaymentRequestByUser BuildPaymentRequestByUser(
            int id,
            decimal amount,
            string invoiceNumber = "INV-001",
            DateTime? paidAt = null,
            TransactionStatus status = TransactionStatus.Approved) =>
            new()
            {
                Id = id,
                UserId = 1,
                TeamId = 1,
                Amount = amount,
                InvoiceNumber = invoiceNumber,
                Status = status,
                PaidAt = paidAt ?? DateTime.UtcNow,
                PaymentDirection = PaymentDirection.Out,
                StatusHistory = [],
            };

        private static Transaction BuildTransaction(
            int id,
            decimal amount,
            DateTime? paidAt = null,
            TransactionStatus status = TransactionStatus.Approved) =>
            BuildPaymentRequestByUser(id, amount, paidAt: paidAt, status: status);
    }
}
