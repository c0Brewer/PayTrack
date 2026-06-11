//AI helped with the test cases

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayTrack.Application.Exceptions;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Implementation;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Repositories
{
    public class SeasonRepositoryTests
    {
        private static AppDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName + Guid.NewGuid())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddSeasonToDatabase()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("AddSeason");
            var repo = new SeasonRepository(context);
            var entity = new Season { Name = "2026" };

            // Act
            var result = await repo.AddAsync(entity);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("2026");
            result.IsActive.Should().BeTrue();
            var dbEntity = await context.Seasons.FindAsync(result.Id);
            dbEntity.Should().NotBeNull();
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenSaveChangesFails()
        {
            // Arrange
            var failingContext = new FailingDbContext("AddSeason_Failing");
            var repo = new SeasonRepository(failingContext);
            var entity = new Season { Name = "Fail" };

            // Act
            var exception = await Assert.ThrowsAsync<InternalErrorException>(
                async () => await repo.AddAsync(entity));

            // Assert
            exception.Message.Should().Contain("Season");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllSeasonsOrderedByName()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllSeasons");
            context.Seasons.AddRange(
                new Season { Name = "2027" },
                new Season { Name = "2025" },
                new Season { Name = "2026" },
                new Season { Name = "2024", IsActive = false });
            await context.SaveChangesAsync();

            var repo = new SeasonRepository(context);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Select(s => s.Name).Should().Equal("2025", "2026", "2027");
        }

        [Fact]
        public async Task GetAllAsync_ShouldIncludeBudgets()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetAllSeasons_WithBudgets");
            var team = new Team { Name = "Team Alpha" };
            var costCentre = new CostCentre { Name = "Aero" };
            var season = new Season { Name = "2026" };
            context.Teams.Add(team);
            context.CostCentres.Add(costCentre);
            context.Seasons.Add(season);
            await context.SaveChangesAsync();

            context.Budgets.Add(new Budget
            {
                Name = "Season budget",
                TeamId = team.Id,
                CostCentreId = costCentre.Id,
                SeasonId = season.Id,
                TargetAmount = 1000m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 12, 31),
            });
            await context.SaveChangesAsync();

            var repo = new SeasonRepository(context);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().ContainSingle();
            result[0].Budgets.Should().ContainSingle(b => b.Name == "Season budget");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnSeason_WhenExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetSeasonById");
            var entity = new Season { Name = "2026" };
            context.Seasons.Add(entity);
            await context.SaveChangesAsync();

            var repo = new SeasonRepository(context);

            // Act
            var result = await repo.GetByIdAsync(entity.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("2026");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("GetSeasonById_NotFound");
            var repo = new SeasonRepository(context);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateName()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UpdateSeason");
            var entity = new Season { Name = "2026" };
            context.Seasons.Add(entity);
            await context.SaveChangesAsync();

            var repo = new SeasonRepository(context);

            // Act
            var result = await repo.UpdateAsync(entity.Id, "2027", null);

            // Assert
            result.Name.Should().Be("2027");
            var dbEntity = await context.Seasons.FindAsync(entity.Id);
            dbEntity!.Name.Should().Be("2027");
        }

        [Fact]
        public async Task UpdateAsync_WithNullName_ShouldReturnEntityWithoutSaving()
        {
            // Arrange
            var failingContext = new FailingDbContext("UpdateSeason_NullName", 1);
            var entity = new Season { Name = "2026" };
            failingContext.Seasons.Add(entity);
            await failingContext.SaveChangesAsync();

            var repo = new SeasonRepository(failingContext);

            // Act
            var result = await repo.UpdateAsync(entity.Id, null, null);

            // Assert
            result.Name.Should().Be("2026");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateIsActive()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UpdateSeason_IsActive");
            var entity = new Season { Name = "2026" };
            context.Seasons.Add(entity);
            await context.SaveChangesAsync();

            var repo = new SeasonRepository(context);

            // Act
            var result = await repo.UpdateAsync(entity.Id, null, false);

            // Assert
            result.IsActive.Should().BeFalse();
            var dbEntity = await context.Seasons.FindAsync(entity.Id);
            dbEntity!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowNotFound_WhenSeasonDoesNotExist()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("UpdateSeason_NotFound");
            var repo = new SeasonRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                async () => await repo.UpdateAsync(999, "2027", null));
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenSaveChangesFails()
        {
            // Arrange
            var failingContext = new FailingDbContext("UpdateSeason_Failing", 1);
            var entity = new Season { Name = "2026" };
            failingContext.Seasons.Add(entity);
            await failingContext.SaveChangesAsync();

            var repo = new SeasonRepository(failingContext);

            // Act & Assert
            await Assert.ThrowsAsync<InternalErrorException>(
                async () => await repo.UpdateAsync(entity.Id, "2027", null));
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveSeason_WhenNoBudgetsExist()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("DeleteSeason");
            var entity = new Season { Name = "2026" };
            context.Seasons.Add(entity);
            await context.SaveChangesAsync();

            var repo = new SeasonRepository(context);

            // Act
            await repo.DeleteAsync(entity.Id);

            // Assert
            var dbEntity = await context.Seasons.FindAsync(entity.Id);
            dbEntity.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeactivateSeason_WhenBudgetsExist()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("DeleteSeason_WithBudgets");
            var team = new Team { Name = "Team Alpha" };
            var costCentre = new CostCentre { Name = "Aero" };
            var season = new Season { Name = "2026" };
            context.Teams.Add(team);
            context.CostCentres.Add(costCentre);
            context.Seasons.Add(season);
            await context.SaveChangesAsync();

            context.Budgets.Add(new Budget
            {
                Name = "Linked budget",
                TeamId = team.Id,
                CostCentreId = costCentre.Id,
                SeasonId = season.Id,
                TargetAmount = 100m,
                PeriodStart = new DateTime(2026, 1, 1),
                PeriodEnd = new DateTime(2026, 12, 31),
            });
            await context.SaveChangesAsync();

            var repo = new SeasonRepository(context);

            // Act
            var result = await repo.DeleteAsync(season.Id);

            // Assert
            result.Should().NotBeNull();
            result!.IsActive.Should().BeFalse();
            var dbEntity = await context.Seasons.FindAsync(season.Id);
            dbEntity.Should().NotBeNull();
            dbEntity!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowNotFound_WhenNotExists()
        {
            // Arrange
            await using var context = GetInMemoryDbContext("DeleteSeason_NotFound");
            var repo = new SeasonRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                async () => await repo.DeleteAsync(999));
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenSaveChangesFails()
        {
            // Arrange
            var failingContext = new FailingDbContext("DeleteSeason_Failing", 1);
            var entity = new Season { Name = "2026" };
            failingContext.Seasons.Add(entity);
            await failingContext.SaveChangesAsync();

            var repo = new SeasonRepository(failingContext);

            // Act & Assert
            await Assert.ThrowsAsync<InternalErrorException>(
                async () => await repo.DeleteAsync(entity.Id));
        }
    }
}
