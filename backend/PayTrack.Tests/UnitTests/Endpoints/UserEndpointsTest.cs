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
using PayTrack.Application.Dto.Pagination;
using PayTrack.Application.Dto.User;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class UserEndpointsTests(UserApiFactory factory) : IClassFixture<UserApiFactory>
    {
        private readonly UserApiFactory _factory = factory;

        [Fact]
        public async Task GetUsers_ReturnsOkWithUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new() { Id = 1, Name = "Alice", Email = "alice@example.com" },
                new() { Id = 2, Name = "Bob", Email = "bob@example.com" },
            };

            _factory.UserServiceMock
                .Setup(s => s.GetAllAsync(It.IsAny<GetUserQuery>()))
                .ReturnsAsync((users, 2));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/user");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<UserDto>>();
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Items[0].Name.Should().Be("Alice");
            result.Items[1].Name.Should().Be("Bob");
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetUserById_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = 1, Name = "Charlie", Email = "charlie@example.com" };
            _factory.UserServiceMock
                .Setup(s => s.GetUserByIdAsync(1, It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync(user);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/user/1?includeTeam=false&includeBankAccounts=false");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<UserDto>();
            result.Should().NotBeNull();
            result.Name.Should().Be("Charlie");
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _factory.UserServiceMock
                .Setup(s => s.GetUserByIdAsync(999))
                .ReturnsAsync((User?)null);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/user/999?includeTeam=false&includeBankAccounts=false");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateUser_ReturnsOkWithUpdatedUser()
        {
            // Arrange
            var updateDto = new UpdateUserDto(
                Name: null,
                Role: Role.Admin,
                IsActive: false,
                TeamId: 1);

            var updatedUser = new User
            {
                Id = 1,
                Name = "Dave",
                Email = "dave@example.com",
                IsActive = false,
                TeamId = 1,
                Role = Role.Admin
            };

            _factory.UserServiceMock
                .Setup(s => s.UpdateUserAsync(1, null, updateDto.IsActive, updateDto.TeamId, updateDto.Role))
                .ReturnsAsync(updatedUser);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.PutAsJsonAsync("api/v1/user/1", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<UserDto>();
            result.Should().NotBeNull();
            result.Name.Should().Be("Dave");
            result.Id.Should().Be(1);
            result.IsActive.Should().BeFalse();
            result.Role.Should().Be(Role.Admin);
        }
    }

    /// <summary>
    /// Uses the real API Program entry point so WebApplicationFactory can
    /// resolve and build the IHost, then replaces IUserService with a mock.
    /// </summary>
    public class UserApiFactory : WebApplicationFactory<Program>
    {
        public Mock<IUserService> UserServiceMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                // Authentication
                services.AddAuthentication("Admin")
                    .AddScheme<AuthenticationSchemeOptions, AdminAuthHandler>("Admin", _ => { });

                _ = services.AddAuthorization(_ => { });

                // DB
                var dbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (dbDescriptor != null)
                    services.Remove(dbDescriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));

                // SERVICE
                var serviceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IUserService));

                if (serviceDescriptor != null)
                    services.Remove(serviceDescriptor);

                services.AddSingleton(UserServiceMock.Object);
            });
        }
    }
}
