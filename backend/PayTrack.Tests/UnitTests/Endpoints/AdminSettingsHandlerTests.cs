using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PayTrack.Application.Dto.AdminSettings;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class AdminSettingsHandlerTests(AdminSettingsApiFactory factory) : IClassFixture<AdminSettingsApiFactory>
    {
        private readonly AdminSettingsApiFactory factory = factory;

        // ── GET /admin/settings/csv-columns ───────────────────────────────────────

        [Fact]
        public async Task GetCsvColumns_ReturnsOk_WithDefaults_WhenAdmin()
        {
            this.factory.ServiceMock
                .Setup(s => s.GetCsvColumnSettingsAsync())
                .ReturnsAsync(new CsvColumnSettingsDto("Name", "Summe"));

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var response = await client.GetAsync("api/v1/admin/settings/csv-columns");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<CsvColumnSettingsDto>();
            dto!.NameColumn.Should().Be("Name");
            dto.SummeColumn.Should().Be("Summe");
        }

        [Fact]
        public async Task GetCsvColumns_ReturnsForbidden_WhenNotAdmin()
        {
            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var response = await client.GetAsync("api/v1/admin/settings/csv-columns");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ── PUT /admin/settings/csv-columns ───────────────────────────────────────

        [Fact]
        public async Task UpdateCsvColumns_ReturnsNoContent_WhenAdmin()
        {
            this.factory.ServiceMock
                .Setup(s => s.UpdateCsvColumnSettingsAsync(It.IsAny<UpdateCsvColumnSettingsRequestDto>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var response = await client.PutAsJsonAsync(
                "api/v1/admin/settings/csv-columns",
                new UpdateCsvColumnSettingsRequestDto("Bezeichnung", "Betrag"));

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateCsvColumns_CallsServiceWithCorrectArguments()
        {
            this.factory.ServiceMock.Invocations.Clear();

            this.factory.ServiceMock
                .Setup(s => s.UpdateCsvColumnSettingsAsync(It.IsAny<UpdateCsvColumnSettingsRequestDto>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            await client.PutAsJsonAsync(
                "api/v1/admin/settings/csv-columns",
                new UpdateCsvColumnSettingsRequestDto("Bezeichnung", "Betrag"));

            this.factory.ServiceMock.Verify(
                s => s.UpdateCsvColumnSettingsAsync(
                    It.Is<UpdateCsvColumnSettingsRequestDto>(d => d.NameColumn == "Bezeichnung" && d.SummeColumn == "Betrag"),
                    It.IsAny<int>()),
                Times.Once);
        }

        // ── GET /admin/settings/notification-channels ─────────────────────────────

        [Fact]
        public async Task GetNotificationChannels_ReturnsOk_WhenAdmin()
        {
            this.factory.ServiceMock
                .Setup(s => s.GetNotificationChannelGroupsAsync())
                .ReturnsAsync(new NotificationChannelGroupsDto(
                    new NotificationChannelDto(true, false),
                    new NotificationChannelDto(true, false),
                    new NotificationChannelDto(true, false),
                    new NotificationChannelDto(false, false)));

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var response = await client.GetAsync("api/v1/admin/settings/notification-channels");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ── PUT /admin/settings/notification-channels ─────────────────────────────

        [Fact]
        public async Task UpdateNotificationChannels_ReturnsNoContent_WhenAdmin()
        {
            this.factory.ServiceMock
                .Setup(s => s.UpdateNotificationChannelGroupsAsync(It.IsAny<UpdateNotificationChannelGroupsRequestDto>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var response = await client.PutAsJsonAsync(
                "api/v1/admin/settings/notification-channels",
                new UpdateNotificationChannelGroupsRequestDto(
                    new NotificationChannelDto(true, false),
                    new NotificationChannelDto(true, false),
                    new NotificationChannelDto(true, false),
                    new NotificationChannelDto(false, false)));

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        // ── GET /admin/settings/reminder-schedule ─────────────────────────────────

        [Fact]
        public async Task GetReminderSchedule_ReturnsOk_WhenAdmin()
        {
            this.factory.ServiceMock
                .Setup(s => s.GetReminderScheduleAsync())
                .ReturnsAsync(new ReminderScheduleDto([7, 2, 1], 8, 0, 500));

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var response = await client.GetAsync("api/v1/admin/settings/reminder-schedule");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<ReminderScheduleDto>();
            dto!.RunAtHourUtc.Should().Be(8);
        }

        // ── PUT /admin/settings/reminder-schedule ─────────────────────────────────

        [Fact]
        public async Task UpdateReminderSchedule_ReturnsNoContent_WhenAdmin()
        {
            this.factory.ServiceMock
                .Setup(s => s.UpdateReminderScheduleAsync(It.IsAny<UpdateReminderScheduleRequestDto>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var response = await client.PutAsJsonAsync(
                "api/v1/admin/settings/reminder-schedule",
                new UpdateReminderScheduleRequestDto([7, 2, 1], 10, 0, 500));

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateReminderSchedule_ReturnsBadRequest_WhenHourOutOfRange()
        {
            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var response = await client.PutAsJsonAsync(
                "api/v1/admin/settings/reminder-schedule",
                new UpdateReminderScheduleRequestDto([7], 99, 0, 500));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    public class AdminSettingsApiFactory : WebApplicationFactory<Program>
    {
        public Mock<ISystemSettingService> ServiceMock { get; } = new();
        public Mock<IAuthService> AuthServiceMock { get; } = new();

        public AdminSettingsApiFactory()
        {
            AuthServiceMock.Setup(a => a.GetCurrentUser(null))
                .ReturnsAsync(new User { Id = 1, IsActive = true, Role = Role.Admin });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, DynamicTestAuthHandler>("Test", _ => { });

                services.AddAuthorization(_ => { });

                var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbDescriptor is not null)
                    services.Remove(dbDescriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("AdminSettingsTestDb"));

                var serviceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ISystemSettingService));
                if (serviceDescriptor is not null)
                    services.Remove(serviceDescriptor);

                var authServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAuthService));
                if (authServiceDescriptor is not null)
                    services.Remove(authServiceDescriptor);

                services.AddSingleton(this.ServiceMock.Object);
                services.AddSingleton(this.AuthServiceMock.Object);
            });
        }
    }
}
