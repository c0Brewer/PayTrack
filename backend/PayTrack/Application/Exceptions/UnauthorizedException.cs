// <copyright file="UnauthorizedException.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Exceptions
{
    /// <summary>
    /// Exception to be thrown when an Entity cannot be found.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UnauthorizedException : ApiException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
        /// Constructor to be called.
        /// </summary>
        /// <param name="message">message.</param>
        /// <returns>Instance of Exception.</returns>
        public UnauthorizedException(string? message)
            : base(message, StatusCodes.Status401Unauthorized)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="statusCode">status code.</param>
        /// <returns>Instance of Exception.</returns>
        protected UnauthorizedException(string? message, int statusCode)
            : base(message, statusCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <returns>Instance of Exception.</returns>
        protected UnauthorizedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">inner exception.</param>
        /// <returns>Instance of Exception.</returns>
        protected UnauthorizedException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
