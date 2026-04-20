using FluentAssertions;
using PayTrack.Api.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class TeamMemberMapperTests
    {
        [Fact]
        public async Task MapperListToDto_ReturnsCorrectResult()
        {
            ICollection<User> users =
            [
                new User { Id = 1, Name = "Alice", Email = "alice@example.com", Role = Role.RegularUser, IsActive = true },
                new User { Id = 2, Name = "Bob", Email = "bob@example.com", Role = Role.TeamLead, IsActive = false },
            ];

            var membersDto = TeamMemberMapper.ListToDto(users);

            membersDto.Should().NotBeNull();
            membersDto.Should().HaveCount(2);
            membersDto[0].Id.Should().Be(1);
            membersDto[0].Name.Should().Be("Alice");
            membersDto[1].Email.Should().Be("bob@example.com");
            membersDto[1].Role.Should().Be(Role.TeamLead);
            membersDto[1].IsActive.Should().BeFalse();
        }
    }
}
