using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using PayTrack.Application.Dto.PaymentRequestByUser;

namespace PayTrack.Tests.UnitTests.Dto
{
    public class MarkPaymentRequestByUserAsPaidDtoTests
    {
        [Fact]
        public void MarkPaymentRequestByUserAsPaidDto_ShouldExposeConstructorValues()
        {
            var paymentDate = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc);

            var dto = new MarkPaymentRequestByUserAsPaidDto(
                "REF-123",
                "Supplier payout",
                paymentDate);

            dto.PaymentReference.Should().Be("REF-123");
            dto.PurposeOfPayment.Should().Be("Supplier payout");
            dto.PaymentDate.Should().Be(paymentDate);
        }

        [Fact]
        public void MarkPaymentRequestByUserAsPaidDto_ShouldRequirePaymentDate()
        {
            var dto = new MarkPaymentRequestByUserAsPaidDto(
                "REF-123",
                "Supplier payout",
                null);
            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                new ValidationContext(dto),
                validationResults,
                validateAllProperties: true);

            isValid.Should().BeFalse();
            validationResults.Should().Contain(result =>
                result.MemberNames.Contains(nameof(MarkPaymentRequestByUserAsPaidDto.PaymentDate)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("AB")]
        public void MarkPaymentRequestByUserAsPaidDto_ShouldValidatePaymentReference(string? paymentReference)
        {
            var dto = new MarkPaymentRequestByUserAsPaidDto(
                paymentReference!,
                "Supplier payout",
                new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc));

            var validationResults = Validate(dto);

            validationResults.Should().Contain(result =>
                result.MemberNames.Contains(nameof(MarkPaymentRequestByUserAsPaidDto.PaymentReference)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("AB")]
        public void MarkPaymentRequestByUserAsPaidDto_ShouldValidatePurposeOfPayment(string? purposeOfPayment)
        {
            var dto = new MarkPaymentRequestByUserAsPaidDto(
                "REF-123",
                purposeOfPayment!,
                new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc));

            var validationResults = Validate(dto);

            validationResults.Should().Contain(result =>
                result.MemberNames.Contains(nameof(MarkPaymentRequestByUserAsPaidDto.PurposeOfPayment)));
        }

        [Fact]
        public void MarkPaymentRequestByUserAsPaidDto_ShouldRejectDefaultPaymentDate()
        {
            var dto = new MarkPaymentRequestByUserAsPaidDto(
                "REF-123",
                "Supplier payout",
                default(DateTime));

            var validationResults = Validate(dto);

            validationResults.Should().Contain(result =>
                result.MemberNames.Contains(nameof(MarkPaymentRequestByUserAsPaidDto.PaymentDate)));
        }

        private static List<ValidationResult> Validate(MarkPaymentRequestByUserAsPaidDto dto)
        {
            var validationResults = new List<ValidationResult>();

            Validator.TryValidateObject(
                dto,
                new ValidationContext(dto),
                validationResults,
                validateAllProperties: true);

            return validationResults;
        }
    }
}
