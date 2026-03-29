using Google.Apis.Auth;
using Moq;
using PayTrack.Application.Dto.Auth;
using PayTrack.Application.Services.Implementation;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly AuthService service;
        private readonly Mock<IJwtService> jwtMock;
        private readonly Mock<IUserService> userMock;
        private readonly GoogleJsonWebSignature.Payload payloadToReturn;

        public AuthServiceTests()
        {
            jwtMock = new Mock<IJwtService>();
            userMock = new Mock<IUserService>();

            this.payloadToReturn = new GoogleJsonWebSignature.Payload
            {
                Email = "test@example.com",
                Name = "Test User",
                Picture = "pic.png"
            };

            service = new TestAuthService(jwtMock.Object, userMock.Object, payloadToReturn);
        }

        [Fact]
        public async Task GoogleValidateCallback_ReturnsJwt_WhenUserExists()
        {
            // Arrange
            var callback = new GoogleAuthCallbackDto("valid-token");
            var user = new User
            {
                Email = payloadToReturn.Email,
                Name = payloadToReturn.Name,
                ProfilePictureUrl = payloadToReturn.Picture
            };

            // Mock the static ValidateAsync (simulate successful Google token)
            userMock.Setup(u => u.GetUserByEmailAsync(payloadToReturn.Email)).ReturnsAsync(user);

            jwtMock.Setup(j => j.GenerateJWTToken(payloadToReturn.Email))
                .ReturnsAsync("jwt-token");

            // Act
            var result = await service.GoogleValidateCallback(callback);

            // Assert
            Assert.Equal("jwt-token", result);
            userMock.Verify(u => u.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GoogleValidateCallback_CreatesUser_WhenUserDoesNotExist()
        {
            // Arrange
            var callback = new GoogleAuthCallbackDto("valid-token");
            var user = new User
            {
                Email = payloadToReturn.Email,
                Name = payloadToReturn.Name,
                ProfilePictureUrl = payloadToReturn.Picture
            };

            userMock.Setup(u => u.GetUserByEmailAsync(payloadToReturn.Email))
                .ReturnsAsync((User?)null); // user does not exist

            userMock.Setup(u => u.CreateUserAsync(payloadToReturn.Name, payloadToReturn.Email, payloadToReturn.Picture))
                .ReturnsAsync(user);

            jwtMock.Setup(j => j.GenerateJWTToken(payloadToReturn.Email))
                .ReturnsAsync("jwt-token");

            // Act
            var result = await service.GoogleValidateCallback(callback);

            // Assert
            Assert.Equal("jwt-token", result);
            userMock.Verify(u => u.CreateUserAsync(payloadToReturn.Name, payloadToReturn.Email, payloadToReturn.Picture), Times.Once);
        }
    }

    public class TestAuthService(IJwtService jwtService, IUserService userService, GoogleJsonWebSignature.Payload payloadToReturn) : AuthService(jwtService, userService)
    {
        private readonly GoogleJsonWebSignature.Payload payloadToReturn = payloadToReturn;

        protected override Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken)
        {
            // Just return the payload we want for tests
            return Task.FromResult(payloadToReturn);
        }
    }
}
