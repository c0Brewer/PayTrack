using FluentAssertions;
using PayTrack.Api.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class TeamMapperTests
    {
        [Fact]
        public void ToDto_MapsAllBudgets_WhenTeamHasBudgets()
        {
            var now = DateTime.UtcNow;
            var team = new Team
            {
                Id = 1,
                Name = "Team 1",
                Budgets =
                [
                    new Budget
                    {
                        Id = 1,
                        Name = "Past budget",
                        CostCentreId = 11,
                        TargetAmount = 100m,
                        PeriodStart = now.AddDays(-10),
                        PeriodEnd = now.AddDays(-5),
                    },
                    new Budget
                    {
                        Id = 2,
                        Name = "Current budget",
                        CostCentreId = 12,
                        TargetAmount = 200m,
                        PeriodStart = now.AddDays(-1),
                        PeriodEnd = now.AddDays(1),
                    },
                ],
            };

            var teamDto = TeamMapper.ToDto(team);

            teamDto.Budgets.Should().HaveCount(2);
            teamDto.Budgets.Should().Contain(b => b.Id == 1 && b.TargetAmount == 100m && b.CostCentreId == 11);
            teamDto.Budgets.Should().Contain(b => b.Id == 2 && b.TargetAmount == 200m && b.CostCentreId == 12);
        }

        [Fact]
        public void ToDto_ReturnsEmptyBudgetList_WhenTeamHasNoBudgets()
        {
            var team = new Team
            {
                Id = 1,
                Name = "Team 1",
            };

            var teamDto = TeamMapper.ToDto(team);

            teamDto.Budgets.Should().NotBeNull().And.BeEmpty();
        }
    }
}
