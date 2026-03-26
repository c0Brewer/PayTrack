using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PayTrack.Application.Dto.Team;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class TeamEndpointsTests(TeamApiFactory factory) : IClassFixture<TeamApiFactory>
    {
        private readonly TeamApiFactory _factory = factory;

        [Fact]
        public async Task GetTeams_ReturnsOkWithTeams()
        {
            // Arrange
            var teams = new List<Team>
            {
                new() { Id = 1, Name = "Alpha" },
                new() { Id = 2, Name = "Beta" },
            };

            _factory.TeamServiceMock
                .Setup(s => s.GetTeamsAsync())
                .ReturnsAsync(teams);

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/v1/team");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<TeamDto>>();
            result.Should().HaveCount(2);
            result![0].name.Should().Be("Alpha");
            result![1].name.Should().Be("Beta");
        }

        [Fact]
        public async Task GetTeamById_ReturnsOk_WhenTeamExists()
        {
            // Arrange
            var team = new Team { Id = 1, Name = "Team1" };

            _factory.TeamServiceMock
                .Setup(s => s.GetTeamByIdAsync(1))
                .ReturnsAsync(team);

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/v1/team/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TeamDto>();
            result.Should().NotBeNull();
            result!.name.Should().Be("Team1");
        }

        [Fact]
        public async Task GetTeamById_ReturnsNotFound_WhenTeamDoesNotExist()
        {
            // Arrange
            _factory.TeamServiceMock
                .Setup(s => s.GetTeamByIdAsync(999))
                .ReturnsAsync((Team?)null);

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/v1/team/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateTeam_ReturnsOkWithCreatedTeam()
        {
            // Arrange
            var requestDto = new CreateTeamRequestDto("New Team");
            var createdTeam = new Team { Id = 1, Name = "New Team" };

            _factory.TeamServiceMock
                .Setup(s => s.CreateTeamAsync(requestDto.name))
                .ReturnsAsync(createdTeam);

            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("api/v1/team", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TeamDto>();
            result.Should().NotBeNull();
            result!.name.Should().Be("New Team");
        }
    }

    /// <summary>
    /// Uses the real API Program entry point so WebApplicationFactory can
    /// resolve and build the IHost, then replaces ITeamService with a mock.
    /// </summary>
    public class TeamApiFactory : WebApplicationFactory<Program>  // <-- KEY FIX
    {
        public Mock<ITeamService> TeamServiceMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real ITeamService registration coming from Program.cs
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ITeamService));

                if (descriptor is not null)
                    services.Remove(descriptor);

                // Register the mock instead
                services.AddSingleton(TeamServiceMock.Object);
            });
        }
    }
}
