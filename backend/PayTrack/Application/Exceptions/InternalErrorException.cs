// <copyright file="InternalErrorException.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Exceptions
{
    /// <summary>
    /// Exception to be thrown when an Entity cannot be found.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InternalErrorException : ApiException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalErrorException"/> class.
        /// Constructor to be called.
        /// </summary>
        /// <param name="message">message.</param>
        /// <returns>Instance of Exception.</returns>
        public InternalErrorException(string? message)
            : base(message, StatusCodes.Status500InternalServerError)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalErrorException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="statusCode">status code.</param>
        /// <returns>Instance of Exception.</returns>
        protected InternalErrorException(string? message, int statusCode)
            : base(message, statusCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalErrorException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <returns>Instance of Exception.</returns>
        protected InternalErrorException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalErrorException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">inner exception.</param>
        /// <returns>Instance of Exception.</returns>
        protected InternalErrorException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
