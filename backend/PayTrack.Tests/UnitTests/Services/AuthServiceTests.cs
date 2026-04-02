using System.Security.Claims;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Moq;
using PayTrack.Application.Dto.Auth;
using PayTrack.Application.Exceptions;
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
        private readonly Mock<IHttpContextAccessor> httpContextMock;
        private readonly GoogleJsonWebSignature.Payload payloadToReturn;

        public AuthServiceTests()
        {
            jwtMock = new Mock<IJwtService>();
            userMock = new Mock<IUserService>();
            httpContextMock = new Mock<IHttpContextAccessor>();

            this.payloadToReturn = new GoogleJsonWebSignature.Payload
            {
                Email = "test@example.com",
                Name = "Test User",
                Picture = "pic.png"
            };

            service = new TestAuthService(jwtMock.Object, userMock.Object, httpContextMock.Object, payloadToReturn);
        }

        [Fact]
        public async Task GetCurrentUser_ReturnsUser_WhenClaimExists()
        {
            // Arrange
            const string expectedEmail = "test@example.com";
            var user = new User { Email = expectedEmail, Name = "Test User" };

            var claims = new[] { new Claim(ClaimTypes.Email, expectedEmail) };
            var identity = new ClaimsIdentity(claims, "mock");
            var principal = new ClaimsPrincipal(identity);

            httpContextMock.Setup(h => h.HttpContext).Returns(new DefaultHttpContext
            {
                User = principal
            });

            userMock.Setup(u => u.GetUserByEmailAsync(expectedEmail)).ReturnsAsync(user);

            // Act
            var result = await service.GetCurrentUser();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedEmail, result.Email);
            userMock.Verify(u => u.GetUserByEmailAsync(expectedEmail), Times.Once);
        }

        [Fact]
        public async Task GetCurrentUser_ThrowsInternalErrorException_WhenClaimMissing()
        {
            // Arrange
            httpContextMock.Setup(h => h.HttpContext).Returns(new DefaultHttpContext
            {
                User = new ClaimsPrincipal() // no claims
            });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InternalErrorException>(service.GetCurrentUser);
            Assert.Contains("ClaimTypes", ex.Message);
        }

        [Fact]
        public async Task GetCurrentUser_ThrowsInternalErrorException_WhenUserInClaimMissing()
        {
            // Arrange
            httpContextMock.Setup(h => h.HttpContext!.User).Returns(new ClaimsPrincipal());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InternalErrorException>(service.GetCurrentUser);
            Assert.Contains("ClaimTypes", ex.Message);
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
                ProfilePictureUrl = payloadToReturn.Picture,
                Role = Role.RegularUser
            };

            // Mock the static ValidateAsync (simulate successful Google token)
            userMock.Setup(u => u.GetUserByEmailAsync(payloadToReturn.Email)).ReturnsAsync(user);

            jwtMock.Setup(j => j.GenerateJWTToken(payloadToReturn.Email, user.Role))
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
                ProfilePictureUrl = payloadToReturn.Picture,
                Role = Role.RegularUser
            };

            userMock.Setup(u => u.GetUserByEmailAsync(payloadToReturn.Email))
                .ReturnsAsync((User?)null); // user does not exist

            userMock.Setup(u => u.CreateUserAsync(payloadToReturn.Name, payloadToReturn.Email, payloadToReturn.Picture))
                .ReturnsAsync(user);

            jwtMock.Setup(j => j.GenerateJWTToken(payloadToReturn.Email, user.Role))
                .ReturnsAsync("jwt-token");

            // Act
            var result = await service.GoogleValidateCallback(callback);

            // Assert
            Assert.Equal("jwt-token", result);
            userMock.Verify(u => u.CreateUserAsync(payloadToReturn.Name, payloadToReturn.Email, payloadToReturn.Picture), Times.Once);
        }
    }

    public class TestAuthService(IJwtService jwtService, IUserService userService, IHttpContextAccessor httpContextAccessor, GoogleJsonWebSignature.Payload payloadToReturn) : AuthService(jwtService, userService, httpContextAccessor)
    {
        private readonly GoogleJsonWebSignature.Payload payloadToReturn = payloadToReturn;

        protected override Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken)
        {
            // Just return the payload we want for tests
            return Task.FromResult(payloadToReturn);
        }
    }
}
