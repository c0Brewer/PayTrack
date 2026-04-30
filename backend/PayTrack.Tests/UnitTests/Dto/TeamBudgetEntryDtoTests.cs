using FluentAssertions;
using PayTrack.Application.Dto.Team;

namespace PayTrack.Tests.UnitTests.Dto
{
    public class TeamBudgetEntryDtoTests
    {
        [Fact]
        public void CreateTeamBudgetEntryDto_ShouldExposeConstructorValues()
        {
            var periodStart = new DateTime(2026, 1, 1);
            var periodEnd = new DateTime(2026, 1, 31);

            var dto = new CreateTeamBudgetEntryDto(12, 250m, periodStart, periodEnd);

            dto.CostCentreId.Should().Be(12);
            dto.TargetAmount.Should().Be(250m);
            dto.PeriodStart.Should().Be(periodStart);
            dto.PeriodEnd.Should().Be(periodEnd);
        }

        [Fact]
        public void UpsertTeamBudgetEntryDto_ShouldExposeConstructorValues()
        {
            var periodStart = new DateTime(2026, 2, 1);
            var periodEnd = new DateTime(2026, 2, 28);

            var dto = new UpsertTeamBudgetEntryDto(7, 12, 500m, periodStart, periodEnd);

            dto.Id.Should().Be(7);
            dto.CostCentreId.Should().Be(12);
            dto.TargetAmount.Should().Be(500m);
            dto.PeriodStart.Should().Be(periodStart);
            dto.PeriodEnd.Should().Be(periodEnd);
        }
    }
}
