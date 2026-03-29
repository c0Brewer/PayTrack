using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Moq;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Implementation;

namespace PayTrack.Tests.UnitTests.Services
{
    public class JwtServiceTests
    {
        private readonly Mock<IConfiguration> configMock;

        public JwtServiceTests()
        {
            configMock = new Mock<IConfiguration>();
        }

        [Fact]
        public async Task GenerateJWTToken_ReturnsToken_WhenSecretExists()
        {
            // Arrange
            const string secret = "my-super-secret-key-which-has-to-have-a-certain-length";
            const string email = "test@example.com";

            configMock.Setup(c => c["JWT:Secret"]).Returns(secret);
            var service = new JwtService(configMock.Object);

            // Act
            var token = await service.GenerateJWTToken(email);

            // Assert
            Assert.False(string.IsNullOrEmpty(token));

            // Validate the token
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            Assert.Contains(jwt.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Email && c.Value == email);
        }

        [Fact]
        public async Task GenerateJWTToken_ThrowsInternalErrorException_WhenSecretIsMissing()
        {
            // Arrange
            configMock.Setup(c => c["JWT:Secret"]).Returns((string?)null);
            var service = new JwtService(configMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InternalErrorException>(async () => await service.GenerateJWTToken("test@example.com"));
        }
    }
}
