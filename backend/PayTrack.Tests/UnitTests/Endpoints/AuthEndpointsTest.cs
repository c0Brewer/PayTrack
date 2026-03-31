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
using PayTrack.Application.Dto.Auth;
using PayTrack.Application.Dto.User;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class AuthEndpointsTest(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
    {
        private readonly AuthApiFactory _factory = factory;

        [Fact]
        public async Task GoogleAuthCallback_ReturnsJwtToken()
        {
            // Arrange
            var googleCallback = new GoogleAuthCallbackDto(IdToken: "123");
            const string jwtToken = "123";

            _factory.AuthServiceMock
                .Setup(s => s.GoogleValidateCallback(googleCallback))
                .ReturnsAsync(jwtToken);

            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("api/v1/auth/google", googleCallback);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<GoogleAuthResponseDto>();
            result.Should().NotBeNull();
            result.JwtToken.Should().Be(jwtToken);
        }

        [Fact]
        public async Task GoogleAuthCallback_ReturnsInternalError_WhenServiceThrows()
        {
            // Arrange
            var googleCallback = new GoogleAuthCallbackDto(IdToken: "123");

            _factory.AuthServiceMock
                .Setup(s => s.GoogleValidateCallback(googleCallback))
                .ThrowsAsync(new InternalErrorException("Simulated failure"));

            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("api/v1/auth/google", googleCallback);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

            var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            result.Should().NotBeNull();
            result.Detail.Should().Be("Simulated failure");
        }

        [Fact]
        public async Task GetCurrentUser_ReturnsCurrentUser()
        {
            // Arrange
            var currentUser = new User { Id = 1, Name = "Name", Email = "Email", IsActive = true, ProfilePictureUrl = "123", Role = Role.RegularUser };

            _factory.AuthServiceMock
                .Setup(s => s.GetCurrentUser())
                .ReturnsAsync(currentUser);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/auth/currentuser");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<UserDto>();
            result.Should().NotBeNull();
            result.Id.Should().Be(currentUser.Id);
            result.Name.Should().Be(currentUser.Name);
            result.Email.Should().Be(currentUser.Email);
            result.IsActive.Should().Be(currentUser.IsActive);
            result.ProfilePictureUrl.Should().Be(currentUser.ProfilePictureUrl);
            result.Role.Should().Be(currentUser.Role);
        }

        [Fact]
        public async Task GetCurrentUser_ShouldReturnProblemDetails_WhenUserNotFound()
        {
            // Arrange
            _factory.AuthServiceMock
                .Setup(s => s.GetCurrentUser())
                .ReturnsAsync((User?)null);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/auth/currentuser");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            result.Should().NotBeNull();
            result.Detail.Should().Be("Current User not found");
        }
    }

    /// <summary>
    /// Uses the real API Program entry point so WebApplicationFactory can
    /// resolve and build the IHost, then replaces ITeamService with a mock.
    /// </summary>
    public class AuthApiFactory : WebApplicationFactory<Program>  // <-- KEY FIX
    {
        public Mock<IAuthService> AuthServiceMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                // Authentication

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                _ = services.AddAuthorization(_ => { });

                // DB

                // Remove real DbContext (prevents Postgres connection)
                var dbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (dbDescriptor is not null)
                    services.Remove(dbDescriptor);

                // Replace with in-memory DB (no connection needed)
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));


                // SERVICE

                // Remove the real ITeamService registration coming from Program.cs
                var serviceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IAuthService));

                if (serviceDescriptor is not null)
                    services.Remove(serviceDescriptor);

                // Register the mock instead
                services.AddSingleton(AuthServiceMock.Object);
            });
        }
    }
}
