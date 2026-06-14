using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PayTrack.Application.Dto.Dashboard;
using PayTrack.Application.Dto.User;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class DashboardEndpointsTests(DashboardApiFactory factory) : IClassFixture<DashboardApiFactory>
    {
        private readonly DashboardApiFactory _factory = factory;

        [Fact]
        public async Task GetHomeDashboard_ReturnsOk()
        {
            // Arrange
            _factory.AuthServiceMock.Reset();
            _factory.HomeDashboardServiceMock.Reset();

            var currentUser = new User
            {
                Id = 1,
                Name = "Alex",
                Email = "alex@example.com",
                Role = Role.RegularUser,
                IsActive = true,
                ProfilePictureUrl = "profile",
            };

            var dashboard = new HomeDashboardDto(
                new HomeDashboardUserDto(1, "Alex", Role.RegularUser),
                new HomeDashboardSectionDto(1, 2, 3, 100m, null, []),
                new HomeDashboardSectionDto(4, 5, 6, 200m, null, []),
                new HomeDashboardActionsDto(true, false, 2));

            _factory.AuthServiceMock
                .Setup(service => service.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync(currentUser);

            _factory.HomeDashboardServiceMock
                .Setup(service => service.GetHomeDashboardAsync(currentUser))
                .ReturnsAsync(dashboard);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/dashboard/home");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<HomeDashboardDto>();
            result.Should().NotBeNull();
            result!.User.Name.Should().Be("Alex");
            result.Actions.MissingBankAccount.Should().BeTrue();
            result.PaymentRequests.OpenCount.Should().Be(4);

            _factory.AuthServiceMock.Verify(
                service => service.GetCurrentUser(It.Is<GetUserQueryById?>(query => query!.IncludeBankAccounts == true)),
                Times.Once);
            _factory.HomeDashboardServiceMock.Verify(service => service.GetHomeDashboardAsync(currentUser), Times.Once);
        }

        [Fact]
        public async Task GetHomeDashboard_ReturnsNotFound_WhenCurrentUserIsMissing()
        {
            // Arrange
            _factory.AuthServiceMock.Reset();
            _factory.HomeDashboardServiceMock.Reset();

            _factory.AuthServiceMock
                .Setup(service => service.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync((User?)null);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/dashboard/home");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            result.Should().NotBeNull();
            result!.Detail.Should().Be("Current user not found");

            _factory.HomeDashboardServiceMock.Verify(service => service.GetHomeDashboardAsync(It.IsAny<User>()), Times.Never);
        }
    }

    public class DashboardApiFactory : WebApplicationFactory<Program>
    {
        public Mock<IAuthService> AuthServiceMock { get; } = new();
        public Mock<IHomeDashboardService> HomeDashboardServiceMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                _ = services.AddAuthorization(_ => { });

                var dbDescriptor = services.SingleOrDefault(
                    descriptor => descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (dbDescriptor is not null)
                {
                    services.Remove(dbDescriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("DashboardTestDb"));

                var authDescriptor = services.SingleOrDefault(
                    descriptor => descriptor.ServiceType == typeof(IAuthService));

                if (authDescriptor is not null)
                {
                    services.Remove(authDescriptor);
                }

                var dashboardDescriptor = services.SingleOrDefault(
                    descriptor => descriptor.ServiceType == typeof(IHomeDashboardService));

                if (dashboardDescriptor is not null)
                {
                    services.Remove(dashboardDescriptor);
                }

                services.AddSingleton(AuthServiceMock.Object);
                services.AddSingleton(HomeDashboardServiceMock.Object);
            });
        }
    }
}
