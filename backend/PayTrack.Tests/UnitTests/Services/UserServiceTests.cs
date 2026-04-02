using FluentAssertions;
using Moq;
using PayTrack.Application.Dto.User;
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
            const string name = "Test User";
            const string email = "test@example.com";
            const string picture = "pic.png";

            userRepoMock
                .Setup(r => r.AddAsync(name, email, picture, true))
                .ReturnsAsync(new User
                {
                    Name = name,
                    Email = email,
                    ProfilePictureUrl = picture,
                    IsActive = true
                });

            var result = await service.CreateUserAsync(name, email, picture, true);

            result.Should().NotBeNull();
            result.Name.Should().Be(name);
            result.Email.Should().Be(email);
            result.ProfilePictureUrl.Should().Be(picture);
            result.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnUser_WhenExists()
        {
            const string email = "test@example.com";
            var user = new User { Name = "Test", Email = email };
            userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);

            var result = await service.GetUserByEmailAsync(email);

            result.Should().NotBeNull();
            result.Email.Should().Be(email);
            userRepoMock.Verify(r => r.GetByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnNull_WhenNotExists()
        {
            const string email = "unknown@example.com";
            userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((User?)null);

            var result = await service.GetUserByEmailAsync(email);

            result.Should().BeNull();
            userRepoMock.Verify(r => r.GetByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnListOfUsers()
        {
            var users = new List<User>
            {
                new() { Id = 1, Name = "Alice" },
                new() { Id = 2, Name = "Bob" }
            };
            userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<GetUserQuery>())).ReturnsAsync((users, 2));

            var query = new GetUserQuery();

            var (resultList, totalCount) = await service.GetAllAsync(query);

            resultList.Should().HaveCount(2);
            resultList.Should().ContainSingle(u => u.Name == "Alice");
            resultList.Should().ContainSingle(u => u.Name == "Bob");
            totalCount.Should().Be(2);

            userRepoMock.Verify(r => r.GetAllAsync(query), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenExists()
        {
            var user = new User { Id = 1, Name = "Charlie" };
            userRepoMock.Setup(r => r.GetByIdAsync(1, true, true)).ReturnsAsync(user);

            var result = await service.GetUserByIdAsync(1, includeTeam: true, includeBankAccounts: true);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Name.Should().Be("Charlie");

            userRepoMock.Verify(r => r.GetByIdAsync(1, true, true), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            userRepoMock.Setup(r => r.GetByIdAsync(999, false, false)).ReturnsAsync((User?)null);

            var result = await service.GetUserByIdAsync(999, includeTeam: false, includeBankAccounts: false);

            result.Should().BeNull();
            userRepoMock.Verify(r => r.GetByIdAsync(999, false, false), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnUpdatedUser()
        {
            var updatedUser = new User
            {
                Id = 1,
                Name = "Dave",
                IsActive = false,
                TeamId = 2,
                Role = Role.Admin
            };

            userRepoMock
                .Setup(r => r.UpdateAsync(1, "Dave", true, 2, Role.Admin))
                .ReturnsAsync(updatedUser);

            var result = await service.UpdateUserAsync(1, "Dave", isActive: true, teamId: 2, role: Role.Admin);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.IsActive.Should().BeFalse(); // Echoed back as in the mock
            result.TeamId.Should().Be(2);
            result.Role.Should().Be(Role.Admin);

            userRepoMock.Verify(r => r.UpdateAsync(1, "Dave", true, 2, Role.Admin), Times.Once);
        }
    }
}
