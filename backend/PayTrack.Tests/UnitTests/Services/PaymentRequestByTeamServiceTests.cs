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
            var userMock = new Mock<IUserService>();
            var costCentreMock = new Mock<ICostCentreService>();

            var list = new List<PaymentRequestByTeam>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

            repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetPaymentRequestByTeamQuery>()))
                .ReturnsAsync((list, list.Count));

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object, userMock.Object, costCentreMock.Object);

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
            var userMock = new Mock<IUserService>();
            var costCentreMock = new Mock<ICostCentreService>();

            var entity = new PaymentRequestByTeam { Id = 1 };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object, userMock.Object, costCentreMock.Object);

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
            var userMock = new Mock<IUserService>();
            var costCentreMock = new Mock<ICostCentreService>();

            teamMock
                .Setup(t => t.GetTeamByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Team?)null);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object, userMock.Object, costCentreMock.Object);

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
            var userMock = new Mock<IUserService>();
            var costCentreMock = new Mock<ICostCentreService>();

            var team = new Team { Id = 5 };
            var assignedUser = new User { Id = 1 };
            var creatingUser = new User { Id = 2 };

            teamMock
                .Setup(t => t.GetTeamByIdAsync(5))
                .ReturnsAsync(team);

            userMock
                .Setup(u => u.GetUserByIdAsync(1))
                .ReturnsAsync(assignedUser);

            userMock
                .Setup(u => u.GetUserByIdAsync(2))
                .ReturnsAsync(creatingUser);

            var created = new PaymentRequestByTeam { Id = 1 };

            repoMock
                .Setup(r => r.AddAsync(It.IsAny<PaymentRequestByTeam>()))
                .ReturnsAsync(created);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object, userMock.Object, costCentreMock.Object);

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
            var userMock = new Mock<IUserService>();
            var costCentreMock = new Mock<ICostCentreService>();

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync((PaymentRequestByTeam?)null);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object, userMock.Object, costCentreMock.Object);

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
            var userMock = new Mock<IUserService>();
            var costCentreMock = new Mock<ICostCentreService>();

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

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object, userMock.Object, costCentreMock.Object);

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
            var userMock = new Mock<IUserService>();
            var costCentreMock = new Mock<ICostCentreService>();

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

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object, userMock.Object, costCentreMock.Object);

            var result = await service.UpdatePaymentRequestByTeamAsync(1, teamId: 5);

            result.TeamId.Should().Be(5);
        }

        [Fact]
        public async Task Update_ShouldThrow_WhenTeamNotFound()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var costCentreMock = new Mock<ICostCentreService>();

            var entity = new PaymentRequestByTeam { Id = 1 };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            teamMock
                .Setup(t => t.GetTeamByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Team?)null);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object, userMock.Object, costCentreMock.Object);

            Func<Task> act = async () =>
                await service.UpdatePaymentRequestByTeamAsync(1, teamId: 99);

            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Team not found");
        }

        // ----------------------------
        // CREATE — input validation
        // ----------------------------
        [Fact]
        public async Task Create_ShouldThrow_WhenAssignedUserNotFound()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var costCentreMock = new Mock<ICostCentreService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync((User?)null);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object, userMock.Object, costCentreMock.Object);

            Func<Task> act = async () =>
                await service.CreatePaymentRequestByTeamAsync(
                    userToAssignToId: 1,
                    creatingUserId: 2,
                    teamId: 5,
                    amount: 100,
                    purposeOfPayment: "test",
                    dueDate: DateTime.Today.AddDays(7));

            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Assigned user could not be found");
        }

        [Fact]
        public async Task Create_ShouldThrow_WhenCreatingUserNotFound()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var costCentreMock = new Mock<ICostCentreService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync((User?)null);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object, userMock.Object, costCentreMock.Object);

            Func<Task> act = async () =>
                await service.CreatePaymentRequestByTeamAsync(
                    userToAssignToId: 1,
                    creatingUserId: 2,
                    teamId: 5,
                    amount: 100,
                    purposeOfPayment: "test",
                    dueDate: DateTime.Today.AddDays(7));

            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Creating user could not be found");
        }

        [Fact]
        public async Task Create_ShouldThrow_WhenCostCentreNotFound()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var costCentreMock = new Mock<ICostCentreService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2 });
            costCentreMock.Setup(c => c.GetByIdAsync(99)).ReturnsAsync((CostCentre?)null);

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object, userMock.Object, costCentreMock.Object);

            Func<Task> act = async () =>
                await service.CreatePaymentRequestByTeamAsync(
                    userToAssignToId: 1,
                    creatingUserId: 2,
                    teamId: 5,
                    amount: 100,
                    purposeOfPayment: "test",
                    dueDate: DateTime.Today.AddDays(7),
                    costCentreId: 99);

            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Cost centre could not be found");
        }

        [Fact]
        public async Task Create_ShouldThrow_WhenAmountIsZero()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var costCentreMock = new Mock<ICostCentreService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2 });

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object, userMock.Object, costCentreMock.Object);

            Func<Task> act = async () =>
                await service.CreatePaymentRequestByTeamAsync(
                    userToAssignToId: 1,
                    creatingUserId: 2,
                    teamId: 5,
                    amount: 0,
                    purposeOfPayment: "test",
                    dueDate: DateTime.Today.AddDays(7));

            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Amount must be greater than 0");
        }

        [Fact]
        public async Task Create_ShouldThrow_WhenAmountIsNegative()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var costCentreMock = new Mock<ICostCentreService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2 });

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object, userMock.Object, costCentreMock.Object);

            Func<Task> act = async () =>
                await service.CreatePaymentRequestByTeamAsync(
                    userToAssignToId: 1,
                    creatingUserId: 2,
                    teamId: 5,
                    amount: -50,
                    purposeOfPayment: "test",
                    dueDate: DateTime.Today.AddDays(7));

            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Amount must be greater than 0");
        }

        [Fact]
        public async Task Create_ShouldThrow_WhenDueDateIsInThePast()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var costCentreMock = new Mock<ICostCentreService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2 });

            var service = new PaymentRequestByTeamService(repoMock.Object, teamMock.Object, userMock.Object, costCentreMock.Object);

            Func<Task> act = async () =>
                await service.CreatePaymentRequestByTeamAsync(
                    userToAssignToId: 1,
                    creatingUserId: 2,
                    teamId: 5,
                    amount: 100,
                    purposeOfPayment: "test",
                    dueDate: DateTime.Today.AddDays(-1));

            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Due date cannot be in the past");
        }

        // ----------------------------
        // VALIDATE QUERY
        // ----------------------------
        [Theory]
        [InlineData(Role.Admin, null, null, true)]
        [InlineData(Role.Admin, 99, 99, true)]
        public void ValidateQuery_Admin_ShouldAlwaysReturnTrue(Role role, int? queryUserId, int? queryTeamId, bool expected)
        {
            var service = new PaymentRequestByTeamService(
                new Mock<ITransactionRepository>().Object,
                new Mock<ITeamService>().Object,
                new Mock<IUserService>().Object,
                new Mock<ICostCentreService>().Object);

            var user = new User { Id = 1, Role = role, TeamId = 1 };
            var query = new GetPaymentRequestByTeamQuery { UserId = queryUserId, TeamId = queryTeamId };

            service.ValidateQuery(query, user).Should().Be(expected);
        }

        [Fact]
        public void ValidateQuery_RegularUser_ShouldReturnTrue_WhenQueryMatchesOwnId()
        {
            var service = new PaymentRequestByTeamService(
                new Mock<ITransactionRepository>().Object,
                new Mock<ITeamService>().Object,
                new Mock<IUserService>().Object,
                new Mock<ICostCentreService>().Object);

            var user = new User { Id = 7, Role = Role.RegularUser };
            var query = new GetPaymentRequestByTeamQuery { UserId = 7 };

            service.ValidateQuery(query, user).Should().BeTrue();
        }

        [Fact]
        public void ValidateQuery_RegularUser_ShouldReturnFalse_WhenQueryHasDifferentUserId()
        {
            var service = new PaymentRequestByTeamService(
                new Mock<ITransactionRepository>().Object,
                new Mock<ITeamService>().Object,
                new Mock<IUserService>().Object,
                new Mock<ICostCentreService>().Object);

            var user = new User { Id = 7, Role = Role.RegularUser };
            var query = new GetPaymentRequestByTeamQuery { UserId = 99 };

            service.ValidateQuery(query, user).Should().BeFalse();
        }

        [Fact]
        public void ValidateQuery_TeamLead_ShouldReturnTrue_WhenQueryMatchesOwnTeam()
        {
            var service = new PaymentRequestByTeamService(
                new Mock<ITransactionRepository>().Object,
                new Mock<ITeamService>().Object,
                new Mock<IUserService>().Object,
                new Mock<ICostCentreService>().Object);

            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = 3 };
            var query = new GetPaymentRequestByTeamQuery { TeamId = 3 };

            service.ValidateQuery(query, user).Should().BeTrue();
        }

        [Fact]
        public void ValidateQuery_TeamLead_ShouldReturnFalse_WhenQueryHasDifferentTeamId()
        {
            var service = new PaymentRequestByTeamService(
                new Mock<ITransactionRepository>().Object,
                new Mock<ITeamService>().Object,
                new Mock<IUserService>().Object,
                new Mock<ICostCentreService>().Object);

            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = 3 };
            var query = new GetPaymentRequestByTeamQuery { TeamId = 99 };

            service.ValidateQuery(query, user).Should().BeFalse();
        }

        [Fact]
        public void ValidateQuery_TeamLead_ShouldReturnFalse_WhenUserHasNoTeam()
        {
            var service = new PaymentRequestByTeamService(
                new Mock<ITransactionRepository>().Object,
                new Mock<ITeamService>().Object,
                new Mock<IUserService>().Object,
                new Mock<ICostCentreService>().Object);

            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = null };
            var query = new GetPaymentRequestByTeamQuery { TeamId = 3 };

            service.ValidateQuery(query, user).Should().BeFalse();
        }
    }
}
