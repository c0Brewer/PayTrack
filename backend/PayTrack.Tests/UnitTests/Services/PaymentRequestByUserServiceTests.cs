using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Implementation;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Tests.UnitTests.Services
{
    public class PaymentRequestByUserServiceTests
    {
        // ----------------------------
        // GET ALL
        // ----------------------------
        [Fact]
        public async Task GetAllAsync_ShouldReturnListAndCount()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();

            var list = new List<PaymentRequestByUser>
            {
                new () { Id = 1, InvoiceNumber = "123" },
                new () { Id = 2, InvoiceNumber = "456" }
            };

            repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetPaymentRequestByUserQuery>()))
                .ReturnsAsync((list, list.Count));

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object);

            var (paymentRequestByUser, totalCount) = await service.GetAllAsync();

            paymentRequestByUser.Should().HaveCount(2);
            totalCount.Should().Be(2);
        }

        // ----------------------------
        // GET BY ID
        // ----------------------------
        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();

            var entity = new PaymentRequestByUser { Id = 1, InvoiceNumber = "123" };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(entity);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object);

            var result = await service.GetPaymentRequestByUserByIdAsync(1);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        // ----------------------------
        // CREATE
        // ----------------------------
        [Fact]
        public async Task Create_ShouldThrow_WhenPaidAtInFuture()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();

            var team = new Team { Id = 5 };

            teamMock
                .Setup(t => t.GetTeamByIdAsync(5))
                .ReturnsAsync(team);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object);

            var futureDate = DateTime.Today.AddDays(1);

            var file = new FormFile(Stream.Null, 0, 0, "file", "test.pdf");

            Func<Task> act = async () =>
                await service.CreatePaymentRequestByUserAsync(
                    1,
                    5,
                    100,
                    "purpose",
                    file,
                    futureDate,
                    "inv-1",
                    null,
                    PayoutType.External,
                    1);

            await act.Should()
                .ThrowAsync<InvalidStateException>()
                .WithMessage("Paid at cannot be in the future!");
        }

        [Fact]
        public async Task Create_ShouldCallRepositoryAndReturnEntity()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();

            var team = new Team { Id = 5 };

            teamMock
                .Setup(t => t.GetTeamByIdAsync(5))
                .ReturnsAsync(team);

            var created = new PaymentRequestByUser { Id = 1, InvoiceNumber = "123" };

            repoMock
                .Setup(r => r.AddAsync(It.IsAny<PaymentRequestByUser>(), It.IsAny<IFormFile>()))
                .ReturnsAsync(created);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object);

            var file = new FormFile(Stream.Null, 0, 0, "file", "test.pdf");

            var result = await service.CreatePaymentRequestByUserAsync(
                1,
                5,
                100,
                "test",
                file,
                DateTime.Today,
                "inv",
                null,
                PayoutType.External,
                1);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);

            repoMock.Verify(r =>
                r.AddAsync(It.IsAny<PaymentRequestByUser>(), file),
                Times.Once);
        }

        // ----------------------------
        // DUPLICATE CHECK
        // ----------------------------
        [Fact]
        public async Task GetDuplicatePaymentRequestsByUserAsync_ShouldSortByScoreAndCreatedAt()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();

            var duplicateCandidates = new List<PaymentRequestByUser>
            {
                new()
                {
                    Id = 1,
                    UserId = 42,
                    TeamId = 99,
                    Amount = 100,
                    InvoiceNumber = "INV-100",
                    CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = 2,
                    UserId = 1,
                    TeamId = 99,
                    Amount = 100,
                    InvoiceNumber = "ANY-1",
                    CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = 3,
                    UserId = 42,
                    TeamId = 1,
                    Amount = 100,
                    InvoiceNumber = "ANY-2",
                    CreatedAt = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = 4,
                    UserId = 1,
                    TeamId = 1,
                    Amount = 50,
                    InvoiceNumber = "OTHER",
                    CreatedAt = new DateTime(2026, 1, 25, 0, 0, 0, DateTimeKind.Utc)
                }
            };

            repoMock
                .Setup(r => r.GetPotentialDuplicatesAsync(42, 99, 100))
                .ReturnsAsync(duplicateCandidates);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object);

            var result = await service.GetDuplicatePaymentRequestsByUserAsync(42, 99, 100);

            result.Should().HaveCount(3);
            result[0].PaymentRequestByUser.Id.Should().Be(1);
            result[0].Score.Should().Be(2);
            result[0].IsAmountAndUserMatch.Should().BeTrue();
            result[0].IsAmountAndTeamMatch.Should().BeTrue();

            result[1].PaymentRequestByUser.Id.Should().Be(3);
            result[1].Score.Should().Be(1);
            result[1].IsAmountAndUserMatch.Should().BeTrue();

            result[2].PaymentRequestByUser.Id.Should().Be(2);
            result[2].Score.Should().Be(1);
            result[2].IsAmountAndTeamMatch.Should().BeTrue();

            repoMock.Verify(
                r => r.GetPotentialDuplicatesAsync(42, 99, 100),
                Times.Once);
        }

        [Fact]
        public async Task GetDuplicatePaymentRequestsByUserAsync_ShouldReturnAtMost10Results()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();

            var duplicateCandidates = Enumerable.Range(1, 12)
                .Select(i => new PaymentRequestByUser
                {
                    Id = i,
                    UserId = 42,
                    TeamId = 99,
                    Amount = 100,
                    InvoiceNumber = "INV-100",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                })
                .ToList();

            repoMock
                .Setup(r => r.GetPotentialDuplicatesAsync(42, 99, 100))
                .ReturnsAsync(duplicateCandidates);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object);

            var result = await service.GetDuplicatePaymentRequestsByUserAsync(42, 99, 100);

            result.Should().HaveCount(10);
            result.Should().OnlyContain(match => match.Score >= 1);
        }

        // ----------------------------
        // UPDATE
        // ----------------------------
        [Fact]
        public async Task Update_ShouldThrow_WhenEntityNotFound()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetTransactionQueryById>()))
                .ReturnsAsync((PaymentRequestByUser?)null);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object);

            Func<Task> act = async () =>
                await service.UpdatePaymentRequestByUserAsync(1);

            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Transaction not found");
        }

        [Fact]
        public async Task Update_ShouldUpdateFields_WhenProvided()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();

            var entity = new PaymentRequestByUser
            {
                InvoiceNumber = "123",
                Id = 1,
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<PaymentRequestByUser>()))
                .ReturnsAsync((PaymentRequestByUser p) => p);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object);

            var result = await service.UpdatePaymentRequestByUserAsync(
                1,
                amount: 999,
                purposeOfPayment: "new");

            result.Amount.Should().Be(999);
            result.PurposeOfPayment.Should().Be("new");
        }

        // ----------------------------
        // GET RECEIPT
        // ----------------------------
        [Fact]
        public async Task GetReceipt_ShouldReturnFile()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();

            var entity = new PaymentRequestByUser
            {
                InvoiceNumber = "123",
                Id = 1,
                ReceiptUrl = "/files/test.pdf"
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(entity);

            fileMock
                .Setup(f => f.GetByPath(entity.ReceiptUrl))
                .ReturnsAsync([1, 2, 3]);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object);

            var result = await service.GetReceiptForPaymentRequestByUserByIdAsync(1);

            result.content.Should().Equal(1, 2, 3);
            result.contentType.Should().Be("application/pdf");
        }

        // ----------------------------
        // VALIDATE QUERY
        // ----------------------------
        [Fact]
        public void ValidateQuery_RegularUser_ReturnsTrue_WhenUserIdMatchesCurrent()
        {
            var service = BuildService();
            var user = new User { Id = 5, Role = Role.RegularUser };
            var query = new GetPaymentRequestByUserQuery { UserId = 5 };

            service.ValidateQuery(query, user).Should().BeTrue();
        }

        [Fact]
        public void ValidateQuery_RegularUser_ReturnsFalse_WhenUserIdDiffers()
        {
            var service = BuildService();
            var user = new User { Id = 5, Role = Role.RegularUser };
            var query = new GetPaymentRequestByUserQuery { UserId = 99 };

            service.ValidateQuery(query, user).Should().BeFalse();
        }

        [Fact]
        public void ValidateQuery_RegularUser_ReturnsFalse_WhenUserIdIsNull()
        {
            var service = BuildService();
            var user = new User { Id = 5, Role = Role.RegularUser };
            var query = new GetPaymentRequestByUserQuery { UserId = null };

            service.ValidateQuery(query, user).Should().BeFalse();
        }

        [Fact]
        public void ValidateQuery_TeamLead_ReturnsTrue_WhenTeamIdMatchesCurrent()
        {
            var service = BuildService();
            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = 10 };
            var query = new GetPaymentRequestByUserQuery { TeamId = 10 };

            service.ValidateQuery(query, user).Should().BeTrue();
        }

        [Fact]
        public void ValidateQuery_TeamLead_ReturnsTrue_WhenUserIdMatchesCurrent()
        {
            var service = BuildService();
            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = 10 };
            var query = new GetPaymentRequestByUserQuery { UserId = 1 };

            service.ValidateQuery(query, user).Should().BeTrue();
        }

        [Fact]
        public void ValidateQuery_TeamLead_ReturnsFalse_WhenTeamIdDiffers()
        {
            var service = BuildService();
            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = 10 };
            var query = new GetPaymentRequestByUserQuery { TeamId = 99 };

            service.ValidateQuery(query, user).Should().BeFalse();
        }

        [Fact]
        public void ValidateQuery_TeamLead_ReturnsFalse_WhenUserIdDiffers()
        {
            var service = BuildService();
            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = 10 };
            var query = new GetPaymentRequestByUserQuery { UserId = 99 };

            service.ValidateQuery(query, user).Should().BeFalse();
        }

        [Fact]
        public void ValidateQuery_TeamLead_ReturnsFalse_WhenTeamIdIsNull()
        {
            var service = BuildService();
            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = 10 };
            var query = new GetPaymentRequestByUserQuery { TeamId = null };

            service.ValidateQuery(query, user).Should().BeFalse();
        }

        [Fact]
        public void ValidateQuery_TeamLead_ReturnsFalse_WhenUserHasNoTeam()
        {
            var service = BuildService();
            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = null };
            var query = new GetPaymentRequestByUserQuery { TeamId = 10 };

            service.ValidateQuery(query, user).Should().BeFalse();
        }

        [Fact]
        public void ValidateQuery_Admin_ReturnsTrue_Always()
        {
            var service = BuildService();
            var user = new User { Id = 1, Role = Role.Admin };
            var query = new GetPaymentRequestByUserQuery { UserId = 99, TeamId = 99 };

            service.ValidateQuery(query, user).Should().BeTrue();
        }

        // ----------------------------
        // VALIDATE ACCESS TO INVOICE
        // ----------------------------
        [Fact]
        public void ValidateAccessToInvoice_RegularUser_ReturnsTrue_WhenOwner()
        {
            var service = BuildService();
            var user = new User { Id = 5, Role = Role.RegularUser };
            var invoice = new PaymentRequestByUser { UserId = 5, InvoiceNumber = "1" };

            service.ValidateAccessToInvoice(invoice, user).Should().BeTrue();
        }

        [Fact]
        public void ValidateAccessToInvoice_RegularUser_ReturnsFalse_WhenNotOwner()
        {
            var service = BuildService();
            var user = new User { Id = 5, Role = Role.RegularUser };
            var invoice = new PaymentRequestByUser { UserId = 99, InvoiceNumber = "1" };

            service.ValidateAccessToInvoice(invoice, user).Should().BeFalse();
        }

        [Fact]
        public void ValidateAccessToInvoice_TeamLead_ReturnsTrue_WhenSameTeam()
        {
            var service = BuildService();
            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = 10 };
            var invoice = new PaymentRequestByUser { TeamId = 10, InvoiceNumber = "1" };

            service.ValidateAccessToInvoice(invoice, user).Should().BeTrue();
        }

        [Fact]
        public void ValidateAccessToInvoice_TeamLead_ReturnsFalse_WhenDifferentTeam()
        {
            var service = BuildService();
            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = 10 };
            var invoice = new PaymentRequestByUser { TeamId = 99, InvoiceNumber = "1" };

            service.ValidateAccessToInvoice(invoice, user).Should().BeFalse();
        }

        [Fact]
        public void ValidateAccessToInvoice_TeamLead_ReturnsFalse_WhenUserHasNoTeam()
        {
            var service = BuildService();
            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = null };
            var invoice = new PaymentRequestByUser { TeamId = 10, InvoiceNumber = "1" };

            service.ValidateAccessToInvoice(invoice, user).Should().BeFalse();
        }

        [Fact]
        public void ValidateAccessToInvoice_Admin_ReturnsTrue_Always()
        {
            var service = BuildService();
            var user = new User { Id = 1, Role = Role.Admin };
            var invoice = new PaymentRequestByUser { UserId = 99, TeamId = 99, InvoiceNumber = "1" };

            service.ValidateAccessToInvoice(invoice, user).Should().BeTrue();
        }

        private static PaymentRequestByUserService BuildService()
        {
            return new PaymentRequestByUserService(
                new Mock<ITransactionRepository>().Object,
                new Mock<ITeamService>().Object,
                new Mock<IFileRepository>().Object,
                new Mock<IBankAccountService>().Object);
        }

        [Fact]
        public async Task GetReceipt_ShouldThrow_WhenReceiptUrlIsNull()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();

            var entity = new PaymentRequestByUser
            {
                InvoiceNumber = "123",
                Id = 1,
                ReceiptUrl = null
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(entity);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object);

            Func<Task> act = async () =>
                await service.GetReceiptForPaymentRequestByUserByIdAsync(1);

            await act.Should()
                .ThrowAsync<InvalidStateException>()
                .WithMessage("Receipt URL is null although it should not be.");
        }
    }
}
