using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Exceptions;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Implementation;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Repositories
{
    public class TeamRepositoryTests
    {
        private static AppDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName + Guid.NewGuid())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddTeamToDatabase()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("AddTeamToDatabase");
            var repo = new TeamRepository(context);
            var team = new Team { Name = "Test Team" };

            // Act
            var result = await repo.AddAsync(team);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Test Team");

            var dbTeam = await context.Teams.FindAsync(result.Id);
            dbTeam.Should().NotBeNull();
            dbTeam.Name.Should().Be("Test Team");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTeam_WhenExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetTeamById");
            var team = new Team { Name = "Existing Team" };
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var repo = new TeamRepository(context);

            // Act
            var result = await repo.GetByIdAsync(team.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(team.Id);
            result.Name.Should().Be("Existing Team");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetTeamByIdWhenNotExists");
            var repo = new TeamRepository(context);

            // Act
            var result = await repo.GetByIdAsync(999); // non-existing ID

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTeams()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllAsync");
            var teams = new List<Team>
            {
                new() { Name = "Team1" },
                new() { Name = "Team2" }
            };
            context.Teams.AddRange(teams);
            await context.SaveChangesAsync();

            var repo = new TeamRepository(context);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainSingle(t => t.Name == "Team1");
            result.Should().ContainSingle(t => t.Name == "Team2");
        }


        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenSaveChangesFails()
        {
            // Arrange
            var failingContext = new FailingDbContext("AddAsyncTeam_FailingDbContext");
            // Override SaveChangesAsync to return 0 to simulate failure

            var repo = new TeamRepository(failingContext);
            var team = new Team { Name = "Fail User" };

            // Act
            var exception = await Assert.ThrowsAsync<InternalErrorException>(
                async () => await repo.AddAsync(team)
            );

            // Assert
            Assert.Contains("teams", exception.Message);
        }
    }
}
