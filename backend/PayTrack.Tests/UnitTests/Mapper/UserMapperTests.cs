using FluentAssertions;
using PayTrack.Api.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class UserMapperTests
    {
        [Theory]
        [InlineData(1, "name")]
        [InlineData(100, "better_name")]
        [InlineData(9999999, "my spaced name")]
        public void MapperToDto_ReturnsCorrectResult(int id, string name)
        {
            User user = new() { Id = id, Name = name };

            var userDto = UserMapper.ToDto(user);

            userDto.Should().NotBeNull();
            userDto.Id.Should().Be(id);
            userDto.Name.Should().Be(name);
            userDto.BankAccounts.Should().BeEmpty();
            userDto.HasBankInformation.Should().BeFalse();
            userDto.BankInformationSkipped.Should().BeFalse();
        }

        [Fact]
        public void MapperListToDto_ReturnsCorrectResult()
        {
            var user = new List<User>();

            User user1 = new() { Id = 1, Name = "123" };
            User user2 = new() { Id = 2, Name = "456" };
            User user3 = new() { Id = 3, Name = "789" };

            user.Add(user1);
            user.Add(user2);
            user.Add(user3);

            var userDto = UserMapper.ListToDto(user);

            userDto.Should().NotBeNull();
            userDto.Should().HaveCount(3);
            userDto.Should().HaveCount(user.Count);
            userDto[0].Name.Should().Be(user1.Name);
            userDto[1].Name.Should().Be(user2.Name);
            userDto[2].Name.Should().Be(user3.Name);
            userDto[0].Name.Should().Be(user[0].Name);
            userDto[1].Name.Should().Be(user[1].Name);
            userDto[2].Name.Should().Be(user[2].Name);
        }

        [Fact]
        public void MapperToDto_ShouldContainTeamDto_IfUserContainsTeam()
        {
            Team team1 = new() { Id = 3, Name = "Team1" };

            User user1 = new() { Id = 1, Name = "123", Team = team1 };

            var userDto = UserMapper.ToDto(user1);

            userDto.Should().NotBeNull();
            userDto.Team.Should().NotBeNull();
            userDto.Team.Id.Should().Be(3);
            userDto.Team.Name.Should().Be("Team1");
        }

        [Fact]
        public async Task MapperToDto_ShouldMapBankInformation()
        {
            User user = new()
            {
                Id = 5,
                Name = "Alice",
                BankInformationSkipped = true,
                BankAccounts =
                [
                    new BankAccount
                    {
                        Id = 7,
                        Iban = "AT611904300234573201",
                        Bic = "BKAUATWW",
                        AccountHolder = "Alice Example",
                    },
                ],
            };

            var userDto = UserMapper.ToDto(user);

            userDto.BankInformationSkipped.Should().BeTrue();
            userDto.HasBankInformation.Should().BeTrue();
            userDto.BankAccounts.Should().ContainSingle();
            userDto.BankAccounts.Single().Iban.Should().Be("AT611904300234573201");
            userDto.BankAccounts.Single().Bic.Should().Be("BKAUATWW");
            userDto.BankAccounts.Single().AccountHolder.Should().Be("Alice Example");
        }
    }
}
