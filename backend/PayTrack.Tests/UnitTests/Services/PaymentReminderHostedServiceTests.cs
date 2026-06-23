// <copyright file="PaymentReminderHostedServiceTests.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PayTrack.Application.Services.Implementation;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Tests.UnitTests.Services
{
    public class PaymentReminderHostedServiceTests
    {
        private static PaymentReminderHostedService BuildService(
            Mock<ITransactionRepository> repoMock,
            Mock<INotificationDispatchService> notificationsMock,
            int[] daysBeforeDue,
            bool sendEmail = true,
            bool sendSlack = false,
            bool sendPush = true,
            Mock<IPushNotificationService>? pushNotificationsMock = null)
        {
            var systemSettingsMock = new Mock<ISystemSettingService>();
            systemSettingsMock
                .Setup(s => s.GetBoolSettingAsync(SystemSettingKeys.NotificationsRemindersEmail, It.IsAny<bool>()))
                .ReturnsAsync(sendEmail);
            systemSettingsMock
                .Setup(s => s.GetBoolSettingAsync(SystemSettingKeys.NotificationsRemindersSlack, It.IsAny<bool>()))
                .ReturnsAsync(sendSlack);
            systemSettingsMock
                .Setup(s => s.GetBoolSettingAsync(SystemSettingKeys.NotificationsRemindersPush, It.IsAny<bool>()))
                .ReturnsAsync(sendPush);
            systemSettingsMock
                .Setup(s => s.GetDaysBeforeDueAsync())
                .ReturnsAsync(daysBeforeDue);
            systemSettingsMock
                .Setup(s => s.GetRunAtHourUtcAsync())
                .ReturnsAsync(8);
            systemSettingsMock
                .Setup(s => s.GetEmailDelayMsAsync())
                .ReturnsAsync(0);

            var scope = new Mock<IServiceScope>();
            scope.Setup(s => s.ServiceProvider.GetService(typeof(ITransactionRepository))).Returns(repoMock.Object);
            scope.Setup(s => s.ServiceProvider.GetService(typeof(INotificationDispatchService))).Returns(notificationsMock.Object);
            scope.Setup(s => s.ServiceProvider.GetService(typeof(ISystemSettingService))).Returns(systemSettingsMock.Object);
            if (pushNotificationsMock is not null)
            {
                scope.Setup(s => s.ServiceProvider.GetService(typeof(IPushNotificationService))).Returns(pushNotificationsMock.Object);
            }

            var scopeFactory = new Mock<IServiceScopeFactory>();
            scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

            var logger = new Mock<ILogger<PaymentReminderHostedService>>();

            return new PaymentReminderHostedService(scopeFactory.Object, logger.Object);
        }

        [Fact]
        public async Task SendRemindersAsync_ShouldNotSendEmail_WhenNoTransactionsDue()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            repoMock
                .Setup(r => r.GetPaymentRequestsByTeamDueOnAsync(It.IsAny<DateTime>()))
                .ReturnsAsync([]);

            var service = BuildService(repoMock, notificationsMock, [7]);

            await service.SendRemindersAsync(CancellationToken.None);

            notificationsMock.Verify(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendRemindersAsync_ShouldSendEmail_ForEachDueTransaction()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var transactions = new List<PaymentRequestByTeam>
            {
                new() { Id = 1, PurposeOfPayment = "Bills", Amount = 100m, DueDate = DateTime.Today.AddDays(7), User = new User { Name = "Alice", Email = "alice@test.com" } },
                new() { Id = 2, PurposeOfPayment = "Rent", Amount = 500m, DueDate = DateTime.Today.AddDays(7), User = new User { Name = "Bob", Email = "bob@test.com" } },
            };

            repoMock
                .Setup(r => r.GetPaymentRequestsByTeamDueOnAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(transactions);

            var service = BuildService(repoMock, notificationsMock, [7]);

            await service.SendRemindersAsync(CancellationToken.None);

            notificationsMock.Verify(n => n.SendEmailAsync(
                "alice@test.com",
                It.Is<string>(s => s.Contains("Payment Reminder") && s.Contains("Bills") && s.Contains("7 day(s)")),
                It.IsAny<string>()), Times.Once);

            notificationsMock.Verify(n => n.SendEmailAsync(
                "bob@test.com",
                It.Is<string>(s => s.Contains("Payment Reminder") && s.Contains("Rent")),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SendRemindersAsync_ShouldQueryOncePerDayInterval()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            repoMock
                .Setup(r => r.GetPaymentRequestsByTeamDueOnAsync(It.IsAny<DateTime>()))
                .ReturnsAsync([]);

            var service = BuildService(repoMock, notificationsMock, [7, 2, 1]);

            await service.SendRemindersAsync(CancellationToken.None);

            repoMock.Verify(r => r.GetPaymentRequestsByTeamDueOnAsync(It.IsAny<DateTime>()), Times.Exactly(3));
        }

        [Fact]
        public async Task SendRemindersAsync_ShouldContinue_WhenRepositoryThrows()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            repoMock
                .Setup(r => r.GetPaymentRequestsByTeamDueOnAsync(It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("DB error"));

            var service = BuildService(repoMock, notificationsMock, [7, 2, 1]);

            Func<Task> act = async () => await service.SendRemindersAsync(CancellationToken.None);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendRemindersAsync_ShouldContinue_WhenSendingOneEmailFails()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var transactions = new List<PaymentRequestByTeam>
            {
                new() { Id = 1, PurposeOfPayment = "Bills", Amount = 100m, DueDate = DateTime.Today.AddDays(7), User = new User { Name = "Alice", Email = "alice@test.com" } },
                new() { Id = 2, PurposeOfPayment = "Rent", Amount = 500m, DueDate = DateTime.Today.AddDays(7), User = new User { Name = "Bob", Email = "bob@test.com" } },
            };

            repoMock
                .Setup(r => r.GetPaymentRequestsByTeamDueOnAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(transactions);

            notificationsMock
                .Setup(n => n.SendEmailAsync("alice@test.com", It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP error"));

            var service = BuildService(repoMock, notificationsMock, [7]);

            Func<Task> act = async () => await service.SendRemindersAsync(CancellationToken.None);

            await act.Should().NotThrowAsync();

            notificationsMock.Verify(n => n.SendEmailAsync("bob@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SendRemindersAsync_ShouldSendSlack_WhenSlackEnabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var transactions = new List<PaymentRequestByTeam>
            {
                new() { Id = 1, PurposeOfPayment = "Bills", Amount = 100m, DueDate = DateTime.Today.AddDays(7), User = new User { Name = "Alice", Email = "alice@test.com" } },
            };

            repoMock
                .Setup(r => r.GetPaymentRequestsByTeamDueOnAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(transactions);

            var service = BuildService(repoMock, notificationsMock, [7], sendEmail: false, sendSlack: true);

            await service.SendRemindersAsync(CancellationToken.None);

            notificationsMock.Verify(n => n.SendSlackAsync(
                "alice@test.com",
                It.Is<string>(s => s.Contains("Payment Reminder") && s.Contains("Bills") && s.Contains("7 day(s)"))),
                Times.Once);

            notificationsMock.Verify(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendRemindersAsync_ShouldNotSendSlack_WhenSlackDisabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var transactions = new List<PaymentRequestByTeam>
            {
                new() { Id = 1, PurposeOfPayment = "Bills", Amount = 100m, DueDate = DateTime.Today.AddDays(7), User = new User { Name = "Alice", Email = "alice@test.com" } },
            };

            repoMock
                .Setup(r => r.GetPaymentRequestsByTeamDueOnAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(transactions);

            var service = BuildService(repoMock, notificationsMock, [7]);

            await service.SendRemindersAsync(CancellationToken.None);

            notificationsMock.Verify(n => n.SendSlackAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendRemindersAsync_ShouldNotSendEmail_WhenEmailDisabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var transactions = new List<PaymentRequestByTeam>
            {
                new() { Id = 1, PurposeOfPayment = "Bills", Amount = 100m, DueDate = DateTime.Today.AddDays(7), User = new User { Name = "Alice", Email = "alice@test.com" } },
            };

            repoMock
                .Setup(r => r.GetPaymentRequestsByTeamDueOnAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(transactions);

            var service = BuildService(repoMock, notificationsMock, [7], sendEmail: false, sendSlack: false);

            await service.SendRemindersAsync(CancellationToken.None);

            notificationsMock.Verify(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendRemindersAsync_ShouldSendPush_WhenPushEnabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var notificationsMock = new Mock<INotificationDispatchService>();
            var pushNotificationsMock = new Mock<IPushNotificationService>();

            var transactions = new List<PaymentRequestByTeam>
            {
                new()
                {
                    Id = 1,
                    UserId = 9,
                    PurposeOfPayment = "Bills",
                    Amount = 100m,
                    DueDate = DateTime.Today.AddDays(7),
                    User = new User { Id = 9, Name = "Alice", Email = "alice@test.com" },
                },
            };

            repoMock
                .Setup(r => r.GetPaymentRequestsByTeamDueOnAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(transactions);

            var service = BuildService(
                repoMock,
                notificationsMock,
                [7],
                sendEmail: false,
                sendSlack: false,
                sendPush: true,
                pushNotificationsMock);

            await service.SendRemindersAsync(CancellationToken.None);

            pushNotificationsMock.Verify(
                p => p.SendWorkflowStatusChangedAsync(
                    9,
                    "Payment reminder",
                    It.Is<string>(body => body.Contains("Bills") && body.Contains("100")),
                    "/my-team-requests/1"),
                Times.Once);
        }

        [Fact]
        public async Task SendRemindersAsync_ShouldNotSendPush_WhenPushDisabled()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var notificationsMock = new Mock<INotificationDispatchService>();
            var pushNotificationsMock = new Mock<IPushNotificationService>();

            var transactions = new List<PaymentRequestByTeam>
            {
                new()
                {
                    Id = 1,
                    UserId = 9,
                    PurposeOfPayment = "Bills",
                    Amount = 100m,
                    DueDate = DateTime.Today.AddDays(7),
                    User = new User { Id = 9, Name = "Alice", Email = "alice@test.com" },
                },
            };

            repoMock
                .Setup(r => r.GetPaymentRequestsByTeamDueOnAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(transactions);

            var service = BuildService(
                repoMock,
                notificationsMock,
                [7],
                sendEmail: false,
                sendSlack: false,
                sendPush: false,
                pushNotificationsMock);

            await service.SendRemindersAsync(CancellationToken.None);

            pushNotificationsMock.Verify(
                p => p.SendWorkflowStatusChangedAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task SendRemindersAsync_ShouldContinue_WhenSlackThrows()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var notificationsMock = new Mock<INotificationDispatchService>();

            var transactions = new List<PaymentRequestByTeam>
            {
                new() { Id = 1, PurposeOfPayment = "Bills", Amount = 100m, DueDate = DateTime.Today.AddDays(7), User = new User { Name = "Alice", Email = "alice@test.com" } },
                new() { Id = 2, PurposeOfPayment = "Rent", Amount = 500m, DueDate = DateTime.Today.AddDays(7), User = new User { Name = "Bob", Email = "bob@test.com" } },
            };

            repoMock
                .Setup(r => r.GetPaymentRequestsByTeamDueOnAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(transactions);

            notificationsMock
                .Setup(n => n.SendSlackAsync("alice@test.com", It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Slack error"));

            var service = BuildService(repoMock, notificationsMock, [7], sendEmail: false, sendSlack: true);

            Func<Task> act = async () => await service.SendRemindersAsync(CancellationToken.None);

            await act.Should().NotThrowAsync();

            notificationsMock.Verify(n => n.SendSlackAsync("bob@test.com", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SendRemindersAsync_ShouldContinue_WhenPushThrows()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var notificationsMock = new Mock<INotificationDispatchService>();
            var pushNotificationsMock = new Mock<IPushNotificationService>();

            var transactions = new List<PaymentRequestByTeam>
            {
                new()
                {
                    Id = 1,
                    UserId = 9,
                    PurposeOfPayment = "Bills",
                    Amount = 100m,
                    DueDate = DateTime.Today.AddDays(7),
                    User = new User { Id = 9, Name = "Alice", Email = "alice@test.com" },
                },
                new()
                {
                    Id = 2,
                    UserId = 10,
                    PurposeOfPayment = "Rent",
                    Amount = 500m,
                    DueDate = DateTime.Today.AddDays(7),
                    User = new User { Id = 10, Name = "Bob", Email = "bob@test.com" },
                },
            };

            repoMock
                .Setup(r => r.GetPaymentRequestsByTeamDueOnAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(transactions);

            pushNotificationsMock
                .Setup(p => p.SendWorkflowStatusChangedAsync(9, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Push error"));

            var service = BuildService(
                repoMock,
                notificationsMock,
                [7],
                sendEmail: false,
                sendSlack: false,
                sendPush: true,
                pushNotificationsMock);

            Func<Task> act = async () => await service.SendRemindersAsync(CancellationToken.None);

            await act.Should().NotThrowAsync();

            pushNotificationsMock.Verify(
                p => p.SendWorkflowStatusChangedAsync(10, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}
