using FluentAssertions;
using Moq;
using PayTrack.Application.Dto.PaymentRequestByTeam;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Implementation;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Tests.UnitTests.Services
{
    public class PaymentRequestByTeamServiceTests
    {
        // ----------------------------
        // GET ALL
        // ----------------------------
        [Fact]
        public async Task GetAllAsync_ShouldReturnListAndCount()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();

            var list = new List<PaymentRequestByTeam>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

            repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetPaymentRequestByTeamQuery>()))
                .ReturnsAsync((list, list.Count));

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object);

            var (result, count) = await service.GetAllAsync();

            result.Should().HaveCount(2);
            count.Should().Be(2);
        }

        // ----------------------------
        // GET BY ID
        // ----------------------------
        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();

            var entity = new PaymentRequestByTeam { Id = 1 };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object);

            var result = await service.GetPaymentRequestByTeamByIdAsync(1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        // ----------------------------
        // CREATE
        // ----------------------------
        [Fact]
        public async Task Create_ShouldThrow_WhenTeamNotFound()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();

            teamMock
                .Setup(t => t.GetTeamByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Team?)null);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object);

            Func<Task> act = async () =>
                await service.CreatePaymentRequestByTeamAsync(
                    userToAssignToId: 1,
                    creatingUserId: 2,
                    teamId: 99,
                    amount: 100,
                    purposeOfPayment: "test",
                    dueDate: DateTime.Today.AddDays(7));

            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Team could not be found");
        }

        [Fact]
        public async Task Create_ShouldCallRepositoryAndReturnEntity()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();

            var team = new Team { Id = 5 };

            teamMock
                .Setup(t => t.GetTeamByIdAsync(5))
                .ReturnsAsync(team);

            var created = new PaymentRequestByTeam { Id = 1 };

            repoMock
                .Setup(r => r.AddAsync(It.IsAny<PaymentRequestByTeam>()))
                .ReturnsAsync(created);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object);

            var result = await service.CreatePaymentRequestByTeamAsync(
                userToAssignToId: 1,
                creatingUserId: 2,
                teamId: 5,
                amount: 100,
                purposeOfPayment: "test",
                dueDate: DateTime.Today.AddDays(7));

            result.Should().NotBeNull();
            result.Id.Should().Be(1);

            repoMock.Verify(r =>
                r.AddAsync(It.Is<PaymentRequestByTeam>(p =>
                    p.UserId == 1 &&
                    p.TeamId == 5 &&
                    p.Amount == 100 &&
                    p.PurposeOfPayment == "test" &&
                    p.RequestedById == 2 &&
                    p.PaymentDirection == PaymentDirection.In &&
                    p.Status == TransactionStatus.Submitted &&
                    p.DueDate != null
                )),
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

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync((PaymentRequestByTeam?)null);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object);

            Func<Task> act = async () =>
                await service.UpdatePaymentRequestByTeamAsync(1);

            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Transaction not found");
        }

        [Fact]
        public async Task Update_ShouldUpdateFields_WhenProvided()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 1,
                Amount = 100,
                PurposeOfPayment = "old"
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<PaymentRequestByTeam>()))
                .ReturnsAsync((PaymentRequestByTeam p) => p);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object);

            var result = await service.UpdatePaymentRequestByTeamAsync(
                1,
                amount: 999,
                purposeOfPayment: "new");

            result.Amount.Should().Be(999);
            result.PurposeOfPayment.Should().Be("new");
        }

        [Fact]
        public async Task Update_ShouldUpdateTeam_WhenTeamIdProvided()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 1,
                TeamId = 1
            };

            var newTeam = new Team { Id = 5 };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            teamMock
                .Setup(t => t.GetTeamByIdAsync(5))
                .ReturnsAsync(newTeam);

            repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<PaymentRequestByTeam>()))
                .ReturnsAsync((PaymentRequestByTeam p) => p);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object);

            var result = await service.UpdatePaymentRequestByTeamAsync(1, teamId: 5);

            result.TeamId.Should().Be(5);
        }

        [Fact]
        public async Task Update_ShouldThrow_WhenTeamNotFound()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();

            var entity = new PaymentRequestByTeam { Id = 1 };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            teamMock
                .Setup(t => t.GetTeamByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Team?)null);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object);

            Func<Task> act = async () =>
                await service.UpdatePaymentRequestByTeamAsync(1, teamId: 99);

            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Team not found");
        }
    }
}
