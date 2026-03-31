using Microsoft.EntityFrameworkCore;
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
            var result = await repo.AddAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Name, result.Name);
            Assert.Equal(user.Email, result.Email);

            var fromDb = await context.User.FirstOrDefaultAsync(u => u.Email == user.Email);
            Assert.NotNull(fromDb);
            Assert.Equal(user.Name, fromDb!.Name);
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
                async () => await repo.AddAsync(user)
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
    }
}
