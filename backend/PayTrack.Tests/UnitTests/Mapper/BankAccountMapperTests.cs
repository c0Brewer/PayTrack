//AI helped with the test cases

using FluentAssertions;
using PayTrack.Api.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class BankAccountMapperTests
    {
        [Theory]
        [InlineData(1, "Max Mustermann", "AT611904300234573201", "BKAUATWW")]
        [InlineData(2, "John Doe", "DE89370400440532013000", "COBADEFF")]
        public void ToDto_ShouldReturnCorrectMappedDto(int id, string accountHolder, string iban, string bic)
        {
            // Arrange
            var bankAccount = new BankAccount
            {
                Id = id,
                AccountHolder = accountHolder,
                Iban = iban,
                Bic = bic,
            };

            // Act
            var dto = BankAccountMapper.ToDto(bankAccount);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(id);
            dto.AccountHolder.Should().Be(accountHolder);
            dto.Iban.Should().Be(iban);
            dto.Bic.Should().Be(bic);
        }

        [Fact]
        public void ListToDto_ShouldMapAllEntries()
        {
            // Arrange
            var bankAccounts = new List<BankAccount>
            {
                new() { Id = 1, AccountHolder = "A", Iban = "AT611904300234573211", Bic = "BKAUATWW" },
                new() { Id = 2, AccountHolder = "B", Iban = "AT611904300234573212", Bic = "BKAUATWW" },
                new() { Id = 3, AccountHolder = "C", Iban = "AT611904300234573213", Bic = "BKAUATWW" },
            };

            // Act
            var result = BankAccountMapper.ListToDto(bankAccounts);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Select(dto => dto.Id).Should().BeEquivalentTo([1, 2, 3]);
            result.Select(dto => dto.AccountHolder).Should().BeEquivalentTo(["A", "B", "C"]);
        }
    }
}
