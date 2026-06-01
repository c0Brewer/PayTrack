// <copyright file="OptionalMinLengthAttribute.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Validation
{
    /// <summary>
    /// Validates a string minimum length only when a non-blank value is provided.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class OptionalMinLengthAttribute : ValidationAttribute
    {
        private readonly int length;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalMinLengthAttribute"/> class.
        /// </summary>
        /// <param name="length">The minimum length required for a non-blank value.</param>
        public OptionalMinLengthAttribute(int length)
        {
            this.length = length;
        }

        /// <inheritdoc/>
        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return true;
            }

            if (value is not string text)
            {
                return false;
            }

            var trimmedText = text.Trim();
            return trimmedText.Length == 0 || trimmedText.Length >= this.length;
        }

        /// <inheritdoc/>
        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be at least {this.length} characters long when provided.";
        }
    }
}
