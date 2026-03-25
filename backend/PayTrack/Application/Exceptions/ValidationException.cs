// <copyright file="ValidationException.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Exceptions
{
    /// <summary>
    /// Exception which gets thrown when invalid data is passed/encountered.
    /// </summary>
    public class ValidationException : ApiException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// Constructor. Automatically sets the right status code.
        /// </summary>
        /// <param name="message">message.</param>
        /// <returns>Instance of Class.</returns>
        public ValidationException(string? message)
            : base(message, StatusCodes.Status400BadRequest)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="statusCode">status code.</param>
        /// <returns>Instance of Class.</returns>
        protected ValidationException(string? message, int statusCode)
            : base(message, statusCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <returns>Instance of Class.</returns>
        protected ValidationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">inner exception.</param>
        /// <returns>Instance of Class.</returns>
        protected ValidationException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
