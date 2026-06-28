//AI helped with the test cases

using System.Net;
using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PayTrack.Application.Dto.Notification;
using PayTrack.Application.Exceptions;
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
            repo.Setup(r => r.GetEnabledForUserAsync(42)).ReturnsAsync([CreateSubscription()]);
            var service = BuildService(repo, new PushNotificationSettings(), new SequentialHttpHandler());

            var config = await service.GetConfigAsync(42, "https://fcm.googleapis.com/fcm/send/123");

            config.IsConfigured.Should().BeFalse();
            config.VapidPublicKey.Should().BeNull();
            config.Enabled.Should().BeTrue();
            config.Devices.Should().ContainSingle(d => d.IsCurrentDevice);
        }

        [Fact]
        public async Task GetConfigAsync_ReturnsDisabledForCurrentBrowser_WhenEndpointDoesNotMatch()
        {
            var repo = new Mock<IPushSubscriptionRepository>();
            repo.Setup(r => r.GetEnabledForUserAsync(42)).ReturnsAsync([CreateSubscription()]);
            var service = BuildService(repo, CreateVapidSettings(), new SequentialHttpHandler());

            var config = await service.GetConfigAsync(42, "https://fcm.googleapis.com/fcm/send/other");

            config.Enabled.Should().BeFalse();
            config.Devices.Should().ContainSingle(d => !d.IsCurrentDevice);
        }

        [Fact]
        public async Task GetConfigAsync_KeepsHeuristicDeviceMatchesAndUnknownMetadata()
        {
            var current = CreateSubscription("https://fcm.googleapis.com/fcm/send/current");
            current.Id = 1;
            current.BrowserName = "Chrome";
            current.DeviceName = "Windows device";
            current.Platform = "Windows";
            current.UpdatedAt = DateTime.UtcNow;

            var sameDeviceMetadata = CreateSubscription("https://fcm.googleapis.com/fcm/send/same-device-metadata");
            sameDeviceMetadata.Id = 2;
            sameDeviceMetadata.BrowserName = "Chrome";
            sameDeviceMetadata.DeviceName = "Windows device";
            sameDeviceMetadata.Platform = "Windows";
            sameDeviceMetadata.UpdatedAt = current.UpdatedAt.AddMinutes(-1);

            var unknown = CreateSubscription("https://fcm.googleapis.com/fcm/send/unknown", includeDeviceMetadata: false);
            unknown.Id = 3;
            var repo = new Mock<IPushSubscriptionRepository>();
            repo.Setup(r => r.GetEnabledForUserAsync(42)).ReturnsAsync([current, sameDeviceMetadata, unknown]);
            var service = BuildService(repo, CreateVapidSettings(), new SequentialHttpHandler());

            var config = await service.GetConfigAsync(42, current.Endpoint);

            config.Devices.Select(d => d.Id).Should().BeEquivalentTo([1, 2, 3]);
            config.Devices.Should().ContainSingle(d => d.IsCurrentDevice);
            config.Devices.Should().ContainSingle(d =>
                d.Id == unknown.Id &&
                d.BrowserName == "Unknown browser" &&
                d.DeviceName == "Unknown device" &&
                d.Platform == "Unknown platform");
            repo.Verify(r => r.DisableByEndpointAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetConfigAsync_ReturnsOnlyOneDevicePerEndpoint()
        {
            var current = CreateSubscription("https://fcm.googleapis.com/fcm/send/current");
            current.Id = 1;
            current.UpdatedAt = DateTime.UtcNow;

            var duplicateEndpoint = CreateSubscription(current.Endpoint);
            duplicateEndpoint.Id = 2;
            duplicateEndpoint.UpdatedAt = current.UpdatedAt.AddMinutes(-1);

            var other = CreateSubscription("https://fcm.googleapis.com/fcm/send/other");
            other.Id = 3;
            var repo = new Mock<IPushSubscriptionRepository>();
            repo.Setup(r => r.GetEnabledForUserAsync(42)).ReturnsAsync([duplicateEndpoint, current, other]);
            var service = BuildService(repo, CreateVapidSettings(), new SequentialHttpHandler());

            var config = await service.GetConfigAsync(42, current.Endpoint);

            config.Devices.Select(d => d.Id).Should().BeEquivalentTo([1, 3]);
            config.Devices.Should().ContainSingle(d => d.IsCurrentDevice);
            repo.Verify(r => r.DisableByEndpointAsync(It.IsAny<string>()), Times.Never);
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
                    Endpoint = "https://fcm.googleapis.com/fcm/send/123",
                    P256dh = "p256dh",
                    Auth = "auth",
                    BrowserName = "Chrome",
                    DeviceName = "Samsung Galaxy S21",
                    Platform = "Android",
                });

            repo.Verify(
                r => r.UpsertAsync(It.Is<PushSubscription>(s =>
                    s.UserId == 42 &&
                    s.Endpoint == "https://fcm.googleapis.com/fcm/send/123" &&
                    s.P256dh == "p256dh" &&
                    s.Auth == "auth" &&
                    s.BrowserName == "Chrome" &&
                    s.DeviceName == "Samsung Galaxy S21" &&
                    s.Platform == "Android" &&
                    s.IsEnabled)),
                Times.Once);
        }

        [Fact]
        public async Task SaveSubscriptionAsync_RejectsUnsupportedEndpoint()
        {
            var repo = new Mock<IPushSubscriptionRepository>();
            var service = BuildService(repo, CreateVapidSettings(), new SequentialHttpHandler());

            var action = async () => await service.SaveSubscriptionAsync(
                42,
                new SavePushSubscriptionDto
                {
                    Endpoint = "https://127.0.0.1/push",
                    P256dh = "p256dh",
                    Auth = "auth",
                });

            await action.Should()
                .ThrowAsync<InvalidStateException>()
                .WithMessage("The push subscription endpoint is not supported.");
            repo.Verify(r => r.UpsertAsync(It.IsAny<PushSubscription>()), Times.Never);
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

        [Fact]
        public async Task SendWorkflowStatusChangedAsync_SkipsDelivery_WhenStoredEndpointIsUnsupported()
        {
            var subscription = CreateSubscription("https://127.0.0.1/push");
            var repo = new Mock<IPushSubscriptionRepository>();
            repo.Setup(r => r.GetEnabledForUserAsync(42)).ReturnsAsync([subscription]);
            var handler = new SequentialHttpHandler();
            var service = BuildService(repo, CreateVapidSettings(), handler);

            await service.SendWorkflowStatusChangedAsync(42, "Invoice approved", "Your invoice was approved.", "/my-invoices/7");

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

        private static PushSubscription CreateSubscription(
            string endpoint = "https://fcm.googleapis.com/fcm/send/123",
            bool includeDeviceMetadata = true)
        {
            using var key = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
            var parameters = key.ExportParameters(false);

            var subscription = new PushSubscription
            {
                Id = 12,
                UserId = 42,
                Endpoint = endpoint,
                P256dh = Base64UrlEncode(ExportRawPublicKey(parameters)),
                Auth = Base64UrlEncode(RandomNumberGenerator.GetBytes(16)),
                IsEnabled = true,
            };

            if (includeDeviceMetadata)
            {
                subscription.BrowserName = "Chrome";
                subscription.DeviceName = "Windows device";
                subscription.Platform = "Windows";
            }

            return subscription;
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
