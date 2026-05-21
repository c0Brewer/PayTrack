//AI helped with the test cases

using FluentAssertions;
using Moq;
using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Implementation;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Tests.UnitTests.Services
{
    public class BudgetServiceTests
    {
        private readonly Mock<IBudgetRepository> repoMock;
        private readonly BudgetService service;

        public BudgetServiceTests()
        {
            this.repoMock = new Mock<IBudgetRepository>();
            this.service = new BudgetService(this.repoMock.Object);
        }

        [Fact]
        public async Task GetBudgetsAsync_ShouldReturnBudgetsFromRepo()
        {
            // Arrange
            var query = new GetBudgetQuery { Name = "Aero" };
            var budgets = new List<Budget>
            {
                new()
                {
                    Id = 1,
                    Name = "Aero budget",
                    TeamId = 2,
                    CostCentreId = 3,
                    SeasonId = 4,
                },
            };
            this.repoMock.Setup(r => r.GetAllAsync(query)).ReturnsAsync((budgets, budgets.Count));

            // Act
            var result = await this.service.GetBudgetsAsync(query);

            // Assert
            result.budget.Should().BeSameAs(budgets);
            result.totalCount.Should().Be(1);
            this.repoMock.Verify(r => r.GetAllAsync(query), Times.Once);
        }

        [Fact]
        public async Task GetBudgetByIdAsync_ShouldReturnBudgetFromRepo()
        {
            // Arrange
            var budget = new Budget { Id = 5, Name = "Platform budget" };
            this.repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(budget);

            // Act
            var result = await this.service.GetByIdAsync(5);

            // Assert
            result.Should().BeSameAs(budget);
            this.repoMock.Verify(r => r.GetByIdAsync(5), Times.Once);
        }

        [Fact]
        public async Task CreateBudgetAsync_ShouldValidatePeriodAndCallRepo()
        {
            // Arrange
            var periodStart = new DateTime(2026, 1, 1);
            var periodEnd = new DateTime(2026, 12, 31);
            var created = new Budget { Id = 1, Name = "2026 budget" };

            this.repoMock
                .Setup(r => r.AddAsync(
                    "2026 budget",
                    "Season budget",
                    2,
                    3,
                    4,
                    1000,
                    It.Is<DateTime>(d => d.Kind == DateTimeKind.Utc && d.Date == periodStart.Date),
                    It.Is<DateTime>(d => d.Kind == DateTimeKind.Utc && d.Date == periodEnd.Date)))
                .ReturnsAsync(created);

            // Act
            var result = await this.service.CreateBudgetAsync(
                "2026 budget",
                "Season budget",
                2,
                3,
                4,
                1000,
                periodStart,
                periodEnd);

            // Assert
            result.Should().BeSameAs(created);
        }

        [Fact]
        public async Task CreateBudgetAsync_ShouldThrow_WhenPeriodEndIsBeforeStart()
        {
            // Act
            var act = () => this.service.CreateBudgetAsync(
                "Invalid budget",
                null,
                1,
                2,
                3,
                100,
                new DateTime(2026, 12, 31),
                new DateTime(2026, 1, 1));

            // Assert
            await act.Should().ThrowAsync<InvalidStateException>();
            this.repoMock.Verify(
                r => r.AddAsync(
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<decimal>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateBudgetAsync_ShouldValidatePeriodAndCallRepo()
        {
            // Arrange
            var periodStart = new DateTime(2026, 2, 1);
            var periodEnd = new DateTime(2026, 11, 30);
            var updated = new Budget { Id = 7, Name = "Updated budget" };

            this.repoMock
                .Setup(r => r.UpdateAsync(
                    7,
                    "Updated budget",
                    "Updated",
                    2,
                    3,
                    4,
                    500,
                    It.Is<DateTime>(d => d.Kind == DateTimeKind.Utc && d.Date == periodStart.Date),
                    It.Is<DateTime>(d => d.Kind == DateTimeKind.Utc && d.Date == periodEnd.Date)))
                .ReturnsAsync(updated);

            // Act
            var result = await this.service.UpdateBudgetAsync(
                7,
                "Updated budget",
                "Updated",
                2,
                3,
                4,
                500,
                periodStart,
                periodEnd);

            // Assert
            result.Should().BeSameAs(updated);
        }

        [Fact]
        public async Task UpdateBudgetAsync_ShouldAllowPartialPeriodUpdate()
        {
            // Arrange
            var periodStart = new DateTime(2026, 3, 1);
            var updated = new Budget { Id = 8, Name = "Partial budget" };

            this.repoMock
                .Setup(r => r.UpdateAsync(
                    8,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    It.Is<DateTime>(d => d.Kind == DateTimeKind.Utc && d.Date == periodStart.Date),
                    null))
                .ReturnsAsync(updated);

            // Act
            var result = await this.service.UpdateBudgetAsync(8, periodStart: periodStart);

            // Assert
            result.Should().BeSameAs(updated);
        }

        [Fact]
        public async Task UpdateBudgetAsync_ShouldThrow_WhenPeriodEndIsBeforeStart()
        {
            // Act
            var act = () => this.service.UpdateBudgetAsync(
                1,
                periodStart: new DateTime(2026, 12, 31),
                periodEnd: new DateTime(2026, 1, 1));

            // Assert
            await act.Should().ThrowAsync<InvalidStateException>();
            this.repoMock.Verify(
                r => r.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteBudgetAsync_ShouldCallRepo()
        {
            // Arrange
            this.repoMock.Setup(r => r.DeleteAsync(9)).Returns(Task.CompletedTask);

            // Act
            await this.service.DeleteBudgetAsync(9);

            // Assert
            this.repoMock.Verify(r => r.DeleteAsync(9), Times.Once);
        }
    }
}
