using FluentAssertions;
using PayTrack.Application.Validation;

namespace PayTrack.Tests.UnitTests.Validation
{
    public class OptionalMinLengthAttributeTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void IsValid_ShouldReturnTrue_WhenValueIsNotProvided(string? value)
        {
            var attribute = new OptionalMinLengthAttribute(3);

            var result = attribute.IsValid(value);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("  abc  ")]
        [InlineData("abcd")]
        public void IsValid_ShouldReturnTrue_WhenStringMeetsMinimumLength(string value)
        {
            var attribute = new OptionalMinLengthAttribute(3);

            var result = attribute.IsValid(value);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("ab")]
        [InlineData("  ab  ")]
        public void IsValid_ShouldReturnFalse_WhenStringIsTooShort(string value)
        {
            var attribute = new OptionalMinLengthAttribute(3);

            var result = attribute.IsValid(value);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsValid_ShouldThrow_WhenValueIsNotAString()
        {
            var attribute = new OptionalMinLengthAttribute(3);

            var action = () => attribute.IsValid(123);

            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("OptionalMinLengthAttribute can only validate string values, but received Int32.");
        }

        [Fact]
        public void FormatErrorMessage_ShouldIncludeMinimumLength()
        {
            var attribute = new OptionalMinLengthAttribute(3);

            var result = attribute.FormatErrorMessage("Comment");

            result.Should().Be("Comment must be at least 3 characters long when provided.");
        }
    }
}
