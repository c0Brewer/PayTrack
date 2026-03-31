using Moq;
using PayTrack.Application.Services.Implementation;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Tests.UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> userRepoMock;
        private readonly UserService service;

        public UserServiceTests()
        {
            userRepoMock = new Mock<IUserRepository>();
            service = new UserService(userRepoMock.Object);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnCreatedUser()
        {
            // Arrange
            const string name = "Test User";
            const string email = "test@example.com";
            const string picture = "pic.png";
            var createdUser = new User
            {
                Name = name,
                Email = email,
                ProfilePictureUrl = picture,
                IsActive = true
            };

            userRepoMock
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u); // Echo back user

            // Act
            var result = await service.CreateUserAsync(name, email, picture);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(email, result.Email);
            Assert.Equal(picture, result.ProfilePictureUrl);
            Assert.True(result.IsActive);

            userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnUser_WhenExists()
        {
            // Arrange
            const string email = "test@example.com";
            var user = new User { Name = "Test", Email = email };
            userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);

            // Act
            var result = await service.GetUserByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
            userRepoMock.Verify(r => r.GetByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            const string email = "unknown@example.com";
            userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((User?)null);

            // Act
            var result = await service.GetUserByEmailAsync(email);

            // Assert
            Assert.Null(result);
            userRepoMock.Verify(r => r.GetByEmailAsync(email), Times.Once);
        }
    }
}
