// <copyright file="ApiException.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Exceptions
{
    /// <summary>
    /// Base Custom Exception Class. All custom exceptions extend this..
    /// </summary>
    public abstract class ApiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// Base Constructor. This is getting called from our custom exceptions.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="statusCode">status code.</param>
        /// <returns>Instance of class.</returns>
        protected ApiException(string? message, int statusCode)
            : base(message)
        {
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <returns>Instance of class.</returns>
        protected ApiException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <param name="message">message.</param>
        /// <returns>Instance of class.</returns>
        protected ApiException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// Unused Constructor.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">inner exception.</param>
        /// <returns>Instance of class.</returns>
        protected ApiException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets status Code which will be returned when this Exception is thrown.
        /// </summary>
        public int StatusCode { get; }
    }
}
