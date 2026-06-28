// <copyright file="SystemSettingServiceTests.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using FluentAssertions;
using Moq;
using PayTrack.Application.Dto.AdminSettings;
using PayTrack.Application.Services.Implementation;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Tests.UnitTests.Services
{
    public class SystemSettingServiceTests
    {
        private static SystemSettingService BuildService(Mock<ISystemSettingRepository> repoMock)
        {
            return new SystemSettingService(repoMock.Object);
        }

        // ── GetCsvColumnSettingsAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetCsvColumnSettingsAsync_ShouldReturnDefaults_WhenNoRowsExist()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(It.IsAny<string>())).ReturnsAsync((SystemSetting?)null);

            var service = BuildService(repoMock);
            var result = await service.GetCsvColumnSettingsAsync();

            result.NameColumn.Should().Be("Name");
            result.SummeColumn.Should().Be("Summe");
        }

        [Fact]
        public async Task GetCsvColumnSettingsAsync_ShouldReturnStoredValues_WhenRowsExist()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(SystemSettingKeys.CsvColumnName))
                .ReturnsAsync(new SystemSetting { Key = SystemSettingKeys.CsvColumnName, Value = "Bezeichnung" });
            repoMock.Setup(r => r.GetByKeyAsync(SystemSettingKeys.CsvColumnSumme))
                .ReturnsAsync(new SystemSetting { Key = SystemSettingKeys.CsvColumnSumme, Value = "Betrag" });

            var service = BuildService(repoMock);
            var result = await service.GetCsvColumnSettingsAsync();

            result.NameColumn.Should().Be("Bezeichnung");
            result.SummeColumn.Should().Be("Betrag");
        }

        [Fact]
        public async Task UpdateCsvColumnSettingsAsync_ShouldUpsertBothKeys()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            var service = BuildService(repoMock);

            await service.UpdateCsvColumnSettingsAsync(
                new UpdateCsvColumnSettingsRequestDto("Bezeichnung", "Betrag"), userId: 1);

            repoMock.Verify(r => r.UpsertManyAsync(
                It.Is<IReadOnlyDictionary<string, string>>(d =>
                    d[SystemSettingKeys.CsvColumnName] == "Bezeichnung" &&
                    d[SystemSettingKeys.CsvColumnSumme] == "Betrag"),
                1), Times.Once);
        }

        // ── GetNotificationChannelGroupsAsync ─────────────────────────────────────

        [Fact]
        public async Task GetNotificationChannelGroupsAsync_ShouldReturnHardcodedDefaults_WhenNoRowsExist()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(It.IsAny<string>())).ReturnsAsync((SystemSetting?)null);

            var service = BuildService(repoMock);
            var result = await service.GetNotificationChannelGroupsAsync();

            result.Creation.SendEmail.Should().BeTrue();
            result.Creation.SendSlack.Should().BeFalse();
            result.Creation.SendPush.Should().BeTrue();
            result.Confirmation.SendEmail.Should().BeTrue();
            result.Confirmation.SendSlack.Should().BeFalse();
            result.Confirmation.SendPush.Should().BeTrue();
            result.Reminders.SendEmail.Should().BeTrue();
            result.Reminders.SendSlack.Should().BeFalse();
            result.Reminders.SendPush.Should().BeTrue();
            result.InvoiceApproval.SendEmail.Should().BeTrue();
            result.InvoiceApproval.SendSlack.Should().BeFalse();
            result.InvoiceApproval.SendPush.Should().BeTrue();
            result.InvoiceRejection.SendEmail.Should().BeTrue();
            result.InvoiceRejection.SendSlack.Should().BeFalse();
            result.InvoiceRejection.SendPush.Should().BeTrue();
            result.InvoiceChangesRequested.SendEmail.Should().BeTrue();
            result.InvoiceChangesRequested.SendSlack.Should().BeFalse();
            result.InvoiceChangesRequested.SendPush.Should().BeTrue();
            result.InvoicePaymentCompleted.SendEmail.Should().BeTrue();
            result.InvoicePaymentCompleted.SendSlack.Should().BeFalse();
            result.InvoicePaymentCompleted.SendPush.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateNotificationChannelGroupsAsync_ShouldUpsertAllNotificationKeys()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            var service = BuildService(repoMock);

            var dto = new UpdateNotificationChannelGroupsRequestDto(
                Creation: new NotificationChannelDto(true, false, true),
                Confirmation: new NotificationChannelDto(false, true, false),
                Reminders: new NotificationChannelDto(true, true, true),
                Deletion: new NotificationChannelDto(false, false, false),
                InvoiceApproval: new NotificationChannelDto(true, false, false),
                InvoiceRejection: new NotificationChannelDto(false, true, true),
                InvoiceChangesRequested: new NotificationChannelDto(true, true, false),
                InvoicePaymentCompleted: new NotificationChannelDto(false, false, true));

            await service.UpdateNotificationChannelGroupsAsync(dto, userId: 5);

            repoMock.Verify(r => r.UpsertManyAsync(
                It.Is<IReadOnlyDictionary<string, string>>(d =>
                    d[SystemSettingKeys.NotificationsCreationEmail] == "True" &&
                    d[SystemSettingKeys.NotificationsCreationSlack] == "False" &&
                    d[SystemSettingKeys.NotificationsCreationPush] == "True" &&
                    d[SystemSettingKeys.NotificationsConfirmationEmail] == "False" &&
                    d[SystemSettingKeys.NotificationsConfirmationSlack] == "True" &&
                    d[SystemSettingKeys.NotificationsConfirmationPush] == "False" &&
                    d[SystemSettingKeys.NotificationsRemindersEmail] == "True" &&
                    d[SystemSettingKeys.NotificationsRemindersSlack] == "True" &&
                    d[SystemSettingKeys.NotificationsRemindersPush] == "True" &&
                    d[SystemSettingKeys.NotificationsDeletionEmail] == "False" &&
                    d[SystemSettingKeys.NotificationsDeletionSlack] == "False" &&
                    d[SystemSettingKeys.NotificationsDeletionPush] == "False" &&
                    d[SystemSettingKeys.NotificationsInvoiceApprovalEmail] == "True" &&
                    d[SystemSettingKeys.NotificationsInvoiceApprovalSlack] == "False" &&
                    d[SystemSettingKeys.NotificationsInvoiceApprovalPush] == "False" &&
                    d[SystemSettingKeys.NotificationsInvoiceRejectionEmail] == "False" &&
                    d[SystemSettingKeys.NotificationsInvoiceRejectionSlack] == "True" &&
                    d[SystemSettingKeys.NotificationsInvoiceRejectionPush] == "True" &&
                    d[SystemSettingKeys.NotificationsInvoiceChangesRequestedEmail] == "True" &&
                    d[SystemSettingKeys.NotificationsInvoiceChangesRequestedSlack] == "True" &&
                    d[SystemSettingKeys.NotificationsInvoiceChangesRequestedPush] == "False" &&
                    d[SystemSettingKeys.NotificationsInvoicePaymentCompletedEmail] == "False" &&
                    d[SystemSettingKeys.NotificationsInvoicePaymentCompletedSlack] == "False" &&
                    d[SystemSettingKeys.NotificationsInvoicePaymentCompletedPush] == "True"),
                5), Times.Once);
        }

        // ── GetReminderScheduleAsync ───────────────────────────────────────────────

        [Fact]
        public async Task GetReminderScheduleAsync_ShouldReturnHardcodedDefaults_WhenNoRowsExist()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(It.IsAny<string>())).ReturnsAsync((SystemSetting?)null);

            var service = BuildService(repoMock);
            var result = await service.GetReminderScheduleAsync();

            result.DaysBeforeDue.Should().BeEquivalentTo(new[] { 7, 2, 1 });
            result.RunAtHourUtc.Should().Be(8);
            result.RunAtMinuteUtc.Should().Be(0);
            result.EmailDelayMs.Should().Be(500);
        }

        [Fact]
        public async Task UpdateReminderScheduleAsync_ShouldUpsertAllKeys()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            var service = BuildService(repoMock);

            await service.UpdateReminderScheduleAsync(
                new UpdateReminderScheduleRequestDto([14, 7], 10, 30, 250), userId: 2);

            repoMock.Verify(r => r.UpsertManyAsync(
                It.Is<IReadOnlyDictionary<string, string>>(d =>
                    d[SystemSettingKeys.RemindersDaysBeforeDue] == "14,7" &&
                    d[SystemSettingKeys.RemindersRunAtHourUtc] == "10" &&
                    d[SystemSettingKeys.RemindersRunAtMinuteUtc] == "30" &&
                    d[SystemSettingKeys.RemindersEmailDelayMs] == "250"),
                2), Times.Once);
        }

        // ── GetInvoiceSubmissionSettingsAsync ─────────────────────────────────────

        [Fact]
        public async Task GetInvoiceSubmissionSettingsAsync_ShouldReturnEnabled_WhenNoRowsExist()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(It.IsAny<string>())).ReturnsAsync((SystemSetting?)null);

            var service = BuildService(repoMock);
            var result = await service.GetInvoiceSubmissionSettingsAsync();

            result.ReceiptExtractionEnabled.Should().BeTrue();
        }

        [Fact]
        public async Task GetInvoiceSubmissionSettingsAsync_ShouldReturnStoredValue_WhenRowExists()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(SystemSettingKeys.InvoiceSubmissionReceiptExtractionEnabled))
                .ReturnsAsync(new SystemSetting
                {
                    Key = SystemSettingKeys.InvoiceSubmissionReceiptExtractionEnabled,
                    Value = "False",
                });

            var service = BuildService(repoMock);
            var result = await service.GetInvoiceSubmissionSettingsAsync();

            result.ReceiptExtractionEnabled.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateInvoiceSubmissionSettingsAsync_ShouldUpsertReceiptExtractionKey()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            var service = BuildService(repoMock);

            await service.UpdateInvoiceSubmissionSettingsAsync(
                new UpdateInvoiceSubmissionSettingsRequestDto(false), userId: 3);

            repoMock.Verify(r => r.UpsertManyAsync(
                It.Is<IReadOnlyDictionary<string, string>>(d =>
                    d[SystemSettingKeys.InvoiceSubmissionReceiptExtractionEnabled] == "False"),
                3), Times.Once);
        }

        // ── GetBoolSettingAsync ────────────────────────────────────────────────────

        [Theory]
        [InlineData("True", true)]
        [InlineData("False", false)]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public async Task GetBoolSettingAsync_ShouldParseBoolString(string storedValue, bool expected)
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync("test.key"))
                .ReturnsAsync(new SystemSetting { Key = "test.key", Value = storedValue });

            var service = BuildService(repoMock);
            var result = await service.GetBoolSettingAsync("test.key", defaultValue: false);

            result.Should().Be(expected);
        }

        [Fact]
        public async Task GetBoolSettingAsync_ShouldReturnDefault_WhenNoRow()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(It.IsAny<string>())).ReturnsAsync((SystemSetting?)null);

            var service = BuildService(repoMock);
            var result = await service.GetBoolSettingAsync("test.key", defaultValue: true);

            result.Should().BeTrue();
        }

        // ── GetDaysBeforeDueAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task GetDaysBeforeDueAsync_ShouldParseCsvString()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(SystemSettingKeys.RemindersDaysBeforeDue))
                .ReturnsAsync(new SystemSetting { Key = SystemSettingKeys.RemindersDaysBeforeDue, Value = "14,7,3" });

            var service = BuildService(repoMock);
            var result = await service.GetDaysBeforeDueAsync();

            result.Should().BeEquivalentTo(new[] { 14, 7, 3 });
        }

        [Fact]
        public async Task GetDaysBeforeDueAsync_ShouldReturnHardcodedDefault_WhenNoRow()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(SystemSettingKeys.RemindersDaysBeforeDue))
                .ReturnsAsync((SystemSetting?)null);

            var service = BuildService(repoMock);
            var result = await service.GetDaysBeforeDueAsync();

            result.Should().BeEquivalentTo(new[] { 7, 2, 1 });
        }

        [Fact]
        public async Task GetDaysBeforeDueAsync_ShouldReturnEmptyArray_WhenValueIsEmptyString()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(SystemSettingKeys.RemindersDaysBeforeDue))
                .ReturnsAsync(new SystemSetting { Key = SystemSettingKeys.RemindersDaysBeforeDue, Value = "" });

            var service = BuildService(repoMock);
            var result = await service.GetDaysBeforeDueAsync();

            result.Should().BeEmpty();
        }

        // ── GetRunAtHourUtcAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task GetRunAtHourUtcAsync_ShouldParseIntString()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(SystemSettingKeys.RemindersRunAtHourUtc))
                .ReturnsAsync(new SystemSetting { Key = SystemSettingKeys.RemindersRunAtHourUtc, Value = "10" });

            var service = BuildService(repoMock);
            var result = await service.GetRunAtHourUtcAsync();

            result.Should().Be(10);
        }

        [Fact]
        public async Task GetRunAtHourUtcAsync_ShouldReturnHardcodedDefault_WhenNoRow()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(SystemSettingKeys.RemindersRunAtHourUtc))
                .ReturnsAsync((SystemSetting?)null);

            var service = BuildService(repoMock);
            var result = await service.GetRunAtHourUtcAsync();

            result.Should().Be(8);
        }

        [Theory]
        [InlineData("notanint")]
        [InlineData("99")]
        [InlineData("-1")]
        public async Task GetRunAtHourUtcAsync_ShouldReturnHardcodedDefault_WhenValueIsInvalid(string storedValue)
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(SystemSettingKeys.RemindersRunAtHourUtc))
                .ReturnsAsync(new SystemSetting { Key = SystemSettingKeys.RemindersRunAtHourUtc, Value = storedValue });

            var service = BuildService(repoMock);
            var result = await service.GetRunAtHourUtcAsync();

            result.Should().Be(8);
        }

        // ── GetEmailDelayMsAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task GetEmailDelayMsAsync_ShouldReturnHardcodedDefault_WhenNoRow()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(SystemSettingKeys.RemindersEmailDelayMs))
                .ReturnsAsync((SystemSetting?)null);

            var service = BuildService(repoMock);
            var result = await service.GetEmailDelayMsAsync();

            result.Should().Be(500);
        }

        [Fact]
        public async Task GetEmailDelayMsAsync_ShouldReturnStoredValue_WhenRowExists()
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(SystemSettingKeys.RemindersEmailDelayMs))
                .ReturnsAsync(new SystemSetting { Key = SystemSettingKeys.RemindersEmailDelayMs, Value = "1000" });

            var service = BuildService(repoMock);
            var result = await service.GetEmailDelayMsAsync();

            result.Should().Be(1000);
        }

        [Theory]
        [InlineData("notanint")]
        [InlineData("-1")]
        public async Task GetEmailDelayMsAsync_ShouldReturnHardcodedDefault_WhenValueIsInvalid(string storedValue)
        {
            var repoMock = new Mock<ISystemSettingRepository>();
            repoMock.Setup(r => r.GetByKeyAsync(SystemSettingKeys.RemindersEmailDelayMs))
                .ReturnsAsync(new SystemSetting { Key = SystemSettingKeys.RemindersEmailDelayMs, Value = storedValue });

            var service = BuildService(repoMock);
            var result = await service.GetEmailDelayMsAsync();

            result.Should().Be(500);
        }
    }
}
