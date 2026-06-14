using FluentAssertions;
using Moq;
using PayTrack.Application.Dto.PaymentRequestByTeam;
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Services.Implementation;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Services
{
    public class HomeDashboardServiceTests
    {
        [Fact]
        public async Task GetHomeDashboardAsync_ShouldBuildDashboardSummary()
        {
            // Arrange
            var paymentRequestByUserServiceMock = new Mock<IPaymentRequestByUserService>();
            var paymentRequestByTeamServiceMock = new Mock<IPaymentRequestByTeamService>();

            var currentUser = new User
            {
                Id = 7,
                Name = "Alex",
                Role = Role.RegularUser,
                BankInformationSkipped = false,
                BankAccounts = [],
            };

            var invoices = new List<PaymentRequestByUser>
            {
                new()
                {
                    Id = 11,
                    UserId = currentUser.Id,
                    Amount = 100m,
                    Status = TransactionStatus.Submitted,
                    InvoiceNumber = "INV-100",
                    PurposeOfPayment = "Hotel",
                    CreatedAt = new DateTime(2026, 06, 10, 8, 0, 0, DateTimeKind.Utc),
                    Team = new Team { Id = 1, Name = "Core Team" },
                    User = currentUser,
                },
                new()
                {
                    Id = 12,
                    UserId = currentUser.Id,
                    Amount = 200m,
                    Status = TransactionStatus.Paid,
                    InvoiceNumber = "INV-200",
                    PurposeOfPayment = "Train",
                    CreatedAt = new DateTime(2026, 06, 09, 8, 0, 0, DateTimeKind.Utc),
                    PaidAt = new DateTime(2026, 06, 09, 0, 0, 0, DateTimeKind.Utc),
                    FinancePaidAt = new DateTime(2026, 06, 11, 0, 0, 0, DateTimeKind.Utc),
                    Team = new Team { Id = 1, Name = "Core Team" },
                    User = currentUser,
                },
                new()
                {
                    Id = 13,
                    UserId = currentUser.Id,
                    Amount = 50m,
                    Status = TransactionStatus.ChangesRequested,
                    InvoiceNumber = "INV-300",
                    PurposeOfPayment = "Taxi",
                    CreatedAt = new DateTime(2026, 06, 08, 8, 0, 0, DateTimeKind.Utc),
                    Team = new Team { Id = 2, Name = "Field Team" },
                    User = currentUser,
                },
                new()
                {
                    Id = 14,
                    UserId = currentUser.Id,
                    Amount = 25m,
                    Status = TransactionStatus.Declined,
                    InvoiceNumber = "INV-400",
                    PurposeOfPayment = "Snacks",
                    CreatedAt = new DateTime(2026, 06, 07, 8, 0, 0, DateTimeKind.Utc),
                    Team = new Team { Id = 2, Name = "Field Team" },
                    User = currentUser,
                },
            };

            var paymentRequests = new List<PaymentRequestByTeam>
            {
                new()
                {
                    Id = 21,
                    UserId = currentUser.Id,
                    Amount = 300m,
                    Status = TransactionStatus.Submitted,
                    PurposeOfPayment = "Membership fee",
                    PaymentReference = "PR-1",
                    CreatedAt = new DateTime(2026, 06, 12, 8, 0, 0, DateTimeKind.Utc),
                    Team = new Team { Id = 3, Name = "Admin Team" },
                    User = currentUser,
                },
                new()
                {
                    Id = 22,
                    UserId = currentUser.Id,
                    Amount = 400m,
                    Status = TransactionStatus.Paid,
                    PurposeOfPayment = "Contribution",
                    PaymentReference = "PR-2",
                    CreatedAt = new DateTime(2026, 06, 11, 8, 0, 0, DateTimeKind.Utc),
                    PaidAt = new DateTime(2026, 06, 13, 0, 0, 0, DateTimeKind.Utc),
                    Team = new Team { Id = 3, Name = "Admin Team" },
                    User = currentUser,
                },
                new()
                {
                    Id = 23,
                    UserId = currentUser.Id,
                    Amount = 50m,
                    Status = TransactionStatus.Declined,
                    PurposeOfPayment = "Late fee",
                    PaymentReference = "PR-3",
                    CreatedAt = new DateTime(2026, 06, 10, 8, 0, 0, DateTimeKind.Utc),
                    Team = new Team { Id = 3, Name = "Admin Team" },
                    User = currentUser,
                },
            };

            paymentRequestByUserServiceMock
                .Setup(service => service.GetAllAsync(It.IsAny<GetPaymentRequestByUserQuery?>()))
                .ReturnsAsync((invoices, invoices.Count));

            paymentRequestByTeamServiceMock
                .Setup(service => service.GetAllAsync(It.IsAny<GetPaymentRequestByTeamQuery?>()))
                .ReturnsAsync((paymentRequests, paymentRequests.Count));

            var service = new HomeDashboardService(
                paymentRequestByUserServiceMock.Object,
                paymentRequestByTeamServiceMock.Object);

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
            result.Invoices.Recent.Should().HaveCount(4);
            result.Invoices.Recent.First().Reference.Should().Be("INV-100");
            result.Invoices.Recent.First().TeamName.Should().Be("Core Team");

            result.PaymentRequests.OpenCount.Should().Be(1);
            result.PaymentRequests.SubmittedCount.Should().Be(1);
            result.PaymentRequests.PaidCount.Should().Be(1);
            result.PaymentRequests.OpenAmount.Should().Be(300m);
            result.PaymentRequests.LastPaidAt.Should().Be(new DateTime(2026, 06, 13, 0, 0, 0, DateTimeKind.Utc));
            result.PaymentRequests.Recent.Should().HaveCount(3);
            result.PaymentRequests.Recent.First().Reference.Should().Be("PR-1");

            result.Actions.MissingBankAccount.Should().BeTrue();
            result.Actions.BankInformationSkipped.Should().BeFalse();
            result.Actions.NeedsAttentionCount.Should().Be(3);

            paymentRequestByUserServiceMock.Verify(
                service => service.GetAllAsync(It.Is<GetPaymentRequestByUserQuery>(query =>
                    query.UserId == currentUser.Id && query.IncludeTeam == true)),
                Times.Once);
            paymentRequestByTeamServiceMock.Verify(
                service => service.GetAllAsync(It.Is<GetPaymentRequestByTeamQuery>(query =>
                    query.UserId == currentUser.Id && query.IncludeTeam == true)),
                Times.Once);
        }

        [Fact]
        public async Task GetHomeDashboardAsync_ShouldUsePaidAtFallbackAndRespectExistingBankAccount()
        {
            // Arrange
            var paymentRequestByUserServiceMock = new Mock<IPaymentRequestByUserService>();
            var paymentRequestByTeamServiceMock = new Mock<IPaymentRequestByTeamService>();

            var currentUser = new User
            {
                Id = 3,
                Name = "Jordan",
                Role = Role.TeamLead,
                BankInformationSkipped = true,
                BankAccounts = [new BankAccount { Id = 1, UserId = 3, Iban = "AT123" }],
            };

            var invoices = new List<PaymentRequestByUser>
            {
                new()
                {
                    Id = 99,
                    UserId = currentUser.Id,
                    Amount = 75m,
                    Status = TransactionStatus.Paid,
                    InvoiceNumber = "INV-999",
                    PurposeOfPayment = "Supplies",
                    CreatedAt = new DateTime(2026, 06, 01, 8, 0, 0, DateTimeKind.Utc),
                    PaidAt = new DateTime(2026, 06, 03, 0, 0, 0, DateTimeKind.Utc),
                    Team = new Team { Id = 8, Name = "Ops Team" },
                    User = currentUser,
                },
            };

            paymentRequestByUserServiceMock
                .Setup(service => service.GetAllAsync(It.IsAny<GetPaymentRequestByUserQuery?>()))
                .ReturnsAsync((invoices, invoices.Count));

            paymentRequestByTeamServiceMock
                .Setup(service => service.GetAllAsync(It.IsAny<GetPaymentRequestByTeamQuery?>()))
                .ReturnsAsync((new List<PaymentRequestByTeam>(), 0));

            var service = new HomeDashboardService(
                paymentRequestByUserServiceMock.Object,
                paymentRequestByTeamServiceMock.Object);

            // Act
            var result = await service.GetHomeDashboardAsync(currentUser);

            // Assert
            result.Actions.MissingBankAccount.Should().BeFalse();
            result.Actions.BankInformationSkipped.Should().BeTrue();
            result.Invoices.LastPaidAt.Should().Be(new DateTime(2026, 06, 03, 0, 0, 0, DateTimeKind.Utc));
            result.PaymentRequests.Recent.Should().BeEmpty();
            result.PaymentRequests.OpenAmount.Should().Be(0m);
        }
    }
}
