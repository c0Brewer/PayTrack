using FluentAssertions;
using PayTrack.Api.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class BudgetMapperTests
    {
        [Fact]
        public async Task MapperListToDto_ReturnsCorrectResult()
        {
            ICollection<Budget> budgets =
            [
                new Budget
                {
                    Id = 1,
                    TeamId = 7,
                    CostCentreId = 11,
                    TargetAmount = 1000m,
                    PeriodStart = new DateTime(2026, 1, 1),
                    PeriodEnd = new DateTime(2026, 6, 30),
                },
                new Budget
                {
                    Id = 2,
                    TeamId = 7,
                    CostCentreId = 12,
                    TargetAmount = 2000m,
                    PeriodStart = new DateTime(2026, 7, 1),
                    PeriodEnd = new DateTime(2026, 12, 31),
                },
            ];

            var budgetDto = BudgetMapper.ListToDto(budgets);

            budgetDto.Should().NotBeNull();
            budgetDto.Should().HaveCount(2);
            budgetDto[0].Id.Should().Be(1);
            budgetDto[0].CostCentreId.Should().Be(11);
            budgetDto[0].TargetAmount.Should().Be(1000m);
            budgetDto[1].PeriodStart.Should().Be(new DateTime(2026, 7, 1));
            budgetDto[1].PeriodEnd.Should().Be(new DateTime(2026, 12, 31));
        }
    }
}
