//AI helped with the test cases

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PayTrack.Application.Dto.BankAccount;
using PayTrack.Application.Dto.User;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class BankAccountEndpointsTests(BankAccountApiFactory factory) : IClassFixture<BankAccountApiFactory>
    {
        private readonly BankAccountApiFactory factory = factory;

        [Fact]
        public async Task GetBankAccounts_ReturnsOkWithBankAccounts()
        {
            // Arrange
            var user = new User { Id = 1, Name = "Test", Email = "test@test.com", IsActive = true };
            var bankAccounts = new List<BankAccount>
            {
                new() { Id = 10, UserId = 1, AccountHolder = "A", Iban = "AT611904300234573201", Bic = "BKAUATWW" },
                new() { Id = 11, UserId = 1, AccountHolder = "B", Iban = "AT611904300234573202", Bic = "BKAUATWW" },
            };

            this.factory.AuthServiceMock
                .Setup(service => service.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync(user);

            this.factory.BankAccountServiceMock
                .Setup(service => service.GetBankAccountsAsync(user.Id))
                .ReturnsAsync(bankAccounts);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/bankaccount");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<BankAccountDto>>();
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Id.Should().Be(10);
            result[1].Id.Should().Be(11);
        }

        [Fact]
        public async Task GetBankAccounts_ReturnsNotFound_WhenCurrentUserIsMissing()
        {
            // Arrange
            this.factory.AuthServiceMock
                .Setup(service => service.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync((User?)null);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/bankaccount");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            result.Should().NotBeNull();
            result.Detail.Should().Be("Current User not found");
        }

        [Fact]
        public async Task CreateBankAccount_ReturnsOkWithCreatedBankAccount()
        {
            // Arrange
            var user = new User { Id = 2, Name = "Test", Email = "test2@test.com", IsActive = true };
            var createDto = new CreateBankAccountRequestDto("Max", "AT611904300234573203", "BKAUATWW");
            var created = new BankAccount
            {
                Id = 15,
                UserId = user.Id,
                AccountHolder = createDto.AccountHolder,
                Iban = createDto.Iban,
                Bic = createDto.Bic,
            };

            this.factory.AuthServiceMock
                .Setup(service => service.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync(user);

            this.factory.BankAccountServiceMock
                .Setup(service => service.CreateBankAccountAsync(user.Id, createDto.AccountHolder, createDto.Iban, createDto.Bic))
                .ReturnsAsync(created);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.PostAsJsonAsync("api/v1/bankaccount", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<BankAccountDto>();
            result.Should().NotBeNull();
            result.Id.Should().Be(15);
            result.AccountHolder.Should().Be("Max");
        }

        [Fact]
        public async Task UpdateBankAccount_ReturnsOkWithUpdatedBankAccount()
        {
            // Arrange
            const int bankAccountId = 20;
            var user = new User { Id = 3, Name = "Test", Email = "test3@test.com", IsActive = true };
            var updateDto = new UpdateBankAccountRequestDto("New Holder", null, "NEWBIC12");

            var updated = new BankAccount
            {
                Id = bankAccountId,
                UserId = user.Id,
                AccountHolder = "New Holder",
                Iban = "AT611904300234573204",
                Bic = "NEWBIC12",
            };

            this.factory.AuthServiceMock
                .Setup(service => service.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync(user);

            this.factory.BankAccountServiceMock
                .Setup(service => service.UpdateBankAccountAsync(
                    user.Id,
                    bankAccountId,
                    updateDto.AccountHolder,
                    updateDto.Iban,
                    updateDto.Bic))
                .ReturnsAsync(updated);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.PutAsJsonAsync($"api/v1/bankaccount/{bankAccountId}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<BankAccountDto>();
            result.Should().NotBeNull();
            result.Id.Should().Be(bankAccountId);
            result.Bic.Should().Be("NEWBIC12");
        }

        [Fact]
        public async Task DeleteBankAccount_ReturnsNoContent()
        {
            // Arrange
            const int bankAccountId = 21;
            var user = new User { Id = 4, Name = "Test", Email = "test4@test.com", IsActive = true };

            this.factory.AuthServiceMock
                .Setup(service => service.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync(user);

            this.factory.BankAccountServiceMock
                .Setup(service => service.DeleteBankAccountAsync(user.Id, bankAccountId))
                .Returns(Task.CompletedTask);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.DeleteAsync($"api/v1/bankaccount/{bankAccountId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            this.factory.BankAccountServiceMock.Verify(service => service.DeleteBankAccountAsync(user.Id, bankAccountId), Times.Once);
        }

        [Fact]
        public async Task CreateBankAccountOnboarding_ReturnsUpdatedUser()
        {
            var user = new User { Id = 2, Name = "Test", Email = "test2@test.com", IsActive = true };
            var createDto = new CreateBankAccountRequestDto("Max", "AT611904300234573203", "BKAUATWW");
            var updatedUser = new User
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IsActive = true,
                BankAccounts =
                [
                    new BankAccount
                    {
                        Id = 15,
                        UserId = user.Id,
                        AccountHolder = createDto.AccountHolder,
                        Iban = createDto.Iban,
                        Bic = createDto.Bic,
                    },
                ],
            };

            this.factory.AuthServiceMock
                .Setup(service => service.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync(user);
            this.factory.BankAccountServiceMock
                .Setup(service => service.CreateBankAccountOnboardingAsync(user.Id, createDto.AccountHolder, createDto.Iban, createDto.Bic))
                .ReturnsAsync(updatedUser.BankAccounts.Single());
            this.factory.UserServiceMock
                .Setup(service => service.GetUserByIdAsync(
                    user.Id,
                    It.Is<GetUserQueryById>(query => query.IncludeBankAccounts == true && query.IncludeTeam == true)))
                .ReturnsAsync(updatedUser);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var response = await client.PostAsJsonAsync("api/v1/bankaccount/onboarding", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<UserDto>();
            result.Should().NotBeNull();
            result.HasBankInformation.Should().BeTrue();
        }

        [Fact]
        public async Task SkipCurrentUserBankInformation_ReturnsUpdatedUser()
        {
            var user = new User { Id = 4, Name = "Test", Email = "test4@test.com", IsActive = true };
            var updatedUser = new User
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IsActive = true,
                BankInformationSkipped = true,
            };

            this.factory.AuthServiceMock
                .Setup(service => service.GetCurrentUser(It.IsAny<GetUserQueryById?>()))
                .ReturnsAsync(user);
            this.factory.UserServiceMock
                .Setup(service => service.UpdateBankInformationSkippedAsync(user.Id, true))
                .ReturnsAsync(updatedUser);

            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var response = await client.PostAsync("api/v1/bankaccount/onboarding/skip", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<UserDto>();
            result.Should().NotBeNull();
            result.BankInformationSkipped.Should().BeTrue();
        }
    }

    public class BankAccountApiFactory : WebApplicationFactory<Program>
    {
        public Mock<IAuthService> AuthServiceMock { get; } = new();

        public Mock<IBankAccountService> BankAccountServiceMock { get; } = new();

        public Mock<IUserService> UserServiceMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                _ = services.AddAuthorization(_ => { });

                var dbDescriptor = services.SingleOrDefault(
                    descriptor => descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (dbDescriptor is not null)
                {
                    services.Remove(dbDescriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("BankAccountTestDb"));

                var authServiceDescriptor = services.SingleOrDefault(
                    descriptor => descriptor.ServiceType == typeof(IAuthService));
                if (authServiceDescriptor is not null)
                {
                    services.Remove(authServiceDescriptor);
                }

                var bankAccountServiceDescriptor = services.SingleOrDefault(
                    descriptor => descriptor.ServiceType == typeof(IBankAccountService));
                if (bankAccountServiceDescriptor is not null)
                {
                    services.Remove(bankAccountServiceDescriptor);
                }

                var userServiceDescriptor = services.SingleOrDefault(
                    descriptor => descriptor.ServiceType == typeof(IUserService));
                if (userServiceDescriptor is not null)
                {
                    services.Remove(userServiceDescriptor);
                }

                services.AddSingleton(this.AuthServiceMock.Object);
                services.AddSingleton(this.BankAccountServiceMock.Object);
                services.AddSingleton(this.UserServiceMock.Object);
            });
        }
    }
}
