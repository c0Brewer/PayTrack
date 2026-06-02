using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PayTrack.Application.Services.Implementation;
using PayTrack.Application.Services.Model;
using PayTrack.Application.Settings;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Tests.UnitTests.Services
{
    public class PaymentReminderHostedServiceTests
    {
        private static PaymentReminderHostedService BuildService(
            Mock<ITransactionRepository> repoMock,
            Mock<INotificationDispatchService> notificationsMock,
            int[] daysBeforeDue)
        {
            var scope = new Mock<IServiceScope>();
            scope.Setup(s => s.ServiceProvider.GetService(typeof(ITransactionRepository))).Returns(repoMock.Object);
            scope.Setup(s => s.ServiceProvider.GetService(typeof(INotificationDispatchService))).Returns(notificationsMock.Object);

            var scopeFactory = new Mock<IServiceScopeFactory>();
            scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

            var settings = Options.Create(new ReminderSettings { DaysBeforeDue = daysBeforeDue });
            var logger = new Mock<ILogger<PaymentReminderHostedService>>();

            return new PaymentReminderHostedService(scopeFactory.Object, settings, logger.Object);
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
    }
}
