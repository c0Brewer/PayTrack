// <copyright file="WebPushEndpointAttribute.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

//AI helped with the functions
using System.ComponentModel.DataAnnotations;

namespace PayTrack.Application.Validation
{
    /// <summary>
    /// Validates that a URL is a browser Web Push endpoint from a supported push service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class WebPushEndpointAttribute : ValidationAttribute
    {
        private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
        {
            "fcm.googleapis.com",
            "updates.push.services.mozilla.com",
            "web.push.apple.com",
        };

        private static readonly string[] AllowedHostSuffixes =
        [
            ".notify.windows.com",
        ];

        /// <summary>
        /// Checks whether a value is an allowed Web Push endpoint.
        /// </summary>
        /// <param name="value">Endpoint value.</param>
        /// <returns>True when the value is an allowed endpoint.</returns>
        public static bool IsAllowedEndpoint(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                return false;
            }

            return uri.Scheme == Uri.UriSchemeHttps
                && (uri.IsDefaultPort || uri.Port == 443)
                && string.IsNullOrEmpty(uri.UserInfo)
                && (AllowedHosts.Contains(uri.IdnHost)
                    || AllowedHostSuffixes.Any(suffix => uri.IdnHost.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)));
        }

        /// <inheritdoc/>
        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return true;
            }

            if (value is not string endpoint)
            {
                throw new InvalidOperationException(
                    $"{nameof(WebPushEndpointAttribute)} can only validate string values, but received {value.GetType().Name}.");
            }

            return IsAllowedEndpoint(endpoint);
        }

        /// <inheritdoc/>
        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be an HTTPS browser push endpoint from a supported push service.";
        }
    }
}
