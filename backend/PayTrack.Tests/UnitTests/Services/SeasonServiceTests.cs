//AI helped with the test cases

using FluentAssertions;
using Moq;
using PayTrack.Application.Dto.Season;
using PayTrack.Application.Services.Implementation;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Tests.UnitTests.Services
{
    public class SeasonServiceTests
    {
        private readonly Mock<ISeasonRepository> repoMock;
        private readonly SeasonService service;

        public SeasonServiceTests()
        {
            this.repoMock = new Mock<ISeasonRepository>();
            this.service = new SeasonService(this.repoMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllSeasons()
        {
            // Arrange
            var seasons = new List<Season>
            {
                new() { Id = 1, Name = "2025" },
                new() { Id = 2, Name = "2026" },
            };
            var query = new GetSeasonQuery { IncludeInactive = true };
            this.repoMock.Setup(r => r.GetAllAsync(query)).ReturnsAsync(seasons);

            // Act
            var result = await this.service.GetAllAsync(query);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainSingle(s => s.Name == "2025");
            result.Should().ContainSingle(s => s.Name == "2026");
            this.repoMock.Verify(r => r.GetAllAsync(query), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnSeason()
        {
            // Arrange
            var season = new Season { Id = 5, Name = "2026" };
            this.repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(season);

            // Act
            var result = await this.service.GetByIdAsync(5);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(5);
            result.Name.Should().Be("2026");
            this.repoMock.Verify(r => r.GetByIdAsync(5), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            this.repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Season?)null);

            // Act
            var result = await this.service.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_ShouldCallRepoAndReturnSeason()
        {
            // Arrange
            this.repoMock.Setup(r => r.AddAsync(It.IsAny<Season>()))
                .ReturnsAsync((Season season) =>
                {
                    season.Id = 1;
                    return season;
                });

            // Act
            var result = await this.service.CreateAsync("2026");

            // Assert
            result.Id.Should().Be(1);
            result.Name.Should().Be("2026");
            this.repoMock.Verify(r => r.AddAsync(It.Is<Season>(s => s.Name == "2026")), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallRepoWithCorrectArgs()
        {
            // Arrange
            var updated = new Season { Id = 3, Name = "2027" };
            this.repoMock.Setup(r => r.UpdateAsync(3, "2027", false)).ReturnsAsync(updated);

            // Act
            var result = await this.service.UpdateAsync(3, "2027", false);

            // Assert
            result.Should().Be(updated);
            this.repoMock.Verify(r => r.UpdateAsync(3, "2027", false), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepo()
        {
            // Arrange
            this.repoMock.Setup(r => r.DeleteAsync(7)).ReturnsAsync((Season?)null);

            // Act
            var result = await this.service.DeleteAsync(7);

            // Assert
            result.Should().BeNull();
            this.repoMock.Verify(r => r.DeleteAsync(7), Times.Once);
        }
    }
}
