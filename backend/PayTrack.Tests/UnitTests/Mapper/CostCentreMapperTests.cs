using FluentAssertions;
using PayTrack.Api.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class CostCentreMapperTests
    {
        [Theory]
        [InlineData(1, "CC1", "Desc1", "#FFFFFF")]
        [InlineData(42, "Marketing", "Marketing costs", "#FF0000")]
        [InlineData(999, "IT", "Tech stuff", "#00FF00")]
        public void MapperToDto_ReturnsCorrectResult(int id, string name, string description, string color)
        {
            // Arrange
            CostCentre costCentre = new()
            {
                Id = id,
                Name = name,
                Description = description,
                DisplayColor = color
            };

            // Act
            var dto = CostCentreMapper.ToDto(costCentre);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(id);
            dto.Name.Should().Be(name);
            dto.Description.Should().Be(description);
            dto.DisplayColor.Should().Be(color);
        }

        [Fact]
        public void MapperListToDto_ReturnsCorrectResult()
        {
            // Arrange
            var list = new List<CostCentre>
            {
                new() { Id = 1, Name = "A", Description = "Desc A", DisplayColor = "#111111" },
                new() { Id = 2, Name = "B", Description = "Desc B", DisplayColor = "#222222" },
                new() { Id = 3, Name = "C", Description = "Desc C", DisplayColor = "#333333" }
            };

            // Act
            var result = CostCentreMapper.ListToDto(list);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                result[i].Id.Should().Be(list[i].Id);
                result[i].Name.Should().Be(list[i].Name);
                result[i].Description.Should().Be(list[i].Description);
                result[i].DisplayColor.Should().Be(list[i].DisplayColor);
            }
        }
    }
}
