using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PayTrack.Application.Dto.CostCentre;
using PayTrack.Application.Dto.Pagination;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class CostCentreEndpointsTests(CostCentreApiFactory factory) : IClassFixture<CostCentreApiFactory>
    {
        private readonly CostCentreApiFactory _factory = factory;

        // ── GET /cost-centre ─────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_ReturnsOk_WithJwtOnly()
        {
            // Arrange
            var costCentres = new List<CostCentre>
            {
                new() { Id = 1, Name = "Aero" },
                new() { Id = 2, Name = "Electronics" },
            };
            _factory.ServiceMock.Setup(s => s.GetAllAsync(It.IsAny<GetCostCentreQuery?>())).ReturnsAsync((costCentres, costCentres.Count));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/cost-centre");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<CostCentreDto>>();
            result!.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAll_ForwardsQueryParamsToService()
        {
            // Arrange
            _factory.ServiceMock
                .Setup(s => s.GetAllAsync(It.IsAny<GetCostCentreQuery?>()))
                .ReturnsAsync((new List<CostCentre>(), 0));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            await client.GetAsync("api/v1/cost-centre?Name=Aero&MinBudget=100&Limit=5&Offset=10");

            // Assert
            _factory.ServiceMock.Verify(s => s.GetAllAsync(
                It.Is<GetCostCentreQuery?>(q =>
                    q != null &&
                    q.Name == "Aero" &&
                    q.MinBudget == 100 &&
                    q.Limit == 5 &&
                    q.Offset == 10)), Times.Once);
        }

        // ── GET /cost-centre/{id} ─────────────────────────────────────────────

        [Fact]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            // Arrange
            var costCentre = new CostCentre { Id = 1, Name = "Aero" };
            _factory.ServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(costCentre);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/cost-centre/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<CostCentreDto>();
            result!.Name.Should().Be("Aero");
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _factory.ServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((CostCentre?)null);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/cost-centre/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ── POST /cost-centre ─────────────────────────────────────────────────

        [Fact]
        public async Task Create_ReturnsOk_WhenAdminRole()
        {
            // Arrange
            var requestDto = new CreateCostCentreRequestDto("Powertrain", "Engine costs", "#FF0000", null);
            var created = new CostCentre { Id = 3, Name = "Powertrain" };

            _factory.ServiceMock
                .Setup(s => s.CreateAsync("Powertrain", "Engine costs", "#FF0000", null))
                .ReturnsAsync(created);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.PostAsJsonAsync("api/v1/cost-centre", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<CostCentreDto>();
            result!.Name.Should().Be("Powertrain");
        }

        [Fact]
        public async Task Create_ReturnsForbidden_WhenRegularUserRole()
        {
            // Arrange
            var requestDto = new CreateCostCentreRequestDto("Powertrain", null, null, null);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.PostAsJsonAsync("api/v1/cost-centre", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ── PUT /cost-centre/{id} ─────────────────────────────────────────────

        [Fact]
        public async Task Update_ReturnsOk_WhenAdminRole()
        {
            // Arrange
            var requestDto = new UpdateCostCentreRequestDto("Updated Name", null, null, null, null);
            var updated = new CostCentre { Id = 1, Name = "Updated Name" };

            _factory.ServiceMock
                .Setup(s => s.UpdateAsync(1, "Updated Name", null, null, null, null))
                .ReturnsAsync(updated);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.PutAsJsonAsync("api/v1/cost-centre/1", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<CostCentreDto>();
            result!.Name.Should().Be("Updated Name");
        }

        [Fact]
        public async Task Update_ReturnsForbidden_WhenRegularUserRole()
        {
            // Arrange
            var requestDto = new UpdateCostCentreRequestDto("Updated Name", null, null, null, null);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.PutAsJsonAsync("api/v1/cost-centre/1", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenServiceThrowsInvalidState()
        {
            // Arrange
            var requestDto = new UpdateCostCentreRequestDto(null, null, null, null, null);

            _factory.ServiceMock
                .Setup(s => s.UpdateAsync(1, null, null, null, null, null))
                .ThrowsAsync(new InvalidStateException("A budget ID cannot appear in both BudgetsToUpsert and BudgetIdsToDelete."));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.PutAsJsonAsync("api/v1/cost-centre/1", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ── GET /cost-centre/{id}/delete-preview ──────────────────────────────

        [Fact]
        public async Task GetDeletePreview_ReturnsOk_WhenAdminRole()
        {
            // Arrange
            var preview = new DeleteCostCentrePreviewDto("Aero", 2, 5, 3, ["Team Alpha"]);
            _factory.ServiceMock.Setup(s => s.GetDeletePreviewAsync(1)).ReturnsAsync(preview);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/cost-centre/1/delete-preview");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<DeleteCostCentrePreviewDto>();
            result!.BudgetCount.Should().Be(2);
            result.TransactionCount.Should().Be(5);
            result.AffectedTeamNames.Should().ContainSingle(n => n == "Team Alpha");
        }

        [Fact]
        public async Task GetDeletePreview_ReturnsForbidden_WhenRegularUserRole()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/cost-centre/1/delete-preview");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ── DELETE /cost-centre/{id} ──────────────────────────────────────────

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenAdminRole()
        {
            // Arrange
            _factory.ServiceMock.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.DeleteAsync("api/v1/cost-centre/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_ReturnsForbidden_WhenRegularUserRole()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.DeleteAsync("api/v1/cost-centre/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }

    /// <summary>
    /// Assigns Admin role when the Authorization header value is "Admin", RegularUser otherwise.
    /// </summary>
    public class DynamicTestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Request.Headers.Authorization.ToString();
            var isAdmin = authHeader == "Admin";

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, isAdmin ? "AdminUser" : "TestUser"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, isAdmin ? nameof(Role.Admin) : nameof(Role.RegularUser)),
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    /// <summary>
    /// Uses the real API Program entry point, replaces ICostCentreService with a mock,
    /// and uses a single dynamic auth scheme that assigns role based on Authorization header value.
    /// </summary>
    public class CostCentreApiFactory : WebApplicationFactory<Program>
    {
        public Mock<ICostCentreService> ServiceMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                // Single scheme: assigns Admin role when header value is "Admin", RegularUser otherwise
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, DynamicTestAuthHandler>("Test", _ => { });

                services.AddAuthorization(_ => { });

                // DB
                var dbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbDescriptor is not null)
                    services.Remove(dbDescriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("CostCentreTestDb"));

                // Service
                var serviceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ICostCentreService));
                if (serviceDescriptor is not null)
                    services.Remove(serviceDescriptor);

                services.AddSingleton(ServiceMock.Object);
            });
        }
    }
}
