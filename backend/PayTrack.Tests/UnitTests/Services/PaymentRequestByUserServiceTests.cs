using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Implementation;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Helpers;
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
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

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
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

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
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

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
                    PayoutType.NotYetPaid,
                    1,
                    "Test Company");

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
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

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
                PayoutType.NotYetPaid,
                1,
                "Test Company");

            result.Should().NotBeNull();
            result.Id.Should().Be(1);

            repoMock.Verify(r =>
                r.AddAsync(It.IsAny<PaymentRequestByUser>(), file),
                Times.Once);
        }

        [Fact]
        public async Task Create_ShouldThrow_WhenUserPayoutHasNoBankAccount()
        {
            var teamMock = new Mock<ITeamService>();
            teamMock
                .Setup(t => t.GetTeamByIdAsync(5))
                .ReturnsAsync(new Team { Id = 5 });

            var service = new PaymentRequestByUserService(
                new Mock<ITransactionRepository>().Object,
                teamMock.Object,
                new Mock<IFileRepository>().Object,
                new Mock<IBankAccountService>().Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            var file = new FormFile(Stream.Null, 0, 0, "file", "test.pdf");

            Func<Task> act = async () =>
                await service.CreatePaymentRequestByUserAsync(
                    1,
                    5,
                    100,
                    "purpose",
                    file,
                    DateTime.Today,
                    "inv",
                    null,
                    PayoutType.User,
                    null,
                    null);

            await act.Should()
                .ThrowAsync<InvalidStateException>()
                .WithMessage("If the money should be paid out to you, you must specify a bankAccount");
        }

        [Fact]
        public async Task Create_ShouldThrow_WhenUserPayoutBankAccountIsNotOwnedByUser()
        {
            var teamMock = new Mock<ITeamService>();
            var bankMock = new Mock<IBankAccountService>();
            teamMock
                .Setup(t => t.GetTeamByIdAsync(5))
                .ReturnsAsync(new Team { Id = 5 });
            bankMock
                .Setup(b => b.GetBankAccountsAsync(1))
                .ReturnsAsync([new BankAccount { Id = 99, UserId = 1 }]);

            var service = new PaymentRequestByUserService(
                new Mock<ITransactionRepository>().Object,
                teamMock.Object,
                new Mock<IFileRepository>().Object,
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            var file = new FormFile(Stream.Null, 0, 0, "file", "test.pdf");

            Func<Task> act = async () =>
                await service.CreatePaymentRequestByUserAsync(
                    1,
                    5,
                    100,
                    "purpose",
                    file,
                    DateTime.Today,
                    "inv",
                    null,
                    PayoutType.User,
                    10,
                    null);

            await act.Should()
                .ThrowAsync<InvalidStateException>()
                .WithMessage("Could not find specified bank account");
        }

        [Fact]
        public async Task Create_ShouldKeepBankAccount_WhenUserPayoutBankAccountIsOwnedByUser()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var bankMock = new Mock<IBankAccountService>();
            teamMock
                .Setup(t => t.GetTeamByIdAsync(5))
                .ReturnsAsync(new Team { Id = 5 });
            bankMock
                .Setup(b => b.GetBankAccountsAsync(1))
                .ReturnsAsync([new BankAccount { Id = 10, UserId = 1 }]);
            repoMock
                .Setup(r => r.AddAsync(It.IsAny<PaymentRequestByUser>(), It.IsAny<IFormFile>()))
                .ReturnsAsync((PaymentRequestByUser p, IFormFile _) => p);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                new Mock<IFileRepository>().Object,
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            var file = new FormFile(Stream.Null, 0, 0, "file", "test.pdf");

            var result = await service.CreatePaymentRequestByUserAsync(
                1,
                5,
                100,
                "purpose",
                file,
                DateTime.Today,
                "inv",
                null,
                PayoutType.User,
                10,
                null);

            result.BankAccountId.Should().Be(10);
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
            var paidAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);

            var duplicateCandidates = new List<PaymentRequestByUser>
            {
                new()
                {
                    Id = 1,
                    UserId = 42,
                    TeamId = 99,
                    Amount = 100,
                    InvoiceNumber = "INV-100",
                    PaidAt = paidAt,
                    CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = 2,
                    UserId = 1,
                    TeamId = 99,
                    Amount = 100,
                    InvoiceNumber = "INV-101",
                    PaidAt = paidAt,
                    CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = 3,
                    UserId = 42,
                    TeamId = 1,
                    Amount = 100,
                    InvoiceNumber = "INV-101",
                    PaidAt = paidAt,
                    CreatedAt = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = 4,
                    UserId = 1,
                    TeamId = 1,
                    Amount = 100,
                    InvoiceNumber = "ONLY-AMOUNT",
                    PaidAt = paidAt.AddDays(3),
                    CreatedAt = new DateTime(2026, 1, 25, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = 5,
                    UserId = 42,
                    TeamId = 99,
                    Amount = 100,
                    InvoiceNumber = "OTHER-DAY",
                    PaidAt = paidAt.AddDays(1),
                    CreatedAt = new DateTime(2026, 1, 30, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = 6,
                    UserId = 42,
                    TeamId = 99,
                    Amount = 100,
                    InvoiceNumber = "INV-101",
                    PaidAt = paidAt.AddDays(3),
                    CreatedAt = new DateTime(2026, 1, 25, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = 7,
                    UserId = 1,
                    TeamId = 1,
                    Amount = 50,
                    InvoiceNumber = "OTHER",
                    PaidAt = paidAt,
                    CreatedAt = new DateTime(2026, 1, 25, 0, 0, 0, DateTimeKind.Utc)
                }
            };

            repoMock
                .Setup(r => r.GetPotentialDuplicatesAsync(42, 99, 100, paidAt, "inv-100", null, false))
                .ReturnsAsync(duplicateCandidates);
            repoMock
                .Setup(r => r.GetPotentialDuplicatesAsync(42, 99, 100, paidAt, "inv-100", null, true))
                .ReturnsAsync(duplicateCandidates);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            var result = await service.GetDuplicatePaymentRequestsByUserAsync(42, 99, 100, paidAt, "inv-100");

            result.Should().HaveCount(3);
            result[0].PaymentRequestByUser.Id.Should().Be(1);
            result[0].Score.Should().Be(15);
            result[0].MatchedFields.Should().Equal("invoiceNumber", "amount", "payday", "user", "team");

            result[1].PaymentRequestByUser.Id.Should().Be(3);
            result[1].Score.Should().Be(9);
            result[1].MatchedFields.Should().Equal("similarInvoiceNumber", "amount", "payday", "user");

            result[2].PaymentRequestByUser.Id.Should().Be(6);
            result[2].Score.Should().Be(8);
            result[2].MatchedFields.Should().Equal("similarInvoiceNumber", "amount", "user", "team");
            result.Should().NotContain(match => match.PaymentRequestByUser.Id == 2);
            result.Should().NotContain(match => match.PaymentRequestByUser.Id == 4);
            result.Should().NotContain(match => match.PaymentRequestByUser.Id == 5);

            var adminResult = await service.GetDuplicatePaymentRequestsByUserAsync(42, 99, 100, paidAt, "inv-100", null, includeOtherUsers: true);

            adminResult.Should().HaveCount(4);
            adminResult.Should().Contain(match => match.PaymentRequestByUser.Id == 2);

            repoMock.Verify(
                r => r.GetPotentialDuplicatesAsync(42, 99, 100, paidAt, "inv-100", null, false),
                Times.Once);
            repoMock.Verify(
                r => r.GetPotentialDuplicatesAsync(42, 99, 100, paidAt, "inv-100", null, true),
                Times.Once);
        }

        [Fact]
        public async Task GetDuplicatePaymentRequestsByUserAsync_ShouldReturnAtMost10Results()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();
            var paidAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);

            var duplicateCandidates = Enumerable.Range(1, 12)
                .Select(i => new PaymentRequestByUser
                {
                    Id = i,
                    UserId = 42,
                    TeamId = 99,
                    Amount = 100,
                    InvoiceNumber = "INV-100",
                    PaidAt = paidAt,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                })
                .ToList();

            repoMock
                .Setup(r => r.GetPotentialDuplicatesAsync(42, 99, 100, paidAt, "INV-100", null, false))
                .ReturnsAsync(duplicateCandidates);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            var result = await service.GetDuplicatePaymentRequestsByUserAsync(42, 99, 100, paidAt, "INV-100");

            result.Should().HaveCount(10);
            result.Should().OnlyContain(match => match.Score >= DuplicatePaymentRequestByUserScorer.MatchThreshold);
        }

        [Fact]
        public async Task GetDuplicatePaymentRequestsByUserAsync_ShouldUseSourceInvoice_WhenProvided()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();
            var paidAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
            var source = new PaymentRequestByUser
            {
                Id = 7,
                UserId = 42,
                TeamId = 99,
                Amount = 100,
                InvoiceNumber = "SRC",
                PaidAt = paidAt,
            };

            repoMock
                .Setup(r => r.GetByIdAsync(7, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(source);
            repoMock
                .Setup(r => r.GetPotentialDuplicatesAsync(42, 99, 100, paidAt, "SRC", 7, false))
                .ReturnsAsync([]);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            var result = await service.GetDuplicatePaymentRequestsByUserAsync(1, 2, 3, DateTime.UtcNow, null, 7);

            result.Should().BeEmpty();
            repoMock.Verify(r => r.GetPotentialDuplicatesAsync(42, 99, 100, paidAt, "SRC", 7, false), Times.Once);
        }

        [Fact]
        public async Task DeletePaymentRequestByUserAsync_ShouldCallRepository_WhenFound()
        {
            var repoMock = new Mock<ITransactionRepository>();
            repoMock.Setup(r => r.DeletePaymentRequestByUserAsync(5)).ReturnsAsync(true);
            var service = new PaymentRequestByUserService(
                repoMock.Object,
                new Mock<ITeamService>().Object,
                new Mock<IFileRepository>().Object,
                new Mock<IBankAccountService>().Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            await service.DeletePaymentRequestByUserAsync(5);

            repoMock.Verify(r => r.DeletePaymentRequestByUserAsync(5), Times.Once);
        }

        [Fact]
        public async Task DeletePaymentRequestByUserAsync_ShouldThrow_WhenMissing()
        {
            var repoMock = new Mock<ITransactionRepository>();
            repoMock.Setup(r => r.DeletePaymentRequestByUserAsync(5)).ReturnsAsync(false);
            var service = new PaymentRequestByUserService(
                repoMock.Object,
                new Mock<ITeamService>().Object,
                new Mock<IFileRepository>().Object,
                new Mock<IBankAccountService>().Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            Func<Task> act = async () => await service.DeletePaymentRequestByUserAsync(5);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DismissDuplicatePaymentRequestByUserAsync_ShouldCallRepository()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var service = new PaymentRequestByUserService(
                repoMock.Object,
                new Mock<ITeamService>().Object,
                new Mock<IFileRepository>().Object,
                new Mock<IBankAccountService>().Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            await service.DismissDuplicatePaymentRequestByUserAsync(1, 2);

            repoMock.Verify(r => r.DismissDuplicatePaymentRequestByUserAsync(1, 2), Times.Once);
        }

        [Fact]
        public async Task DismissDuplicatePaymentRequestByUserAsync_ShouldThrow_WhenSameInvoice()
        {
            var service = BuildService();

            Func<Task> act = async () => await service.DismissDuplicatePaymentRequestByUserAsync(1, 1);

            await act.Should().ThrowAsync<InvalidStateException>();
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
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

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
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            var result = await service.UpdatePaymentRequestByUserAsync(
                1,
                amount: 999,
                purposeOfPayment: "new");

            result.Amount.Should().Be(999);
            result.PurposeOfPayment.Should().Be("new");
        }

        [Fact]
        public async Task Update_ShouldUpdateAllOptionalFields_WhenProvided()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var bankMock = new Mock<IBankAccountService>();
            var paidAt = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc);
            var entity = new PaymentRequestByUser
            {
                Id = 1,
                UserId = 9,
                InvoiceNumber = "old",
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(entity);
            repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<PaymentRequestByUser>()))
                .ReturnsAsync((PaymentRequestByUser p) => p);
            teamMock
                .Setup(t => t.GetTeamByIdAsync(7))
                .ReturnsAsync(new Team { Id = 7 });
            bankMock
                .Setup(b => b.GetBankAccountsAsync(9))
                .ReturnsAsync([new BankAccount { Id = 22, UserId = 9 }]);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                new Mock<IFileRepository>().Object,
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            var result = await service.UpdatePaymentRequestByUserAsync(
                1,
                teamId: 7,
                amount: 250,
                purposeOfPayment: "new purpose",
                paidAt: paidAt,
                invoiceNumber: "new invoice",
                comment: "new comment",
                payoutType: PayoutType.User,
                bankAccountId: 22);

            result.TeamId.Should().Be(7);
            result.Amount.Should().Be(250);
            result.PurposeOfPayment.Should().Be("new purpose");
            result.PaidAt.Should().Be(paidAt);
            result.InvoiceNumber.Should().Be("new invoice");
            result.Comment.Should().Be("new comment");
            result.PayoutType.Should().Be(PayoutType.User);
            result.BankAccountId.Should().Be(22);
        }

        [Fact]
        public async Task MarkAsPaid_ShouldUpdatePaymentFieldsStatusAndHistory_WhenApproved()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();
            var paymentDate = DateTime.Today;
            var entity = new PaymentRequestByUser
            {
                Id = 1,
                InvoiceNumber = "123",
                Status = TransactionStatus.Approved,
                StatusHistory = []
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.Is<GetPaymentRequestByUserQueryById>(q => q.IncludeStatusHistory == true)))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<PaymentRequestByUser>()))
                .ReturnsAsync((PaymentRequestByUser p) => p);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            var result = await service.MarkPaymentRequestByUserAsPaidAsync(
                1,
                42,
                " REF-123 ",
                " Reimbursement May ",
                paymentDate);

            result.Status.Should().Be(TransactionStatus.Paid);
            result.PaymentReference.Should().Be("REF-123");
            result.PurposeOfPayment.Should().Be("Reimbursement May");
            result.FinancePaidAt.Should().Be(DateTime.SpecifyKind(paymentDate, DateTimeKind.Utc));
            result.StatusHistory.Should().ContainSingle();
            result.StatusHistory.Single().ChangedById.Should().Be(42);
            result.StatusHistory.Single().FromStatus.Should().Be(TransactionStatus.Approved);
            result.StatusHistory.Single().ToStatus.Should().Be(TransactionStatus.Paid);
        }

        [Fact]
        public async Task MarkAsPaid_ShouldThrow_WhenInvoiceIsNotApproved()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();
            var entity = new PaymentRequestByUser
            {
                Id = 1,
                InvoiceNumber = "123",
                Status = TransactionStatus.Submitted,
                StatusHistory = []
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(entity);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            Func<Task> act = async () =>
                await service.MarkPaymentRequestByUserAsPaidAsync(
                    1,
                    42,
                    "REF-123",
                    "Reimbursement May",
                    DateTime.Today);

            await act.Should()
                .ThrowAsync<InvalidStateException>()
                .WithMessage("Cannot change invoice status from Submitted to Paid");
        }

        [Theory]
        [InlineData("", "Reimbursement May", "Payment reference is required")]
        [InlineData("REF-123", " ", "Purpose of payment is required")]
        public async Task MarkAsPaid_ShouldThrow_WhenRequiredPaymentFieldIsMissing(
            string paymentReference,
            string purposeOfPayment,
            string expectedMessage)
        {
            var repoMock = new Mock<ITransactionRepository>();
            var entity = new PaymentRequestByUser
            {
                Id = 1,
                InvoiceNumber = "123",
                Status = TransactionStatus.Approved,
                StatusHistory = []
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(entity);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                new Mock<ITeamService>().Object,
                new Mock<IFileRepository>().Object,
                new Mock<IBankAccountService>().Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            Func<Task> act = async () =>
                await service.MarkPaymentRequestByUserAsPaidAsync(
                    1,
                    42,
                    paymentReference,
                    purposeOfPayment,
                    DateTime.Today);

            await act.Should()
                .ThrowAsync<InvalidStateException>()
                .WithMessage(expectedMessage);
        }

        [Fact]
        public async Task MarkAsPaid_ShouldThrow_WhenPaymentDateIsInFuture()
        {
            var repoMock = new Mock<ITransactionRepository>();
            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(new PaymentRequestByUser
                {
                    Id = 1,
                    InvoiceNumber = "123",
                    Status = TransactionStatus.Approved,
                    StatusHistory = []
                });

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                new Mock<ITeamService>().Object,
                new Mock<IFileRepository>().Object,
                new Mock<IBankAccountService>().Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            Func<Task> act = async () =>
                await service.MarkPaymentRequestByUserAsPaidAsync(
                    1,
                    42,
                    "REF-123",
                    "Reimbursement May",
                    DateTime.Today.AddDays(1));

            await act.Should()
                .ThrowAsync<InvalidStateException>()
                .WithMessage("Payment date cannot be in the future!");
        }

        [Fact]
        public async Task Approve_ShouldAssignCostCentreStatusAndHistory_WhenSubmitted()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();
            var costCentreMock = new Mock<ICostCentreService>();
            var budgetMock = new Mock<IBudgetService>();
            var entity = new PaymentRequestByUser
            {
                Id = 1,
                TeamId = 3,
                InvoiceNumber = "123",
                Status = TransactionStatus.Submitted,
                StatusHistory = []
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<PaymentRequestByUser>()))
                .ReturnsAsync((PaymentRequestByUser p) => p);

            costCentreMock
                .Setup(c => c.GetByIdAsync(7))
                .ReturnsAsync(new CostCentre { Id = 7, Name = "Operations" });

            budgetMock
                .Setup(b => b.GetBudgetsAsync(It.IsAny<PayTrack.Application.Dto.Budget.GetBudgetQuery>()))
                .ReturnsAsync(([new Budget { Id = 9, TeamId = 3, CostCentreId = 7, Name = "Ops budget" }], 1));

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object,
                costCentreMock.Object,
                budgetMock.Object);

            var result = await service.ApprovePaymentRequestByUserAsync(1, 42, 7, " approved ");

            result.Status.Should().Be(TransactionStatus.Approved);
            result.BudgetId.Should().Be(9);
            result.StatusHistory.Should().ContainSingle();
            result.StatusHistory.Single().ChangedById.Should().Be(42);
            result.StatusHistory.Single().FromStatus.Should().Be(TransactionStatus.Submitted);
            result.StatusHistory.Single().ToStatus.Should().Be(TransactionStatus.Approved);
            result.StatusHistory.Single().Comment.Should().Be("approved");
        }

        [Fact]
        public async Task Approve_ShouldThrow_WhenCostCentreIsMissing()
        {
            var repoMock = new Mock<ITransactionRepository>();
            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(new PaymentRequestByUser
                {
                    Id = 1,
                    InvoiceNumber = "123",
                    Status = TransactionStatus.Submitted,
                    StatusHistory = []
                });

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                new Mock<ITeamService>().Object,
                new Mock<IFileRepository>().Object,
                new Mock<IBankAccountService>().Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            Func<Task> act = async () =>
                await service.ApprovePaymentRequestByUserAsync(1, 42, 0, null);

            await act.Should()
                .ThrowAsync<InvalidStateException>()
                .WithMessage("Cost centre is required");
        }

        [Fact]
        public async Task Approve_ShouldThrow_WhenCostCentreDoesNotExist()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var costCentreMock = new Mock<ICostCentreService>();
            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(new PaymentRequestByUser
                {
                    Id = 1,
                    InvoiceNumber = "123",
                    Status = TransactionStatus.Submitted,
                    StatusHistory = []
                });
            costCentreMock
                .Setup(c => c.GetByIdAsync(7))
                .ReturnsAsync((CostCentre?)null);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                new Mock<ITeamService>().Object,
                new Mock<IFileRepository>().Object,
                new Mock<IBankAccountService>().Object,
                costCentreMock.Object,
                new Mock<IBudgetService>().Object);

            Func<Task> act = async () =>
                await service.ApprovePaymentRequestByUserAsync(1, 42, 7, null);

            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Cost centre not found");
            repoMock.Verify(r => r.UpdateAsync(It.IsAny<PaymentRequestByUser>()), Times.Never);
        }

        [Fact]
        public async Task RequestChanges_ShouldStoreReasonAndHistory_WhenSubmitted()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();
            var entity = new PaymentRequestByUser
            {
                Id = 1,
                InvoiceNumber = "123",
                Status = TransactionStatus.Submitted,
                StatusHistory = []
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
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            var result = await service.RequestChangesPaymentRequestByUserAsync(1, 42, " upload clearer receipt ");

            result.Status.Should().Be(TransactionStatus.ChangesRequested);
            result.StatusHistory.Should().ContainSingle();
            result.StatusHistory.Single().FromStatus.Should().Be(TransactionStatus.Submitted);
            result.StatusHistory.Single().ToStatus.Should().Be(TransactionStatus.ChangesRequested);
            result.StatusHistory.Single().Comment.Should().Be("upload clearer receipt");
        }

        [Fact]
        public async Task Decline_ShouldStoreReasonAndHistory_WhenNotPaid()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();
            var entity = new PaymentRequestByUser
            {
                Id = 1,
                InvoiceNumber = "123",
                Status = TransactionStatus.Approved,
                StatusHistory = []
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
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            var result = await service.DeclinePaymentRequestByUserAsync(1, 42, " duplicate invoice ");

            result.Status.Should().Be(TransactionStatus.Declined);
            result.StatusHistory.Should().ContainSingle();
            result.StatusHistory.Single().FromStatus.Should().Be(TransactionStatus.Approved);
            result.StatusHistory.Single().ToStatus.Should().Be(TransactionStatus.Declined);
            result.StatusHistory.Single().Comment.Should().Be("duplicate invoice");
        }

        [Fact]
        public async Task Decline_ShouldThrow_WhenInvoiceIsPaid()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();
            var bankMock = new Mock<IBankAccountService>();
            var entity = new PaymentRequestByUser
            {
                Id = 1,
                InvoiceNumber = "123",
                Status = TransactionStatus.Paid,
                StatusHistory = []
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(entity);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object,
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            Func<Task> act = async () =>
                await service.DeclinePaymentRequestByUserAsync(1, 42, "duplicate invoice");

            await act.Should()
                .ThrowAsync<InvalidStateException>()
                .WithMessage("Cannot change invoice status from Paid to Declined");
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
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            var result = await service.GetReceiptForPaymentRequestByUserByIdAsync(1);

            result.content.Should().Equal(1, 2, 3);
            result.contentType.Should().Be("application/pdf");
        }

        [Theory]
        [InlineData("/files/test.jpg", "image/jpeg")]
        [InlineData("/files/test.jpeg", "image/jpeg")]
        [InlineData("/files/test.png", "image/png")]
        [InlineData("/files/test.bin", "application/octet-stream")]
        public async Task GetReceipt_ShouldInferContentTypeFromReceiptExtension(
            string receiptUrl,
            string expectedContentType)
        {
            var repoMock = new Mock<ITransactionRepository>();
            var fileMock = new Mock<IFileRepository>();
            var entity = new PaymentRequestByUser
            {
                InvoiceNumber = "123",
                Id = 1,
                ReceiptUrl = receiptUrl
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(entity);
            fileMock
                .Setup(f => f.GetByPath(receiptUrl))
                .ReturnsAsync([1, 2, 3]);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                new Mock<ITeamService>().Object,
                fileMock.Object,
                new Mock<IBankAccountService>().Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            var result = await service.GetReceiptForPaymentRequestByUserByIdAsync(1);

            result.contentType.Should().Be(expectedContentType);
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

        [Fact]
        public void ValidateQuery_UnknownRole_ReturnsFalse()
        {
            var service = BuildService();
            var user = new User { Id = 1, Role = (Role)999 };
            var query = new GetPaymentRequestByUserQuery { UserId = 1, TeamId = 1 };

            service.ValidateQuery(query, user).Should().BeFalse();
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

        [Fact]
        public void ValidateAccessToInvoice_UnknownRole_ReturnsFalse()
        {
            var service = BuildService();
            var user = new User { Id = 1, Role = (Role)999 };
            var invoice = new PaymentRequestByUser { UserId = 1, TeamId = 1, InvoiceNumber = "1" };

            service.ValidateAccessToInvoice(invoice, user).Should().BeFalse();
        }

        private static PaymentRequestByUserService BuildService()
        {
            return new PaymentRequestByUserService(
                new Mock<ITransactionRepository>().Object,
                new Mock<ITeamService>().Object,
                new Mock<IFileRepository>().Object,
                new Mock<IBankAccountService>().Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);
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
                bankMock.Object,
                new Mock<ICostCentreService>().Object,
                new Mock<IBudgetService>().Object);

            Func<Task> act = async () =>
                await service.GetReceiptForPaymentRequestByUserByIdAsync(1);

            await act.Should()
                .ThrowAsync<InvalidStateException>()
                .WithMessage("Receipt URL is null although it should not be.");
        }
    }
}
