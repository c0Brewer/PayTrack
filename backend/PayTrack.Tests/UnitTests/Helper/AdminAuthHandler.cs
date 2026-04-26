using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Helper
{
    /// <summary>
    /// Circumvents the authentication to make our tests not be dependent on login
    /// </summary>
    public class AdminAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "AdminUser"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, nameof(Role.Admin))
            };

            var identity = new ClaimsIdentity(claims, "Admin");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Admin");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
