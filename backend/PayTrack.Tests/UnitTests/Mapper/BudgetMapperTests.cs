using FluentAssertions;
using PayTrack.Api.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class BudgetMapperTests
    {
        [Fact]
        public void ToDto_ShouldMapAllFields()
        {
            // Arrange
            var budget = new Budget
            {
                Id = 7,
                TeamId = 3,
                CostCentreId = 5,
                TargetAmount = 2500m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 12, 31),
            };

            // Act
            var dto = BudgetMapper.ToDto(budget);

            // Assert
            dto.Id.Should().Be(7);
            dto.TeamId.Should().Be(3);
            dto.CostCentreId.Should().Be(5);
            dto.TargetAmount.Should().Be(2500m);
            dto.PeriodStart.Should().Be(new DateTime(2026, 1, 1));
            dto.PeriodEnd.Should().Be(new DateTime(2026, 12, 31));
        }

        [Fact]
        public void ListToDto_ShouldMapAllEntities()
        {
            // Arrange
            var budget1 = new Budget { Id = 1, TeamId = 1, CostCentreId = 1, TargetAmount = 100m, PeriodStart = new DateTime(2026, 1, 1), PeriodEnd = new DateTime(2026, 6, 30) };
            var budget2 = new Budget { Id = 2, TeamId = 2, CostCentreId = 1, TargetAmount = 200m, PeriodStart = new DateTime(2026, 7, 1), PeriodEnd = new DateTime(2026, 12, 31) };
            var budget3 = new Budget { Id = 3, TeamId = 3, CostCentreId = 2, TargetAmount = 300m, PeriodStart = new DateTime(2027, 1, 1), PeriodEnd = new DateTime(2027, 12, 31) };
            var budgets = new List<Budget> { budget1, budget2, budget3 };

            // Act
            var dtos = BudgetMapper.ListToDto(budgets);

            // Assert
            dtos.Should().NotBeNull();
            dtos.Should().HaveCount(3);
            dtos.Should().HaveCount(budgets.Count);
            dtos[0].Id.Should().Be(budget1.Id);
            dtos[1].Id.Should().Be(budget2.Id);
            dtos[2].Id.Should().Be(budget3.Id);
            dtos[0].TargetAmount.Should().Be(budget1.TargetAmount);
            dtos[1].TargetAmount.Should().Be(budget2.TargetAmount);
            dtos[2].TargetAmount.Should().Be(budget3.TargetAmount);
        }
    }
}
