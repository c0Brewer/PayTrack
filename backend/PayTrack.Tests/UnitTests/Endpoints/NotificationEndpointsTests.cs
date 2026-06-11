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
using PayTrack.Application.Dto.Notification;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class NotificationEndpointsTests(NotificationApiFactory factory) : IClassFixture<NotificationApiFactory>
    {
        private readonly NotificationApiFactory factory = factory;

        // ── POST /notification/email ────────────────────────────────────────────────

        [Fact]
        public async Task SendEmail_ReturnsOk_WhenAdmin()
        {
            this.factory.ServiceMock
                .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var response = await client.PostAsJsonAsync(
                "api/v1/notification/email",
                new SendEmailNotificationDto("user@example.com", "Subject", "Body"));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task SendEmail_CallsServiceWithCorrectArguments()
        {
            this.factory.ServiceMock
                .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            await client.PostAsJsonAsync(
                "api/v1/notification/email",
                new SendEmailNotificationDto("user@example.com", "Test Subject", "Test Body"));

            this.factory.ServiceMock.Verify(
                s => s.SendEmailAsync("user@example.com", "Test Subject", "Test Body"),
                Times.Once);
        }

        [Fact]
        public async Task SendEmail_ReturnsForbidden_WhenNotAdmin()
        {
            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var response = await client.PostAsJsonAsync(
                "api/v1/notification/email",
                new SendEmailNotificationDto("user@example.com", "Subject", "Body"));

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ── POST /notification/slack ────────────────────────────────────────────────

        [Fact]
        public async Task SendSlack_ReturnsOk_WhenAdmin()
        {
            this.factory.ServiceMock
                .Setup(s => s.SendSlackAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var response = await client.PostAsJsonAsync(
                "api/v1/notification/slack",
                new SendSlackNotificationDto("user@example.com", "Hello"));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task SendSlack_CallsServiceWithCorrectArguments()
        {
            this.factory.ServiceMock
                .Setup(s => s.SendSlackAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            await client.PostAsJsonAsync(
                "api/v1/notification/slack",
                new SendSlackNotificationDto("user@example.com", "Hello there"));

            this.factory.ServiceMock.Verify(
                s => s.SendSlackAsync("user@example.com", "Hello there"),
                Times.Once);
        }

        [Fact]
        public async Task SendSlack_ReturnsForbidden_WhenNotAdmin()
        {
            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var response = await client.PostAsJsonAsync(
                "api/v1/notification/slack",
                new SendSlackNotificationDto("user@example.com", "Hello"));

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }

    public class NotificationApiFactory : WebApplicationFactory<Program>
    {
        public Mock<INotificationDispatchService> ServiceMock { get; } = new();
        public Mock<IAuthService> AuthServiceMock { get; } = new();

        public NotificationApiFactory()
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
                    options.UseInMemoryDatabase("NotificationTestDb"));

                var serviceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(INotificationDispatchService));
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
