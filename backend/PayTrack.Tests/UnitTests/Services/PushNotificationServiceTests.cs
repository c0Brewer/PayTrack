using System.Net;
using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PayTrack.Application.Dto.Notification;
using PayTrack.Application.Services.Implementation;
using PayTrack.Application.Settings;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Tests.UnitTests.Services
{
    public class PushNotificationServiceTests
    {
        [Fact]
        public async Task GetConfigAsync_ReturnsDisabledServerConfig_WhenVapidSettingsAreMissing()
        {
            var repo = new Mock<IPushSubscriptionRepository>();
            repo.Setup(r => r.HasEnabledSubscriptionAsync(42)).ReturnsAsync(true);
            var service = BuildService(repo, new PushNotificationSettings(), new SequentialHttpHandler());

            var config = await service.GetConfigAsync(42);

            config.IsConfigured.Should().BeFalse();
            config.VapidPublicKey.Should().BeNull();
            config.Enabled.Should().BeTrue();
        }

        [Fact]
        public async Task SaveSubscriptionAsync_UpsertsEnabledSubscriptionForUser()
        {
            var repo = new Mock<IPushSubscriptionRepository>();
            var service = BuildService(repo, CreateVapidSettings(), new SequentialHttpHandler());

            await service.SaveSubscriptionAsync(
                42,
                new SavePushSubscriptionDto
                {
                    Endpoint = "https://push.example.test/send/123",
                    P256dh = "p256dh",
                    Auth = "auth",
                });

            repo.Verify(
                r => r.UpsertAsync(It.Is<PushSubscription>(s =>
                    s.UserId == 42 &&
                    s.Endpoint == "https://push.example.test/send/123" &&
                    s.P256dh == "p256dh" &&
                    s.Auth == "auth" &&
                    s.IsEnabled)),
                Times.Once);
        }

        [Fact]
        public async Task DisableSubscriptionAsync_DisablesSubscriptionForUser()
        {
            var repo = new Mock<IPushSubscriptionRepository>();
            var service = BuildService(repo, CreateVapidSettings(), new SequentialHttpHandler());

            await service.DisableSubscriptionAsync(42, "https://push.example.test/send/123");

            repo.Verify(r => r.DisableAsync(42, "https://push.example.test/send/123"), Times.Once);
        }

        [Fact]
        public async Task SendWorkflowStatusChangedAsync_PostsEncryptedPushRequest()
        {
            var subscription = CreateSubscription();
            var repo = new Mock<IPushSubscriptionRepository>();
            repo.Setup(r => r.GetEnabledForUserAsync(42)).ReturnsAsync([subscription]);
            var handler = new SequentialHttpHandler(new HttpResponseMessage(HttpStatusCode.Created));
            var service = BuildService(repo, CreateVapidSettings(), handler);

            await service.SendWorkflowStatusChangedAsync(42, "Invoice approved", "Your invoice was approved.", "/my-invoices/7");

            handler.Requests.Should().ContainSingle();
            var request = handler.Requests[0];
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.AbsoluteUri.Should().Be(subscription.Endpoint);
            request.Headers.GetValues("TTL").Should().Contain("86400");
            request.Content!.Headers.ContentEncoding.Should().Contain("aes128gcm");
            request.Headers.GetValues("Authorization").Single().Should().StartWith("vapid t=");
            handler.RequestBodies[0].Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SendWorkflowStatusChangedAsync_DisablesGoneSubscriptions()
        {
            var subscription = CreateSubscription();
            var repo = new Mock<IPushSubscriptionRepository>();
            repo.Setup(r => r.GetEnabledForUserAsync(42)).ReturnsAsync([subscription]);
            var handler = new SequentialHttpHandler(new HttpResponseMessage(HttpStatusCode.Gone));
            var service = BuildService(repo, CreateVapidSettings(), handler);

            await service.SendWorkflowStatusChangedAsync(42, "Invoice approved", "Your invoice was approved.", "/my-invoices/7");

            repo.Verify(r => r.DisableByEndpointAsync(subscription.Endpoint), Times.Once);
        }

        [Fact]
        public async Task SendWorkflowStatusChangedAsync_SkipsDelivery_WhenServerIsNotConfigured()
        {
            var repo = new Mock<IPushSubscriptionRepository>();
            var handler = new SequentialHttpHandler();
            var service = BuildService(repo, new PushNotificationSettings(), handler);

            await service.SendWorkflowStatusChangedAsync(42, "Invoice approved", "Your invoice was approved.", "/my-invoices/7");

            repo.Verify(r => r.GetEnabledForUserAsync(It.IsAny<int>()), Times.Never);
            handler.Requests.Should().BeEmpty();
        }

        private static PushNotificationService BuildService(
            Mock<IPushSubscriptionRepository> repo,
            PushNotificationSettings settings,
            SequentialHttpHandler handler)
        {
            return new PushNotificationService(
                repo.Object,
                Options.Create(settings),
                new HttpClient(handler),
                NullLogger<PushNotificationService>.Instance);
        }

        private static PushNotificationSettings CreateVapidSettings()
        {
            using var key = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
            var parameters = key.ExportParameters(true);

            return new PushNotificationSettings
            {
                PublicKey = Base64UrlEncode(ExportRawPublicKey(parameters)),
                PrivateKey = Base64UrlEncode(parameters.D!),
                Subject = "mailto:test@example.com",
            };
        }

        private static PushSubscription CreateSubscription()
        {
            using var key = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
            var parameters = key.ExportParameters(false);

            return new PushSubscription
            {
                Id = 12,
                UserId = 42,
                Endpoint = "https://push.example.test/send/123",
                P256dh = Base64UrlEncode(ExportRawPublicKey(parameters)),
                Auth = Base64UrlEncode(RandomNumberGenerator.GetBytes(16)),
                IsEnabled = true,
            };
        }

        private static byte[] ExportRawPublicKey(ECParameters parameters)
        {
            return [0x04, .. parameters.Q.X!, .. parameters.Q.Y!];
        }

        private static string Base64UrlEncode(byte[] value)
        {
            return Convert.ToBase64String(value)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
