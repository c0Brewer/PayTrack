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
using PayTrack.Application.Dto.Team;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class TeamEndpointsTests(TeamApiFactory factory) : IClassFixture<TeamApiFactory>
    {
        private readonly TeamApiFactory _factory = factory;

        [Fact]
        public async Task GetTeams_ReturnsOkWithTeams()
        {
            // Arrange
            var teams = new List<Team>
            {
                new() { Id = 1, Name = "Alpha" },
                new() { Id = 2, Name = "Beta" },
            };

            _factory.TeamServiceMock
                .Setup(s => s.GetTeamsAsync(It.IsAny<GetTeamQuery>()))
                .ReturnsAsync((teams, 2));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/team");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<TeamDto>>();
            result.Should().NotBeNull();
            result!.Items.Should().HaveCount(2);
            result.Items[0].Name.Should().Be("Alpha");
            result.Items[1].Name.Should().Be("Beta");
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetTeams_ForwardsQueryParametersAndMapsOptionalData()
        {
            // Arrange
            var teams = new List<Team>
            {
                new()
                {
                    Id = 1,
                    Name = "Core Platform",
                    Members =
                    [
                        new User
                        {
                            Id = 5,
                            Name = "Alice",
                            Email = "alice@example.com",
                            Role = Role.TeamLead,
                            IsActive = true,
                        },
                    ],
                    Budgets =
                    [
                        new Budget
                        {
                            Id = 8,
                            TeamId = 1,
                            CostCentreId = 12,
                            TargetAmount = 600m,
                            PeriodStart = new DateTime(2026, 1, 1),
                            PeriodEnd = new DateTime(2026, 12, 31),
                        },
                    ],
                },
            };

            _factory.TeamServiceMock
                .Setup(s => s.GetTeamsAsync(It.Is<GetTeamQuery>(q =>
                    q.Name == "Core" &&
                    q.Description == "budget" &&
                    q.MinBudget == 100m &&
                    q.MaxBudget == 900m &&
                    q.IncludeMembers == true &&
                    q.IncludeBudgets == true &&
                    q.Limit == 1 &&
                    q.Offset == 2)))
                .ReturnsAsync((teams, 4));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync(
                "api/v1/team?name=Core&description=budget&minBudget=100&maxBudget=900&includeMembers=true&includeBudgets=true&limit=1&offset=2");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<TeamDto>>();
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(4);
            result.Limit.Should().Be(1);
            result.Offset.Should().Be(2);
            result.Items.Should().ContainSingle();
            result.Items[0].Members.Should().ContainSingle();
            result.Items[0].Budgets.Should().ContainSingle();
            result.Items[0].Members![0].Email.Should().Be("alice@example.com");
            result.Items[0].Budgets![0].TargetAmount.Should().Be(600m);
        }

        [Fact]
        public async Task GetTeamById_ReturnsOk_WhenTeamExists()
        {
            // Arrange
            var team = new Team { Id = 1, Name = "Team1" };

            _factory.TeamServiceMock
                .Setup(s => s.GetTeamByIdAsync(1, It.IsAny<GetTeamQueryById?>()))
                .ReturnsAsync(team);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/team/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TeamDto>();
            result.Should().NotBeNull();
            result.Name.Should().Be("Team1");
            result.Members.Should().BeEmpty();
            result.Budgets.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTeamById_ReturnsMembers_WhenIncludeMembersIsTrue()
        {
            // Arrange
            var team = new Team
            {
                Id = 1,
                Name = "Team1",
                Members =
                [
                    new User
                    {
                        Id = 10,
                        Name = "Alice",
                        Email = "alice@example.com",
                        Role = Role.RegularUser,
                        IsActive = true,
                    },
                ],
            };

            _factory.TeamServiceMock
                .Setup(s => s.GetTeamByIdAsync(1, It.Is<GetTeamQueryById?>(q => q != null && q.IncludeMembers == true)))
                .ReturnsAsync(team);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/team/1?includeMembers=true");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TeamDto>();
            result.Should().NotBeNull();
            result!.Members.Should().NotBeNull();
            result.Members.Should().ContainSingle();
            result.Members[0].Email.Should().Be("alice@example.com");
            result.Budgets.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTeamById_ReturnsBudgets_WhenIncludeBudgetsIsTrue()
        {
            // Arrange
            var team = new Team
            {
                Id = 1,
                Name = "Team1",
                Budgets =
                [
                    new Budget
                    {
                        Id = 5,
                        TeamId = 1,
                        CostCentreId = 12,
                        TargetAmount = 2500m,
                        PeriodStart = new DateTime(2026, 1, 1),
                        PeriodEnd = new DateTime(2026, 12, 31),
                    },
                ],
            };

            _factory.TeamServiceMock
                .Setup(s => s.GetTeamByIdAsync(1, It.Is<GetTeamQueryById?>(q => q != null && q.IncludeBudgets == true)))
                .ReturnsAsync(team);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/team/1?includeBudgets=true");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TeamDto>();
            result.Should().NotBeNull();
            result!.Budgets.Should().NotBeNull();
            result.Budgets.Should().ContainSingle();
            result.Budgets[0].CostCentreId.Should().Be(12);
            result.Budgets[0].TargetAmount.Should().Be(2500m);
            result.Members.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTeamById_ReturnsNotFound_WhenTeamDoesNotExist()
        {
            // Arrange
            _factory.TeamServiceMock
                .Setup(s => s.GetTeamByIdAsync(999, It.IsAny<GetTeamQueryById?>()))
                .ReturnsAsync((Team?)null);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/team/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateTeam_ReturnsOkWithCreatedTeam()
        {
            // Arrange
            var requestDto = new CreateTeamRequestDto("New Team", "My Description", "#112233");
            var createdTeam = new Team { Id = 1, Name = "New Team" };

            _factory.TeamServiceMock
                .Setup(s => s.CreateTeamAsync(requestDto.Name, requestDto.Description, requestDto.DisplayColor))
                .ReturnsAsync(createdTeam);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.PostAsJsonAsync("api/v1/team", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TeamDto>();
            result.Should().NotBeNull();
            result.Name.Should().Be("New Team");
            result.Members.Should().BeEmpty();
            result.Budgets.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateTeam_ReturnsBadRequest_WhenDisplayColorIsInvalid()
        {
            // Arrange
            var requestDto = new CreateTeamRequestDto("New Team", "My Description", "My Color");

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.PostAsJsonAsync("api/v1/team", requestDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _factory.TeamServiceMock.Verify(
                s => s.CreateTeamAsync(requestDto.Name, requestDto.Description, requestDto.DisplayColor),
                Times.Never);
        }

        [Fact]
        public async Task GetDeleteTeamImpact_ReturnsOk_WhenTeamExists()
        {
            // Arrange
            var deleteImpact = new DeleteTeamImpactDto(
                1,
                "Finance",
                false,
                2,
                1,
                3,
                1,
                "Deleting this team is currently blocked.");

            _factory.TeamServiceMock
                .Setup(s => s.GetDeleteTeamImpactAsync(1))
                .ReturnsAsync(deleteImpact);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/team/1/delete-impact");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<DeleteTeamImpactDto>();
            result.Should().NotBeNull();
            result!.TeamId.Should().Be(1);
            result.TeamName.Should().Be("Finance");
            result.CanDelete.Should().BeFalse();
            result.AffectedUserCount.Should().Be(2);
        }

        [Fact]
        public async Task GetDeleteTeamImpact_ReturnsNotFound_WhenTeamDoesNotExist()
        {
            // Arrange
            _factory.TeamServiceMock
                .Setup(s => s.GetDeleteTeamImpactAsync(999))
                .ReturnsAsync((DeleteTeamImpactDto?)null);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Admin");

            // Act
            var response = await client.GetAsync("api/v1/team/999/delete-impact");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetTeams_ReturnsForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            using var factory = new TeamRegularUserApiFactory();
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("api/v1/team");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            factory.TeamServiceMock.VerifyNoOtherCalls();
        }
    }

    /// <summary>
    /// Uses the real API Program entry point so WebApplicationFactory can
    /// resolve and build the IHost, then replaces ITeamService with a mock.
    /// </summary>
    public class TeamApiFactory : WebApplicationFactory<Program>  // <-- KEY FIX
    {
        public Mock<ITeamService> TeamServiceMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                // Authentication

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


                // SERVICE

                // Remove the real ITeamService registration coming from Program.cs
                var serviceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ITeamService));

                if (serviceDescriptor is not null)
                    services.Remove(serviceDescriptor);

                // Register the mock instead
                services.AddSingleton(TeamServiceMock.Object);
            });
        }
    }

    public class TeamRegularUserApiFactory : WebApplicationFactory<Program>
    {
        public Mock<ITeamService> TeamServiceMock { get; } = new();

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
                    options.UseInMemoryDatabase("RegularUserTestDb"));

                var serviceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ITeamService));

                if (serviceDescriptor is not null)
                    services.Remove(serviceDescriptor);

                services.AddSingleton(TeamServiceMock.Object);
            });
        }
    }
}
