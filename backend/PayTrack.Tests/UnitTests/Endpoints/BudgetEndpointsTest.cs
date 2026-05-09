//AI helped with the test cases

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Dto.Pagination;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class BudgetEndpointsTests(BudgetApiFactory factory) : IClassFixture<BudgetApiFactory>
    {
        private readonly BudgetApiFactory factory = factory;

        [Fact]
        public async Task GetBudgets_ReturnsOk_WithJwtOnly()
        {
            // Arrange
            var budgets = new List<Budget>
            {
                CreateBudget(1, "Aero budget"),
                CreateBudget(2, "Electronics budget"),
            };
            this.factory.ServiceMock
                .Setup(s => s.GetBudgetsAsync(It.IsAny<GetBudgetQuery?>()))
                .ReturnsAsync((budgets, budgets.Count));

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/budget?Limit=5&Offset=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<BudgetDto>>();
            result!.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Limit.Should().Be(5);
            result.Offset.Should().Be(10);
        }

        [Fact]
        public async Task GetBudgets_ForwardsQueryParamsToService()
        {
            // Arrange
            this.factory.ServiceMock
                .Setup(s => s.GetBudgetsAsync(It.IsAny<GetBudgetQuery?>()))
                .ReturnsAsync((new List<Budget>(), 0));

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            await client.GetAsync("api/v1/budget?Name=Aero&TeamId=2&CostCentreId=3&SeasonId=4&TargetAmount=100&Limit=5&Offset=10");

            // Assert
            this.factory.ServiceMock.Verify(s => s.GetBudgetsAsync(
                It.Is<GetBudgetQuery?>(q =>
                    q != null &&
                    q.Name == "Aero" &&
                    q.TeamId == 2 &&
                    q.CostCentreId == 3 &&
                    q.SeasonId == 4 &&
                    q.TargetAmount == 100 &&
                    q.Limit == 5 &&
                    q.Offset == 10)), Times.Once);
        }

        [Fact]
        public async Task GetBudgetById_ReturnsOk_WhenExists()
        {
            // Arrange
            this.factory.ServiceMock
                .Setup(s => s.GetBudgetByIdAsync(1))
                .ReturnsAsync(CreateBudget(1, "Aero budget"));

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/budget/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<BudgetDto>();
            result!.Name.Should().Be("Aero budget");
        }

        [Fact]
        public async Task GetBudgetById_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            this.factory.ServiceMock
                .Setup(s => s.GetBudgetByIdAsync(999))
                .ReturnsAsync((Budget?)null);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/budget/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateBudget_ReturnsOk_WhenAdminRole()
        {
            // Arrange
            var requestDto = new CreateBudgetRequestDto(
                "2026 budget",
                "Season budget",
                2,
                3,
                4,
                1000,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 12, 31));

            this.factory.ServiceMock
                .Setup(s => s.CreateBudgetAsync(
                    requestDto.Name,
                    requestDto.Description,
                    requestDto.TeamId,
                    requestDto.CostCentreId,
                    requestDto.SeasonId,
                    requestDto.TargetAmount,
                    requestDto.PeriodStart,
                    requestDto.PeriodEnd))
                .ReturnsAsync(CreateBudget(3, requestDto.Name));

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.PostAsJsonAsync("api/v1/budget", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<BudgetDto>();
            result!.Name.Should().Be("2026 budget");
        }

        [Fact]
        public async Task CreateBudget_ReturnsForbidden_WhenRegularUserRole()
        {
            // Arrange
            var requestDto = new CreateBudgetRequestDto(
                "2026 budget",
                null,
                2,
                3,
                4,
                1000,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 12, 31));

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.PostAsJsonAsync("api/v1/budget", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateBudget_ReturnsOk_WhenAdminRole()
        {
            // Arrange
            var requestDto = new UpdateBudgetRequestDto(
                "Updated budget",
                "Updated",
                2,
                3,
                4,
                1500,
                new DateTime(2026, 2, 1),
                new DateTime(2026, 11, 30));
            var updatedName = "Updated budget";

            this.factory.ServiceMock
                .Setup(s => s.UpdateBudgetAsync(
                    1,
                    requestDto.Name,
                    requestDto.Description,
                    requestDto.TeamId,
                    requestDto.CostCentreId,
                    requestDto.SeasonId,
                    requestDto.TargetAmount,
                    requestDto.PeriodStart,
                    requestDto.PeriodEnd))
                .ReturnsAsync(CreateBudget(1, updatedName));

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.PutAsJsonAsync("api/v1/budget/1", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<BudgetDto>();
            result!.Name.Should().Be("Updated budget");
        }

        [Fact]
        public async Task UpdateBudget_ReturnsBadRequest_WhenServiceThrowsInvalidState()
        {
            // Arrange
            var requestDto = new UpdateBudgetRequestDto(null, null, null, null, null, null, null, null);
            this.factory.ServiceMock
                .Setup(s => s.UpdateBudgetAsync(1, null, null, null, null, null, null, null, null))
                .ThrowsAsync(new InvalidStateException("Budget period end must be after period start."));

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.PutAsJsonAsync("api/v1/budget/1", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeleteBudget_ReturnsNoContent_WhenAdminRole()
        {
            // Arrange
            this.factory.ServiceMock.Setup(s => s.DeleteBudgetAsync(1)).Returns(Task.CompletedTask);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.DeleteAsync("api/v1/budget/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteBudget_ReturnsForbidden_WhenRegularUserRole()
        {
            // Arrange
            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.DeleteAsync("api/v1/budget/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        private static Budget CreateBudget(int id, string name)
        {
            return new Budget
            {
                Id = id,
                Name = name,
                Description = "Budget description",
                TeamId = 2,
                CostCentreId = 3,
                SeasonId = 4,
                TargetAmount = 1000,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 12, 31),
            };
        }
    }

    public class BudgetApiFactory : WebApplicationFactory<Program>
    {
        public Mock<IBudgetService> ServiceMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, DynamicTestAuthHandler>("Test", _ => { });

                services.AddAuthorization(_ => { });

                var dbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbDescriptor is not null)
                {
                    services.Remove(dbDescriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("BudgetTestDb"));

                var serviceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IBudgetService));
                if (serviceDescriptor is not null)
                {
                    services.Remove(serviceDescriptor);
                }

                services.AddSingleton(this.ServiceMock.Object);
            });
        }
    }
}
