// <copyright file="NotFoundException.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Exceptions
{
    /// <summary>
    /// Exception to be thrown when an Entity cannot be found.
    /// </summary>
    public class NotFoundException : ApiException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// Constructor to be called.
        /// </summary>
        /// <param name="message">message.</param>
        /// <returns>Instance of Exception.</returns>
        public NotFoundException(string? message)
            : base(message, StatusCodes.Status404NotFound)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="statusCode">status code.</param>
        /// <returns>Instance of Exception.</returns>
        protected NotFoundException(string? message, int statusCode)
            : base(message, statusCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <returns>Instance of Exception.</returns>
        protected NotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">inner exception.</param>
        /// <returns>Instance of Exception.</returns>
        protected NotFoundException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
