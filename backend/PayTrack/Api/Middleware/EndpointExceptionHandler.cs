// <copyright file="EndpointExceptionHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

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
            var problem = exception is ApiException apiException
                ? new ProblemDetails
                {
                    Status = apiException.StatusCode,
                    Title = exception.GetType().Name,
                    Detail = exception.Message,
                }
                : new ProblemDetails
                {
                    Status = 500,
                    Title = "Internal Server Error",
                    Detail = "An error occured. Please try again or contact support.",
                };

            if (exception is ApiException handledApiException && handledApiException.StatusCode < 500)
            {
                this.logger.LogWarning("Handled API exception: {ExceptionType} - {Message}", exception.GetType().Name, exception.Message);
            }
            else
            {
                this.logger.LogError(exception, "Exception occurred");
            }

            httpContext.Response.StatusCode = problem.Status ?? 500;

            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

            return true;
        }
    }
}
