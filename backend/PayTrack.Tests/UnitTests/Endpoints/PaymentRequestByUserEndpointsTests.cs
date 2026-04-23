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
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class PaymentRequestByUserEndpointsTests(PaymentRequestByUserApiFactory factory)
        : IClassFixture<PaymentRequestByUserApiFactory>
    {
        private readonly PaymentRequestByUserApiFactory _factory = factory;

        // ----------------------------
        // GET ALL
        // ----------------------------
        [Fact]
        public async Task GetPaymentRequests_ReturnsOk()
        {
            // Arrange
            var list = new List<PaymentRequestByUser>
            {
                new() { Id = 1, Amount = 100, InvoiceNumber = "123" },
                new() { Id = 2, Amount = 200, InvoiceNumber = "456" }
            };

            _factory.ServiceMock
                .Setup(s => s.GetAllAsync(It.IsAny<GetPaymentRequestByUserQuery>()))
                .ReturnsAsync((list, list.Count));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/transaction/user");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content
                .ReadFromJsonAsync<PaginatedResponse<PaymentRequestByUserDto>>();

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
            var entity = new PaymentRequestByUser { Id = 1, Amount = 100, InvoiceNumber = "123" };

            _factory.ServiceMock
                .Setup(s => s.GetPaymentRequestByUserByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(entity);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/transaction/user/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<PaymentRequestByUserDto>();
            dto.Should().NotBeNull();
            dto.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            _factory.ServiceMock
                .Setup(s => s.GetPaymentRequestByUserByIdAsync(999, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync((PaymentRequestByUser?)null);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/transaction/user/999");

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

            var created = new PaymentRequestByUser
            {
                Id = 1,
                Amount = 50,
                InvoiceNumber = "123"
            };

            _factory.AuthServiceMock
                .Setup(a => a.GetCurrentUser())
                .ReturnsAsync(user);

            _factory.ServiceMock
                .Setup(s => s.CreatePaymentRequestByUserAsync(
                    user.Id,
                    It.IsAny<int>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<IFormFile>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<PayoutType>(),
                    It.IsAny<int>()))
                .ReturnsAsync(created);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var content = new MultipartFormDataContent();

            // -----------------------
            // FILE (IFormFile)
            // -----------------------
            var fileBytes = new byte[] { 1, 2, 3 };
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            content.Add(fileContent, "Receipt", "test.pdf");

            // -----------------------
            // TRANSACTION (nested DTO)
            // -----------------------
            content.Add(new StringContent("0"), "Transaction.TeamId");
            content.Add(new StringContent("50"), "Transaction.Amount");
            content.Add(new StringContent("TestPurpose"), "Transaction.PurposeOfPayment");
            content.Add(new StringContent(DateTime.Today.ToString("o")), "Transaction.PaidAt");

            // -----------------------
            // ROOT DTO FIELDS
            // -----------------------
            content.Add(new StringContent("123"), "InvoiceNumber");
            content.Add(new StringContent("MyComment"), "Comment");
            content.Add(new StringContent(((int)PayoutType.External).ToString()), "PayoutType");
            content.Add(new StringContent("0"), "BankAccountId");

            // Act
            var response = await client.PostAsync("api/v1/transaction/user", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<PaymentRequestByUserDto>();
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        // ----------------------------
        // UPDATE
        // ----------------------------
        [Fact]
        public async Task Update_ReturnsOk()
        {
            // Arrange
            var updated = new PaymentRequestByUser { Id = 1, Amount = 999, InvoiceNumber = "123" };

            _factory.ServiceMock
                .Setup(s => s.UpdatePaymentRequestByUserAsync(
                    1,
                    It.IsAny<int?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<PayoutType?>(),
                    It.IsAny<int?>()))
                .ReturnsAsync(updated);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var dto = new UpdatePaymentRequestByUserDto
            (
                new(0, 0, "111", DateTime.Today),
                "123",
                null,
                PayoutType.External,
                0
            );

            // Act
            var response = await client.PutAsJsonAsync("api/v1/transaction/user/1", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<PaymentRequestByUserDto>();
            result!.Amount.Should().Be(999);
        }

        // ----------------------------
        // FILE
        // ----------------------------
        [Fact]
        public async Task GetReceipt_ReturnsFile()
        {
            // Arrange
            var fileBytes = new byte[] { 1, 2, 3 };

            _factory.ServiceMock
                .Setup(s => s.GetReceiptForPaymentRequestByUserByIdAsync(1))
                .ReturnsAsync(fileBytes);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/transaction/user/1/receipt");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsByteArrayAsync();
            content.Should().Equal(fileBytes);
        }
    }
    public class PaymentRequestByUserApiFactory : WebApplicationFactory<Program>
    {
        public Mock<IPaymentRequestByUserService> ServiceMock { get; } = new();
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
                    d => d.ServiceType == typeof(IPaymentRequestByUserService));

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
