//AI helped with the test cases

using FluentAssertions;
using PayTrack.Application.Dto.BankAccount;

namespace PayTrack.Tests.UnitTests.Dto
{
    public class BankAccountDtosTests
    {
        [Fact]
        public void BankAccountDto_ShouldExposeConstructorValues()
        {
            // Arrange + Act
            var dto = new BankAccountDto(1, "Max Mustermann", "AT611904300234573201", "BKAUATWW");

            // Assert
            dto.id.Should().Be(1);
            dto.accountHolder.Should().Be("Max Mustermann");
            dto.iban.Should().Be("AT611904300234573201");
            dto.bic.Should().Be("BKAUATWW");
        }

        [Fact]
        public void CreateBankAccountRequestDto_ShouldExposeConstructorValues()
        {
            // Arrange + Act
            var dto = new CreateBankAccountRequestDto("Max", "AT611904300234573202", "BKAUATWW");

            // Assert
            dto.accountHolder.Should().Be("Max");
            dto.iban.Should().Be("AT611904300234573202");
            dto.bic.Should().Be("BKAUATWW");
        }

        [Fact]
        public void UpdateBankAccountRequestDto_ShouldExposeConstructorValues()
        {
            // Arrange + Act
            var dto = new UpdateBankAccountRequestDto("Max", null, "NEWBIC34");

            // Assert
            dto.accountHolder.Should().Be("Max");
            dto.iban.Should().BeNull();
            dto.bic.Should().Be("NEWBIC34");
        }
    }
}
