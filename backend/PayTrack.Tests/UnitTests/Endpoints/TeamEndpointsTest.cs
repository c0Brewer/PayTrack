using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PayTrack.Application.Dto;
using PayTrack.Application.Services;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class TeamEndpointsTests
    {
        private (WebApplicationFactory<Program> Factory, Mock<ITeamService> ServiceMock) CreateFactoryWithMock()
        {
            var serviceMock = new Mock<ITeamService>();

            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        // Replace the real ITeamService with the mock
                        services.AddSingleton(serviceMock.Object);
                    });
                });

            return (factory, serviceMock);
        }

        [Fact]
        public async Task GetTeams_ReturnsOkWithTeams()
        {
            // Arrange
            var (factory, serviceMock) = CreateFactoryWithMock();
            var teams = new List<Data.Entities.Team>
            {
                new() { Id = 1, Name = "Team1" },
                new() { Id = 2, Name = "Team2" }
            };
            serviceMock.Setup(s => s.GetTeamsAsync())
                       .ReturnsAsync(teams);

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/team/");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<TeamDto>>();
            result.Should().HaveCount(2);
            result[0].name.Should().Be("Team1");
            result[1].name.Should().Be("Team2");
        }

        [Fact]
        public async Task GetTeamById_ReturnsOk_WhenTeamExists()
        {
            // Arrange
            var (factory, serviceMock) = CreateFactoryWithMock();
            var team = new Data.Entities.Team { Id = 1, Name = "Team1" };
            serviceMock.Setup(s => s.GetTeamByIdAsync(1))
                       .ReturnsAsync(team);

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/team/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TeamDto>();
            result.Should().NotBeNull();
            result.name.Should().Be("Team1");
        }

        [Fact]
        public async Task GetTeamById_ReturnsNotFound_WhenTeamDoesNotExist()
        {
            // Arrange
            var (factory, serviceMock) = CreateFactoryWithMock();
            serviceMock.Setup(s => s.GetTeamByIdAsync(999))
                       .ReturnsAsync((Data.Entities.Team?)null);

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/team/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateTeam_ReturnsOkWithCreatedTeam()
        {
            // Arrange
            var (factory, serviceMock) = CreateFactoryWithMock();
            var requestDto = new CreateTeamRequestDto("New Team");
            var createdTeam = new Data.Entities.Team { Id = 1, Name = "New Team" };
            serviceMock.Setup(s => s.CreateTeamAsync(requestDto.name))
                       .ReturnsAsync(createdTeam);

            var client = factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/team/", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TeamDto>();
            result.Should().NotBeNull();
            result.name.Should().Be("New Team");
        }
    }
}
