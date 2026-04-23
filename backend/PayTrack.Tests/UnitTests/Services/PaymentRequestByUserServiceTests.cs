using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using PayTrack.Application.Dto.PaymentRequestByUser;
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

            var list = new List<PaymentRequestByUser>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

            repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetPaymentRequestByUserQuery>()))
                .ReturnsAsync((list, list.Count));

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object);

            var result = await service.GetAllAsync();

            result.paymentRequestByUser.Should().HaveCount(2);
            result.totalCount.Should().Be(2);
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

            var entity = new PaymentRequestByUser { Id = 1 };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(entity);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object);

            var result = await service.GetPaymentRequestByUserByIdAsync(1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
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

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object);

            var futureDate = DateTime.Today.AddDays(1);

            var file = new FormFile(Stream.Null, 0, 0, "file", "test.pdf");

            Func<Task> act = async () =>
                await service.CreatePaymentRequestByUserAsync(
                    1,
                    1,
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

            var team = new Team { Id = 5 };

            teamMock
                .Setup(t => t.GetTeamByIdAsync(5))
                .ReturnsAsync(team);

            var created = new PaymentRequestByUser { Id = 1 };

            repoMock
                .Setup(r => r.AddAsync(It.IsAny<PaymentRequestByUser>(), It.IsAny<IFormFile>()))
                .ReturnsAsync(created);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object);

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
        // UPDATE
        // ----------------------------
        [Fact]
        public async Task Update_ShouldThrow_WhenEntityNotFound()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<object>()))
                .ReturnsAsync((PaymentRequestByUser?)null);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object);

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

            var entity = new PaymentRequestByUser
            {
                Id = 1,
                Amount = 10,
                PurposeOfPayment = "old"
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<object>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<PaymentRequestByUser>()))
                .ReturnsAsync((PaymentRequestByUser p) => p);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object);

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

            var entity = new PaymentRequestByUser
            {
                Id = 1,
                ReceiptUrl = "/files/test.pdf"
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<object>()))
                .ReturnsAsync(entity);

            fileMock
                .Setup(f => f.GetByPath(entity.ReceiptUrl))
                .ReturnsAsync(new byte[] { 1, 2, 3 });

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object);

            var result = await service.GetReceiptForPaymentRequestByUserByIdAsync(1);

            result.Should().Equal(1, 2, 3);
        }

        [Fact]
        public async Task GetReceipt_ShouldThrow_WhenReceiptUrlIsNull()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var fileMock = new Mock<IFileRepository>();

            var entity = new PaymentRequestByUser
            {
                Id = 1,
                ReceiptUrl = null
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<object>()))
                .ReturnsAsync(entity);

            var service = new PaymentRequestByUserService(
                repoMock.Object,
                teamMock.Object,
                fileMock.Object);

            Func<Task> act = async () =>
                await service.GetReceiptForPaymentRequestByUserByIdAsync(1);

            await act.Should()
                .ThrowAsync<InvalidStateException>()
                .WithMessage("Receipt URL is null although it should not be.");
        }
    }
}
