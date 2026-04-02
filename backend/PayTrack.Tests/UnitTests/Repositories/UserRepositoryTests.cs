using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Dto.User;
using PayTrack.Application.Exceptions;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Implementation;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Repositories
{
    public class UserRepositoryTests
    {
        private static AppDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName + Guid.NewGuid())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddUser_WhenSuccessful()
        {
            // Arrange
            var context = GetInMemoryDbContext("AddUserDb");
            var repo = new UserRepository(context);
            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                ProfilePictureUrl = "pic.png",
                IsActive = true
            };

            // Act
            var result = await repo.AddAsync(user.Name, user.Email, user.ProfilePictureUrl, user.IsActive);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Name, result.Name);
            Assert.Equal(user.Email, result.Email);

            var fromDb = await context.User.FirstOrDefaultAsync(u => u.Email == user.Email);
            Assert.NotNull(fromDb);
            Assert.Equal(user.Name, fromDb.Name);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenSaveChangesFails()
        {
            // Arrange
            var failingContext = new FailingDbContext("AddAsync_FailingDbContext");

            // Override SaveChangesAsync to return 0 to simulate failure

            var repo = new UserRepository(failingContext);
            var user = new User { Name = "Fail User", Email = "fail@example.com" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InternalErrorException>(
                async () => await repo.AddAsync(user.Name, user.Email, profilePictureUrl: null)
            );

            // Assert
            Assert.Contains("user", exception.Message);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser_WhenExists()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetUserDb");
            var user = new User { Name = "Test", Email = "get@example.com" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            // Act
            var result = await repo.GetByEmailAsync("get@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("get@example.com", result.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetNullDb");
            var repo = new UserRepository(context);

            // Act
            var result = await repo.GetByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetUserById");
            var user = new User { Name = "Existing User", Email = "testmail" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            // Act
            var result = await repo.GetByIdAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Name.Should().Be("Existing User");
            result.Team.Should().BeNull();
            result.BankAccounts.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsyncWithOptionalParameters_ShouldReturnFullUser_WhenExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetUserById");

            var team = new Team { Name = "Existing Team" };
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var user = new User { Name = "Existing User", Email = "testmail", TeamId = team.Id };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var bankAccount = new BankAccount { UserId = user.Id, Bic = "123", Iban = "456", AccountHolder = "Existing Team" };
            context.BankAccounts.Add(bankAccount);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            // Act
            var result = await repo.GetByIdAsync(
                user.Id,
                includeTeam: true,
                includeBankAccounts: true);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Name.Should().Be("Existing User");
            result.Email.Should().Be("testmail");
            result.Team.Should().NotBeNull();
            result.Team.Name.Should().Be("Existing Team");
            result.BankAccounts.Count.Should().Be(1);
            result.BankAccounts.First().Bic.Should().Be("123");
            result.BankAccounts.First().Iban.Should().Be("456");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUser()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllUserAsync");
            var user = new List<User>
            {
                new() { Name = "User1", Email = "E1" },
                new() { Name = "User2", Email = "E2" }
            };
            context.User.AddRange(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            // Act
            var (result, totalCount) = await repo.GetAllAsync(new GetUserQuery { Limit = 10 });

            // Assert
            result.Should().HaveCount(2);
            result.Count.Should().Be(totalCount);
            result.Should().ContainSingle(t => t.Name == "User1" && t.Email == "E1");
            result.Should().ContainSingle(t => t.Name == "User2" && t.Email == "E2");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnOnlyUsersAfterOffsetAndWithLimit_IfOffsetAndLimitAreSet()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllUserAsync");
            var user = new List<User>
            {
                new() { Name = "User1", Email = "E1" },
                new() { Name = "User2", Email = "E2" },
                new() { Name = "User3", Email = "E3" },
                new() { Name = "User4", Email = "E4" },
                new() { Name = "User5", Email = "E5" }
            };
            context.User.AddRange(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            const int limit = 3;
            const int offset = 1;

            // Act
            var (resultList, totalCount) = await repo.GetAllAsync(new GetUserQuery { Limit = limit, Offset = offset });

            // Assert
            resultList.Should().HaveCount(limit);
            resultList.Should().NotContain(t => t.Name == "User1" || t.Email == "E1");
            resultList.Should().ContainSingle(t => t.Name == "User2" && t.Email == "E2");
            resultList.Should().ContainSingle(t => t.Name == "User3" && t.Email == "E3");
            resultList.Should().ContainSingle(t => t.Name == "User4" && t.Email == "E4");
            resultList.Should().NotContain(t => t.Name == "User5" || t.Email == "E5");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnOnlyUsersIncludedInquery_IfAllValuesAreSet()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllUserAsync");

            var teams = new List<Team>
            {
                new () { Name = "AAA", Description = "123", DisplayColor = "123"},
                new () { Name = "BBB", Description = "123", DisplayColor = "123"}
            };
            context.Teams.AddRange(teams);
            await context.SaveChangesAsync();

            var user = new List<User>
            {
                new() { Name = "AAA", Email = "AAA", TeamId = teams[0].Id, Role = Role.RegularUser, IsActive = true },
                new() { Name = "BBB", Email = "AAA", TeamId = teams[0].Id, Role = Role.RegularUser, IsActive = true },
                new() { Name = "AAA", Email = "BBB", TeamId = teams[0].Id, Role = Role.RegularUser, IsActive = true },
                new() { Name = "AAA", Email = "AAA", TeamId = teams[1].Id, Role = Role.RegularUser, IsActive = true },
                new() { Name = "AAA", Email = "AAA", TeamId = teams[0].Id, Role = Role.Admin, IsActive = true },
                new() { Name = "AAA", Email = "AAA", TeamId = teams[0].Id, Role = Role.RegularUser, IsActive = false },
            };
            context.User.AddRange(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            // Act
            var (resultList, totalCount) = await repo.GetAllAsync(new GetUserQuery
            {
                Name = "A",
                Email = "A",
                TeamName = "A",
                Role = Role.RegularUser,
                IsActive = true,
                IncludeTeam = true,
                Limit = 10,
                Offset = 0
            });

            // Assert
            totalCount.Should().Be(1);
            resultList.Count.Should().Be(1);

            resultList.Should().Contain(u =>
                    u.Name == user[0].Name &&
                    u.Email == user[0].Email &&
                    u.TeamId == user[0].TeamId &&
                    u.Role == user[0].Role &&
                    u.IsActive == user[0].IsActive);

            for (int i = 1; i < user.Count; i++)
            {
                resultList.Should().NotContain(u =>
                        u.Name == user[i].Name &&
                        u.Email == user[i].Email &&
                        u.TeamId == user[i].TeamId &&
                        u.Role == user[i].Role &&
                        u.IsActive == user[i].IsActive);
            }
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateIsActive_WhenProvided()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("Update_IsActive");
            var user = new User { Name = "Test", Email = "test@example.com", IsActive = true };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            // Act
            var updated = await repo.UpdateAsync(user.Id, null, isActive: false);

            // Assert
            updated.IsActive.Should().BeFalse();
            updated.Name.Should().Be(user.Name);
            (await context.User.FindAsync(user.Id))!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTeamId_WhenProvided()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("Update_TeamId");
            var team = new Team { Name = "Team A" };
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var user = new User { Name = "Test", Email = "test@example.com" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            // Act
            var updated = await repo.UpdateAsync(user.Id, null, teamId: team.Id);

            // Assert
            updated.TeamId.Should().Be(team.Id);
            updated.Name.Should().Be(user.Name);
            (await context.User.FindAsync(user.Id))!.TeamId.Should().Be(team.Id);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateRole_WhenProvided()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("Update_Role");
            var user = new User { Name = "Test", Email = "test@example.com", Role = Role.RegularUser };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            // Act
            var updated = await repo.UpdateAsync(user.Id, null, role: Role.Admin);

            // Assert
            updated.Role.Should().Be(Role.Admin);
            updated.Name.Should().Be(user.Name);
            (await context.User.FindAsync(user.Id))!.Role.Should().Be(Role.Admin);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateMultipleFields_WhenMultipleProvided()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("Update_Multiple");
            var team = new Team { Name = "Team B" };
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var user = new User { Name = "Test", Email = "test@example.com", IsActive = true, Role = Role.RegularUser };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            // Act
            var updated = await repo.UpdateAsync(user.Id, "New Name", isActive: false, teamId: team.Id, role: Role.Admin);

            // Assert
            updated.Name.Should().Be("New Name");
            updated.IsActive.Should().BeFalse();
            updated.TeamId.Should().Be(team.Id);
            updated.Role.Should().Be(Role.Admin);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("Update_NotFound");
            var repo = new UserRepository(context);

            // Act
            var act = async () => await repo.UpdateAsync(999, null, isActive: true);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("User with id 999 not found.");
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowInternalErrorException_WhenSaveFails()
        {
            // Arrange
            var failingContext = new FailingDbContext("Update_Failing", _countOfSuccessBeforeFailure: 1);
            var repo = new UserRepository(failingContext);

            var user = new User { Name = "Fail", Email = "fail@example.com" };
            var res = failingContext.User.Add(user);
            await failingContext.SaveChangesAsync();

            // Act
            var act = async () => await repo.UpdateAsync(user.Id, null, isActive: false);

            // Assert
            await act.Should().ThrowAsync<InternalErrorException>()
                .WithMessage("Updating user failed*");
        }
    }
}
