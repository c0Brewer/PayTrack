using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayTrack.Data;
using PayTrack.Data.Entities;

namespace PayTrack.Tests.UnitTests.Data
{
    public class AppDbContextTests
    {
        [Fact]
        public void PaymentRequestByUserPayoutTypeConverter_ShouldReadLegacyExternalValue()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("PayoutTypeConverter" + Guid.NewGuid())
                .Options;

            using var context = new AppDbContext(options);
            var converter = context.Model
                .FindEntityType(typeof(PaymentRequestByUser))!
                .FindProperty(nameof(PaymentRequestByUser.PayoutType))!
                .GetValueConverter();

            converter.Should().NotBeNull();
            converter!.ConvertFromProvider("External").Should().Be(PayoutType.NotYetPaid);
            converter.ConvertFromProvider("NotYetPaid").Should().Be(PayoutType.NotYetPaid);
            converter.ConvertToProvider(PayoutType.NotYetPaid).Should().Be("NotYetPaid");
        }
    }
}
