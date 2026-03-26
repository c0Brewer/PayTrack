using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Implementation;

namespace PayTrack.Tests.UnitTests.Repositories
{
    public class TeamRepositoryTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddTeamToDatabase()
        {
            // Arrange
            await using var context = GetInMemoryDbContext();
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
            await using var context = GetInMemoryDbContext();
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
            await using var context = GetInMemoryDbContext();
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
            await using var context = GetInMemoryDbContext();
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
    }
}
