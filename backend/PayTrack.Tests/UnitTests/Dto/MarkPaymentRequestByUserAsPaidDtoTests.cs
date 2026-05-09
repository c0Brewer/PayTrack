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
    }
}
