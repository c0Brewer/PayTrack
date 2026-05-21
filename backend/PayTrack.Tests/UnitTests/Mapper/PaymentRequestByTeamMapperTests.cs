using FluentAssertions;
using PayTrack.Api.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class PaymentRequestByTeamMapperTests
    {
        [Fact]
        public void ToDto_ShouldMapScalarFields()
        {
            var entity = new PaymentRequestByTeam
            {
                Id = 1,
                Amount = 250,
                PurposeOfPayment = "Spare parts",
                PaymentReference = "REF-99",
                Status = TransactionStatus.Submitted,
                PaymentDirection = PaymentDirection.In,
                CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                PaidAt = null,
                DueDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            };

            var dto = PaymentRequestByTeamMapper.ToDto(entity);

            dto.Should().NotBeNull();
            dto.Id.Should().Be(entity.Id);
            dto.Amount.Should().Be(entity.Amount);
            dto.PurposeOfPayment.Should().Be(entity.PurposeOfPayment);
            dto.PaymentReference.Should().Be(entity.PaymentReference);
            dto.Status.Should().Be(entity.Status);
            dto.PaymentDirection.Should().Be(entity.PaymentDirection);
            dto.CreatedAt.Should().Be(entity.CreatedAt);
            dto.PaidAt.Should().BeNull();
            dto.DueDate.Should().Be(entity.DueDate);
        }

        [Fact]
        public void ToDto_ShouldMapAllNavigationProperties()
        {
            var entity = new PaymentRequestByTeam
            {
                Id = 2,
                Amount = 100,
                User = new User { Id = 10, Name = "Assignee" },
                RequestedBy = new User { Id = 20, Name = "Creator" },
                Budget = new Budget { Id = 30, Name = "Electrical Parts"},
                Team = new Team { Id = 40, Name = "Engineering" },
                StatusHistory =
                [
                    new() { Id = 1 },
                    new() { Id = 2 },
                ],
            };

            var dto = PaymentRequestByTeamMapper.ToDto(entity);

            dto.User.Should().NotBeNull();
            dto.User!.Id.Should().Be(10);

            dto.CreatedByUser.Should().NotBeNull();
            dto.CreatedByUser!.Id.Should().Be(20);

            dto.Budget.Should().NotBeNull();
            dto.Budget!.Id.Should().Be(30);

            dto.Team.Should().NotBeNull();
            dto.Team!.Id.Should().Be(40);

            dto.StatusHistory.Should().NotBeNull();
            dto.StatusHistory.Should().HaveCount(2);
        }

        [Fact]
        public void ToDto_ShouldHandleUnloadedNavigationProperties()
        {
            // Navigation properties default to null! (User, CostCentre, Team) or [] (StatusHistory)
            // when not populated by EF. The mapper must handle both cases gracefully.
            var entity = new PaymentRequestByTeam
            {
                Id = 3,
                Amount = 50,
            };

            var dto = PaymentRequestByTeamMapper.ToDto(entity);

            dto.User.Should().BeNull();
            dto.CreatedByUser.Should().BeNull();
            dto.Budget.Should().BeNull();
            dto.Team.Should().BeNull();
            dto.StatusHistory.Should().NotBeNull();
            dto.StatusHistory.Should().BeEmpty();
        }

        [Fact]
        public void ListToDto_ShouldReturnMappedList()
        {
            var list = new List<PaymentRequestByTeam>
            {
                new() { Id = 1, Amount = 100 },
                new() { Id = 2, Amount = 200 },
                new() { Id = 3, Amount = 300 },
            };

            var result = PaymentRequestByTeamMapper.ListToDto(list);

            result.Should().HaveCount(3);
            result[0].Id.Should().Be(1);
            result[1].Id.Should().Be(2);
            result[2].Id.Should().Be(3);
        }
    }
}
