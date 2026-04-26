// <copyright file="LockedException.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Exceptions
{
    /// <summary>
    /// Exception to be thrown when a user is locked.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LockedException : ApiException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockedException"/> class.
        /// Constructor to be called.
        /// </summary>
        /// <param name="message">message.</param>
        /// <returns>Instance of Exception.</returns>
        public LockedException(string? message)
            : base(message, StatusCodes.Status423Locked)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockedException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="statusCode">status code.</param>
        /// <returns>Instance of Exception.</returns>
        protected LockedException(string? message, int statusCode)
            : base(message, statusCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockedException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <returns>Instance of Exception.</returns>
        protected LockedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockedException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">inner exception.</param>
        /// <returns>Instance of Exception.</returns>
        protected LockedException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
