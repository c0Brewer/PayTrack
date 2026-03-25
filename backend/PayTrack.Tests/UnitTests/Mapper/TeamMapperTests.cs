using FluentAssertions;
using PayTrack.Application.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class TeamMapperTests
    {
        [Theory]
        [InlineData(1, "name")]
        [InlineData(100, "better_name")]
        [InlineData(9999999, "my spaced name")]
        public async Task MapperToDto_ReturnsCorrectResult(int id, string name)
        {
            Team team = new() { Id = id, Name = name };

            var teamDto = TeamMapper.ToDto(team);

            teamDto.Should().NotBeNull();
            teamDto.id.Should().Be(id);
            teamDto.name.Should().Be(name);
        }

        [Fact]
        public async Task MapperListToDto_ReturnsCorrectResult()
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
            teamsDto[0].name.Should().Be(team1.Name);
            teamsDto[1].name.Should().Be(team2.Name);
            teamsDto[2].name.Should().Be(team3.Name);
            teamsDto[0].name.Should().Be(teams[0].Name);
            teamsDto[1].name.Should().Be(teams[1].Name);
            teamsDto[2].name.Should().Be(teams[2].Name);
        }
    }
}
