//AI helped with the test cases

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
using PayTrack.Application.Dto.User;
using PayTrack.Application.Dto.Transaction;
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
            var adminUser = new User { Id = 1, Role = Role.Admin };
            var list = new List<PaymentRequestByUser>
            {
                new() { Id = 1, Amount = 100, InvoiceNumber = "123" },
                new() { Id = 2, Amount = 200, InvoiceNumber = "456" }
            };

            _factory.AuthServiceMock.Setup(a => a.GetCurrentUser(It.IsAny<GetUserQueryById?>())).ReturnsAsync(adminUser);
            _factory.ServiceMock
                .Setup(s => s.ValidateQuery(It.IsAny<GetPaymentRequestByUserQuery>(), It.IsAny<User>()))
                .Returns(true);
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
            var adminUser = new User { Id = 1, Role = Role.Admin };
            var entity = new PaymentRequestByUser { Id = 1, Amount = 100, InvoiceNumber = "123" };

            _factory.AuthServiceMock.Setup(a => a.GetCurrentUser(It.IsAny<GetUserQueryById?>())).ReturnsAsync(adminUser);
            _factory.ServiceMock
                .Setup(s => s.GetPaymentRequestByUserByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById?>()))
                .ReturnsAsync(entity);
            _factory.ServiceMock
                .Setup(s => s.ValidateAccessToInvoice(It.IsAny<PaymentRequestByUser>(), It.IsAny<User>()))
                .Returns(true);

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
                .Setup(a => a.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
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
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>()))
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
            content.Add(new StringContent(((int)PayoutType.NotYetPaid).ToString()), "PayoutType");
            content.Add(new StringContent("Test Company"), "CreditorName");

            // Act
            var response = await client.PostAsync("api/v1/transaction/user", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK, "response body: {0}", responseBody);

            var result = await response.Content.ReadFromJsonAsync<PaymentRequestByUserDto>();
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Create_ReturnsOk_WhenCommentIsEmpty()
        {
            // Arrange
            _factory.ServiceMock.Reset();
            _factory.AuthServiceMock.Reset();

            var user = new User { Id = 123 };
            var created = new PaymentRequestByUser
            {
                Id = 1,
                Amount = 50,
                InvoiceNumber = "123"
            };

            _factory.AuthServiceMock
                .Setup(a => a.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
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
                    It.Is<string?>(comment => comment == null),
                    It.IsAny<PayoutType>(),
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(created);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent([1, 2, 3]);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            content.Add(fileContent, "Receipt", "test.pdf");
            content.Add(new StringContent("0"), "Transaction.TeamId");
            content.Add(new StringContent("50"), "Transaction.Amount");
            content.Add(new StringContent("TestPurpose"), "Transaction.PurposeOfPayment");
            content.Add(new StringContent(DateTime.Today.ToString("o")), "Transaction.PaidAt");
            content.Add(new StringContent("123"), "InvoiceNumber");
            content.Add(new StringContent(string.Empty), "Comment");
            content.Add(new StringContent(((int)PayoutType.NotYetPaid).ToString()), "PayoutType");
            content.Add(new StringContent("0"), "BankAccountId");

            // Act
            var response = await client.PostAsync("api/v1/transaction/user", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenCommentIsShort()
        {
            // Arrange
            _factory.ServiceMock.Reset();
            _factory.AuthServiceMock.Reset();

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent([1, 2, 3]);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            content.Add(fileContent, "Receipt", "test.pdf");
            content.Add(new StringContent("0"), "Transaction.TeamId");
            content.Add(new StringContent("50"), "Transaction.Amount");
            content.Add(new StringContent("TestPurpose"), "Transaction.PurposeOfPayment");
            content.Add(new StringContent(DateTime.Today.ToString("o")), "Transaction.PaidAt");
            content.Add(new StringContent("123"), "InvoiceNumber");
            content.Add(new StringContent("ab"), "Comment");
            content.Add(new StringContent(((int)PayoutType.NotYetPaid).ToString()), "PayoutType");
            content.Add(new StringContent("0"), "BankAccountId");

            // Act
            var response = await client.PostAsync("api/v1/transaction/user", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ----------------------------
        // DUPLICATE CHECK
        // ----------------------------
        [Fact]
        public async Task GetDuplicatePaymentRequests_ReturnsOk()
        {
            // Arrange
            var user = new User { Id = 123, Role = Role.Admin };
            var paidAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
            var matches = new List<DuplicatePaymentRequestByUserMatch>
            {
                new(
                    new PaymentRequestByUser
                    {
                        Id = 1,
                        Amount = 100,
                        InvoiceNumber = "INV-100",
                        PaidAt = paidAt,
                        User = new User { Id = 123, Name = "Test User", Email = "test@paytrack.dev" },
                        Team = new Team { Id = 99, Name = "Team A" }
                    },
                    150,
                    ["invoiceNumber", "amount", "payday", "user", "team"]),
            };

            _factory.AuthServiceMock
                .Setup(a => a.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync(user);
            _factory.ServiceMock
                .Setup(s => s.GetPaymentRequestByUserByIdAsync(7, null))
                .ReturnsAsync(new PaymentRequestByUser { Id = 7, UserId = user.Id, InvoiceNumber = "SRC" });
            _factory.ServiceMock
                .Setup(s => s.ValidateAccessToInvoice(It.IsAny<PaymentRequestByUser>(), user))
                .Returns(true);

            _factory.ServiceMock
                .Setup(s => s.GetDuplicatePaymentRequestsByUserAsync(user.Id, 99, 100, It.Is<DateTime>(d => d.Date == paidAt.Date), "INV-100", 7, It.IsAny<bool>()))
                .ReturnsAsync(matches);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/transaction/user/duplicate?TeamId=99&Amount=100&PaidAt=2026-01-05T00:00:00.0000000Z&InvoiceNumber=INV-100&PaymentRequestByUserId=7");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<List<DuplicatePaymentRequestByUserDto>>();
            dto.Should().NotBeNull();
            dto.Should().HaveCount(1);
            dto![0].PaymentRequestByUser.Id.Should().Be(1);
            dto[0].Score.Should().Be(150);
            dto[0].MatchedFields.Should().Contain("invoiceNumber");

            _factory.ServiceMock.Verify(
                s => s.GetDuplicatePaymentRequestsByUserAsync(user.Id, 99, 100, It.Is<DateTime>(d => d.Date == paidAt.Date), "INV-100", 7, true),
                Times.Once);
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
                PayoutType.NotYetPaid,
                0
            );

            // Act
            var response = await client.PutAsJsonAsync("api/v1/transaction/user/1", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<PaymentRequestByUserDto>();
            result!.Amount.Should().Be(999);
        }

        [Fact]
        public async Task DeletePaymentRequest_ReturnsNoContent()
        {
            _factory.AuthServiceMock
                .Setup(a => a.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync(new User { Id = 1, IsActive = true, Role = Role.Admin });

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var response = await client.DeleteAsync("api/v1/transaction/user/1");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            _factory.ServiceMock.Verify(s => s.DeletePaymentRequestByUserAsync(1), Times.Once);
        }

        [Fact]
        public async Task DismissDuplicatePaymentRequest_ReturnsNoContent()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            var response = await client.PostAsync("api/v1/transaction/user/1/duplicate/2/dismiss", null);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            _factory.ServiceMock.Verify(s => s.DismissDuplicatePaymentRequestByUserAsync(1, 2), Times.Once);
        }

        [Fact]
        public async Task Approve_ReturnsOk()
        {
            // Arrange
            var adminUser = new User { Id = 7, Role = Role.Admin };
            var updated = new PaymentRequestByUser
            {
                Id = 1,
                InvoiceNumber = "123",
                Status = TransactionStatus.Approved,
                BudgetId = 5
            };

            _factory.AuthServiceMock.Setup(a => a.GetCurrentUser(It.IsAny<GetUserQueryById?>())).ReturnsAsync(adminUser);
            _factory.ServiceMock
                .Setup(s => s.ApprovePaymentRequestByUserAsync(1, adminUser.Id, 5, "ok"))
                .ReturnsAsync(updated);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");
            var dto = new ApprovePaymentRequestByUserDto(5, "ok");

            // Act
            var response = await client.PostAsJsonAsync("api/v1/transaction/user/1/approve", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaymentRequestByUserDto>();
            result!.Status.Should().Be(TransactionStatus.Approved);
        }

        [Fact]
        public async Task MarkPaid_ReturnsOk()
        {
            // Arrange
            var adminUser = new User { Id = 7, Role = Role.Admin };
            var paymentDate = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc);
            var updated = new PaymentRequestByUser
            {
                Id = 1,
                InvoiceNumber = "123",
                Status = TransactionStatus.Paid,
                PaymentReference = "REF-123",
                PurposeOfPayment = "Supplier payout",
                FinancePaidAt = paymentDate,
            };

            _factory.AuthServiceMock.Setup(a => a.GetCurrentUser(It.IsAny<GetUserQueryById?>())).ReturnsAsync(adminUser);
            _factory.ServiceMock
                .Setup(s => s.MarkPaymentRequestByUserAsPaidAsync(
                    1,
                    adminUser.Id,
                    "REF-123",
                    "Supplier payout",
                    paymentDate))
                .ReturnsAsync(updated);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");
            var dto = new MarkPaymentRequestByUserAsPaidDto(
                "REF-123",
                "Supplier payout",
                paymentDate);

            // Act
            var response = await client.PostAsJsonAsync("api/v1/transaction/user/1/mark-paid", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaymentRequestByUserDto>();
            result!.Status.Should().Be(TransactionStatus.Paid);
            result.PaymentReference.Should().Be("REF-123");
            result.FinancePaidAt.Should().Be(paymentDate);
        }

        [Fact]
        public async Task Decline_ReturnsOk()
        {
            // Arrange
            var adminUser = new User { Id = 7, Role = Role.Admin };
            var updated = new PaymentRequestByUser
            {
                Id = 1,
                InvoiceNumber = "123",
                Status = TransactionStatus.Declined
            };

            _factory.AuthServiceMock.Setup(a => a.GetCurrentUser(It.IsAny<GetUserQueryById?>())).ReturnsAsync(adminUser);
            _factory.ServiceMock
                .Setup(s => s.DeclinePaymentRequestByUserAsync(1, adminUser.Id, "duplicate"))
                .ReturnsAsync(updated);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");
            var dto = new DeclinePaymentRequestByUserDto("duplicate");

            // Act
            var response = await client.PostAsJsonAsync("api/v1/transaction/user/1/decline", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaymentRequestByUserDto>();
            result!.Status.Should().Be(TransactionStatus.Declined);
        }

        [Fact]
        public async Task RequestChanges_ReturnsOk()
        {
            // Arrange
            var adminUser = new User { Id = 7, Role = Role.Admin };
            var updated = new PaymentRequestByUser
            {
                Id = 1,
                InvoiceNumber = "123",
                Status = TransactionStatus.ChangesRequested
            };

            _factory.AuthServiceMock.Setup(a => a.GetCurrentUser(It.IsAny<GetUserQueryById?>())).ReturnsAsync(adminUser);
            _factory.ServiceMock
                .Setup(s => s.RequestChangesPaymentRequestByUserAsync(1, adminUser.Id, "missing receipt"))
                .ReturnsAsync(updated);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");
            var dto = new RequestChangesPaymentRequestByUserDto("missing receipt");

            // Act
            var response = await client.PostAsJsonAsync("api/v1/transaction/user/1/request-changes", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaymentRequestByUserDto>();
            result!.Status.Should().Be(TransactionStatus.ChangesRequested);
        }

        // ----------------------------
        // FILE
        // ----------------------------
        [Fact]
        public async Task GetReceipt_ReturnsFile()
        {
            // Arrange
            var adminUser = new User { Id = 1, Role = Role.Admin };
            var invoice = new PaymentRequestByUser { Id = 1, InvoiceNumber = "123" };
            var fileBytes = new byte[] { 1, 2, 3 };

            _factory.AuthServiceMock.Setup(a => a.GetCurrentUser(It.IsAny<GetUserQueryById?>())).ReturnsAsync(adminUser);
            _factory.ServiceMock
                .Setup(s => s.GetPaymentRequestByUserByIdAsync(1, It.IsAny<GetPaymentRequestByUserQueryById?>()))
                .ReturnsAsync(invoice);
            _factory.ServiceMock
                .Setup(s => s.ValidateAccessToInvoice(It.IsAny<PaymentRequestByUser>(), It.IsAny<User>()))
                .Returns(true);
            _factory.ServiceMock
                .Setup(s => s.GetReceiptForPaymentRequestByUserByIdAsync(1))
                .ReturnsAsync((fileBytes, "application/pdf"));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/transaction/user/1/receipt");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsByteArrayAsync();
            content.Should().Equal(fileBytes);
        }

        [Fact]
        public async Task ExtractReceipt_ReturnsExtractedFields()
        {
            _factory.AuthServiceMock
                .Setup(service => service.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync(new User { Id = 1, IsActive = true });

            var extraction = new ReceiptExtractionDto(
                true,
                null,
                new ExtractedReceiptFieldDto<decimal?>(120m, 0.95m),
                new ExtractedReceiptFieldDto<DateTime?>(new DateTime(2026, 6, 14), 0.92m),
                new ExtractedReceiptFieldDto<string?>("INV-42", 0.95m));

            _factory.ReceiptExtractionServiceMock
                .Setup(service => service.ExtractAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(extraction);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");
            using var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent([1, 2, 3]), "receipt", "invoice.pdf");

            var response = await client.PostAsync("api/v1/transaction/user/receipt/extract", content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ReceiptExtractionDto>();
            result.Should().BeEquivalentTo(extraction);
        }

        [Fact]
        public async Task ExportFinancialData_ReturnsFile()
        {
            // Arrange
            var fileBytes = new byte[] { 1, 2, 3 };
            _factory.FinancialExportServiceMock
                .Setup(s => s.ExportFinancialDataAsync(It.Is<GetFinancialExportQuery>(q =>
                    q.Format == FinancialExportFormat.Csv &&
                    q.Source == FinancialExportSource.SubmittedInvoices &&
                    q.TeamId == 7 &&
                    q.CostCentreId == 4)))
                .ReturnsAsync(new FinancialExportResult(
                    fileBytes,
                    "text/csv; charset=utf-8",
                    "submitted-invoices-export.csv"));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/transaction/export?Format=Csv&Source=SubmittedInvoices&TeamId=7&CostCentreId=4");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition!.FileNameStar.Should().Be("submitted-invoices-export.csv");

            var content = await response.Content.ReadAsByteArrayAsync();
            content.Should().Equal(fileBytes);

            _factory.FinancialExportServiceMock.Verify(
                s => s.ExportFinancialDataAsync(It.IsAny<GetFinancialExportQuery>()),
                Times.Once);
        }
    }
    public class PaymentRequestByUserApiFactory : WebApplicationFactory<Program>
    {
        public Mock<IPaymentRequestByUserService> ServiceMock { get; } = new();
        public Mock<IAuthService> AuthServiceMock { get; } = new();
        public Mock<IReceiptExtractionService> ReceiptExtractionServiceMock { get; } = new();
        public Mock<IFinancialExportService> FinancialExportServiceMock { get; } = new();

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

                var financialExportServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IFinancialExportService));

                if (financialExportServiceDescriptor is not null)
                    services.Remove(financialExportServiceDescriptor);

                var receiptExtractionServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IReceiptExtractionService));

                if (receiptExtractionServiceDescriptor is not null)
                    services.Remove(receiptExtractionServiceDescriptor);


                services.AddSingleton(ServiceMock.Object);
                services.AddSingleton(AuthServiceMock.Object);
                services.AddSingleton(FinancialExportServiceMock.Object);
                services.AddSingleton(ReceiptExtractionServiceMock.Object);
            });
        }
    }

}
