using System.Security.Claims;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using PayTrack.Application.Dto.Auth;
using PayTrack.Application.Dto.User;
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
        private readonly Mock<IHttpClientFactory> httpClientFactoryMock;
        private readonly Mock<IConfiguration> configurationMock;
        private readonly GoogleJsonWebSignature.Payload payloadToReturn;

        public AuthServiceTests()
        {
            jwtMock = new Mock<IJwtService>();
            userMock = new Mock<IUserService>();
            httpContextMock = new Mock<IHttpContextAccessor>();
            httpClientFactoryMock = new Mock<IHttpClientFactory>();
            configurationMock = new Mock<IConfiguration>();

            this.payloadToReturn = new GoogleJsonWebSignature.Payload
            {
                Email = "test@example.com",
                Name = "Test User",
                Picture = "pic.png"
            };

            service = new TestAuthService(
                jwtMock.Object,
                userMock.Object,
                httpContextMock.Object,
                httpClientFactoryMock.Object,
                configurationMock.Object,
                payloadToReturn);
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

            userMock.Setup(u => u.GetUserByEmailAsync(expectedEmail, It.IsAny<GetUserQueryById?>())).ReturnsAsync(user);

            // Act
            var result = await service.GetCurrentUser();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedEmail, result.Email);
            userMock.Verify(u => u.GetUserByEmailAsync(expectedEmail, It.IsAny<GetUserQueryById?>()), Times.Once);
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
            var ex = await Assert.ThrowsAsync<InternalErrorException>(() => service.GetCurrentUser());
            Assert.Contains("ClaimTypes", ex.Message);
        }

        [Fact]
        public async Task GetCurrentUser_ThrowsInternalErrorException_WhenUserInClaimMissing()
        {
            // Arrange
            httpContextMock.Setup(h => h.HttpContext!.User).Returns(new ClaimsPrincipal());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InternalErrorException>(() => service.GetCurrentUser());
            Assert.Contains("ClaimTypes", ex.Message);
        }

        [Fact]
        public async Task GoogleValidateCallback_ReturnsJwt_WhenUserExists()
        {
            // Arrange
            var callback = new GoogleAuthCallbackDto("valid-code");
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
            var callback = new GoogleAuthCallbackDto("valid-code");
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

        [Fact]
        public async Task GoogleValidateCallback_ThrowsLockedException_WhenUserIsInactive()
        {
            // Arrange
            var callback = new GoogleAuthCallbackDto("valid-code");
            var user = new User
            {
                Email = payloadToReturn.Email,
                Name = payloadToReturn.Name,
                ProfilePictureUrl = payloadToReturn.Picture,
                Role = Role.RegularUser,
                IsActive = false
            };

            userMock.Setup(u => u.GetUserByEmailAsync(payloadToReturn.Email))
                .ReturnsAsync(user);

            // Act
            await Assert.ThrowsAsync<LockedException>(async () => await service.GoogleValidateCallback(callback));
        }
    }

    public class TestAuthService(
        IJwtService jwtService,
        IUserService userService,
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        GoogleJsonWebSignature.Payload payloadToReturn) : AuthService(jwtService, userService, httpContextAccessor, httpClientFactory, configuration)
    {
        private readonly GoogleJsonWebSignature.Payload payloadToReturn = payloadToReturn;

        protected override Task<string> ExchangeCodeForIdTokenAsync(string code)
        {
            return Task.FromResult("valid-token");
        }

        protected override Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken)
        {
            // Just return the payload we want for tests
            return Task.FromResult(payloadToReturn);
        }
    }
}
