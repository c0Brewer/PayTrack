using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Dto.Team;
using PayTrack.Application.Exceptions;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Implementation;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Repositories
{
    public class TeamRepositoryTests
    {
        private static AppDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName + Guid.NewGuid())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddTeamToDatabase()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("AddTeamToDatabase");
            var repo = new TeamRepository(context);
            var team = new Team { Name = "Test Team" };

            // Act
            var result = await repo.AddAsync(team);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Test Team");

            var dbTeam = await context.Teams.FindAsync(result.Id);
            dbTeam.Should().NotBeNull();
            dbTeam.Name.Should().Be("Test Team");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTeam_WhenExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetTeamById");
            var team = new Team { Name = "Existing Team" };
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var repo = new TeamRepository(context);

            // Act
            var result = await repo.GetByIdAsync(team.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(team.Id);
            result.Name.Should().Be("Existing Team");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetTeamByIdWhenNotExists");
            var repo = new TeamRepository(context);

            // Act
            var result = await repo.GetByIdAsync(999); // non-existing ID

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTeams()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllAsync");
            var teams = new List<Team>
            {
                new() { Name = "Team1" },
                new() { Name = "Team2" }
            };
            context.Teams.AddRange(teams);
            await context.SaveChangesAsync();

            var repo = new TeamRepository(context);

            // Act
            var (resultList, totalCount) = await repo.GetAllAsync();

            // Assert
            resultList.Should().HaveCount(2);
            resultList.Should().ContainSingle(t => t.Name == "Team1");
            resultList.Should().ContainSingle(t => t.Name == "Team2");
            totalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterTeamsByNameAndDescription()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllAsync_FilterByNameAndDescription");
            context.Teams.AddRange(
                new Team { Name = "Alpha Finance", Description = "Handles budget approvals" },
                new Team { Name = "Alpha Engineering", Description = "Builds the platform" },
                new Team { Name = "Beta Finance", Description = "Handles invoices" });
            await context.SaveChangesAsync();

            var repo = new TeamRepository(context);

            // Act
            var (resultList, totalCount) = await repo.GetAllAsync(new GetTeamQuery
            {
                Name = "Alpha",
                Description = "budget",
            });

            // Assert
            resultList.Should().ContainSingle();
            resultList[0].Name.Should().Be("Alpha Finance");
            totalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterTeamsByBudgetRange()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllAsync_FilterByBudget");
            var today = DateTime.UtcNow.Date;
            var costCentre = new CostCentre { Name = "Operations" };
            var teams = new List<Team>
            {
                new() { Name = "Past Budget Team" },
                new() { Name = "Current Budget Team" },
                new() { Name = "Future Budget Team" },
                new() { Name = "Current But Too High Team" },
            };

            context.CostCentres.Add(costCentre);
            context.Teams.AddRange(teams);
            await context.SaveChangesAsync();

            context.Budgets.AddRange(
                new Budget
                {
                    TeamId = teams[0].Id,
                    CostCentreId = costCentre.Id,
                    TargetAmount = 500m,
                    PeriodStart = today.AddMonths(-2),
                    PeriodEnd = today.AddDays(-1),
                },
                new Budget
                {
                    TeamId = teams[1].Id,
                    CostCentreId = costCentre.Id,
                    TargetAmount = 500m,
                    PeriodStart = today.AddDays(-1),
                    PeriodEnd = today.AddDays(1),
                },
                new Budget
                {
                    TeamId = teams[2].Id,
                    CostCentreId = costCentre.Id,
                    TargetAmount = 500m,
                    PeriodStart = today.AddDays(1),
                    PeriodEnd = today.AddMonths(2),
                },
                new Budget
                {
                    TeamId = teams[3].Id,
                    CostCentreId = costCentre.Id,
                    TargetAmount = 1_000m,
                    PeriodStart = today.AddDays(-1),
                    PeriodEnd = today.AddDays(1),
                });
            await context.SaveChangesAsync();

            var repo = new TeamRepository(context);

            // Act
            var (resultList, totalCount) = await repo.GetAllAsync(new GetTeamQuery
            {
                MinBudget = 200m,
                MaxBudget = 800m,
            });

            // Assert
            resultList.Should().ContainSingle();
            resultList[0].Name.Should().Be("Current Budget Team");
            totalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_ShouldOrderBeforeApplyingPagination()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllAsync_Pagination");
            context.Teams.AddRange(
                new Team { Name = "Alpha" },
                new Team { Name = "Gamma" },
                new Team { Name = "Beta" });
            await context.SaveChangesAsync();

            var repo = new TeamRepository(context);

            // Act
            var (resultList, totalCount) = await repo.GetAllAsync(new GetTeamQuery
            {
                Limit = 2,
                Offset = 1,
            });

            // Assert
            resultList.Select(t => t.Name).Should().Equal("Beta", "Gamma");
            totalCount.Should().Be(3);
        }

        [Fact]
        public async Task GetAllAsync_ShouldIncludeMembersAndBudgets_WhenRequested()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllAsync_IncludeRelatedData");
            var team = new Team { Name = "Platform" };
            var costCentre = new CostCentre { Name = "Engineering" };

            context.Teams.Add(team);
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            context.User.Add(new User
            {
                Name = "Alice",
                Email = "alice@example.com",
                TeamId = team.Id,
                Role = Role.RegularUser,
                IsActive = true,
            });

            context.Budgets.Add(new Budget
            {
                TeamId = team.Id,
                CostCentreId = costCentre.Id,
                TargetAmount = 2000m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 12, 31),
            });
            await context.SaveChangesAsync();

            // Clear tracking so the query only sees what the repository explicitly includes.
            context.ChangeTracker.Clear();
            var repo = new TeamRepository(context);

            // Act
            var (resultList, totalCount) = await repo.GetAllAsync(new GetTeamQuery
            {
                IncludeMembers = true,
                IncludeBudgets = true,
            });

            // Assert
            resultList.Should().ContainSingle();
            resultList[0].Members.Should().ContainSingle();
            resultList[0].Budgets.Should().ContainSingle();
            totalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldOnlyIncludeMembersAndBudgets_WhenRequested()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetByIdAsync_IncludeRelatedData");
            var team = new Team { Name = "Platform" };
            var costCentre = new CostCentre { Name = "Operations" };

            context.Teams.Add(team);
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            context.User.Add(new User
            {
                Name = "Alice",
                Email = "alice@example.com",
                TeamId = team.Id,
                Role = Role.TeamLead,
                IsActive = true,
            });

            context.Budgets.Add(new Budget
            {
                TeamId = team.Id,
                CostCentreId = costCentre.Id,
                TargetAmount = 900m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 12, 31),
            });
            await context.SaveChangesAsync();

            var repo = new TeamRepository(context);

            // Clear tracking between reads so EF does not reuse the previously loaded navigation properties.
            context.ChangeTracker.Clear();
            var withoutIncludes = await repo.GetByIdAsync(team.Id, new GetTeamQueryById
            {
                IncludeMembers = false,
                IncludeBudgets = false,
            });

            context.ChangeTracker.Clear();
            var withIncludes = await repo.GetByIdAsync(team.Id, new GetTeamQueryById
            {
                IncludeMembers = true,
                IncludeBudgets = true,
            });

            // Assert
            withoutIncludes.Should().NotBeNull();
            withoutIncludes!.Members.Should().BeEmpty();
            withoutIncludes.Budgets.Should().BeEmpty();

            withIncludes.Should().NotBeNull();
            withIncludes!.Members.Should().ContainSingle();
            withIncludes.Budgets.Should().ContainSingle();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateSpecifiedFields()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UpdateAsync");
            var team = new Team { Name = "Before", Description = "Old", DisplayColor = "#000000" };
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var repo = new TeamRepository(context);

            // Act
            var result = await repo.UpdateAsync(team.Id, "After", "New", "#ffffff");

            // Assert
            result.Name.Should().Be("After");
            result.Description.Should().Be("New");
            result.DisplayColor.Should().Be("#ffffff");
        }

        [Fact]
        public async Task GetDeleteTeamImpactAsync_ShouldReturnCountsForRelatedData()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetDeleteTeamImpactAsync");
            var team = new Team { Name = "Finance" };
            var requester = new User
            {
                Name = "Alice",
                Email = "alice@example.com",
                Role = Role.Admin,
                IsActive = true,
            };
            var member = new User
            {
                Name = "Bob",
                Email = "bob@example.com",
                Role = Role.RegularUser,
                IsActive = true,
            };
            var costCentre = new CostCentre { Name = "Operations" };

            context.Teams.Add(team);
            context.User.AddRange(requester, member);
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            member.TeamId = team.Id;
            await context.SaveChangesAsync();

            context.Budgets.Add(new Budget
            {
                TeamId = team.Id,
                CostCentreId = costCentre.Id,
                TargetAmount = 500m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 12, 31),
            });

            context.PaymentRequestsByUser.Add(new PaymentRequestByUser
            {
                UserId = requester.Id,
                TeamId = team.Id,
                CostCentreId = costCentre.Id,
                Amount = 75m,
                PaymentDirection = PaymentDirection.Out,
                PayoutType = PayoutType.User,
                InvoiceNumber = "INV-1",
            });
            await context.SaveChangesAsync();

            var repo = new TeamRepository(context);

            // Act
            var result = await repo.GetDeleteTeamImpactAsync(team.Id);

            // Assert
            result.Should().NotBeNull();
            result!.TeamId.Should().Be(team.Id);
            result.TeamName.Should().Be("Finance");
            result.CanDelete.Should().BeFalse();
            result.AffectedUserCount.Should().Be(1);
            result.BlockingBudgetCount.Should().Be(1);
            result.BlockingTransactionCount.Should().Be(1);
            result.InvoiceCount.Should().Be(1);
            result.WarningMessage.Should().Contain("blocked");
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenDeleteIsBlocked()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("DeleteAsync_Blocked");
            var team = new Team { Name = "Finance" };
            var requester = new User
            {
                Name = "Alice",
                Email = "alice@example.com",
                Role = Role.Admin,
                IsActive = true,
            };
            var costCentre = new CostCentre { Name = "Operations" };

            context.Teams.Add(team);
            context.User.Add(requester);
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            context.Budgets.Add(new Budget
            {
                TeamId = team.Id,
                CostCentreId = costCentre.Id,
                TargetAmount = 100m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 12, 31),
            });
            await context.SaveChangesAsync();

            var repo = new TeamRepository(context);

            // Act
            var action = async () => await repo.DeleteAsync(team.Id);

            // Assert
            await action.Should().ThrowAsync<InvalidStateException>();
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveTeam_WhenNoBlockingRelationsExist()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("DeleteAsync_Success");
            var team = new Team { Name = "Finance" };
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var repo = new TeamRepository(context);

            // Act
            var result = await repo.DeleteAsync(team.Id);

            // Assert
            result.Id.Should().Be(team.Id);
            (await context.Teams.FindAsync(team.Id)).Should().BeNull();
        }


        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenSaveChangesFails()
        {
            // Arrange
            var failingContext = new FailingDbContext("AddAsyncTeam_FailingDbContext");
            // Override SaveChangesAsync to return 0 to simulate failure

            var repo = new TeamRepository(failingContext);
            var team = new Team { Name = "Fail User" };

            // Act
            var exception = await Assert.ThrowsAsync<InternalErrorException>(
                async () => await repo.AddAsync(team)
            );

            // Assert
            Assert.Contains("teams", exception.Message);
        }
    }
}
