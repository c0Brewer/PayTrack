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
using PayTrack.Application.Dto.Pagination;
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class MyInvoicesEndpointsTests(MyInvoicesApiFactory factory)
        : IClassFixture<MyInvoicesApiFactory>
    {
        private readonly MyInvoicesApiFactory _factory = factory;

        // ----------------------------
        // GET ALL
        // ----------------------------
        [Fact]
        public async Task GetMyInvoices_ReturnsOk_WithOwnInvoices()
        {
            // Arrange
            var user = new User { Id = 1 };
            var list = new List<PaymentRequestByUser>
            {
                new() { Id = 1, Amount = 100, InvoiceNumber = "INV-001", UserId = 1 },
                new() { Id = 2, Amount = 200, InvoiceNumber = "INV-002", UserId = 1 },
            };

            _factory.AuthServiceMock.Setup(a => a.GetCurrentUser()).ReturnsAsync(user);
            _factory.ServiceMock
                .Setup(s => s.GetAllAsync(It.IsAny<GetPaymentRequestByUserQuery>()))
                .ReturnsAsync((list, list.Count));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/my-invoices");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content
                .ReadFromJsonAsync<PaginatedResponse<PaymentRequestByUserDto>>();

            result.Should().NotBeNull();
            result!.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetMyInvoices_ForcesCurrentUserId_IgnoringClientSuppliedUserId()
        {
            // Arrange
            var user = new User { Id = 99 };
            var list = new List<PaymentRequestByUser>
            {
                new() { Id = 1, Amount = 100, InvoiceNumber = "INV-001", UserId = 99 },
            };

            _factory.AuthServiceMock.Setup(a => a.GetCurrentUser()).ReturnsAsync(user);
            _factory.ServiceMock
                .Setup(s => s.GetAllAsync(It.Is<GetPaymentRequestByUserQuery>(q => q.UserId == 99)))
                .ReturnsAsync((list, list.Count));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act — client tries to supply a different UserId; handler must override it
            var response = await client.GetAsync("api/v1/my-invoices?UserId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _factory.ServiceMock.Verify(
                s => s.GetAllAsync(It.Is<GetPaymentRequestByUserQuery>(q => q.UserId == 99)),
                Times.Once);
        }

        // ----------------------------
        // GET BY ID
        // ----------------------------
        [Fact]
        public async Task GetById_ReturnsOk_WhenInvoiceBelongsToCurrentUser()
        {
            // Arrange
            var currentUser = new User { Id = 1 };
            var invoice = new PaymentRequestByUser
            {
                Id = 5,
                Amount = 100,
                InvoiceNumber = "INV-005",
                UserId = 1,
            };

            _factory.AuthServiceMock.Setup(a => a.GetCurrentUser()).ReturnsAsync(currentUser);
            _factory.ServiceMock
                .Setup(s => s.GetPaymentRequestByUserByIdAsync(5, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(invoice);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/my-invoices/5");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<PaymentRequestByUserDto>();
            dto.Should().NotBeNull();
            dto!.Id.Should().Be(5);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenInvoiceDoesNotExist()
        {
            // Arrange
            var currentUser = new User { Id = 1 };

            _factory.AuthServiceMock.Setup(a => a.GetCurrentUser()).ReturnsAsync(currentUser);
            _factory.ServiceMock
                .Setup(s => s.GetPaymentRequestByUserByIdAsync(999, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync((PaymentRequestByUser?)null);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/my-invoices/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetById_ReturnsForbid_WhenInvoiceBelongsToAnotherUser()
        {
            // Arrange
            var currentUser = new User { Id = 1 };
            var otherUsersInvoice = new PaymentRequestByUser
            {
                Id = 5,
                Amount = 100,
                InvoiceNumber = "INV-005",
                UserId = 2,
            };

            _factory.AuthServiceMock.Setup(a => a.GetCurrentUser()).ReturnsAsync(currentUser);
            _factory.ServiceMock
                .Setup(s => s.GetPaymentRequestByUserByIdAsync(5, It.IsAny<GetPaymentRequestByUserQueryById>()))
                .ReturnsAsync(otherUsersInvoice);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/my-invoices/5");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ----------------------------
        // RECEIPT
        // ----------------------------
        [Fact]
        public async Task GetReceipt_ReturnsForbid_WhenInvoiceBelongsToAnotherUser()
        {
            // Arrange
            var currentUser = new User { Id = 1 };
            var otherUsersInvoice = new PaymentRequestByUser
            {
                Id = 5,
                Amount = 100,
                InvoiceNumber = "INV-005",
                UserId = 2,
            };

            _factory.AuthServiceMock.Setup(a => a.GetCurrentUser()).ReturnsAsync(currentUser);
            _factory.ServiceMock
                .Setup(s => s.GetPaymentRequestByUserByIdAsync(5, It.IsAny<GetPaymentRequestByUserQueryById?>()))
                .ReturnsAsync(otherUsersInvoice);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/my-invoices/5/receipt");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetReceipt_ReturnsFile_WhenInvoiceBelongsToCurrentUser()
        {
            // Arrange
            var currentUser = new User { Id = 1 };
            var invoice = new PaymentRequestByUser
            {
                Id = 5,
                Amount = 100,
                InvoiceNumber = "INV-005",
                UserId = 1,
            };
            var fileBytes = new byte[] { 1, 2, 3 };

            _factory.AuthServiceMock.Setup(a => a.GetCurrentUser()).ReturnsAsync(currentUser);
            _factory.ServiceMock
                .Setup(s => s.GetPaymentRequestByUserByIdAsync(5, It.IsAny<GetPaymentRequestByUserQueryById?>()))
                .ReturnsAsync(invoice);
            _factory.ServiceMock
                .Setup(s => s.GetReceiptForPaymentRequestByUserByIdAsync(5))
                .ReturnsAsync((fileBytes, "application/pdf"));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/my-invoices/5/receipt");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsByteArrayAsync();
            content.Should().Equal(fileBytes);
        }
    }

    public class MyInvoicesUnauthEndpointsTests
    {
        [Fact]
        public async Task GetMyInvoices_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            // Arrange — use default JWT Bearer auth without overriding; no valid token → 401
            await using var factory = new MyInvoicesUnauthApiFactory();
            var client = factory.CreateClient();

            // Act — no Authorization header
            var response = await client.GetAsync("api/v1/my-invoices");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    public class MyInvoicesApiFactory : WebApplicationFactory<Program>
    {
        public Mock<IPaymentRequestByUserService> ServiceMock { get; } = new();
        public Mock<IAuthService> AuthServiceMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                _ = services.AddAuthorization(_ => { });

                var dbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbDescriptor is not null)
                    services.Remove(dbDescriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("MyInvoicesTestDb"));

                var serviceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IPaymentRequestByUserService));
                if (serviceDescriptor is not null)
                    services.Remove(serviceDescriptor);

                var authServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IAuthService));
                if (authServiceDescriptor is not null)
                    services.Remove(authServiceDescriptor);

                services.AddSingleton(ServiceMock.Object);
                services.AddSingleton(AuthServiceMock.Object);
            });
        }
    }

    public class MyInvoicesUnauthApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                var dbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbDescriptor is not null)
                    services.Remove(dbDescriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("MyInvoicesUnauthTestDb"));

                var serviceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IPaymentRequestByUserService));
                if (serviceDescriptor is not null)
                    services.Remove(serviceDescriptor);

                var authServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IAuthService));
                if (authServiceDescriptor is not null)
                    services.Remove(authServiceDescriptor);

                services.AddSingleton(new Mock<IPaymentRequestByUserService>().Object);
                services.AddSingleton(new Mock<IAuthService>().Object);
            });
        }
    }
}
