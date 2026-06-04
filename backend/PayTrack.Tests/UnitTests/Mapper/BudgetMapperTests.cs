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
                    Name = "First budget",
                    TeamId = 7,
                    CostCentreId = 11,
                    TargetAmount = 1000m,
                    PeriodStart = new DateTime(2026, 1, 1),
                    PeriodEnd = new DateTime(2026, 6, 30),
                    Type = BudgetType.Expense,
                },
                new Budget
                {
                    Id = 2,
                    Name = "Second budget",
                    TeamId = 7,
                    CostCentreId = 12,
                    TargetAmount = 2000m,
                    PeriodStart = new DateTime(2026, 7, 1),
                    PeriodEnd = new DateTime(2026, 12, 31),
                    Type = BudgetType.Expense,
                },
            ];

            var budgetDto = BudgetMapper.CollectionToDto(budgets);

            budgetDto.Should().NotBeNull();
            budgetDto.Should().HaveCount(2);
            budgetDto[0].Id.Should().Be(1);
            budgetDto[0].CostCentreId.Should().Be(11);
            budgetDto[0].TargetAmount.Should().Be(1000m);
            budgetDto[0].Type.Should().Be(BudgetType.Expense);
            budgetDto[1].PeriodStart.Should().Be(new DateTime(2026, 7, 1));
            budgetDto[1].PeriodEnd.Should().Be(new DateTime(2026, 12, 31));
        }

        [Fact]
        public async Task MapperListToDto_MapsIncomeBudget_WithNullTargetAmount()
        {
            ICollection<Budget> budgets =
            [
                new Budget
                {
                    Id = 3,
                    Name = "Merch sales",
                    TeamId = 5,
                    CostCentreId = 9,
                    TargetAmount = null,
                    PeriodStart = new DateTime(2026, 1, 1),
                    PeriodEnd = new DateTime(2026, 12, 31),
                    Type = BudgetType.Income,
                },
            ];

            var budgetDto = BudgetMapper.CollectionToDto(budgets);

            budgetDto.Should().HaveCount(1);
            budgetDto[0].Type.Should().Be(BudgetType.Income);
            budgetDto[0].TargetAmount.Should().BeNull();
        }
    }
}
