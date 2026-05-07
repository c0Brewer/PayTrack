using FluentAssertions;
using Moq;
using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Dto.CostCentre;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Implementation;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Tests.UnitTests.Services
{
    public class CostCentreServiceTests
    {
        private readonly Mock<ICostCentreRepository> repoMock;
        private readonly CostCentreService service;

        public CostCentreServiceTests()
        {
            repoMock = new Mock<ICostCentreRepository>();
            service = new CostCentreService(repoMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCostCentres()
        {
            // Arrange
            var costCentres = new List<CostCentre>
            {
                new() { Id = 1, Name = "Aero" },
                new() { Id = 2, Name = "Electronics" },
            };
            repoMock.Setup(r => r.GetAllAsync(null)).ReturnsAsync((costCentres, costCentres.Count));

            // Act
            var (items, totalCount) = await service.GetAllAsync();

            // Assert
            items.Should().HaveCount(2);
            items.Should().ContainSingle(c => c.Name == "Aero");
            items.Should().ContainSingle(c => c.Name == "Electronics");
            totalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllAsync_ShouldForwardQueryToRepository()
        {
            // Arrange
            var costCentres = new List<CostCentre> { new() { Id = 1, Name = "Aero" } };
            var query = new GetCostCentreQuery { Name = "Aero", Limit = 10, Offset = 0 };
            repoMock.Setup(r => r.GetAllAsync(query)).ReturnsAsync((costCentres, 1));

            // Act
            var (items, totalCount) = await service.GetAllAsync(query);

            // Assert
            items.Should().HaveCount(1);
            totalCount.Should().Be(1);
            repoMock.Verify(r => r.GetAllAsync(query), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCostCentre()
        {
            // Arrange
            var expected = new CostCentre { Id = 5, Name = "Suspension" };
            repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(5);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(5);
            result.Name.Should().Be("Suspension");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((CostCentre?)null);

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_ShouldCallRepoAndReturnCostCentre()
        {
            // Arrange
            const string name = "Powertrain";
            const string description = "Engine and drivetrain costs";
            const string color = "#FF0000";
            repoMock.Setup(r => r.AddAsync(It.IsAny<CostCentre>(), null))
                    .ReturnsAsync((CostCentre c, IList<CreateCostCentreBudgetEntryDto>? _) => c);

            // Act
            var result = await service.CreateAsync(name, description, color, null);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(name);
            result.Description.Should().Be(description);
            result.DisplayColor.Should().Be(color);
            repoMock.Verify(r => r.AddAsync(It.Is<CostCentre>(c => c.Name == name), null), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithBudgets_ShouldPassBudgetEntriesToRepo()
        {
            // Arrange
            var budgets = new List<CreateCostCentreBudgetEntryDto>
            {
                new(TeamId: 1, TargetAmount: 5000m, PeriodStart: new DateTime(2026, 1, 1), PeriodEnd: new DateTime(2026, 12, 31)),
            };
            repoMock.Setup(r => r.AddAsync(It.IsAny<CostCentre>(), budgets))
                    .ReturnsAsync((CostCentre c, IList<CreateCostCentreBudgetEntryDto>? _) => c);

            // Act
            var result = await service.CreateAsync("Aero", null, null, budgets);

            // Assert
            result.Should().NotBeNull();
            repoMock.Verify(r => r.AddAsync(It.IsAny<CostCentre>(), budgets), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallRepoWithCorrectArgs()
        {
            // Arrange
            var updated = new CostCentre { Id = 3, Name = "NewName" };
            repoMock.Setup(r => r.UpdateAsync(3, "NewName", null, null, null, null)).ReturnsAsync(updated);

            // Act
            var result = await service.UpdateAsync(3, "NewName", null, null, null, null);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("NewName");
            repoMock.Verify(r => r.UpdateAsync(3, "NewName", null, null, null, null), Times.Once);
        }

        [Fact]
        public async Task GetDeletePreviewAsync_ShouldReturnPreviewFromRepo()
        {
            // Arrange
            var preview = new DeleteCostCentrePreviewDto("Aero", 2, 5, 3, ["Team Alpha"]);
            repoMock.Setup(r => r.GetDeletePreviewAsync(1)).ReturnsAsync(preview);

            // Act
            var result = await service.GetDeletePreviewAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.BudgetCount.Should().Be(2);
            result.TransactionCount.Should().Be(5);
            result.AffectedUserCount.Should().Be(3);
            result.AffectedTeamNames.Should().ContainSingle(n => n == "Team Alpha");
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNull_WhenRepoPerformsHardDelete()
        {
            // Arrange
            repoMock.Setup(r => r.DeleteAsync(7)).ReturnsAsync((CostCentre?)null);

            // Act
            var result = await service.DeleteAsync(7);

            // Assert
            result.Should().BeNull();
            repoMock.Verify(r => r.DeleteAsync(7), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnDeactivatedCostCentre_WhenRepoPerformsSoftDelete()
        {
            // Arrange
            var deactivated = new CostCentre { Id = 7, Name = "Linked", IsActive = false };
            repoMock.Setup(r => r.DeleteAsync(7)).ReturnsAsync(deactivated);

            // Act
            var result = await service.DeleteAsync(7);

            // Assert
            result.Should().NotBeNull();
            result!.IsActive.Should().BeFalse();
            repoMock.Verify(r => r.DeleteAsync(7), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithOverlappingBudgetIds_ShouldThrowInvalidStateException()
        {
            // Arrange
            var budgetsToUpsert = new List<UpsertCostCentreBudgetEntryDto>
            {
                new(Id: 5, TeamId: 1, TargetAmount: 100m, PeriodStart: new DateTime(2026, 1, 1), PeriodEnd: new DateTime(2026, 12, 31)),
            };
            var budgetIdsToDelete = new List<int> { 5 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidStateException>(
                async () => await service.UpdateAsync(1, null, null, null, budgetsToUpsert, budgetIdsToDelete));

            exception.Message.Should().Contain("BudgetsToUpsert");
            repoMock.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<IList<UpsertCostCentreBudgetEntryDto>?>(), It.IsAny<IList<int>?>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithBothListsPopulated_ShouldPassBothToRepo()
        {
            // Arrange
            var budgetsToUpsert = new List<UpsertCostCentreBudgetEntryDto>
            {
                new(Id: null, TeamId: 2, TargetAmount: 500m, PeriodStart: new DateTime(2026, 1, 1), PeriodEnd: new DateTime(2026, 12, 31)),
            };
            var budgetIdsToDelete = new List<int> { 10 };
            var updated = new CostCentre { Id = 1, Name = "Aero" };
            repoMock.Setup(r => r.UpdateAsync(1, null, null, null, budgetsToUpsert, budgetIdsToDelete)).ReturnsAsync(updated);

            // Act
            var result = await service.UpdateAsync(1, null, null, null, budgetsToUpsert, budgetIdsToDelete);

            // Assert
            result.Should().NotBeNull();
            repoMock.Verify(r => r.UpdateAsync(1, null, null, null, budgetsToUpsert, budgetIdsToDelete), Times.Once);
        }
    }
}
