// <copyright file="EndpointExceptionHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Application.Exceptions;

namespace PayTrack.Api.Middleware
{
    /// <summary>
    /// Class which handles exceptions that arise during our Operations.
    /// </summary>
    public class EndpointExceptionHandler(ILogger<EndpointExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<EndpointExceptionHandler> logger = logger;

        /// <summary>
        /// Handles incoming exceptions and returns a unified result.
        /// </summary>
        /// <param name="httpContext">httpcontext.</param>
        /// <param name="exception">exception that arises.</param>
        /// <param name="cancellationToken">cancellation token.</param>
        /// <returns>true if other exception handlers should get called as well.</returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            this.logger.LogError($"Exception occured: {exception}");

            var statusCode = exception is ApiException apiException
                ? apiException.StatusCode
                : StatusCodes.Status500InternalServerError;

            httpContext.Response.StatusCode = statusCode;

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = exception.GetType().Name,
                Detail = exception.Message,
            };

            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

            return true;
        }
    }
}
