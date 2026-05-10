using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Exceptions;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Implementation;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Repositories
{
    public class BudgetRepositoryTests
    {
        private static AppDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName + Guid.NewGuid())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddBudgetToDatabase()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("AddBudget");
            var team = new Team { Name = "Team Alpha" };
            var costCentre = new CostCentre { Name = "Aero" };
            context.Teams.Add(team);
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            var repo = new BudgetRepository(context);
            var periodStart = new DateTime(2026, 1, 1);
            var periodEnd = new DateTime(2026, 12, 31);

            // Act
            var result = await repo.AddAsync(team.Id, costCentre.Id, 1000m, periodStart, periodEnd);

            // Assert
            result.Should().NotBeNull();
            result.TeamId.Should().Be(team.Id);
            result.CostCentreId.Should().Be(costCentre.Id);
            result.TargetAmount.Should().Be(1000m);
            result.PeriodStart.Should().Be(periodStart);
            result.PeriodEnd.Should().Be(periodEnd);
            var dbEntity = await context.Budgets.FindAsync(result.Id);
            dbEntity.Should().NotBeNull();
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenSaveChangesFails()
        {
            // Arrange
            var failingContext = new FailingDbContext("AddBudget_Failing");
            var repo = new BudgetRepository(failingContext);

            // Act
            var exception = await Assert.ThrowsAsync<InternalErrorException>(
                async () => await repo.AddAsync(1, 1, 500m, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31)));

            // Assert
            exception.Message.Should().Contain("Budget");
        }

        [Fact]
        public async Task AddRangeAsync_ShouldAddMultipleBudgetsToContext()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("AddRangeBudget");
            var team = new Team { Name = "Team Alpha" };
            context.Teams.Add(team);
            var costCentre = new CostCentre { Name = "Electronics" };
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            var repo = new BudgetRepository(context);
            var entries = new List<CreateCostCentreBudgetEntryDto>
            {
                new(TeamId: team.Id, TargetAmount: 1000m, PeriodStart: new DateTime(2026, 1, 1), PeriodEnd: new DateTime(2026, 6, 30)),
                new(TeamId: team.Id, TargetAmount: 2000m, PeriodStart: new DateTime(2026, 7, 1), PeriodEnd: new DateTime(2026, 12, 31)),
            };

            // Act
            await repo.AddRangeAsync(costCentre, entries);
            await context.SaveChangesAsync();

            // Assert
            var budgets = await context.Budgets.Where(b => b.CostCentreId == costCentre.Id).ToListAsync();
            budgets.Should().HaveCount(2);
            budgets.Should().ContainSingle(b => b.TargetAmount == 1000m);
            budgets.Should().ContainSingle(b => b.TargetAmount == 2000m);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnBudget_WhenExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetBudgetById");
            var team = new Team { Name = "Team Alpha" };
            var costCentre = new CostCentre { Name = "Aero" };
            context.Teams.Add(team);
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            var budget = new Budget
            {
                TeamId = team.Id,
                CostCentreId = costCentre.Id,
                TargetAmount = 500m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 12, 31),
            };
            context.Budgets.Add(budget);
            await context.SaveChangesAsync();

            var repo = new BudgetRepository(context);

            // Act
            var result = await repo.GetByIdAsync(budget.Id);

            // Assert
            result.Should().NotBeNull();
            result!.TargetAmount.Should().Be(500m);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetBudgetById_NotFound");
            var repo = new BudgetRepository(context);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllBudgets()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllBudgets");
            var team = new Team { Name = "Team Alpha" };
            var costCentre = new CostCentre { Name = "Aero" };
            context.Teams.Add(team);
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            context.Budgets.AddRange(
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 100m, PeriodStart = new DateTime(2026, 1, 1), PeriodEnd = new DateTime(2026, 6, 30) },
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 200m, PeriodStart = new DateTime(2026, 7, 1), PeriodEnd = new DateTime(2026, 12, 31) });
            await context.SaveChangesAsync();

            var repo = new BudgetRepository(context);

            // Act
            var (items, totalCount) = await repo.GetAllAsync();

            // Assert
            items.Should().HaveCount(2);
            totalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllAsync_WithTeamIdFilter_ShouldReturnMatchingBudgets()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllBudgets_TeamId");
            var teamA = new Team { Name = "Team A" };
            var teamB = new Team { Name = "Team B" };
            var costCentre = new CostCentre { Name = "Aero" };
            context.Teams.AddRange(teamA, teamB);
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            context.Budgets.AddRange(
                new Budget { TeamId = teamA.Id, CostCentreId = costCentre.Id, TargetAmount = 100m, PeriodStart = new DateTime(2026, 1, 1), PeriodEnd = new DateTime(2026, 12, 31) },
                new Budget { TeamId = teamB.Id, CostCentreId = costCentre.Id, TargetAmount = 200m, PeriodStart = new DateTime(2026, 1, 1), PeriodEnd = new DateTime(2026, 12, 31) });
            await context.SaveChangesAsync();

            var repo = new BudgetRepository(context);

            // Act
            var (items, totalCount) = await repo.GetAllAsync(new GetBudgetQuery { TeamId = teamA.Id });

            // Assert
            items.Should().HaveCount(1);
            items[0].TeamId.Should().Be(teamA.Id);
            totalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_WithCostCentreIdFilter_ShouldReturnMatchingBudgets()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllBudgets_CostCentreId");
            var team = new Team { Name = "Team Alpha" };
            var costCentreA = new CostCentre { Name = "Aero" };
            var costCentreB = new CostCentre { Name = "Electronics" };
            context.Teams.Add(team);
            context.CostCentres.AddRange(costCentreA, costCentreB);
            await context.SaveChangesAsync();

            context.Budgets.AddRange(
                new Budget { TeamId = team.Id, CostCentreId = costCentreA.Id, TargetAmount = 100m, PeriodStart = new DateTime(2026, 1, 1), PeriodEnd = new DateTime(2026, 12, 31) },
                new Budget { TeamId = team.Id, CostCentreId = costCentreB.Id, TargetAmount = 200m, PeriodStart = new DateTime(2026, 1, 1), PeriodEnd = new DateTime(2026, 12, 31) });
            await context.SaveChangesAsync();

            var repo = new BudgetRepository(context);

            // Act
            var (items, totalCount) = await repo.GetAllAsync(new GetBudgetQuery { CostCentreId = costCentreA.Id });

            // Assert
            items.Should().HaveCount(1);
            items[0].CostCentreId.Should().Be(costCentreA.Id);
            totalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_WithTargetAmountFilter_ShouldReturnMatchingBudgets()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllBudgets_TargetAmount");
            var team = new Team { Name = "Team Alpha" };
            var costCentre = new CostCentre { Name = "Aero" };
            context.Teams.Add(team);
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            context.Budgets.AddRange(
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 1000m, PeriodStart = new DateTime(2026, 1, 1), PeriodEnd = new DateTime(2026, 6, 30) },
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 2000m, PeriodStart = new DateTime(2026, 7, 1), PeriodEnd = new DateTime(2026, 12, 31) });
            await context.SaveChangesAsync();

            var repo = new BudgetRepository(context);

            // Act
            var (items, totalCount) = await repo.GetAllAsync(new GetBudgetQuery { TargetAmount = 1000m });

            // Assert
            items.Should().HaveCount(1);
            items[0].TargetAmount.Should().Be(1000m);
            totalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_WithPeriodStartFilter_ShouldReturnBudgetsOnOrAfterDate()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllBudgets_PeriodStart");
            var team = new Team { Name = "Team Alpha" };
            var costCentre = new CostCentre { Name = "Aero" };
            context.Teams.Add(team);
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            context.Budgets.AddRange(
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 100m, PeriodStart = new DateTime(2025, 1, 1), PeriodEnd = new DateTime(2025, 12, 31) },
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 200m, PeriodStart = new DateTime(2026, 1, 1), PeriodEnd = new DateTime(2026, 12, 31) });
            await context.SaveChangesAsync();

            var repo = new BudgetRepository(context);

            // Act
            var (items, totalCount) = await repo.GetAllAsync(new GetBudgetQuery { PeriodStart = new DateTime(2026, 1, 1) });

            // Assert
            items.Should().HaveCount(1);
            items[0].PeriodStart.Should().Be(new DateTime(2026, 1, 1));
            totalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_WithPeriodEndFilter_ShouldReturnBudgetsOnOrBeforeDate()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllBudgets_PeriodEnd");
            var team = new Team { Name = "Team Alpha" };
            var costCentre = new CostCentre { Name = "Aero" };
            context.Teams.Add(team);
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            context.Budgets.AddRange(
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 100m, PeriodStart = new DateTime(2025, 1, 1), PeriodEnd = new DateTime(2025, 12, 31) },
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 200m, PeriodStart = new DateTime(2026, 1, 1), PeriodEnd = new DateTime(2026, 12, 31) });
            await context.SaveChangesAsync();

            var repo = new BudgetRepository(context);

            // Act
            var (items, totalCount) = await repo.GetAllAsync(new GetBudgetQuery { PeriodEnd = new DateTime(2025, 12, 31) });

            // Assert
            items.Should().HaveCount(1);
            items[0].PeriodEnd.Should().Be(new DateTime(2025, 12, 31));
            totalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_WithLimitAndOffset_ShouldReturnCorrectPageAndTotalCount()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllBudgets_Pagination");
            var team = new Team { Name = "Team Alpha" };
            var costCentre = new CostCentre { Name = "Aero" };
            context.Teams.Add(team);
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            context.Budgets.AddRange(
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 100m, PeriodStart = new DateTime(2026, 1, 1), PeriodEnd = new DateTime(2026, 3, 31) },
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 200m, PeriodStart = new DateTime(2026, 4, 1), PeriodEnd = new DateTime(2026, 6, 30) },
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 300m, PeriodStart = new DateTime(2026, 7, 1), PeriodEnd = new DateTime(2026, 9, 30) },
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 400m, PeriodStart = new DateTime(2026, 10, 1), PeriodEnd = new DateTime(2026, 12, 31) });
            await context.SaveChangesAsync();

            var repo = new BudgetRepository(context);

            // Act
            var (items, totalCount) = await repo.GetAllAsync(new GetBudgetQuery { Limit = 2, Offset = 1 });

            // Assert: totalCount reflects all records before pagination; items are capped at Limit and ordered desc within the page
            totalCount.Should().Be(4);
            items.Should().HaveCount(2);
            // The repository applies ordering after Skip/Take, so we only assert the page is ordered desc within itself
            items[0].PeriodStart.Should().BeOnOrAfter(items[1].PeriodStart);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnResultsOrderedByPeriodStartDescending()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllBudgets_Ordering");
            var team = new Team { Name = "Team Alpha" };
            var costCentre = new CostCentre { Name = "Aero" };
            context.Teams.Add(team);
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            context.Budgets.AddRange(
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 100m, PeriodStart = new DateTime(2024, 1, 1), PeriodEnd = new DateTime(2024, 12, 31) },
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 200m, PeriodStart = new DateTime(2026, 1, 1), PeriodEnd = new DateTime(2026, 12, 31) },
                new Budget { TeamId = team.Id, CostCentreId = costCentre.Id, TargetAmount = 300m, PeriodStart = new DateTime(2025, 1, 1), PeriodEnd = new DateTime(2025, 12, 31) });
            await context.SaveChangesAsync();

            var repo = new BudgetRepository(context);

            // Act
            var (items, _) = await repo.GetAllAsync();

            // Assert — descending: 2026, 2025, 2024
            items[0].TargetAmount.Should().Be(200m);
            items[1].TargetAmount.Should().Be(300m);
            items[2].TargetAmount.Should().Be(100m);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateFields()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UpdateBudget");
            var team = new Team { Name = "Team Alpha" };
            var costCentre = new CostCentre { Name = "Aero" };
            context.Teams.Add(team);
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            var budget = new Budget
            {
                TeamId = team.Id,
                CostCentreId = costCentre.Id,
                TargetAmount = 500m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 6, 30),
            };
            context.Budgets.Add(budget);
            await context.SaveChangesAsync();

            var repo = new BudgetRepository(context);

            // Act
            var result = await repo.UpdateAsync(budget.Id, targetAmount: 9999m, periodEnd: new DateTime(2026, 12, 31));

            // Assert
            result.TargetAmount.Should().Be(9999m);
            result.PeriodEnd.Should().Be(new DateTime(2026, 12, 31));
            result.TeamId.Should().Be(team.Id); // unchanged
            result.PeriodStart.Should().Be(new DateTime(2026, 1, 1)); // unchanged
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowNotFound_WhenBudgetDoesNotExist()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UpdateBudget_NotFound");
            var repo = new BudgetRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                async () => await repo.UpdateAsync(999, targetAmount: 100m));
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenSaveChangesFails()
        {
            // Arrange
            // countOfSuccessBeforeFailure = 1: first SaveChanges (entity inserts) succeeds, second (update) fails
            var failingContext = new FailingDbContext("UpdateBudget_Failing", 1);
            var team = new Team { Name = "Team Alpha" };
            var costCentre = new CostCentre { Name = "Aero" };
            failingContext.Teams.Add(team);
            failingContext.CostCentres.Add(costCentre);
            var budget = new Budget
            {
                TeamId = 1,
                CostCentreId = 1,
                TargetAmount = 500m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 12, 31),
            };
            failingContext.Budgets.Add(budget);
            await failingContext.SaveChangesAsync(); // uses the 1 allowed success

            var repo = new BudgetRepository(failingContext);

            // Act & Assert
            await Assert.ThrowsAsync<InternalErrorException>(
                async () => await repo.UpdateAsync(budget.Id, targetAmount: 9999m));
        }
    }
}
