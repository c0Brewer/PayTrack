using Moq;
using FluentAssertions;
using PayTrack.Application.Dto.Budget;
using PayTrack.Application.Dto.Team;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;
using PayTrack.Application.Services.Implementation;
using PayTrack.Application.Exceptions;

namespace PayTrack.Tests.UnitTests.Services
{
    public class TeamServiceTests
    {
        private readonly Mock<ITeamRepository> repoMock;
        private readonly TeamService service;

        public TeamServiceTests()
        {
            repoMock = new Mock<ITeamRepository>();
            service = new TeamService(repoMock.Object);
        }

        [Fact]
        public async Task CreateTeamAsync_ShouldCallRepoAndReturnTeam()
        {
            // Arrange
            const string teamName = "My Team";
            const string teamDescription = "My Description";
            const string teamColor = "My Color";
            var expectedTeam = new Team { Name = teamName, Description = teamDescription, DisplayColor = teamColor };
            repoMock.Setup(r => r.AddAsync(It.IsAny<Team>(), null))
                    .ReturnsAsync((Team t, IList<CreateTeamBudgetEntryDto>? _) => t);

            // Act
            var result = await service.CreateTeamAsync(teamName, teamDescription, teamColor, null);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(teamName);
            result.Description.Should().Be(teamDescription);
            result.DisplayColor.Should().Be(teamColor);
            repoMock.Verify(r => r.AddAsync(It.Is<Team>(t => t.Name == teamName), null), Times.Once);
        }

        [Fact]
        public async Task GetTeamByIdAsync_ShouldForwardQueryToRepo()
        {
            // Arrange
            const int teamId = 42;
            var query = new GetTeamQueryById
            {
                IncludeMembers = true,
                IncludeBudgets = true,
            };
            var expectedTeam = new Team { Id = teamId, Name = "Team42" };
            repoMock.Setup(r => r.GetByIdAsync(
                    teamId,
                    It.Is<GetTeamQueryById?>(q => q != null && q.IncludeMembers == true && q.IncludeBudgets == true)))
                    .ReturnsAsync(expectedTeam);

            // Act
            var result = await service.GetTeamByIdAsync(teamId, query);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(teamId);
            result.Name.Should().Be("Team42");
            repoMock.Verify(r => r.GetByIdAsync(
                teamId,
                It.Is<GetTeamQueryById?>(q => q != null && q.IncludeMembers == true && q.IncludeBudgets == true)), Times.Once);
        }

