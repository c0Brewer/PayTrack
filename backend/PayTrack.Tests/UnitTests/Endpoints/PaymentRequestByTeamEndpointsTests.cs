using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PayTrack.Application.Dto.Pagination;
using PayTrack.Application.Dto.PaymentRequestByTeam;
using PayTrack.Application.Dto.User;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class PaymentRequestByTeamEndpointsTests(PaymentRequestByTeamApiFactory factory)
        : IClassFixture<PaymentRequestByTeamApiFactory>
    {
        private readonly PaymentRequestByTeamApiFactory _factory = factory;

        // ----------------------------
        // GET ALL
        // ----------------------------
        [Fact]
        public async Task GetPaymentRequests_ReturnsOk()
        {
            // Arrange
            var list = new List<PaymentRequestByTeam>
            {
                new() { Id = 1, Amount = 100 },
                new() { Id = 2, Amount = 200 }
            };

            _factory.AuthServiceMock
                .Setup(a => a.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync(new User { Id = 1, Role = Role.Admin });

            _factory.ServiceMock
                .Setup(s => s.GetAllAsync(It.IsAny<GetPaymentRequestByTeamQuery>()))
                .ReturnsAsync((list, list.Count));

            _factory.ServiceMock
                .Setup(s => s.ValidateQuery(It.IsAny<GetPaymentRequestByTeamQuery>(), It.IsAny<User>()))
                .Returns(true);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/transaction/team");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content
                .ReadFromJsonAsync<PaginatedResponse<PaymentRequestByTeamDto>>();

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
        }

        // ----------------------------
        // GET BY ID
        // ----------------------------
        [Fact]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            // Arrange
            var entity = new PaymentRequestByTeam { Id = 1, Amount = 100 };

            _factory.ServiceMock
                .Setup(s => s.GetPaymentRequestByTeamByIdAsync(1, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync(entity);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/transaction/team/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<PaymentRequestByTeamDto>();
            dto.Should().NotBeNull();
            dto.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            _factory.ServiceMock
                .Setup(s => s.GetPaymentRequestByTeamByIdAsync(999, It.IsAny<GetPaymentRequestByTeamQueryById>()))
                .ReturnsAsync((PaymentRequestByTeam?)null);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/transaction/team/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ----------------------------
        // CREATE
        // ----------------------------
        [Fact]
        public async Task Create_ReturnsOk()
        {
            // Arrange
            var user = new User { Id = 123 };
            var team = new Team { Id = 123 };

            var created = new PaymentRequestByTeam
            {
                Id = 1,
                Amount = 50,
            };

            _factory.AuthServiceMock
                .Setup(a => a.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync(user);

            _factory.ServiceMock
                .Setup(s => s.CreatePaymentRequestByTeamAsync(
                    It.IsAny<int>(),
                    user.Id,
                    It.IsAny<int>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int?>()))
                .ReturnsAsync(created);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var requestDto = new CreatePaymentRequestByTeamDto(
                new()
                {
                    TeamId = 123,
                    Amount = 50,
                    PurposeOfPayment = "test 123",
                    PaidAt = DateTime.Today,
                },
                UserToAssignToId: 0,
                DueDate: DateTime.Today.AddDays(7));

            // Act
            var response = await client.PostAsJsonAsync("api/v1/transaction/team", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<PaymentRequestByTeamDto>();
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Create_ReturnsNotFound_WhenCurrentUserIsNull()
        {
            // Arrange
            _factory.AuthServiceMock
                .Setup(a => a.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync((User?)null);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var requestDto = new CreatePaymentRequestByTeamDto(
                new()
                {
                    TeamId = 1,
                    Amount = 50,
                    PurposeOfPayment = "test",
                    PaidAt = DateTime.Today,
                },
                UserToAssignToId: 1,
                DueDate: DateTime.Today.AddDays(7));

            // Act
            var response = await client.PostAsJsonAsync("api/v1/transaction/team", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ----------------------------
        // MARK AS PAID
        // ----------------------------
        [Fact]
        public async Task MarkAsPaid_ReturnsOk()
        {
            // Arrange
            var updated = new PaymentRequestByTeam { Id = 7, Status = TransactionStatus.Paid };

            _factory.AuthServiceMock
                .Setup(a => a.GetCurrentUser())
                .ReturnsAsync(new User { Id = 1, Role = Role.Admin });

            _factory.ServiceMock
                .Setup(s => s.MarkAsPaidAsync(7, 1, It.IsAny<string?>()))
                .ReturnsAsync(updated);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var dto = new MarkAsPaidPaymentRequestByTeamDto("Payment manually approved and processed.");

            // Act
            var response = await client.PostAsJsonAsync("api/v1/transaction/team/7/mark-as-paid", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<PaymentRequestByTeamDto>();
            result.Should().NotBeNull();
            result!.Id.Should().Be(7);

            _factory.ServiceMock.Verify(
                s => s.MarkAsPaidAsync(7, 1, "Payment manually approved and processed."),
                Times.Once);
        }

        [Fact]
        public async Task MarkAsPaid_ReturnsNotFound_WhenCurrentUserIsNull()
        {
            // Arrange
            _factory.AuthServiceMock
                .Setup(a => a.GetCurrentUser())
                .ReturnsAsync((User?)null);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var dto = new MarkAsPaidPaymentRequestByTeamDto(null);

            // Act
            var response = await client.PostAsJsonAsync("api/v1/transaction/team/1/mark-as-paid", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task MarkAsPaid_ReturnsBadRequest_WhenInvalidState()
        {
            // Arrange
            _factory.AuthServiceMock
                .Setup(a => a.GetCurrentUser())
                .ReturnsAsync(new User { Id = 1, Role = Role.Admin });

            _factory.ServiceMock
                .Setup(s => s.MarkAsPaidAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ThrowsAsync(new InvalidStateException("Cannot mark a transaction as Paid when its current status is Paid."));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var dto = new MarkAsPaidPaymentRequestByTeamDto(null);

            // Act
            var response = await client.PostAsJsonAsync("api/v1/transaction/team/1/mark-as-paid", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ----------------------------
        // UPDATE
        // ----------------------------
        [Fact]
        public async Task Update_ReturnsOk()
        {
            // Arrange
            var updated = new PaymentRequestByTeam { Id = 1, Amount = 999 };

            _factory.ServiceMock
                .Setup(s => s.UpdatePaymentRequestByTeamAsync(
                    1,
                    It.IsAny<int?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(updated);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var dto = new UpdatePaymentRequestByTeamDto
            (
                new(0, 0, "111", DateTime.Today)
            );

            // Act
            var response = await client.PutAsJsonAsync("api/v1/transaction/team/1", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<PaymentRequestByTeamDto>();
            result!.Amount.Should().Be(999);
        }
    }
    public class PaymentRequestByTeamApiFactory : WebApplicationFactory<Program>
    {
        public Mock<IPaymentRequestByTeamService> ServiceMock { get; } = new();
        public Mock<IAuthService> AuthServiceMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("Admin")
                    .AddScheme<AuthenticationSchemeOptions, AdminAuthHandler>("Admin", _ => { });

                _ = services.AddAuthorization(_ => { });

                // DB

                // Remove real DbContext (prevents Postgres connection)
                var dbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (dbDescriptor is not null)
                    services.Remove(dbDescriptor);

                // Replace with in-memory DB (no connection needed)
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));

                // Remove the real ITeamService registration coming from Program.cs
                var serviceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IPaymentRequestByTeamService));

                if (serviceDescriptor is not null)
                    services.Remove(serviceDescriptor);

                var serviceDescriptorAuth = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IAuthService));

                if (serviceDescriptorAuth is not null)
                    services.Remove(serviceDescriptorAuth);


                services.AddSingleton(ServiceMock.Object);
                services.AddSingleton(AuthServiceMock.Object);
            });
        }
    }

}
