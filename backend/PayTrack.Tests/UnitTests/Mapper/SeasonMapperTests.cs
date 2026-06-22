//AI helped with the test cases

using FluentAssertions;
using PayTrack.Api.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class SeasonMapperTests
    {
        [Fact]
        public void ToDto_ShouldMapAllFields()
        {
            // Arrange
            var budget = new Budget
            {
                Id = 10,
                Name = "Season budget",
                TeamId = 2,
                CostCentreId = 1,
                SeasonId = 3,
                TargetAmount = 1500m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 12, 31),
            };

            var season = new Season
            {
                Id = 3,
                Name = "2026",
                Budgets = [budget],
            };

            // Act
            var dto = SeasonMapper.ToDto(season);

            // Assert
            dto.Id.Should().Be(3);
            dto.Name.Should().Be("2026");
            dto.IsActive.Should().BeTrue();
            dto.Budgets.Should().HaveCount(1);
            dto.Budgets![0].Id.Should().Be(10);
            dto.Budgets[0].Name.Should().Be("Season budget");
            dto.Budgets[0].SeasonId.Should().Be(3);
        }

        [Fact]
        public void ToDto_ShouldMapEmptyBudgets()
        {
            // Arrange
            var season = new Season { Id = 4, Name = "2027", IsActive = false };

            // Act
            var dto = SeasonMapper.ToDto(season);

            // Assert
            dto.Id.Should().Be(4);
            dto.Name.Should().Be("2027");
            dto.IsActive.Should().BeFalse();
            dto.Budgets.Should().BeEmpty();
        }

        [Fact]
        public void ListToDto_ShouldMapAllEntities()
        {
            // Arrange
            var seasons = new List<Season>
            {
                new() { Id = 1, Name = "2025" },
                new() { Id = 2, Name = "2026" },
            };

            // Act
            var dtos = SeasonMapper.ListToDto(seasons);

            // Assert
            dtos.Should().HaveCount(2);
            dtos.Should().ContainSingle(d => d.Name == "2025");
            dtos.Should().ContainSingle(d => d.Name == "2026");
        }
    }
}