        [Fact]
        public async Task GetTeamsAsync_ShouldReturnAllTeams()
        {
            // Arrange
            var teams = new List<Team>
            {
                new() { Id = 1, Name = "Team1" },
                new() { Id = 2, Name = "Team2" }
            };
            repoMock.Setup(r => r.GetAllAsync(null))
                    .ReturnsAsync((teams, 2));

            // Act
            var (resultList, totalCount) = await service.GetTeamsAsync();

            // Assert
            resultList.Should().HaveCount(2);
            resultList.Should().ContainSingle(t => t.Name == "Team1");
            resultList.Should().ContainSingle(t => t.Name == "Team2");
            totalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetTeamsAsync_ShouldForwardQueryToRepoAndReturnPaginationData()
        {
            // Arrange
            var query = new GetTeamQuery
            {
                Name = "Team",
                IsActive = true,
                IncludeMembers = true,
                Limit = 1,
                Offset = 2,
            };

            var teams = new List<Team>
            {
                new() { Id = 3, Name = "Team3" },
            };

            repoMock.Setup(r => r.GetAllAsync(
                    It.Is<GetTeamQuery?>(q =>
                        q != null &&
                        q.Name == query.Name &&
                        q.IsActive == query.IsActive &&
                        q.IncludeMembers == query.IncludeMembers &&
                        q.Limit == query.Limit &&
                        q.Offset == query.Offset)))
                .ReturnsAsync((teams, 4));

            // Act
            var (resultList, totalCount) = await service.GetTeamsAsync(query);

            // Assert
            resultList.Should().ContainSingle();
            resultList[0].Name.Should().Be("Team3");
            totalCount.Should().Be(4);
            repoMock.Verify(r => r.GetAllAsync(
                It.Is<GetTeamQuery?>(q =>
                    q != null &&
                    q.Name == query.Name &&
                    q.IsActive == query.IsActive &&
                    q.IncludeMembers == query.IncludeMembers &&
                    q.Limit == query.Limit &&
                    q.Offset == query.Offset)), Times.Once);
        }

        [Fact]
        public async Task UpdateTeamAsync_ShouldForwardArgumentsToRepo()
        {
            // Arrange
            var expectedTeam = new Team
            {
                Id = 8,
                Name = "Updated Team",
                Description = "New Description",
                DisplayColor = "#112233",
            };

            repoMock.Setup(r => r.UpdateAsync(8, "Updated Team", "New Description", "#112233", null, null))
                .ReturnsAsync(expectedTeam);

            // Act
            var result = await service.UpdateTeamAsync(8, "Updated Team", "New Description", "#112233", null, null);

            // Assert
            result.Should().BeSameAs(expectedTeam);
            repoMock.Verify(r => r.UpdateAsync(8, "Updated Team", "New Description", "#112233", null, null), Times.Once);
        }

        [Fact]
        public async Task UpdateTeamAsync_ShouldThrow_WhenBudgetIdIsUpsertedAndDeleted()
        {
            // Arrange
            var budgetsToUpsert = new List<UpsertTeamBudgetEntryDto>
            {
                new(5, 10, 100m, new DateTime(2026, 1, 1), new DateTime(2026, 1, 31)),
            };
            var budgetIdsToDelete = new List<int> { 5 };

            // Act
            var act = async () => await service.UpdateTeamAsync(8, null, null, null, budgetsToUpsert, budgetIdsToDelete);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidStateException>()
                .WithMessage("A budget ID cannot appear in both BudgetsToUpsert and BudgetIdsToDelete.");
            repoMock.Verify(
                r => r.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<IList<UpsertTeamBudgetEntryDto>?>(),
                    It.IsAny<IList<int>?>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteTeamAsync_ShouldReturnNull_WhenRepoPerformsHardDelete()
        {
            // Arrange
            repoMock.Setup(r => r.DeleteAsync(4))
                .ReturnsAsync((Team?)null);

            // Act
            var result = await service.DeleteTeamAsync(4);

            // Assert
            result.Should().BeNull();
            repoMock.Verify(r => r.DeleteAsync(4), Times.Once);
        }

        [Fact]
        public async Task DeleteTeamAsync_ShouldReturnDeactivatedTeam_WhenRepoPerformsSoftDelete()
        {
            // Arrange
            var expectedTeam = new Team { Id = 4, Name = "Delete Me", IsActive = false };
            repoMock.Setup(r => r.DeleteAsync(4))
                .ReturnsAsync(expectedTeam);

            // Act
            var result = await service.DeleteTeamAsync(4);

            // Assert
            result.Should().BeSameAs(expectedTeam);
            result!.IsActive.Should().BeFalse();
            repoMock.Verify(r => r.DeleteAsync(4), Times.Once);
        }

        [Fact]
        public async Task GetDeleteTeamImpactAsync_ShouldForwardQueryToRepo()
        {
            // Arrange
            var expectedImpact = new DeleteTeamImpactDto(
                9,
                "Finance",
                false,
                2,
                1,
                3,
                1,
                "Deleting this team is currently blocked.");

            repoMock.Setup(r => r.GetDeleteTeamImpactAsync(9))
                .ReturnsAsync(expectedImpact);

            // Act
            var result = await service.GetDeleteTeamImpactAsync(9);

            // Assert
            result.Should().Be(expectedImpact);
            repoMock.Verify(r => r.GetDeleteTeamImpactAsync(9), Times.Once);
        }
    }
}
