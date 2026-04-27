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
        private readonly BankAccountService service;

        public BankAccountServiceTests()
        {
            this.repoMock = new Mock<IBankAccountRepository>();
            this.service = new BankAccountService(this.repoMock.Object);
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
            const string newIban = "AT951904300234573202";

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
