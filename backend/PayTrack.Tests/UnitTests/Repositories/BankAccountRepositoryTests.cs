//AI helped with the test cases

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Exceptions;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Implementation;

namespace PayTrack.Tests.UnitTests.Repositories
{
    public class BankAccountRepositoryTests
    {
        private static AppDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName + Guid.NewGuid())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddBankAccountToDatabase()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("AddBankAccount");
            var user = new User { Name = "User", Email = "user1@test.com" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repository = new BankAccountRepository(context);
            var bankAccount = new BankAccount
            {
                UserId = user.Id,
                AccountHolder = "Max Mustermann",
                Iban = "AT611904300234573201",
                Bic = "BKAUATWW",
            };

            // Act
            var result = await repository.AddAsync(bankAccount);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            (await context.BankAccounts.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task DeleteByIdAsync_ShouldReturnTrueAndDeleteBankAccount_WhenBankAccountExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("DeleteById_Existing");
            var user = new User { Name = "User", Email = "user2@test.com" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var bankAccount = new BankAccount
            {
                UserId = user.Id,
                AccountHolder = "Holder",
                Iban = "AT611904300234573202",
                Bic = "BKAUATWW",
            };
            context.BankAccounts.Add(bankAccount);
            await context.SaveChangesAsync();

            var repository = new BankAccountRepository(context);

            // Act
            var result = await repository.DeleteByIdAsync(user.Id, bankAccount.Id);

            // Assert
            result.Should().BeTrue();
            (await context.BankAccounts.AnyAsync(b => b.Id == bankAccount.Id)).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteByIdAsync_ShouldReturnFalse_WhenBankAccountDoesNotExist()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("DeleteById_Missing");
            var user = new User { Name = "User", Email = "user3@test.com" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repository = new BankAccountRepository(context);

            // Act
            var result = await repository.DeleteByIdAsync(user.Id, 999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnOnlyBankAccountsOfRequestedUser()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetByUserId");

            var user1 = new User { Name = "User1", Email = "user4@test.com" };
            var user2 = new User { Name = "User2", Email = "user5@test.com" };
            context.User.AddRange(user1, user2);
            await context.SaveChangesAsync();

            context.BankAccounts.AddRange(
                new BankAccount { UserId = user1.Id, AccountHolder = "A", Iban = "AT611904300234573203", Bic = "BKAUATWW" },
                new BankAccount { UserId = user1.Id, AccountHolder = "B", Iban = "AT611904300234573204", Bic = "BKAUATWW" },
                new BankAccount { UserId = user2.Id, AccountHolder = "C", Iban = "AT611904300234573205", Bic = "BKAUATWW" });
            await context.SaveChangesAsync();

            var repository = new BankAccountRepository(context);

            // Act
            var result = await repository.GetByUserIdAsync(user1.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(b => b.UserId == user1.Id);
        }

        [Fact]
        public async Task UserExistsAsync_ShouldReturnTrueForExistingUserAndFalseForMissingUser()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UserExists");
            var user = new User { Name = "User", Email = "user6@test.com" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repository = new BankAccountRepository(context);

            // Act
            var exists = await repository.UserExistsAsync(user.Id);
            var missing = await repository.UserExistsAsync(9999);

            // Assert
            exists.Should().BeTrue();
            missing.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAllProvidedFields()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("Update_AllFields");
            var user = new User { Name = "User", Email = "user7@test.com" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var bankAccount = new BankAccount
            {
                UserId = user.Id,
                AccountHolder = "Old Holder",
                Iban = "AT611904300234573206",
                Bic = "OLDBIC12",
            };
            context.BankAccounts.Add(bankAccount);
            await context.SaveChangesAsync();

            var repository = new BankAccountRepository(context);

            // Act
            var result = await repository.UpdateAsync(
                user.Id,
                bankAccount.Id,
                accountHolder: "New Holder",
                iban: "AT611904300234573207",
                bic: "NEWBIC12");

            // Assert
            result.AccountHolder.Should().Be("New Holder");
            result.Iban.Should().Be("AT611904300234573207");
            result.Bic.Should().Be("NEWBIC12");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateOnlyProvidedField_WhenOtherFieldsAreNull()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("Update_OnlyBic");
            var user = new User { Name = "User", Email = "user8@test.com" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var bankAccount = new BankAccount
            {
                UserId = user.Id,
                AccountHolder = "Holder",
                Iban = "AT611904300234573208",
                Bic = "OLDBIC34",
            };
            context.BankAccounts.Add(bankAccount);
            await context.SaveChangesAsync();

            var repository = new BankAccountRepository(context);

            // Act
            var result = await repository.UpdateAsync(user.Id, bankAccount.Id, bic: "NEWBIC34");

            // Assert
            result.AccountHolder.Should().Be("Holder");
            result.Iban.Should().Be("AT611904300234573208");
            result.Bic.Should().Be("NEWBIC34");
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowNotFoundException_WhenBankAccountDoesNotExist()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("Update_NotFound");
            var user = new User { Name = "User", Email = "user9@test.com" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repository = new BankAccountRepository(context);

            // Act
            var act = async () => await repository.UpdateAsync(user.Id, 999, bic: "NEWBIC56");

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Bank account not found");
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowInvalidStateException_WhenIbanAlreadyUsedByAnotherAccount()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("Update_DuplicateIban");
            var user = new User { Name = "User", Email = "user10@test.com" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var account1 = new BankAccount
            {
                UserId = user.Id,
                AccountHolder = "Holder1",
                Iban = "AT611904300234573209",
                Bic = "BKAUATWW",
            };
            var account2 = new BankAccount
            {
                UserId = user.Id,
                AccountHolder = "Holder2",
                Iban = "AT611904300234573210",
                Bic = "BKAUATWW",
            };
            context.BankAccounts.AddRange(account1, account2);
            await context.SaveChangesAsync();

            var repository = new BankAccountRepository(context);

            // Act
            var act = async () => await repository.UpdateAsync(user.Id, account1.Id, iban: account2.Iban);

            // Assert
            await act.Should().ThrowAsync<InvalidStateException>()
                .WithMessage("Bank account with the same IBAN already exists for this user");
        }
    }
}
