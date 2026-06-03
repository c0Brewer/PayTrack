using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using PayTrack.Application.Services.Implementation;
using PayTrack.Application.Services.Model;
using PayTrack.Application.Settings;

namespace PayTrack.Tests.UnitTests.Services
{
    public class NotificationDispatchServiceTests
    {
        private static readonly string LookupOkJson =
            JsonSerializer.Serialize(new { ok = true, user = new { id = "U123456" } });

        private static readonly string PostOkJson =
            JsonSerializer.Serialize(new { ok = true });

        private static readonly string LookupFailJson =
            JsonSerializer.Serialize(new { ok = false, error = "users_not_found" });

        private static readonly string PostFailJson =
            JsonSerializer.Serialize(new { ok = false, error = "channel_not_found" });

        private static IOptions<SlackSettings> SlackOptions(string token = "xoxb-test")
            => Options.Create(new SlackSettings { BotToken = token });

        private static NotificationDispatchService BuildService(
            Mock<IEmailSender> emailSender,
            SequentialHttpHandler handler)
            => new(emailSender.Object, SlackOptions(), new HttpClient(handler));

        private static HttpResponseMessage JsonResponse(string json)
            => new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

        // ── SendEmailAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task SendEmailAsync_DelegatesToEmailSenderWithCorrectArguments()
        {
            var emailMock = new Mock<IEmailSender>();
            var service = BuildService(emailMock, new SequentialHttpHandler());

            await service.SendEmailAsync("user@example.com", "My Subject", "My Body");

            emailMock.Verify(s => s.SendAsync("user@example.com", "My Subject", "My Body"), Times.Once);
        }

        [Fact]
        public async Task SendEmailAsync_WithAttachments_DelegatesToEmailSenderWithAttachments()
        {
            var emailMock = new Mock<IEmailSender>();
            var service = BuildService(emailMock, new SequentialHttpHandler());
            var attachments = new[]
            {
                new EmailAttachment("invoice.pdf", [1, 2, 3], "application/pdf"),
            };

            await service.SendEmailAsync("user@example.com", "My Subject", "My Body", attachments);

            emailMock.Verify(
                s => s.SendAsync("user@example.com", "My Subject", "My Body", attachments),
                Times.Once);
        }

        // ── SendSlackAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task SendSlackAsync_FirstRequestGoesToLookupByEmailWithEncodedEmail()
        {
            var handler = new SequentialHttpHandler(
                JsonResponse(LookupOkJson),
                JsonResponse(PostOkJson));

            await BuildService(new Mock<IEmailSender>(), handler).SendSlackAsync("user@example.com", "Hello");

            var lookup = handler.Requests[0];
            lookup.RequestUri!.AbsoluteUri.Should().Contain("users.lookupByEmail");
            lookup.RequestUri.Query.Should().Contain(Uri.EscapeDataString("user@example.com"));
        }

        [Fact]
        public async Task SendSlackAsync_LookupRequestCarriesBotToken()
        {
            var handler = new SequentialHttpHandler(
                JsonResponse(LookupOkJson),
                JsonResponse(PostOkJson));

            await BuildService(new Mock<IEmailSender>(), handler).SendSlackAsync("user@example.com", "Hello");

            handler.Requests[0].Headers.Authorization?.ToString().Should().Be("Bearer xoxb-test");
        }

        [Fact]
        public async Task SendSlackAsync_SecondRequestPostsMessageWithResolvedUserIdAndText()
        {
            var handler = new SequentialHttpHandler(
                JsonResponse(LookupOkJson),
                JsonResponse(PostOkJson));

            await BuildService(new Mock<IEmailSender>(), handler).SendSlackAsync("user@example.com", "Hello Slack");

            var post = handler.Requests[1];
            post.RequestUri!.AbsoluteUri.Should().Contain("chat.postMessage");
            var body = handler.RequestBodies[1];
            body.Should().Contain("U123456");
            body.Should().Contain("Hello Slack");
        }

        [Fact]
        public async Task SendSlackAsync_ThrowsInvalidOperationException_WhenLookupReturnsFalse()
        {
            var handler = new SequentialHttpHandler(JsonResponse(LookupFailJson));

            var act = async () => await BuildService(new Mock<IEmailSender>(), handler)
                .SendSlackAsync("user@example.com", "Hello");

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*users_not_found*");
        }

        [Fact]
        public async Task SendSlackAsync_ThrowsInvalidOperationException_WhenPostMessageReturnsFalse()
        {
            var handler = new SequentialHttpHandler(
                JsonResponse(LookupOkJson),
                JsonResponse(PostFailJson));

            var act = async () => await BuildService(new Mock<IEmailSender>(), handler)
                .SendSlackAsync("user@example.com", "Hello");

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*channel_not_found*");
        }
    }

    internal sealed class SequentialHttpHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> queue = new(responses);

        public List<HttpRequestMessage> Requests { get; } = [];

        /// <summary>Body strings captured before the content is disposed, indexed by call order.</summary>
        public List<string?> RequestBodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.Requests.Add(request);
            this.RequestBodies.Add(request.Content is not null
                ? await request.Content.ReadAsStringAsync(cancellationToken)
                : null);
            return this.queue.Dequeue();
        }
    }
}
