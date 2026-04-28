// <copyright file="InvalidFileException.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Exceptions
{
    /// <summary>
    /// Exception to be thrown when an invalid file is encountered.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InvalidFileException : ApiException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidFileException"/> class.
        /// Constructor to be called.
        /// </summary>
        /// <param name="message">message.</param>
        /// <returns>Instance of Exception.</returns>
        public InvalidFileException(string? message)
            : base(message, StatusCodes.Status422UnprocessableEntity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidFileException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="statusCode">status code.</param>
        /// <returns>Instance of Exception.</returns>
        protected InvalidFileException(string? message, int statusCode)
            : base(message, statusCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidFileException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <returns>Instance of Exception.</returns>
        protected InvalidFileException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidFileException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">inner exception.</param>
        /// <returns>Instance of Exception.</returns>
        protected InvalidFileException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
