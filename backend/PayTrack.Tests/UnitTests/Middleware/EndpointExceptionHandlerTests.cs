using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PayTrack.Api.Middleware;
using PayTrack.Application.Exceptions;

namespace PayTrack.Tests.UnitTests.Middleware
{
    public class EndpointExceptionHandlerTests
    {
        [Fact]
        public async Task TryHandleAsync_ShouldReturnProblemDetails_With500_ForGenericException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EndpointExceptionHandler>>();
            var handler = new EndpointExceptionHandler(loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream(); // capture the response
            var exception = new Exception("Something went wrong");

            // Act
            var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            handled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var problem = JsonSerializer.Deserialize<ProblemDetails>(json);

            problem.Should().NotBeNull();
            problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
            problem.Title.Should().Be("Internal Server Error");
            problem.Detail.Should().Be("An error occured. Please try again or contact support.");
        }

        [Fact]
        public async Task TryHandleAsync_ShouldReturnProblemDetails_WithNotFoundStatusCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EndpointExceptionHandler>>();
            var handler = new EndpointExceptionHandler(loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream(); // capture the response
            var exception = new NotFoundException("Not allowed");

            // Act
            var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            handled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var problem = JsonSerializer.Deserialize<ProblemDetails>(json);

            problem.Should().NotBeNull();
            problem.Status.Should().Be(StatusCodes.Status404NotFound);
            problem.Title.Should().Be(nameof(NotFoundException));
            problem.Detail.Should().Be("Not allowed");
        }
    }
}
