using FluentAssertions;
using PayTrack.Application.Dto.Budget;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Dto
{
    public class TeamBudgetEntryDtoTests
    {
        [Fact]
        public void CreateTeamBudgetEntryDto_ShouldExposeConstructorValues()
        {
            var periodStart = new DateTime(2026, 1, 1);
            var periodEnd = new DateTime(2026, 1, 31);

            var dto = new CreateTeamBudgetEntryDto("Q1 budget", null, 12, 3, 250m, periodStart, periodEnd);

            dto.Name.Should().Be("Q1 budget");
            dto.CostCentreId.Should().Be(12);
            dto.SeasonId.Should().Be(3);
            dto.TargetAmount.Should().Be(250m);
            dto.PeriodStart.Should().Be(periodStart);
            dto.PeriodEnd.Should().Be(periodEnd);
            dto.Type.Should().Be(BudgetType.Expense);
        }

        [Fact]
        public void CreateTeamBudgetEntryDto_Income_ShouldExposeNullTargetAmount()
        {
            var periodStart = new DateTime(2026, 1, 1);
            var periodEnd = new DateTime(2026, 12, 31);

            var dto = new CreateTeamBudgetEntryDto("Merch sales", null, 12, 3, null, periodStart, periodEnd, BudgetType.Income);

            dto.TargetAmount.Should().BeNull();
            dto.Type.Should().Be(BudgetType.Income);
        }

        [Fact]
        public void UpsertTeamBudgetEntryDto_ShouldExposeConstructorValues()
        {
            var periodStart = new DateTime(2026, 2, 1);
            var periodEnd = new DateTime(2026, 2, 28);

            var dto = new UpsertTeamBudgetEntryDto(7, "Q2 budget", null, 12, 4, 500m, periodStart, periodEnd);

            dto.Id.Should().Be(7);
            dto.Name.Should().Be("Q2 budget");
            dto.CostCentreId.Should().Be(12);
            dto.SeasonId.Should().Be(4);
            dto.TargetAmount.Should().Be(500m);
            dto.PeriodStart.Should().Be(periodStart);
            dto.PeriodEnd.Should().Be(periodEnd);
            dto.Type.Should().Be(BudgetType.Expense);
        }
    }
}
