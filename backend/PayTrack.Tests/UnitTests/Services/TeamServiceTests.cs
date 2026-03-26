using Moq;
using FluentAssertions;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;
using PayTrack.Application.Services.Implementation;

namespace PayTrack.Tests.UnitTests.Services
{
    public class TeamServiceTests
    {
        private readonly Mock<ITeamRepository> repoMock;
        private readonly TeamService service;

        public TeamServiceTests()
        {
            repoMock = new Mock<ITeamRepository>();
            service = new TeamService(repoMock.Object);
        }

        [Fact]
        public async Task CreateTeamAsync_ShouldCallRepoAndReturnTeam()
        {
            // Arrange
            const string teamName = "My Team";
            const string teamDescription = "My Description";
            const string teamColor = "My Color";
            var expectedTeam = new Team { Name = teamName, Description = teamDescription, DisplayColor = teamColor };
            repoMock.Setup(r => r.AddAsync(It.IsAny<Team>()))
                    .ReturnsAsync((Team t) => t);

            // Act
            var result = await service.CreateTeamAsync(teamName, teamDescription, teamColor);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(teamName);
            result.Description.Should().Be(teamDescription);
            result.DisplayColor.Should().Be(teamColor);
            repoMock.Verify(r => r.AddAsync(It.Is<Team>(t => t.Name == teamName)), Times.Once);
        }

        [Fact]
        public async Task GetTeamByIdAsync_ShouldReturnTeamFromRepo()
        {
            // Arrange
            const int teamId = 42;
            var expectedTeam = new Team { Id = teamId, Name = "Team42" };
            repoMock.Setup(r => r.GetByIdAsync(teamId))
                    .ReturnsAsync(expectedTeam);

            // Act
            var result = await service.GetTeamByIdAsync(teamId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(teamId);
            result.Name.Should().Be("Team42");
        }

        [Fact]
        public async Task GetTeamsAsync_ShouldReturnAllTeams()
        {
            // Arrange
            var teams = new List<Team>
            {
                new() { Id = 1, Name = "Team1" },
                new() { Id = 2, Name = "Team2" }
            };
            repoMock.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(teams);

            // Act
            var result = await service.GetTeamsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainSingle(t => t.Name == "Team1");
            result.Should().ContainSingle(t => t.Name == "Team2");
        }
    }
}
