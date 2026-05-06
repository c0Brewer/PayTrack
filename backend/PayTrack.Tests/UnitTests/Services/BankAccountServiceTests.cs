//AI helped with the test cases

using FluentAssertions;
using Moq;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Implementation;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Tests.UnitTests.Services
{
    public class BankAccountServiceTests
    {
        private readonly Mock<IBankAccountRepository> repoMock;
        private readonly Mock<IUserRepository> userRepoMock;
        private readonly BankAccountService service;

        public BankAccountServiceTests()
        {
            this.repoMock = new Mock<IBankAccountRepository>();
            this.userRepoMock = new Mock<IUserRepository>();
            this.service = new BankAccountService(this.repoMock.Object, this.userRepoMock.Object);
        }

        [Fact]
        public async Task CreateBankAccountAsync_ShouldCreateBankAccount_WhenIbanIsUnique()
        {
            // Arrange
            const int userId = 5;
            const string accountHolder = "Max Mustermann";
            const string iban = "AT611904300234573201";
            const string bic = "BKAUATWW";

            this.repoMock
                .Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync([]);
            this.repoMock
                .Setup(r => r.UserExistsAsync(userId))
                .ReturnsAsync(true);
            this.repoMock
                .Setup(r => r.AddAsync(It.IsAny<BankAccount>()))
                .ReturnsAsync((BankAccount bankAccount) => bankAccount);

            // Act
            var result = await this.service.CreateBankAccountAsync(userId, accountHolder, iban, bic);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.AccountHolder.Should().Be(accountHolder);
            result.Iban.Should().Be(iban);
            result.Bic.Should().Be(bic);
        }

        [Fact]
        public async Task CreateBankAccountAsync_ShouldNormalizeValues_WhenCreating()
        {
            const int userId = 5;

            this.repoMock
                .Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync([]);
            this.repoMock
                .Setup(r => r.UserExistsAsync(userId))
                .ReturnsAsync(true);
            this.repoMock
                .Setup(r => r.AddAsync(It.IsAny<BankAccount>()))
                .ReturnsAsync((BankAccount bankAccount) => bankAccount);

            var result = await this.service.CreateBankAccountAsync(
                userId,
                "  Max Mustermann  ",
                "at61 1904 3002 3457 3201",
                "bkauatww");

            result.AccountHolder.Should().Be("Max Mustermann");
            result.Iban.Should().Be("AT611904300234573201");
            result.Bic.Should().Be("BKAUATWW");
        }

        [Fact]
        public async Task CreateBankAccountAsync_ShouldThrowInvalidStateException_WhenIbanAlreadyExists()
        {
            // Arrange
            const int userId = 5;
            const string iban = "AT611904300234573201";

            this.repoMock
                .Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync([new BankAccount { UserId = userId, Iban = iban, Bic = "ABCDEFGH", AccountHolder = "Existing" }]);

            // Act
            var act = async () => await this.service.CreateBankAccountAsync(userId, "Max", iban, "ABCDEFGH");

            // Assert
            await act.Should().ThrowAsync<InvalidStateException>()
                .WithMessage("Bank account with the same IBAN already exists for this user");
        }

        [Theory]
        [InlineData("   ", "AT611904300234573201", "BKAUATWW", "Account holder must not be empty.")]
        [InlineData("Max", "INVALID", "BKAUATWW", "IBAN is invalid.")]
        [InlineData("Max", "AT611904300234573201", "INVALID", "BIC is invalid.")]
        public async Task CreateBankAccountAsync_ShouldThrowInvalidStateException_ForInvalidInput(
            string accountHolder,
            string iban,
            string bic,
            string message)
        {
            var act = async () => await this.service.CreateBankAccountAsync(5, accountHolder, iban, bic);

            await act.Should().ThrowAsync<InvalidStateException>()
                .WithMessage(message);
        }

        [Fact]
        public async Task CreateBankAccountOnboardingAsync_ShouldResetSkipFlag()
        {
            const int userId = 5;

            this.repoMock
                .Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync([]);
            this.repoMock
                .Setup(r => r.UserExistsAsync(userId))
                .ReturnsAsync(true);
            this.repoMock
                .Setup(r => r.AddAsync(It.IsAny<BankAccount>()))
                .ReturnsAsync((BankAccount bankAccount) => bankAccount);
            this.userRepoMock
                .Setup(r => r.UpdateBankInformationSkippedAsync(userId, false))
                .ReturnsAsync(new User { Id = userId, BankInformationSkipped = false });

            await this.service.CreateBankAccountOnboardingAsync(userId, "Max", "AT611904300234573201", "BKAUATWW");

            this.userRepoMock.Verify(r => r.UpdateBankInformationSkippedAsync(userId, false), Times.Once);
        }

        [Fact]
        public async Task GetBankAccountsAsync_ShouldReturnEmptyList_WhenUserExistsWithoutBankAccounts()
        {
            // Arrange
            const int userId = 7;

            this.repoMock
                .Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync([]);
            this.repoMock
                .Setup(r => r.UserExistsAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await this.service.GetBankAccountsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetBankAccountsAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            const int userId = 7;

            this.repoMock
                .Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync([]);
            this.repoMock
                .Setup(r => r.UserExistsAsync(userId))
                .ReturnsAsync(false);

            // Act
            var act = async () => await this.service.GetBankAccountsAsync(userId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("User not found");
        }

        [Fact]
        public async Task UpdateBankAccountAsync_ShouldForwardOptionalParametersToRepository()
        {
            // Arrange
            const int userId = 5;
            const int bankAccountId = 11;
            const string newIban = "DE89370400440532013000";

            var updated = new BankAccount
            {
                Id = bankAccountId,
                UserId = userId,
                AccountHolder = "Updated",
                Iban = newIban,
                Bic = "BKAUATWW",
            };

            this.repoMock
                .Setup(r => r.UpdateAsync(userId, bankAccountId, null, newIban, null))
                .ReturnsAsync(updated);

            // Act
            var result = await this.service.UpdateBankAccountAsync(userId, bankAccountId, null, newIban, null);

            // Assert
            result.Should().BeSameAs(updated);
            this.repoMock.Verify(r => r.UpdateAsync(userId, bankAccountId, null, newIban, null), Times.Once);
        }

        [Fact]
        public async Task UpdateBankAccountAsync_ShouldNormalizeValuesBeforeForwarding()
        {
            const int userId = 5;
            const int bankAccountId = 11;
            var updated = new BankAccount
            {
                Id = bankAccountId,
                UserId = userId,
                AccountHolder = "Updated",
                Iban = "DE89370400440532013000",
                Bic = "BKAUATWW",
            };

            this.repoMock
                .Setup(r => r.UpdateAsync(userId, bankAccountId, "Updated", "DE89370400440532013000", "BKAUATWW"))
                .ReturnsAsync(updated);

            var result = await this.service.UpdateBankAccountAsync(
                userId,
                bankAccountId,
                "  Updated  ",
                "de89 3704 0044 0532 0130 00",
                "bkauatww");

            result.Should().BeSameAs(updated);
            this.repoMock.Verify(
                r => r.UpdateAsync(userId, bankAccountId, "Updated", "DE89370400440532013000", "BKAUATWW"),
                Times.Once);
        }

        [Fact]
        public async Task DeleteBankAccountAsync_ShouldCallRepository_WhenBankAccountExists()
        {
            // Arrange
            const int userId = 5;
            const int bankAccountId = 12;

            this.repoMock
                .Setup(r => r.DeleteByIdAsync(userId, bankAccountId))
                .ReturnsAsync(true);

            // Act
            await this.service.DeleteBankAccountAsync(userId, bankAccountId);

            // Assert
            this.repoMock.Verify(r => r.DeleteByIdAsync(userId, bankAccountId), Times.Once);
        }

        [Fact]
        public async Task DeleteBankAccountAsync_ShouldThrowNotFoundException_WhenRepositoryReturnsFalse()
        {
            // Arrange
            const int userId = 5;
            const int bankAccountId = 12;

            this.repoMock
                .Setup(r => r.DeleteByIdAsync(userId, bankAccountId))
                .ReturnsAsync(false);

            // Act
            var act = async () => await this.service.DeleteBankAccountAsync(userId, bankAccountId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Bank account not found");
        }
    }
}
