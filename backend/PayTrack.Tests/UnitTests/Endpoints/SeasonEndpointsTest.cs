//AI helped with the test cases

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PayTrack.Application.Dto.Season;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class SeasonEndpointsTests(SeasonApiFactory factory) : IClassFixture<SeasonApiFactory>
    {
        private readonly SeasonApiFactory factory = factory;

        // GET /season

        [Fact]
        public async Task GetSeasons_ReturnsOk_WithJwtOnly()
        {
            // Arrange
            var seasons = new List<Season>
            {
                new() { Id = 1, Name = "2025" },
                new() { Id = 2, Name = "2026" },
            };
            this.factory.ServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(seasons);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/season");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<SeasonDto>>();
            result.Should().HaveCount(2);
            result.Should().ContainSingle(s => s.Name == "2025");
            result.Should().ContainSingle(s => s.Name == "2026");
        }

        // POST /season

        [Fact]
        public async Task CreateSeason_ReturnsOk_WhenAdminRole()
        {
            // Arrange
            var requestDto = new CreateSeasonRequestDto("2026");
            var created = new Season { Id = 3, Name = "2026" };

            this.factory.ServiceMock
                .Setup(s => s.CreateAsync("2026"))
                .ReturnsAsync(created);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.PostAsJsonAsync("api/v1/season", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<SeasonDto>();
            result!.Name.Should().Be("2026");
        }

        [Fact]
        public async Task CreateSeason_ReturnsForbidden_WhenRegularUserRole()
        {
            // Arrange
            var requestDto = new CreateSeasonRequestDto("2026");

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.PostAsJsonAsync("api/v1/season", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // PUT /season/{id}

        [Fact]
        public async Task UpdateSeason_ReturnsOk_WhenAdminRole()
        {
            // Arrange
            var requestDto = new UpdateSeasonRequestDto("2027");
            var updated = new Season { Id = 1, Name = "2027" };

            this.factory.ServiceMock
                .Setup(s => s.UpdateAsync(1, "2027"))
                .ReturnsAsync(updated);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.PutAsJsonAsync("api/v1/season/1", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<SeasonDto>();
            result!.Name.Should().Be("2027");
        }

        [Fact]
        public async Task UpdateSeason_ReturnsForbidden_WhenRegularUserRole()
        {
            // Arrange
            var requestDto = new UpdateSeasonRequestDto("2027");

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.PutAsJsonAsync("api/v1/season/1", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateSeason_ReturnsNotFound_WhenServiceThrowsNotFound()
        {
            // Arrange
            var requestDto = new UpdateSeasonRequestDto("2027");
            this.factory.ServiceMock
                .Setup(s => s.UpdateAsync(999, "2027"))
                .ThrowsAsync(new NotFoundException("Season could not be found."));

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.PutAsJsonAsync("api/v1/season/999", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // DELETE /season/{id}

        [Fact]
        public async Task DeleteSeason_ReturnsNoContent_WhenAdminRole()
        {
            // Arrange
            this.factory.ServiceMock.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.DeleteAsync("api/v1/season/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteSeason_ReturnsForbidden_WhenRegularUserRole()
        {
            // Arrange
            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.DeleteAsync("api/v1/season/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteSeason_ReturnsBadRequest_WhenServiceThrowsInvalidState()
        {
            // Arrange
            this.factory.ServiceMock
                .Setup(s => s.DeleteAsync(2))
                .ThrowsAsync(new InvalidStateException("Season cannot be deleted while budgets are linked."));

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.DeleteAsync("api/v1/season/2");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    public class SeasonApiFactory : WebApplicationFactory<Program>
    {
        public Mock<ISeasonService> ServiceMock { get; } = new();

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
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DynamicTestAuthHandler>("Test", _ => { });

                services.AddAuthorization(_ => { });

                var dbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbDescriptor is not null)
                {
                    services.Remove(dbDescriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("SeasonTestDb"));

                var serviceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ISeasonService));
                if (serviceDescriptor is not null)
                {
                    services.Remove(serviceDescriptor);
                }

                services.AddSingleton(this.ServiceMock.Object);
            });
        }
    }
}
