using FluentAssertions;
using PayTrack.Api.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class BankAccountMapperTests
    {
        [Theory]
        [InlineData(1, "AT611904300234573201", "BKAUATWW", "John Doe")]
        [InlineData(42, "DE89370400440532013000", "COBADEFF", "Max Mustermann")]
        [InlineData(999, "CH9300762011623852957", "POFICHBEXXX", "Test User")]
        public void MapperToDto_ReturnsCorrectResult(
            int id,
            string iban,
            string bic,
            string accountHolder)
        {
            // Arrange
            var entity = new BankAccount
            {
                Id = id,
                Iban = iban,
                Bic = bic,
                AccountHolder = accountHolder
            };

            // Act
            var dto = BankAccountMapper.ToDto(entity);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(id);
            dto.IBAN.Should().Be(iban);
            dto.BIC.Should().Be(bic);
            dto.AccountHolder.Should().Be(accountHolder);
        }

        [Fact]
        public void MapperListToDto_ReturnsCorrectResult()
        {
            // Arrange
            var list = new List<BankAccount>
            {
                new()
                {
                    Id = 1,
                    Iban = "AT111",
                    Bic = "BIC1",
                    AccountHolder = "A"
                },
                new()
                {
                    Id = 2,
                    Iban = "AT222",
                    Bic = "BIC2",
                    AccountHolder = "B"
                },
                new()
                {
                    Id = 3,
                    Iban = "AT333",
                    Bic = "BIC3",
                    AccountHolder = "C"
                }
            };

            // Act
            var result = BankAccountMapper.ListToDto(list);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                result[i].Id.Should().Be(list[i].Id);
                result[i].IBAN.Should().Be(list[i].Iban);
                result[i].BIC.Should().Be(list[i].Bic);
                result[i].AccountHolder.Should().Be(list[i].AccountHolder);
            }
        }

        [Fact]
        public void MapperListToDto_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var list = new List<BankAccount>();

            // Act
            var result = BankAccountMapper.ListToDto(list);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
