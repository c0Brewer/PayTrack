using FluentAssertions;
using PayTrack.Api.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class TeamMapperTests
    {
        [Theory]
        [InlineData(1, "name")]
        [InlineData(100, "better_name")]
        [InlineData(9999999, "my spaced name")]
        public void MapperToDto_ReturnsCorrectResult(int id, string name)
        {
            Team team = new() { Id = id, Name = name };

            var teamDto = TeamMapper.ToDto(team);

            teamDto.Should().NotBeNull();
            teamDto.Id.Should().Be(id);
            teamDto.Name.Should().Be(name);
            teamDto.Members.Should().BeNull();
            teamDto.Budgets.Should().BeNull();
        }

        [Fact]
        public void MapperListToDto_ReturnsCorrectResult()
        {
            var teams = new List<Team>();

            Team team1 = new() { Id = 1, Name = "123" };
            Team team2 = new() { Id = 2, Name = "456" };
            Team team3 = new() { Id = 3, Name = "789" };

            teams.Add(team1);
            teams.Add(team2);
            teams.Add(team3);

            var teamsDto = TeamMapper.ListToDto(teams);

            teamsDto.Should().NotBeNull();
            teamsDto.Should().HaveCount(3);
            teamsDto.Should().HaveCount(teams.Count);
            teamsDto[0].Name.Should().Be(team1.Name);
            teamsDto[1].Name.Should().Be(team2.Name);
            teamsDto[2].Name.Should().Be(team3.Name);
            teamsDto[0].Name.Should().Be(teams[0].Name);
            teamsDto[1].Name.Should().Be(teams[1].Name);
            teamsDto[2].Name.Should().Be(teams[2].Name);
            teamsDto[0].Budgets.Should().BeNull();
            teamsDto[1].Budgets.Should().BeNull();
            teamsDto[2].Budgets.Should().BeNull();
        }

        [Fact]
        public void MapperToDto_MapsMembersAndBudgets_WhenRequested()
        {
            // The mapper should keep nested data opt-in so list endpoints can stay lightweight by default.
            Team team = new()
            {
                Id = 7,
                Name = "Operations",
                Members =
                [
                    new User
                    {
                        Id = 1,
                        Name = "Alice",
                        Email = "alice@example.com",
                        Role = Role.TeamLead,
                        IsActive = true,
                    },
                ],
                Budgets =
                [
                    new Budget
                    {
                        Id = 3,
                        TeamId = 7,
                        CostCentreId = 11,
                        TargetAmount = 1250m,
                        PeriodStart = new DateTime(2026, 1, 1),
                        PeriodEnd = new DateTime(2026, 12, 31),
                    },
                ],
            };

            var teamDto = TeamMapper.ToDto(team, includeMembers: true, includeBudgets: true);

            teamDto.Members.Should().ContainSingle();
            teamDto.Members![0].Email.Should().Be("alice@example.com");
            teamDto.Budgets.Should().ContainSingle();
            teamDto.Budgets![0].TargetAmount.Should().Be(1250m);
        }

    }
}
