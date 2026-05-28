//AI helped with the test cases

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PayTrack.Tests.UnitTests.Endpoints
{
    public class HealthEndpointsTests
    {
        [Fact]
        public async Task Live_ReturnsOk()
        {
            // Arrange
            using var factory = new HealthApiFactory();
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health/live");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<HealthResponse>();
            result.Should().NotBeNull();
            result.Status.Should().Be("live");
        }

        [Fact]
        public async Task Ready_ReturnsOk_WhenApplicationIsNotDraining()
        {
            // Arrange
            using var factory = new HealthApiFactory();
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health/ready");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<HealthResponse>();
            result.Should().NotBeNull();
            result.Status.Should().Be("ready");
        }

        [Fact]
        public async Task Ready_ReturnsServiceUnavailable_AfterShutdownPreparation()
        {
            // Arrange
            using var factory = new HealthApiFactory();
            var client = factory.CreateClient();

            // Act
            var shutdownResponse = await client.GetAsync("/health/prepareShutdown");
            var readyResponse = await client.GetAsync("/health/ready");

            // Assert
            shutdownResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await shutdownResponse.Content.ReadFromJsonAsync<HealthResponse>();
            result.Should().NotBeNull();
            result.Status.Should().Be("draining");

            readyResponse.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        private sealed record HealthResponse(string Status);
    }

    public class HealthApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
        }
    }
}
