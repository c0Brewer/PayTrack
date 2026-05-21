using FluentAssertions;
using PayTrack.Api.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class CostCentreMapperTests
    {
        [Fact]
        public void ToDto_ShouldMapAllFields()
        {
            // Arrange
            var budget = new Budget
            {
                Id = 10,
                Name = "Aero budget",
                TeamId = 2,
                CostCentreId = 1,
                TargetAmount = 1500m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 12, 31),
            };

            var costCentre = new CostCentre
            {
                Id = 1,
                Name = "Aero",
                Description = "Aerodynamics",
                DisplayColor = "#00FF00",
                Budgets = [budget],
            };

            // Act
            var dto = CostCentreMapper.ToDto(costCentre);

            // Assert
            dto.Id.Should().Be(1);
            dto.Name.Should().Be("Aero");
            dto.Description.Should().Be("Aerodynamics");
            dto.DisplayColor.Should().Be("#00FF00");
            dto.Budgets.Should().HaveCount(1);
            dto.Budgets[0].Id.Should().Be(10);
            dto.Budgets[0].TeamId.Should().Be(2);
            dto.Budgets[0].TargetAmount.Should().Be(1500m);
        }

        [Fact]
        public void ToDto_ShouldMapNullableFieldsAsNull()
        {
            // Arrange
            var costCentre = new CostCentre { Id = 2, Name = "Electronics" };

            // Act
            var dto = CostCentreMapper.ToDto(costCentre);

            // Assert
            dto.Description.Should().BeNull();
            dto.DisplayColor.Should().BeNull();
            dto.Budgets.Should().BeEmpty();
        }

        [Fact]
        public void ListToDto_ShouldMapAllEntities()
        {
            // Arrange
            var costCentres = new List<CostCentre>
            {
                new() { Id = 1, Name = "Aero" },
                new() { Id = 2, Name = "Electronics" },
            };

            // Act
            var dtos = CostCentreMapper.ListToDto(costCentres);

            // Assert
            dtos.Should().HaveCount(2);
            dtos.Should().ContainSingle(d => d.Name == "Aero");
            dtos.Should().ContainSingle(d => d.Name == "Electronics");
        }
    }
}
