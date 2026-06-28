//AI helped with the test cases

using FluentAssertions;
using Microsoft.Extensions.Logging;
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
        private static PaymentRequestByTeamService BuildService(
            Mock<ITransactionRepository> repoMock,
            Mock<ITeamService> teamMock,
            Mock<IUserService> userMock,
            Mock<IBudgetService> budgetMock,
            Mock<INotificationDispatchService>? notificationsMock = null,
            bool creationEmail = true,
            bool creationSlack = false,
            bool creationPush = true,
            bool confirmationEmail = true,
            bool confirmationSlack = false,
            bool confirmationPush = true,
            bool deletionEmail = true,
            bool deletionSlack = false,
            bool deletionPush = true,
            Mock<IPushNotificationService>? pushNotificationsMock = null)
        {
            notificationsMock ??= new Mock<INotificationDispatchService>();

            var systemSettingsMock = new Mock<ISystemSettingService>();
            systemSettingsMock
                .Setup(s => s.GetBoolSettingAsync(SystemSettingKeys.NotificationsCreationEmail, It.IsAny<bool>()))
                .ReturnsAsync(creationEmail);
            systemSettingsMock
                .Setup(s => s.GetBoolSettingAsync(SystemSettingKeys.NotificationsCreationSlack, It.IsAny<bool>()))
                .ReturnsAsync(creationSlack);
            systemSettingsMock
                .Setup(s => s.GetBoolSettingAsync(SystemSettingKeys.NotificationsCreationPush, It.IsAny<bool>()))
                .ReturnsAsync(creationPush);
            systemSettingsMock
                .Setup(s => s.GetBoolSettingAsync(SystemSettingKeys.NotificationsConfirmationEmail, It.IsAny<bool>()))
                .ReturnsAsync(confirmationEmail);
            systemSettingsMock
                .Setup(s => s.GetBoolSettingAsync(SystemSettingKeys.NotificationsConfirmationSlack, It.IsAny<bool>()))
                .ReturnsAsync(confirmationSlack);
            systemSettingsMock
                .Setup(s => s.GetBoolSettingAsync(SystemSettingKeys.NotificationsConfirmationPush, It.IsAny<bool>()))
                .ReturnsAsync(confirmationPush);
            systemSettingsMock
                .Setup(s => s.GetBoolSettingAsync(SystemSettingKeys.NotificationsDeletionEmail, It.IsAny<bool>()))
                .ReturnsAsync(deletionEmail);
            systemSettingsMock
                .Setup(s => s.GetBoolSettingAsync(SystemSettingKeys.NotificationsDeletionSlack, It.IsAny<bool>()))
                .ReturnsAsync(deletionSlack);
            systemSettingsMock
                .Setup(s => s.GetBoolSettingAsync(SystemSettingKeys.NotificationsDeletionPush, It.IsAny<bool>()))
                .ReturnsAsync(deletionPush);

            var logger = new Mock<ILogger<PaymentRequestByTeamService>>();
            return new PaymentRequestByTeamService(
                repoMock.Object,
                teamMock.Object,
                userMock.Object,
                budgetMock.Object,
                notificationsMock.Object,
                systemSettingsMock.Object,
                logger.Object,
                pushNotificationsMock?.Object);
        }

        // ----------------------------
        // GET ALL
        // ----------------------------
        [Fact]
        public async Task GetAllAsync_ShouldReturnListAndCount()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();

            var list = new List<PaymentRequestByTeam>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

            repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<GetPaymentRequestByTeamQuery>()))
                .ReturnsAsync((list, list.Count));

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

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
            var budgetMock = new Mock<IBudgetService>();

            var entity = new PaymentRequestByTeam { Id = 1 };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

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
            var budgetMock = new Mock<IBudgetService>();

            teamMock
                .Setup(t => t.GetTeamByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Team?)null);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

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
            var budgetMock = new Mock<IBudgetService>();

            var team = new Team { Id = 5 };
            var assignedUser = new User { Id = 1, Name = "Alice", Email = "alice@test.com" };
            var creatingUser = new User { Id = 2 };

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(team);
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(assignedUser);
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(creatingUser);

            var created = new PaymentRequestByTeam { Id = 1 };

            repoMock
                .Setup(r => r.AddAsync(It.IsAny<PaymentRequestByTeam>()))
                .ReturnsAsync(created);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

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

        [Fact]
        public async Task Create_ShouldSendEmailToAssignedUser()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var assignedUser = new User { Id = 1, Name = "Alice", Email = "alice@test.com" };
            var creatingUser = new User { Id = 2, Name = "Bob", Email = "bob@test.com" };

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(assignedUser);
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(creatingUser);
            repoMock.Setup(r => r.AddAsync(It.IsAny<PaymentRequestByTeam>())).ReturnsAsync(new PaymentRequestByTeam());

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock);

            await service.CreatePaymentRequestByTeamAsync(
                userToAssignToId: 1,
                creatingUserId: 2,
                teamId: 5,
                amount: 100,
                purposeOfPayment: "Office Supplies",
                dueDate: DateTime.Today.AddDays(7));

            notificationsMock.Verify(
                n => n.SendEmailAsync(
                    "alice@test.com",
                    It.Is<string>(s => s.Contains("New Payment Request") && s.Contains("Office Supplies")),
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Create_ShouldNotSendEmail_WhenTeamNotFound()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(It.IsAny<int>())).ReturnsAsync((Team?)null);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock);

            Func<Task> act = async () =>
                await service.CreatePaymentRequestByTeamAsync(1, 2, 99, 100, "test", DateTime.Today.AddDays(7));

            await act.Should().ThrowAsync<NotFoundException>();

            notificationsMock.Verify(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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
            var budgetMock = new Mock<IBudgetService>();

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync((PaymentRequestByTeam?)null);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

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
            var budgetMock = new Mock<IBudgetService>();

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

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

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
            var budgetMock = new Mock<IBudgetService>();

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

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

            var result = await service.UpdatePaymentRequestByTeamAsync(1, teamId: 5);

            result.TeamId.Should().Be(5);
        }

        [Fact]
        public async Task Update_ShouldThrow_WhenTeamNotFound()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();

            var entity = new PaymentRequestByTeam { Id = 1 };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            teamMock
                .Setup(t => t.GetTeamByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Team?)null);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

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
            var budgetMock = new Mock<IBudgetService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync((User?)null);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

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
            var budgetMock = new Mock<IBudgetService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync((User?)null);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

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
        public async Task Create_ShouldThrow_WhenBudgetNotFound()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2 });
            budgetMock.Setup(c => c.GetByIdAsync(99)).ReturnsAsync((Budget?)null);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

            Func<Task> act = async () =>
                await service.CreatePaymentRequestByTeamAsync(
                    userToAssignToId: 1,
                    creatingUserId: 2,
                    teamId: 5,
                    amount: 100,
                    purposeOfPayment: "test",
                    dueDate: DateTime.Today.AddDays(7),
                    budgetId: 99);

            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Budget could not be found");
        }

        [Fact]
        public async Task Create_ShouldThrow_WhenAmountIsZero()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2 });

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

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
            var budgetMock = new Mock<IBudgetService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2 });

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

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
            var budgetMock = new Mock<IBudgetService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2 });

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

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
        // MARK AS PAID
        // ----------------------------
        [Fact]
        public async Task MarkAsPaid_ShouldThrow_WhenTransactionNotFound()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync((PaymentRequestByTeam?)null);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

            Func<Task> act = async () => await service.MarkAsPaidAsync(1, 99, null);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Transaction not found");
        }

        [Theory]
        [InlineData(TransactionStatus.Paid)]
        [InlineData(TransactionStatus.Declined)]
        public async Task MarkAsPaid_ShouldThrow_WhenStatusIsPaidOrDeclined(TransactionStatus status)
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var entity = new PaymentRequestByTeam { Id = 1, Status = status };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock);

            Func<Task> act = async () => await service.MarkAsPaidAsync(1, 99, null);

            await act.Should().ThrowAsync<InvalidStateException>();
            notificationsMock.Verify(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(TransactionStatus.Submitted)]
        [InlineData(TransactionStatus.ChangesRequested)]
        [InlineData(TransactionStatus.Approved)]
        public async Task MarkAsPaid_ShouldUpdateStatusAndCreateHistory_WhenStatusIsAllowed(TransactionStatus fromStatus)
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 5,
                Status = fromStatus,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(5, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.UpdateAndAddStatusHistoryAsync(It.IsAny<PaymentRequestByTeam>(), It.IsAny<TransactionStatusHistory>()))
                .ReturnsAsync((PaymentRequestByTeam p, TransactionStatusHistory h) => p);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

            var result = await service.MarkAsPaidAsync(5, 42, "my comment");

            result.Status.Should().Be(TransactionStatus.Paid);
            result.PaidAt.Should().NotBeNull();

            repoMock.Verify(r => r.UpdateAndAddStatusHistoryAsync(
                It.Is<PaymentRequestByTeam>(p =>
                    p.Status == TransactionStatus.Paid &&
                    p.PaidAt != null),
                It.Is<TransactionStatusHistory>(h =>
                    h.TransactionId == 5 &&
                    h.ChangedById == 42 &&
                    h.FromStatus == fromStatus &&
                    h.ToStatus == TransactionStatus.Paid &&
                    h.Comment == "my comment")), Times.Once);
        }

        [Fact]
        public async Task MarkAsPaid_ShouldSendConfirmationEmailToUser()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 5,
                Status = TransactionStatus.Submitted,
                PurposeOfPayment = "Office Supplies",
                Amount = 250m,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(5, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.UpdateAndAddStatusHistoryAsync(It.IsAny<PaymentRequestByTeam>(), It.IsAny<TransactionStatusHistory>()))
                .ReturnsAsync((PaymentRequestByTeam p, TransactionStatusHistory h) => p);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock);

            await service.MarkAsPaidAsync(5, 42, null);

            notificationsMock.Verify(
                n => n.SendEmailAsync(
                    "alice@test.com",
                    It.Is<string>(s => s.Contains("Payment Confirmed") && s.Contains("Office Supplies")),
                    It.IsAny<string>()),
                Times.Once);
        }

        // ----------------------------
        // VALIDATE QUERY
        // ----------------------------
        [Theory]
        [InlineData(Role.Admin, null, null, true)]
        [InlineData(Role.Admin, 99, 99, true)]
        public void ValidateQuery_Admin_ShouldAlwaysReturnTrue(Role role, int? queryUserId, int? queryTeamId, bool expected)
        {
            var service = BuildService(
                new Mock<ITransactionRepository>(),
                new Mock<ITeamService>(),
                new Mock<IUserService>(),
                new Mock<IBudgetService>());

            var user = new User { Id = 1, Role = role, TeamId = 1 };
            var query = new GetPaymentRequestByTeamQuery { UserId = queryUserId, TeamId = queryTeamId };

            service.ValidateQuery(query, user).Should().Be(expected);
        }

        [Fact]
        public void ValidateQuery_RegularUser_ShouldReturnTrue_WhenQueryMatchesOwnId()
        {
            var service = BuildService(
                new Mock<ITransactionRepository>(),
                new Mock<ITeamService>(),
                new Mock<IUserService>(),
                new Mock<IBudgetService>());

            var user = new User { Id = 7, Role = Role.RegularUser };
            var query = new GetPaymentRequestByTeamQuery { UserId = 7 };

            service.ValidateQuery(query, user).Should().BeTrue();
        }

        [Fact]
        public void ValidateQuery_RegularUser_ShouldReturnFalse_WhenQueryHasDifferentUserId()
        {
            var service = BuildService(
                new Mock<ITransactionRepository>(),
                new Mock<ITeamService>(),
                new Mock<IUserService>(),
                new Mock<IBudgetService>());

            var user = new User { Id = 7, Role = Role.RegularUser };
            var query = new GetPaymentRequestByTeamQuery { UserId = 99 };

            service.ValidateQuery(query, user).Should().BeFalse();
        }

        [Fact]
        public void ValidateQuery_TeamLead_ShouldReturnTrue_WhenQueryMatchesOwnTeam()
        {
            var service = BuildService(
                new Mock<ITransactionRepository>(),
                new Mock<ITeamService>(),
                new Mock<IUserService>(),
                new Mock<IBudgetService>());

            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = 3 };
            var query = new GetPaymentRequestByTeamQuery { TeamId = 3 };

            service.ValidateQuery(query, user).Should().BeTrue();
        }

        [Fact]
        public void ValidateQuery_TeamLead_ShouldReturnFalse_WhenQueryHasDifferentTeamId()
        {
            var service = BuildService(
                new Mock<ITransactionRepository>(),
                new Mock<ITeamService>(),
                new Mock<IUserService>(),
                new Mock<IBudgetService>());

            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = 3 };
            var query = new GetPaymentRequestByTeamQuery { TeamId = 99 };

            service.ValidateQuery(query, user).Should().BeFalse();
        }

        [Fact]
        public void ValidateQuery_TeamLead_ShouldReturnFalse_WhenUserHasNoTeam()
        {
            var service = BuildService(
                new Mock<ITransactionRepository>(),
                new Mock<ITeamService>(),
                new Mock<IUserService>(),
                new Mock<IBudgetService>());

            var user = new User { Id = 1, Role = Role.TeamLead, TeamId = null };
            var query = new GetPaymentRequestByTeamQuery { TeamId = 3 };

            service.ValidateQuery(query, user).Should().BeFalse();
        }

        // ----------------------------
        // CREATE — Slack channel
        // ----------------------------
        [Fact]
        public async Task Create_ShouldSendSlack_WhenSlackEnabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var assignedUser = new User { Id = 1, Name = "Alice", Email = "alice@test.com" };
            var creatingUser = new User { Id = 2, Name = "Bob", Email = "bob@test.com" };

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(assignedUser);
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(creatingUser);
            repoMock.Setup(r => r.AddAsync(It.IsAny<PaymentRequestByTeam>())).ReturnsAsync(new PaymentRequestByTeam());

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock, creationEmail: false, creationSlack: true);

            await service.CreatePaymentRequestByTeamAsync(1, 2, 5, 100, "Office Supplies", DateTime.Today.AddDays(7));

            notificationsMock.Verify(
                n => n.SendSlackAsync(
                    "alice@test.com",
                    It.Is<string>(s => s.Contains("New Payment Request") && s.Contains("Office Supplies"))),
                Times.Once);

            notificationsMock.Verify(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Create_ShouldNotSendSlack_WhenSlackDisabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1, Name = "Alice", Email = "alice@test.com" });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2 });
            repoMock.Setup(r => r.AddAsync(It.IsAny<PaymentRequestByTeam>())).ReturnsAsync(new PaymentRequestByTeam());

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock);

            await service.CreatePaymentRequestByTeamAsync(1, 2, 5, 100, "Office Supplies", DateTime.Today.AddDays(7));

            notificationsMock.Verify(n => n.SendSlackAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Create_ShouldContinue_WhenSlackThrows()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1, Name = "Alice", Email = "alice@test.com" });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2 });

            var created = new PaymentRequestByTeam { Id = 1 };
            repoMock.Setup(r => r.AddAsync(It.IsAny<PaymentRequestByTeam>())).ReturnsAsync(created);

            notificationsMock
                .Setup(n => n.SendSlackAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Slack error"));

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock, creationEmail: false, creationSlack: true);

            var result = await service.CreatePaymentRequestByTeamAsync(1, 2, 5, 100, "test", DateTime.Today.AddDays(7));

            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Create_ShouldContinue_WhenEmailThrows()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1, Name = "Alice", Email = "alice@test.com" });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2 });

            var created = new PaymentRequestByTeam { Id = 1 };
            repoMock.Setup(r => r.AddAsync(It.IsAny<PaymentRequestByTeam>())).ReturnsAsync(created);

            notificationsMock
                .Setup(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("SMTP error"));

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock);

            var result = await service.CreatePaymentRequestByTeamAsync(1, 2, 5, 100, "test", DateTime.Today.AddDays(7));

            result.Id.Should().Be(1);
        }

        // ----------------------------
        // MARK AS PAID — Slack channel
        // ----------------------------
        [Fact]
        public async Task Create_ShouldSendPush_WhenPushEnabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var pushNotificationsMock = new Mock<IPushNotificationService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1, Name = "Alice", Email = "alice@test.com" });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2 });

            repoMock
                .Setup(r => r.AddAsync(It.IsAny<PaymentRequestByTeam>()))
                .ReturnsAsync(new PaymentRequestByTeam
                {
                    Id = 11,
                    UserId = 1,
                    PurposeOfPayment = "Office Supplies",
                    Amount = 100m,
                });

            var service = BuildService(
                repoMock,
                teamMock,
                userMock,
                budgetMock,
                creationEmail: false,
                creationSlack: false,
                pushNotificationsMock: pushNotificationsMock);

            await service.CreatePaymentRequestByTeamAsync(1, 2, 5, 100, "Office Supplies", DateTime.Today.AddDays(7));

            pushNotificationsMock.Verify(
                p => p.SendWorkflowStatusChangedAsync(
                    1,
                    "New payment request",
                    It.Is<string>(body => body.Contains("Office Supplies") && body.Contains("100")),
                    "/my-team-requests/11"),
                Times.Once);
        }

        [Fact]
        public async Task Create_ShouldNotSendPush_WhenPushDisabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var pushNotificationsMock = new Mock<IPushNotificationService>();

            teamMock.Setup(t => t.GetTeamByIdAsync(5)).ReturnsAsync(new Team { Id = 5 });
            userMock.Setup(u => u.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1, Name = "Alice", Email = "alice@test.com" });
            userMock.Setup(u => u.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2 });
            repoMock.Setup(r => r.AddAsync(It.IsAny<PaymentRequestByTeam>())).ReturnsAsync(new PaymentRequestByTeam { Id = 11 });

            var service = BuildService(
                repoMock,
                teamMock,
                userMock,
                budgetMock,
                creationEmail: false,
                creationSlack: false,
                creationPush: false,
                pushNotificationsMock: pushNotificationsMock);

            await service.CreatePaymentRequestByTeamAsync(1, 2, 5, 100, "Office Supplies", DateTime.Today.AddDays(7));

            pushNotificationsMock.Verify(
                p => p.SendWorkflowStatusChangedAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task MarkAsPaid_ShouldSendSlack_WhenSlackEnabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 5,
                Status = TransactionStatus.Submitted,
                PurposeOfPayment = "Office Supplies",
                Amount = 250m,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(5, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.UpdateAndAddStatusHistoryAsync(It.IsAny<PaymentRequestByTeam>(), It.IsAny<TransactionStatusHistory>()))
                .ReturnsAsync((PaymentRequestByTeam p, TransactionStatusHistory h) => p);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock, confirmationEmail: false, confirmationSlack: true);

            await service.MarkAsPaidAsync(5, 42, null);

            notificationsMock.Verify(
                n => n.SendSlackAsync(
                    "alice@test.com",
                    It.Is<string>(s => s.Contains("Payment Confirmed") && s.Contains("Office Supplies"))),
                Times.Once);

            notificationsMock.Verify(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task MarkAsPaid_ShouldNotSendSlack_WhenSlackDisabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 5,
                Status = TransactionStatus.Submitted,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(5, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.UpdateAndAddStatusHistoryAsync(It.IsAny<PaymentRequestByTeam>(), It.IsAny<TransactionStatusHistory>()))
                .ReturnsAsync((PaymentRequestByTeam p, TransactionStatusHistory h) => p);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock);

            await service.MarkAsPaidAsync(5, 42, null);

            notificationsMock.Verify(n => n.SendSlackAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task MarkAsPaid_ShouldSendPush_WhenPushEnabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var pushNotificationsMock = new Mock<IPushNotificationService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 5,
                UserId = 1,
                Status = TransactionStatus.Submitted,
                PurposeOfPayment = "Office Supplies",
                Amount = 250m,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(5, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.UpdateAndAddStatusHistoryAsync(It.IsAny<PaymentRequestByTeam>(), It.IsAny<TransactionStatusHistory>()))
                .ReturnsAsync((PaymentRequestByTeam p, TransactionStatusHistory h) => p);

            var service = BuildService(
                repoMock,
                teamMock,
                userMock,
                budgetMock,
                confirmationEmail: false,
                confirmationSlack: false,
                pushNotificationsMock: pushNotificationsMock);

            await service.MarkAsPaidAsync(5, 42, null);

            pushNotificationsMock.Verify(
                p => p.SendWorkflowStatusChangedAsync(
                    1,
                    "Payment request paid",
                    It.Is<string>(body => body.Contains("Office Supplies") && body.Contains("250")),
                    "/my-team-requests/5"),
                Times.Once);
        }

        [Fact]
        public async Task MarkAsPaid_ShouldContinue_WhenSlackThrows()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 5,
                Status = TransactionStatus.Submitted,
                PurposeOfPayment = "Office Supplies",
                Amount = 250m,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(5, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.UpdateAndAddStatusHistoryAsync(It.IsAny<PaymentRequestByTeam>(), It.IsAny<TransactionStatusHistory>()))
                .ReturnsAsync((PaymentRequestByTeam p, TransactionStatusHistory h) => p);

            notificationsMock
                .Setup(n => n.SendSlackAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Slack error"));

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock, confirmationEmail: false, confirmationSlack: true);

            var result = await service.MarkAsPaidAsync(5, 42, null);

            result.Status.Should().Be(TransactionStatus.Paid);
        }

        [Fact]
        public async Task MarkAsPaid_ShouldContinue_WhenEmailThrows()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 5,
                Status = TransactionStatus.Submitted,
                PurposeOfPayment = "Office Supplies",
                Amount = 250m,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(5, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.UpdateAndAddStatusHistoryAsync(It.IsAny<PaymentRequestByTeam>(), It.IsAny<TransactionStatusHistory>()))
                .ReturnsAsync((PaymentRequestByTeam p, TransactionStatusHistory h) => p);

            notificationsMock
                .Setup(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("SMTP error"));

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock);

            var result = await service.MarkAsPaidAsync(5, 42, null);

            result.Status.Should().Be(TransactionStatus.Paid);
        }

        // ----------------------------
        // DELETE
        // ----------------------------
        [Fact]
        public async Task Delete_ShouldThrow_WhenNotFound()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync((PaymentRequestByTeam?)null);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

            Func<Task> act = async () => await service.DeletePaymentRequestByTeamAsync(1);

            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("PaymentRequestByTeam could not be found");
        }

        [Theory]
        [InlineData(TransactionStatus.Paid)]
        [InlineData(TransactionStatus.Approved)]
        [InlineData(TransactionStatus.Declined)]
        [InlineData(TransactionStatus.ChangesRequested)]
        public async Task Delete_ShouldThrow_WhenStatusIsNotSubmitted(TransactionStatus status)
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 1,
                Status = status,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

            Func<Task> act = async () => await service.DeletePaymentRequestByTeamAsync(1);

            await act.Should().ThrowAsync<InvalidStateException>();
            repoMock.Verify(r => r.DeletePaymentRequestByTeamAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Delete_ShouldThrow_WhenDeleteReturnsFalse_DueToConcurrentStatusChange()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 7,
                Status = TransactionStatus.Submitted,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(7, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.DeletePaymentRequestByTeamAsync(7))
                .ReturnsAsync(false);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

            Func<Task> act = async () => await service.DeletePaymentRequestByTeamAsync(7);

            await act.Should().ThrowAsync<InvalidStateException>();
        }

        [Fact]
        public async Task Delete_ShouldCallRepository_WhenStatusIsSubmitted()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 7,
                Status = TransactionStatus.Submitted,
                PurposeOfPayment = "Office Supplies",
                Amount = 100m,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(7, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.DeletePaymentRequestByTeamAsync(7))
                .ReturnsAsync(true);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock);

            await service.DeletePaymentRequestByTeamAsync(7);

            repoMock.Verify(r => r.DeletePaymentRequestByTeamAsync(7), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldSendEmail_WhenEmailEnabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 7,
                Status = TransactionStatus.Submitted,
                PurposeOfPayment = "Office Supplies",
                Amount = 100m,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(7, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.DeletePaymentRequestByTeamAsync(7))
                .ReturnsAsync(true);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock);

            await service.DeletePaymentRequestByTeamAsync(7);

            notificationsMock.Verify(
                n => n.SendEmailAsync(
                    "alice@test.com",
                    It.Is<string>(s => s.Contains("Payment Request Deleted") && s.Contains("Office Supplies")),
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldNotSendEmail_WhenEmailDisabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 7,
                Status = TransactionStatus.Submitted,
                PurposeOfPayment = "Office Supplies",
                Amount = 100m,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(7, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.DeletePaymentRequestByTeamAsync(7))
                .ReturnsAsync(true);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock, deletionEmail: false, deletionSlack: false);

            await service.DeletePaymentRequestByTeamAsync(7);

            notificationsMock.Verify(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Delete_ShouldIncludeReason_InEmail_WhenReasonProvided()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 7,
                Status = TransactionStatus.Submitted,
                PurposeOfPayment = "Office Supplies",
                Amount = 100m,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(7, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.DeletePaymentRequestByTeamAsync(7))
                .ReturnsAsync(true);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock);

            await service.DeletePaymentRequestByTeamAsync(7, "Budget cut");

            notificationsMock.Verify(
                n => n.SendEmailAsync(
                    "alice@test.com",
                    It.IsAny<string>(),
                    It.Is<string>(b => b.Contains("Budget cut"))),
                Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldSendPush_WhenPushEnabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var pushNotificationsMock = new Mock<IPushNotificationService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 7,
                UserId = 1,
                Status = TransactionStatus.Submitted,
                PurposeOfPayment = "Office Supplies",
                Amount = 100m,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(7, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.DeletePaymentRequestByTeamAsync(7))
                .ReturnsAsync(true);

            var service = BuildService(
                repoMock,
                teamMock,
                userMock,
                budgetMock,
                deletionEmail: false,
                deletionSlack: false,
                pushNotificationsMock: pushNotificationsMock);

            await service.DeletePaymentRequestByTeamAsync(7);

            pushNotificationsMock.Verify(
                p => p.SendWorkflowStatusChangedAsync(
                    1,
                    "Payment request deleted",
                    It.Is<string>(body => body.Contains("Office Supplies") && body.Contains("100")),
                    "/my-team-requests"),
                Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldContinue_WhenEmailThrows()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 7,
                Status = TransactionStatus.Submitted,
                PurposeOfPayment = "Office Supplies",
                Amount = 100m,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(7, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.DeletePaymentRequestByTeamAsync(7))
                .ReturnsAsync(true);

            notificationsMock
                .Setup(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("SMTP error"));

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock);

            Func<Task> act = async () => await service.DeletePaymentRequestByTeamAsync(7);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Delete_ShouldSendSlack_WhenSlackEnabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 7,
                Status = TransactionStatus.Submitted,
                PurposeOfPayment = "Office Supplies",
                Amount = 100m,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(7, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.DeletePaymentRequestByTeamAsync(7))
                .ReturnsAsync(true);

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock, deletionEmail: false, deletionSlack: true);

            await service.DeletePaymentRequestByTeamAsync(7);

            notificationsMock.Verify(
                n => n.SendSlackAsync(
                    "alice@test.com",
                    It.Is<string>(s => s.Contains("Payment Request Deleted") && s.Contains("Office Supplies"))),
                Times.Once);

            notificationsMock.Verify(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Delete_ShouldContinue_WhenSlackThrows()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var teamMock = new Mock<ITeamService>();
            var userMock = new Mock<IUserService>();
            var budgetMock = new Mock<IBudgetService>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var entity = new PaymentRequestByTeam
            {
                Id = 7,
                Status = TransactionStatus.Submitted,
                PurposeOfPayment = "Office Supplies",
                Amount = 100m,
                User = new User { Id = 1, Name = "Alice", Email = "alice@test.com" },
            };

            repoMock
                .Setup(r => r.GetByIdAsync(7, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            repoMock
                .Setup(r => r.DeletePaymentRequestByTeamAsync(7))
                .ReturnsAsync(true);

            notificationsMock
                .Setup(n => n.SendSlackAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Slack error"));

            var service = BuildService(repoMock, teamMock, userMock, budgetMock, notificationsMock, deletionEmail: false, deletionSlack: true);

            Func<Task> act = async () => await service.DeletePaymentRequestByTeamAsync(7);

            await act.Should().NotThrowAsync();
        }
    }
}
