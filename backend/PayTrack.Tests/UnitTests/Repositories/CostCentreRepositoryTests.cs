using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
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
            var result = await repo.UpdateAsync(entity.Id, "New Name", null, null);

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
                async () => await repo.UpdateAsync(999, "Name", null, null));
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
            preview.AffectedTeamNames.Should().ContainSingle(n => n == "Team Alpha");
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
    }
}
