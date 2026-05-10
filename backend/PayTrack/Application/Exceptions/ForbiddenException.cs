// <copyright file="ForbiddenException.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Exceptions
{
    /// <summary>
    /// Exception to be thrown when the current user is not allowed to access a resource.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ForbiddenException : ApiException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public ForbiddenException(string? message)
            : base(message, StatusCodes.Status403Forbidden)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
        /// </summary>
        protected ForbiddenException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">inner exception.</param>
        protected ForbiddenException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
