using FluentAssertions;
using Moq;
using PayTrack.Application.Services.Implementation;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Tests.UnitTests.Services
{
    public class HomeDashboardServiceTests
    {
        [Fact]
        public async Task GetHomeDashboardAsync_ShouldBuildDashboardSummary()
        {
            // Arrange
            var repoMock = new Mock<ITransactionRepository>();

            var currentUser = new User
            {
                Id = 7,
                Name = "Alex",
                Role = Role.RegularUser,
                BankInformationSkipped = false,
                BankAccounts = [],
            };

            repoMock
                .Setup(repo => repo.GetHomeDashboardInvoiceSectionAsync(currentUser.Id, 5))
                .ReturnsAsync(new HomeDashboardSectionProjection(
                    2,
                    1,
                    1,
                    150m,
                    new DateTime(2026, 06, 11, 0, 0, 0, DateTimeKind.Utc),
                    4,
                    1,
                    [
                        new HomeDashboardRecentItemProjection(
                            11,
                            100m,
                            TransactionStatus.Submitted,
                            new DateTime(2026, 06, 10, 8, 0, 0, DateTimeKind.Utc),
                            null,
                            "INV-100",
                            "Hotel",
                            "Core Team",
                            "Alex"),
                    ]));

            repoMock
                .Setup(repo => repo.GetHomeDashboardPaymentRequestSectionAsync(currentUser.Id, 5))
                .ReturnsAsync(new HomeDashboardSectionProjection(
                    1,
                    1,
                    1,
                    300m,
                    new DateTime(2026, 06, 13, 0, 0, 0, DateTimeKind.Utc),
                    3,
                    0,
                    [
                        new HomeDashboardRecentItemProjection(
                            21,
                            300m,
                            TransactionStatus.Submitted,
                            new DateTime(2026, 06, 12, 8, 0, 0, DateTimeKind.Utc),
                            null,
                            "PR-1",
                            "Membership fee",
                            "Admin Team",
                            "Alex"),
                    ]));

            var service = new HomeDashboardService(repoMock.Object);

            // Act
            var result = await service.GetHomeDashboardAsync(currentUser);

            // Assert
            result.User.Id.Should().Be(7);
            result.User.Name.Should().Be("Alex");
            result.Invoices.OpenCount.Should().Be(2);
            result.Invoices.SubmittedCount.Should().Be(1);
            result.Invoices.PaidCount.Should().Be(1);
            result.Invoices.OpenAmount.Should().Be(150m);
            result.Invoices.LastPaidAt.Should().Be(new DateTime(2026, 06, 11, 0, 0, 0, DateTimeKind.Utc));
            result.Invoices.TotalRecentCount.Should().Be(4);
            result.Invoices.Recent.Should().HaveCount(1);
            result.Invoices.Recent.First().Reference.Should().Be("INV-100");
            result.Invoices.Recent.First().TeamName.Should().Be("Core Team");

            result.PaymentRequests.OpenCount.Should().Be(1);
            result.PaymentRequests.SubmittedCount.Should().Be(1);
            result.PaymentRequests.PaidCount.Should().Be(1);
            result.PaymentRequests.OpenAmount.Should().Be(300m);
            result.PaymentRequests.LastPaidAt.Should().Be(new DateTime(2026, 06, 13, 0, 0, 0, DateTimeKind.Utc));
            result.PaymentRequests.TotalRecentCount.Should().Be(3);
            result.PaymentRequests.Recent.Should().HaveCount(1);
            result.PaymentRequests.Recent.First().Reference.Should().Be("PR-1");

            result.Actions.MissingBankAccount.Should().BeTrue();
            result.Actions.BankInformationSkipped.Should().BeFalse();
            result.Actions.NeedsAttentionCount.Should().Be(1);

            repoMock.Verify(
                repo => repo.GetHomeDashboardInvoiceSectionAsync(currentUser.Id, 5),
                Times.Once);
            repoMock.Verify(
                repo => repo.GetHomeDashboardPaymentRequestSectionAsync(currentUser.Id, 5),
                Times.Once);
        }

        [Fact]
        public async Task GetHomeDashboardAsync_ShouldUsePaidAtFallbackAndRespectExistingBankAccount()
        {
            // Arrange
            var repoMock = new Mock<ITransactionRepository>();

            var currentUser = new User
            {
                Id = 3,
                Name = "Jordan",
                Role = Role.TeamLead,
                BankInformationSkipped = true,
                BankAccounts = [new BankAccount { Id = 1, UserId = 3, Iban = "AT123" }],
            };

            repoMock
                .Setup(repo => repo.GetHomeDashboardInvoiceSectionAsync(currentUser.Id, 5))
                .ReturnsAsync(new HomeDashboardSectionProjection(
                    0,
                    0,
                    1,
                    0m,
                    new DateTime(2026, 06, 03, 0, 0, 0, DateTimeKind.Utc),
                    1,
                    0,
                    [
                        new HomeDashboardRecentItemProjection(
                            99,
                            75m,
                            TransactionStatus.Paid,
                            new DateTime(2026, 06, 01, 8, 0, 0, DateTimeKind.Utc),
                            new DateTime(2026, 06, 03, 0, 0, 0, DateTimeKind.Utc),
                            "INV-999",
                            "Supplies",
                            "Ops Team",
                            "Jordan"),
                    ]));

            repoMock
                .Setup(repo => repo.GetHomeDashboardPaymentRequestSectionAsync(currentUser.Id, 5))
                .ReturnsAsync(new HomeDashboardSectionProjection(0, 0, 0, 0m, null, 0, 0, []));

            var service = new HomeDashboardService(repoMock.Object);

            // Act
            var result = await service.GetHomeDashboardAsync(currentUser);

            // Assert
            result.Actions.MissingBankAccount.Should().BeFalse();
            result.Actions.BankInformationSkipped.Should().BeTrue();
            result.Invoices.LastPaidAt.Should().Be(new DateTime(2026, 06, 03, 0, 0, 0, DateTimeKind.Utc));
            result.Invoices.TotalRecentCount.Should().Be(1);
            result.PaymentRequests.TotalRecentCount.Should().Be(0);
            result.PaymentRequests.Recent.Should().BeEmpty();
            result.PaymentRequests.OpenAmount.Should().Be(0m);
        }
    }
}
