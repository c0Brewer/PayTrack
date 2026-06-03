using FluentAssertions;
using PayTrack.Api.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class TransactionStatusHistoryMapperTests
    {
        [Theory]
        [InlineData(1, "comment1", TransactionStatus.Submitted, TransactionStatus.Approved)]
        [InlineData(42, "another comment", TransactionStatus.Submitted, TransactionStatus.Approved)]
        [InlineData(999, "", TransactionStatus.Submitted, TransactionStatus.Approved)]
        public void MapperToDto_ReturnsCorrectResult(
            int changedById,
            string comment,
            TransactionStatus fromStatus,
            TransactionStatus toStatus)
        {
            // Arrange
            var changedAt = DateTime.UtcNow;

            var entity = new TransactionStatusHistory
            {
                ChangedById = changedById,
                ChangedBy = new User
                {
                    Id = changedById,
                    Name = "Finance User",
                    Email = "finance@example.com",
                    ProfilePictureUrl = "https://example.com/avatar.png",
                    Role = Role.Admin,
                    IsActive = true
                },
                Comment = comment,
                FromStatus = TransactionStatus.Submitted,
                ToStatus = TransactionStatus.Approved,
                ChangedAt = changedAt
            };

            // Act
            var dto = TransactionStatusHistoryMapper.ToDto(entity);

            // Assert
            dto.Should().NotBeNull();
            dto.ChangedById.Should().Be(changedById);
            dto.ChangedBy.Should().NotBeNull();
            dto.ChangedBy!.Name.Should().Be("Finance User");
            dto.Comment.Should().Be(comment);
            dto.FromStatus.Should().Be(fromStatus);
            dto.ToStatus.Should().Be(toStatus);
            dto.ChangedAt.Should().Be(changedAt);
        }

        [Fact]
        public void MapperListToDto_ReturnsCorrectResult()
        {
            // Arrange
            var list = new List<TransactionStatusHistory>
            {
                new()
                {
                    ChangedById = 1,
                    ChangedBy = new User
                    {
                        Id = 1,
                        Name = "User One",
                        Email = "user1@example.com",
                        ProfilePictureUrl = "https://example.com/1.png",
                        Role = Role.Admin,
                        IsActive = true
                    },
                    Comment = "A",
                    FromStatus = TransactionStatus.Submitted,
                    ToStatus = TransactionStatus.Approved,
                    ChangedAt = DateTime.UtcNow
                },
                new()
                {
                    ChangedById = 2,
                    ChangedBy = new User
                    {
                        Id = 2,
                        Name = "User Two",
                        Email = "user2@example.com",
                        ProfilePictureUrl = "https://example.com/2.png",
                        Role = Role.Admin,
                        IsActive = true
                    },
                    Comment = "B",
                    FromStatus = TransactionStatus.Submitted,
                    ToStatus = TransactionStatus.Approved,
                    ChangedAt = DateTime.UtcNow
                },
                new()
                {
                    ChangedById = 3,
                    ChangedBy = new User
                    {
                        Id = 3,
                        Name = "User Three",
                        Email = "user3@example.com",
                        ProfilePictureUrl = "https://example.com/3.png",
                        Role = Role.Admin,
                        IsActive = true
                    },
                    Comment = "C",
                    FromStatus = TransactionStatus.Submitted,
                    ToStatus = TransactionStatus.Approved,
                    ChangedAt = DateTime.UtcNow
                }
            };

            // Act
            var result = TransactionStatusHistoryMapper.ListToDto(list);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                result[i].ChangedById.Should().Be(list[i].ChangedById);
                result[i].ChangedBy?.Name.Should().Be(list[i].ChangedBy.Name);
                result[i].Comment.Should().Be(list[i].Comment);
                result[i].FromStatus.Should().Be(list[i].FromStatus);
                result[i].ToStatus.Should().Be(list[i].ToStatus);
                result[i].ChangedAt.Should().Be(list[i].ChangedAt);
            }
        }

        [Fact]
        public void MapperListToDto_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var list = new List<TransactionStatusHistory>();

            // Act
            var result = TransactionStatusHistoryMapper.ListToDto(list);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void MapperToDto_ReturnsNullChangedBy_WhenNavigationIsMissing()
        {
            // Arrange
            var entity = new TransactionStatusHistory
            {
                ChangedById = 5,
                ChangedBy = null!,
                Comment = "legacy history",
                FromStatus = TransactionStatus.Submitted,
                ToStatus = TransactionStatus.Declined,
                ChangedAt = DateTime.UtcNow
            };

            // Act
            var dto = TransactionStatusHistoryMapper.ToDto(entity);

            // Assert
            dto.ChangedById.Should().Be(5);
            dto.ChangedBy.Should().BeNull();
            dto.Comment.Should().Be("legacy history");
            dto.FromStatus.Should().Be(TransactionStatus.Submitted);
            dto.ToStatus.Should().Be(TransactionStatus.Declined);
        }
    }
}
