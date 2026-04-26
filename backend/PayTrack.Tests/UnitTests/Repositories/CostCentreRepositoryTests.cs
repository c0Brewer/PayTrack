using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Dto.CostCentre;
using PayTrack.Application.Exceptions;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Implementation;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Repositories
{
    public class CostCentreRepositoryTests
    {
        private static AppDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName + Guid.NewGuid())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddCostCentreToDatabase()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("AddCostCentre");
            var repo = new CostCentreRepository(context);
            var entity = new CostCentre { Name = "Aero" };

            // Act
            var result = await repo.AddAsync(entity, null);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Aero");
            var dbEntity = await context.CostCentres.FindAsync(result.Id);
            dbEntity.Should().NotBeNull();
        }

        [Fact]
        public async Task AddAsync_WithBudgets_ShouldAddCostCentreAndBudgets()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("AddCostCentreWithBudgets");

            var team = new Team { Name = "Team Alpha" };
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);
            var entity = new CostCentre { Name = "Electronics" };
            var budgets = new List<CreateBudgetEntryDto>
            {
                new(TeamId: team.Id, TargetAmount: 1000m, PeriodStart: new DateTime(2026, 1, 1), PeriodEnd: new DateTime(2026, 12, 31)),
            };

            // Act
            var result = await repo.AddAsync(entity, budgets);

            // Assert
            result.Should().NotBeNull();
            var dbBudgets = await context.Budgets.Where(b => b.CostCentreId == result.Id).ToListAsync();
            dbBudgets.Should().HaveCount(1);
            dbBudgets[0].TargetAmount.Should().Be(1000m);
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenSaveChangesFails()
        {
            // Arrange
            var failingContext = new FailingDbContext("AddCostCentre_Failing");
            var repo = new CostCentreRepository(failingContext);
            var entity = new CostCentre { Name = "Fail" };

            // Act
            var exception = await Assert.ThrowsAsync<InternalErrorException>(
                async () => await repo.AddAsync(entity, null));

            // Assert
            exception.Message.Should().Contain("CostCentre");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCostCentre_WhenExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetCostCentreById");
            var entity = new CostCentre { Name = "Suspension" };
            context.CostCentres.Add(entity);
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);

            // Act
            var result = await repo.GetByIdAsync(entity.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Suspension");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetCostCentreById_NotFound");
            var repo = new CostCentreRepository(context);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCostCentres()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllCostCentres");
            context.CostCentres.AddRange(
                new CostCentre { Name = "Aero" },
                new CostCentre { Name = "Electronics" });
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);

            // Act
            var (items, totalCount) = await repo.GetAllAsync();

            // Assert
            items.Should().HaveCount(2);
            totalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllAsync_WithNameFilter_ShouldReturnMatchingCostCentres()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllCostCentres_NameFilter");
            context.CostCentres.AddRange(
                new CostCentre { Name = "Aerodynamics" },
                new CostCentre { Name = "Powertrain" });
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);

            // Act
            var (items, totalCount) = await repo.GetAllAsync(new GetCostCentreQuery { Name = "Aero" });

            // Assert
            items.Should().HaveCount(1);
            items[0].Name.Should().Be("Aerodynamics");
            totalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_WithDescriptionFilter_ShouldReturnMatchingCostCentres()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllCostCentres_DescriptionFilter");
            context.CostCentres.AddRange(
                new CostCentre { Name = "Aero", Description = "Wind tunnel costs" },
                new CostCentre { Name = "Electronics", Description = "Sensor costs" });
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);

            // Act
            var (items, totalCount) = await repo.GetAllAsync(new GetCostCentreQuery { Description = "Wind" });

            // Assert
            items.Should().HaveCount(1);
            items[0].Name.Should().Be("Aero");
            totalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_WithMinBudgetFilter_ShouldReturnOnlyMatchingCostCentres()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllCostCentres_MinBudget");
            var team = new Team { Name = "Team Alpha" };
            context.Teams.Add(team);
            var costCentreA = new CostCentre { Name = "Aero" };
            var costCentreB = new CostCentre { Name = "Electronics" };
            context.CostCentres.AddRange(costCentreA, costCentreB);
            await context.SaveChangesAsync();

            context.Budgets.AddRange(
                new Budget { TeamId = team.Id, CostCentreId = costCentreA.Id, TargetAmount = 1000m, PeriodStart = DateTime.UtcNow.AddDays(-30), PeriodEnd = DateTime.UtcNow.AddDays(30) },
                new Budget { TeamId = team.Id, CostCentreId = costCentreB.Id, TargetAmount = 100m, PeriodStart = DateTime.UtcNow.AddDays(-30), PeriodEnd = DateTime.UtcNow.AddDays(30) });
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);

            // Act
            var (items, totalCount) = await repo.GetAllAsync(new GetCostCentreQuery { MinBudget = 500m });

            // Assert
            items.Should().HaveCount(1);
            items[0].Name.Should().Be("Aero");
            totalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_WithMaxBudgetFilter_ShouldReturnOnlyMatchingCostCentres()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllCostCentres_MaxBudget");
            var team = new Team { Name = "Team Alpha" };
            context.Teams.Add(team);
            var costCentreA = new CostCentre { Name = "Aero" };
            var costCentreB = new CostCentre { Name = "Electronics" };
            context.CostCentres.AddRange(costCentreA, costCentreB);
            await context.SaveChangesAsync();

            context.Budgets.AddRange(
                new Budget { TeamId = team.Id, CostCentreId = costCentreA.Id, TargetAmount = 1000m, PeriodStart = DateTime.UtcNow.AddDays(-30), PeriodEnd = DateTime.UtcNow.AddDays(30) },
                new Budget { TeamId = team.Id, CostCentreId = costCentreB.Id, TargetAmount = 100m, PeriodStart = DateTime.UtcNow.AddDays(-30), PeriodEnd = DateTime.UtcNow.AddDays(30) });
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);

            // Act
            var (items, totalCount) = await repo.GetAllAsync(new GetCostCentreQuery { MaxBudget = 500m });

            // Assert
            items.Should().HaveCount(1);
            items[0].Name.Should().Be("Electronics");
            totalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_WithBudgetFilter_ShouldExcludeInactiveBudgets()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllCostCentres_InactiveBudget");
            var team = new Team { Name = "Team Alpha" };
            context.Teams.Add(team);
            var costCentre = new CostCentre { Name = "Aero" };
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            // Expired budget — PeriodEnd is in the past, should not match MinBudget filter
            context.Budgets.Add(new Budget
            {
                TeamId = team.Id,
                CostCentreId = costCentre.Id,
                TargetAmount = 1000m,
                PeriodStart = DateTime.UtcNow.AddDays(-60),
                PeriodEnd = DateTime.UtcNow.AddDays(-1),
            });
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);

            // Act
            var (items, totalCount) = await repo.GetAllAsync(new GetCostCentreQuery { MinBudget = 500m });

            // Assert
            items.Should().BeEmpty();
            totalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_WithLimitAndOffset_ShouldReturnCorrectPageAndTotalCount()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllCostCentres_Pagination");
            context.CostCentres.AddRange(
                new CostCentre { Name = "Aero" },
                new CostCentre { Name = "Electronics" },
                new CostCentre { Name = "Powertrain" },
                new CostCentre { Name = "Suspension" },
                new CostCentre { Name = "Tyres" });
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);

            // Act
            var (items, totalCount) = await repo.GetAllAsync(new GetCostCentreQuery { Limit = 2, Offset = 1 });

            // Assert
            totalCount.Should().Be(5); // total before pagination
            items.Should().HaveCount(2);
            // OrderBy Name: Aero(0), Electronics(1), Powertrain(2), Suspension(3), Tyres(4)
            // Offset=1 skips Aero; Limit=2 returns Electronics and Powertrain
            items[0].Name.Should().Be("Electronics");
            items[1].Name.Should().Be("Powertrain");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateFields()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UpdateCostCentre");
            var entity = new CostCentre { Name = "Old Name", Description = "Old Desc" };
            context.CostCentres.Add(entity);
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);

            // Act
            var result = await repo.UpdateAsync(entity.Id, "New Name", null, null, null, null);

            // Assert
            result.Name.Should().Be("New Name");
            result.Description.Should().Be("Old Desc");
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowNotFound_WhenCostCentreDoesNotExist()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UpdateCostCentre_NotFound");
            var repo = new CostCentreRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                async () => await repo.UpdateAsync(999, "Name", null, null, null, null));
        }

        [Fact]
        public async Task GetDeletePreviewAsync_ShouldReturnCounts()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("DeletePreview");

            var team = new Team { Name = "Team Alpha" };
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var costCentre = new CostCentre { Name = "Powertrain" };
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            context.Budgets.Add(new Budget
            {
                TeamId = team.Id,
                CostCentreId = costCentre.Id,
                TargetAmount = 500m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 12, 31),
            });
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);

            // Act
            var preview = await repo.GetDeletePreviewAsync(costCentre.Id);

            // Assert
            preview.CostCentreName.Should().Be("Powertrain");
            preview.BudgetCount.Should().Be(1);
            preview.TransactionCount.Should().Be(0);
            preview.AffectedUserCount.Should().Be(0);
            preview.AffectedTeamNames.Should().ContainSingle(n => n == "Team Alpha");
        }

        [Fact]
        public async Task GetDeletePreviewAsync_ShouldReturnCorrectAffectedUserCount()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("DeletePreview_AffectedUsers");

            var team = new Team { Name = "Team Alpha" };
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var user1 = new User { Name = "Alice", Email = "alice@test.com", TeamId = team.Id };
            var user2 = new User { Name = "Bob", Email = "bob@test.com", TeamId = team.Id };
            context.User.Add(user1);
            context.User.Add(user2);
            await context.SaveChangesAsync();

            var costCentre = new CostCentre { Name = "Powertrain" };
            context.CostCentres.Add(costCentre);
            await context.SaveChangesAsync();

            // 2 transactions from user1, 1 from user2 → 2 distinct users
            context.PaymentManuals.AddRange(
                new PaymentManual { UserId = user1.Id, TeamId = team.Id, CostCentreId = costCentre.Id, Amount = 100m, PaymentDirection = PaymentDirection.Out },
                new PaymentManual { UserId = user1.Id, TeamId = team.Id, CostCentreId = costCentre.Id, Amount = 200m, PaymentDirection = PaymentDirection.Out },
                new PaymentManual { UserId = user2.Id, TeamId = team.Id, CostCentreId = costCentre.Id, Amount = 300m, PaymentDirection = PaymentDirection.Out });
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);

            // Act
            var preview = await repo.GetDeletePreviewAsync(costCentre.Id);

            // Assert
            preview.TransactionCount.Should().Be(3);
            preview.AffectedUserCount.Should().Be(2);
        }

        [Fact]
        public async Task GetDeletePreviewAsync_ShouldThrowNotFound_WhenNotExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("DeletePreview_NotFound");
            var repo = new CostCentreRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                async () => await repo.GetDeletePreviewAsync(999));
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveCostCentre_WhenNoLinkedRecords()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("DeleteCostCentre");
            var entity = new CostCentre { Name = "Empty" };
            context.CostCentres.Add(entity);
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);

            // Act
            await repo.DeleteAsync(entity.Id);

            // Assert
            var dbEntity = await context.CostCentres.FindAsync(entity.Id);
            dbEntity.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowInvalidState_WhenBudgetsExist()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("DeleteCostCentre_WithBudgets");

            var team = new Team { Name = "Team Alpha" };
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var costCentre = new CostCentre { Name = "Linked" };
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

            var repo = new CostCentreRepository(context);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidStateException>(
                async () => await repo.DeleteAsync(costCentre.Id));

            exception.Message.Should().Contain("budget(s)");
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowNotFound_WhenNotExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("DeleteCostCentre_NotFound");
            var repo = new CostCentreRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                async () => await repo.DeleteAsync(999));
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenSaveChangesFails()
        {
            // Arrange
            // countOfSuccessBeforeFailure = 1: first SaveChanges (entity insert) succeeds, second (delete) fails
            var failingContext = new FailingDbContext("DeleteCostCentre_Failing", 1);
            var entity = new CostCentre { Name = "ToDelete" };
            failingContext.CostCentres.Add(entity);
            await failingContext.SaveChangesAsync();

            var repo = new CostCentreRepository(failingContext);

            // Act & Assert
            await Assert.ThrowsAsync<InternalErrorException>(
                async () => await repo.DeleteAsync(entity.Id));
        }

        [Fact]
        public async Task UpdateAsync_WithNewBudgetEntry_ShouldAddBudget()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UpdateCostCentre_AddBudget");
            var team = new Team { Name = "Team A" };
            context.Teams.Add(team);
            var entity = new CostCentre { Name = "Aero" };
            context.CostCentres.Add(entity);
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);
            var budgetsToUpsert = new List<UpsertBudgetEntryDto>
            {
                new(Id: null, TeamId: team.Id, TargetAmount: 1000m, PeriodStart: new DateTime(2026, 1, 1), PeriodEnd: new DateTime(2026, 12, 31)),
            };

            // Act
            await repo.UpdateAsync(entity.Id, null, null, null, budgetsToUpsert, null);

            // Assert
            var budgets = await context.Budgets.Where(b => b.CostCentreId == entity.Id).ToListAsync();
            budgets.Should().HaveCount(1);
            budgets[0].TargetAmount.Should().Be(1000m);
            budgets[0].TeamId.Should().Be(team.Id);
        }

        [Fact]
        public async Task UpdateAsync_WithExistingBudgetId_ShouldUpdateBudget()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UpdateCostCentre_UpdateBudget");
            var team = new Team { Name = "Team A" };
            context.Teams.Add(team);
            var entity = new CostCentre { Name = "Aero" };
            context.CostCentres.Add(entity);
            await context.SaveChangesAsync();

            var budget = new Budget
            {
                TeamId = team.Id,
                CostCentreId = entity.Id,
                TargetAmount = 500m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 6, 30),
            };
            context.Budgets.Add(budget);
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);
            var budgetsToUpsert = new List<UpsertBudgetEntryDto>
            {
                new(Id: budget.Id, TeamId: team.Id, TargetAmount: 9999m, PeriodStart: new DateTime(2026, 1, 1), PeriodEnd: new DateTime(2026, 12, 31)),
            };

            // Act
            await repo.UpdateAsync(entity.Id, null, null, null, budgetsToUpsert, null);

            // Assert
            var updated = await context.Budgets.FindAsync(budget.Id);
            updated!.TargetAmount.Should().Be(9999m);
            updated.PeriodEnd.Should().Be(new DateTime(2026, 12, 31));
        }

        [Fact]
        public async Task UpdateAsync_WithBudgetIdToDelete_ShouldRemoveBudget()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UpdateCostCentre_DeleteBudget");
            var team = new Team { Name = "Team A" };
            context.Teams.Add(team);
            var entity = new CostCentre { Name = "Aero" };
            context.CostCentres.Add(entity);
            await context.SaveChangesAsync();

            var budget = new Budget
            {
                TeamId = team.Id,
                CostCentreId = entity.Id,
                TargetAmount = 500m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 12, 31),
            };
            context.Budgets.Add(budget);
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);

            // Act
            await repo.UpdateAsync(entity.Id, null, null, null, null, [budget.Id]);

            // Assert
            var deleted = await context.Budgets.FindAsync(budget.Id);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_WithMixedUpsertAndDelete_ShouldApplyBoth()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UpdateCostCentre_Mixed");
            var team = new Team { Name = "Team A" };
            context.Teams.Add(team);
            var entity = new CostCentre { Name = "Aero" };
            context.CostCentres.Add(entity);
            await context.SaveChangesAsync();

            var budgetToDelete = new Budget
            {
                TeamId = team.Id,
                CostCentreId = entity.Id,
                TargetAmount = 100m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 6, 30),
            };
            context.Budgets.Add(budgetToDelete);
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);
            var budgetsToUpsert = new List<UpsertBudgetEntryDto>
            {
                new(Id: null, TeamId: team.Id, TargetAmount: 5000m, PeriodStart: new DateTime(2026, 7, 1), PeriodEnd: new DateTime(2026, 12, 31)),
            };

            // Act
            await repo.UpdateAsync(entity.Id, null, null, null, budgetsToUpsert, [budgetToDelete.Id]);

            // Assert
            var deleted = await context.Budgets.FindAsync(budgetToDelete.Id);
            deleted.Should().BeNull();

            var remaining = await context.Budgets.Where(b => b.CostCentreId == entity.Id).ToListAsync();
            remaining.Should().HaveCount(1);
            remaining[0].TargetAmount.Should().Be(5000m);
        }

        [Fact]
        public async Task UpdateAsync_WithUnknownBudgetIdToDelete_ShouldThrowNotFoundException()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UpdateCostCentre_DeleteUnknown");
            var entity = new CostCentre { Name = "Aero" };
            context.CostCentres.Add(entity);
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                async () => await repo.UpdateAsync(entity.Id, null, null, null, null, [999]));
        }

        [Fact]
        public async Task UpdateAsync_WithUnknownExistingBudgetIdToUpsert_ShouldThrowNotFoundException()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UpdateCostCentre_UpsertUnknown");
            var entity = new CostCentre { Name = "Aero" };
            context.CostCentres.Add(entity);
            await context.SaveChangesAsync();

            var repo = new CostCentreRepository(context);
            var budgetsToUpsert = new List<UpsertBudgetEntryDto>
            {
                new(Id: 999, TeamId: 1, TargetAmount: 100m, PeriodStart: new DateTime(2026, 1, 1), PeriodEnd: new DateTime(2026, 12, 31)),
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                async () => await repo.UpdateAsync(entity.Id, null, null, null, budgetsToUpsert, null));
        }

        [Fact]
        public async Task UpdateAsync_WithAllNullInputs_ShouldReturnEntityWithoutSaving()
        {
            // Arrange
            // FailingDbContext returns 0 from SaveChangesAsync after the first real save.
            // If SaveChangesAsync is called here it returns 0, which would trigger InternalErrorException.
            // Passing without exception proves SaveChangesAsync was NOT called.
            var failingContext = new FailingDbContext("UpdateCostCentre_AllNull", 1);
            var entity = new CostCentre { Name = "Aero" };
            failingContext.CostCentres.Add(entity);
            await failingContext.SaveChangesAsync(); // uses the 1 allowed success

            var repo = new CostCentreRepository(failingContext);

            // Act
            var result = await repo.UpdateAsync(entity.Id, null, null, null, null, null);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Aero");
        }
    }
}
