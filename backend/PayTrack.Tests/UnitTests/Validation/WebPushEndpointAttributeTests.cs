//AI helped with the test cases

using FluentAssertions;
using PayTrack.Application.Validation;

namespace PayTrack.Tests.UnitTests.Validation
{
    public class WebPushEndpointAttributeTests
    {
        [Theory]
        [InlineData("https://fcm.googleapis.com/fcm/send/abc")]
        [InlineData("https://updates.push.services.mozilla.com/wpush/v2/abc")]
        [InlineData("https://web.push.apple.com/abc")]
        [InlineData("https://wns2-by3p.notify.windows.com/w/?token=abc")]
        public void IsValid_ShouldReturnTrue_WhenEndpointUsesSupportedPushService(string endpoint)
        {
            var attribute = new WebPushEndpointAttribute();

            var result = attribute.IsValid(endpoint);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("not-a-url")]
        [InlineData("http://fcm.googleapis.com/fcm/send/abc")]
        [InlineData("https://127.0.0.1/push")]
        [InlineData("https://localhost/push")]
        [InlineData("https://example.com/push")]
        [InlineData("https://fcm.googleapis.com:444/fcm/send/abc")]
        [InlineData("https://user@fcm.googleapis.com/fcm/send/abc")]
        public void IsValid_ShouldReturnFalse_WhenEndpointIsNotSupported(string endpoint)
        {
            var attribute = new WebPushEndpointAttribute();

            var result = attribute.IsValid(endpoint);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_WhenValueIsNull()
        {
            var attribute = new WebPushEndpointAttribute();

            var result = attribute.IsValid(null);

            result.Should().BeTrue();
        }

        [Fact]
        public void IsValid_ShouldThrow_WhenValueIsNotAString()
        {
            var attribute = new WebPushEndpointAttribute();

            var action = () => attribute.IsValid(123);

            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("WebPushEndpointAttribute can only validate string values, but received Int32.");
        }

        [Fact]
        public void FormatErrorMessage_ShouldDescribeSupportedEndpointRequirement()
        {
            var attribute = new WebPushEndpointAttribute();

            var result = attribute.FormatErrorMessage("Endpoint");

            result.Should().Be("Endpoint must be an HTTPS browser push endpoint from a supported push service.");
        }
    }
}
