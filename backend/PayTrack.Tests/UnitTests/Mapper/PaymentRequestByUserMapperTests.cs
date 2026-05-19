using FluentAssertions;
using PayTrack.Api.Mapper;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Mapper
{
    public class PaymentRequestByUserMapperTests
    {
        [Fact]
        public void MapperToDto_ReturnsCorrectResult()
        {
            // Arrange
            var entity = new PaymentRequestByUser
            {
                Id = 1,
                Amount = 100,
                PurposeOfPayment = "Test",
                PaymentReference = "Ref123",
                Status = TransactionStatus.Submitted,
                PaymentDirection = PaymentDirection.Out,
                CreatedAt = DateTime.UtcNow,
                PaidAt = null,
                InvoiceNumber = "INV-1",
                Comment = "Comment",
                PayoutType = PayoutType.User,
                HasPotentialDuplicate = true
            };

            // Act
            var dto = PaymentRequestByUserMapper.ToDto(entity);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(entity.Id);
            dto.Amount.Should().Be(entity.Amount);
            dto.PurposeOfPayment.Should().Be(entity.PurposeOfPayment);
            dto.PaymentReference.Should().Be(entity.PaymentReference);
            dto.Status.Should().Be(entity.Status);
            dto.PaymentDirection.Should().Be(entity.PaymentDirection);
            dto.InvoiceNumber.Should().Be(entity.InvoiceNumber);
            dto.Comment.Should().Be(entity.Comment);
            dto.PayoutType.Should().Be(entity.PayoutType);
            dto.HasPotentialDuplicate.Should().BeTrue();
        }

        [Fact]
        public void MapperToDto_ShouldMapNestedObjects()
        {
            // Arrange
            var entity = new PaymentRequestByUser
            {
                Id = 1,
                Amount = 100,
                InvoiceNumber = "123",

                User = new User { Id = 10, Name = "User1" },
                CostCentre = new CostCentre { Id = 20, Name = "CC" },
                Team = new Team { Id = 30, Name = "Team1" },
                BankAccount = new BankAccount { Id = 40, Iban = "IBAN123" },

                StatusHistory =
                [
                    new() { Id = 1 },
                    new() { Id = 2 }
                ]
            };

            // Act
            var dto = PaymentRequestByUserMapper.ToDto(entity);

            // Assert
            dto.Should().NotBeNull();

            dto.User.Should().NotBeNull();
            dto.User.Id.Should().Be(10);

            dto.CostCentre.Should().NotBeNull();
            dto.CostCentre.Id.Should().Be(20);

            dto.Team.Should().NotBeNull();
            dto.Team.Id.Should().Be(30);

            dto.BankAccount.Should().NotBeNull();
            dto.BankAccount.Id.Should().Be(40);

            dto.StatusHistory.Should().NotBeNull();
            dto.StatusHistory.Should().HaveCount(2);
        }

        [Fact]
        public void MapperToDto_ShouldHandleNullNestedObjects()
        {
            // Arrange
            var entity = new PaymentRequestByUser
            {
                Id = 1,
                Amount = 50,
                InvoiceNumber = "123",
                User = new User(),
                CostCentre = new CostCentre(),
                Team = new Team(),
                BankAccount = null,
                StatusHistory = []
            };

            // Act
            var dto = PaymentRequestByUserMapper.ToDto(entity);

            // Assert
            dto.Should().NotBeNull();
            dto.User.Should().NotBeNull();
            dto.CostCentre.Should().NotBeNull();
            dto.Team.Should().NotBeNull();
            dto.BankAccount.Should().BeNull();

            // NOTE: your mapper initializes [] instead of null
            dto.StatusHistory.Should().NotBeNull();
            dto.StatusHistory.Should().BeEmpty();
        }

        [Fact]
        public void MapperListToDto_ReturnsCorrectResult()
        {
            // Arrange
            var list = new List<PaymentRequestByUser>
            {
                new() { Id = 1, Amount = 10, InvoiceNumber = "123" },
                new() { Id = 2, Amount = 20, InvoiceNumber = "456" },
                new() { Id = 3, Amount = 30, InvoiceNumber = "789" }
            };

            // Act
            var result = PaymentRequestByUserMapper.ListToDto(list);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                result[i].Id.Should().Be(list[i].Id);
                result[i].Amount.Should().Be(list[i].Amount);
            }
        }
    }
}
