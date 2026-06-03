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
using PayTrack.Application.Dto.BankStatement;
using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class BankStatementMatchingEndpointsTests(BankStatementMatchingApiFactory factory)
        : IClassFixture<BankStatementMatchingApiFactory>
    {
        private readonly BankStatementMatchingApiFactory factory = factory;

        private static readonly BankStatementEntryDto SampleEntry = new()
        {
            Booking = new DateTime(2026, 5, 21, 0, 0, 0, DateTimeKind.Utc),
            PartnerName = "ACME Corp",
            Amount = new BankStatementAmountDto { Value = 120.50m, Currency = "EUR" },
            ReceiverReference = "INV-2026-042",
        };

        private static readonly TransactionDto SampleTransactionDto = new()
        {
            Id = 7,
            UserId = 1,
            TeamId = 2,
            Amount = 120.50m,
            PurposeOfPayment = "ACME Invoice",
            PaymentReference = "INV-2026-042",
            Status = TransactionStatus.Approved,
            PaidAt = new DateTime(2026, 5, 21, 0, 0, 0, DateTimeKind.Utc),
        };

        // ── POST /api/v1/transaction/bank-statement-matches ───────────────────

        [Fact]
        public async Task PostBankStatementMatches_WithValidEntries_ReturnsOkWithMatchResults()
        {
            // Arrange
            var matchResponse = new BankStatementMatchResponseDto(
            [
                new BankStatementMatchResultDto(SampleEntry, HasMatch: true, SampleTransactionDto, MatchScore: 85),
            ]);

            this.factory.AuthServiceMock
                .Setup(s => s.GetCurrentUser())
                .ReturnsAsync(new User { Id = 1, Name = "Test", Email = "test@test.com" });

            this.factory.BankStatementMatchingServiceMock
                .Setup(s => s.MatchBankStatementEntriesAsync(It.IsAny<List<BankStatementEntryDto>>()))
                .ReturnsAsync(matchResponse);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.PostAsJsonAsync(
                "api/v1/transaction/bank-statement-matches",
                new List<BankStatementEntryDto> { SampleEntry });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<BankStatementMatchResponseDto>();
            result.Should().NotBeNull();
            result!.Results.Should().HaveCount(1);
            result.Results![0].HasMatch.Should().BeTrue();
            result.Results![0].MatchScore.Should().Be(85);
        }

        [Fact]
        public async Task PostBankStatementMatches_WithEmptyEntryList_ReturnsOkWithEmptyResults()
        {
            // Arrange
            this.factory.BankStatementMatchingServiceMock
                .Setup(s => s.MatchBankStatementEntriesAsync(It.IsAny<List<BankStatementEntryDto>>()))
                .ReturnsAsync(new BankStatementMatchResponseDto([]));

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.PostAsJsonAsync(
                "api/v1/transaction/bank-statement-matches",
                new List<BankStatementEntryDto>());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<BankStatementMatchResponseDto>();
            result!.Results.Should().BeEmpty();
        }

        // ── PUT /api/v1/transaction/bank-statement-matches ────────────────────

        [Fact]
        public async Task PutBankStatementMatches_WithValidUpdates_ReturnsOkWithUpdatedTransactions()
        {
            // Arrange
            var currentUser = new User { Id = 1, Name = "Test", Email = "test@test.com" };
            var updates = new List<BankStatementUpdateRequestDto>
            {
                new("entry-0", MatchedTransactionId: 7, Skipped: false),
            };
            var updatedTransaction = new Transaction[] { /* entity returned by service */ }.ToList();

            this.factory.AuthServiceMock
                .Setup(s => s.GetCurrentUser())
                .ReturnsAsync(currentUser);

            this.factory.BankStatementMatchingServiceMock
                .Setup(s => s.UpdateBankStatementMatchesAsync(
                    It.IsAny<List<BankStatementUpdateRequestDto>>(),
                    currentUser.Id))
                .ReturnsAsync([]);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.PutAsJsonAsync(
                "api/v1/transaction/bank-statement-matches",
                updates);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            this.factory.BankStatementMatchingServiceMock.Verify(
                s => s.UpdateBankStatementMatchesAsync(It.IsAny<List<BankStatementUpdateRequestDto>>(), currentUser.Id),
                Times.Once);
        }

        [Fact]
        public async Task PutBankStatementMatches_WhenCurrentUserNotFound_ReturnsProblem()
        {
            // Arrange — authService returns null (user session not found)
            this.factory.AuthServiceMock
                .Setup(s => s.GetCurrentUser())
                .ReturnsAsync((User?)null);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.PutAsJsonAsync(
                "api/v1/transaction/bank-statement-matches",
                new List<BankStatementUpdateRequestDto>());

            // Assert — NotFoundException from handler maps to a non-OK response
            response.StatusCode.Should().NotBe(HttpStatusCode.OK);
        }
    }

    public class BankStatementMatchingApiFactory : WebApplicationFactory<Program>
    {
        public Mock<IAuthService> AuthServiceMock { get; } = new();

        public Mock<IBankStatementMatchingService> BankStatementMatchingServiceMock { get; } = new();

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
                if (dbDescriptor is not null) services.Remove(dbDescriptor);
                services.AddDbContext<AppDbContext>(opt =>
                    opt.UseInMemoryDatabase("BankStatementMatchingTestDb" + Guid.NewGuid()));

                Replace<IAuthService>(services, this.AuthServiceMock.Object);
                Replace<IBankStatementMatchingService>(services, this.BankStatementMatchingServiceMock.Object);
            });
        }

        private static void Replace<TService>(IServiceCollection services, TService implementation)
            where TService : class
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TService));
            if (descriptor is not null) services.Remove(descriptor);
            services.AddSingleton(implementation);
        }
    }
}
