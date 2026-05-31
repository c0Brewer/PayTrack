using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using PayTrack.Application.Dto.Transaction;

namespace PayTrack.Tests.UnitTests.Dto
{
    public class CreateTransactionDtoTests
    {
        [Fact]
        public void ValidateObject_ShouldFail_WhenRequiredReferencePropertyIsMissing()
        {
            var dto = new CreateTransactionDto
            {
                TeamId = 1,
                Amount = 10m,
                PaidAt = new DateTime(2026, 5, 30),
            };
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                new ValidationContext(dto),
                results,
                true);

            isValid.Should().BeFalse();
            results.Should().ContainSingle(result =>
                result.MemberNames.Contains(nameof(CreateTransactionDto.PurposeOfPayment)));
        }

        [Fact]
        public void ValidateObject_ShouldPass_WhenRequiredPropertiesAreSet()
        {
            var dto = new CreateTransactionDto
            {
                TeamId = 1,
                Amount = 10m,
                PurposeOfPayment = "Invoice",
                PaidAt = new DateTime(2026, 5, 30),
            };
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                new ValidationContext(dto),
                results,
                true);

            isValid.Should().BeTrue();
            results.Should().BeEmpty();
        }
    }
}
