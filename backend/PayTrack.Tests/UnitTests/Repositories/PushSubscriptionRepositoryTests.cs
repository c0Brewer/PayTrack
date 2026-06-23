using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Implementation;

namespace PayTrack.Tests.UnitTests.Repositories
{
    public class PushSubscriptionRepositoryTests
    {
        private static AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "PushSubscriptionDb_" + Guid.NewGuid())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task HasEnabledSubscriptionAsync_ShouldReturnTrue_WhenUserHasEnabledSubscription()
        {
            await using var context = GetInMemoryDbContext();
            context.PushSubscriptions.Add(new PushSubscription
            {
                UserId = 1,
                Endpoint = "https://push.test/enabled",
                P256dh = "key",
                Auth = "auth",
                IsEnabled = true,
            });
            context.PushSubscriptions.Add(new PushSubscription
            {
                UserId = 1,
                Endpoint = "https://push.test/disabled",
                P256dh = "key",
                Auth = "auth",
                IsEnabled = false,
            });
            await context.SaveChangesAsync();

            var repo = new PushSubscriptionRepository(context);

            var result = await repo.HasEnabledSubscriptionAsync(1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasEnabledSubscriptionAsync_ShouldReturnFalse_WhenUserHasNoEnabledSubscription()
        {
            await using var context = GetInMemoryDbContext();
            context.PushSubscriptions.Add(new PushSubscription
            {
                UserId = 1,
                Endpoint = "https://push.test/disabled",
                P256dh = "key",
                Auth = "auth",
                IsEnabled = false,
            });
            await context.SaveChangesAsync();

            var repo = new PushSubscriptionRepository(context);

            var result = await repo.HasEnabledSubscriptionAsync(1);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetEnabledForUserAsync_ShouldReturnOnlyEnabledSubscriptionsForUser()
        {
            await using var context = GetInMemoryDbContext();
            context.PushSubscriptions.AddRange(
                new PushSubscription
                {
                    UserId = 1,
                    Endpoint = "https://push.test/enabled",
                    P256dh = "key",
                    Auth = "auth",
                    IsEnabled = true,
                },
                new PushSubscription
                {
                    UserId = 1,
                    Endpoint = "https://push.test/disabled",
                    P256dh = "key",
                    Auth = "auth",
                    IsEnabled = false,
                },
                new PushSubscription
                {
                    UserId = 2,
                    Endpoint = "https://push.test/other-user",
                    P256dh = "key",
                    Auth = "auth",
                    IsEnabled = true,
                });
            await context.SaveChangesAsync();

            var repo = new PushSubscriptionRepository(context);

            var result = await repo.GetEnabledForUserAsync(1);

            result.Should().ContainSingle();
            result[0].Endpoint.Should().Be("https://push.test/enabled");
        }

        [Fact]
        public async Task UpsertAsync_ShouldInsert_WhenEndpointDoesNotExist()
        {
            await using var context = GetInMemoryDbContext();
            var repo = new PushSubscriptionRepository(context);

            var result = await repo.UpsertAsync(new PushSubscription
            {
                UserId = 1,
                Endpoint = "https://push.test/new",
                P256dh = "key",
                Auth = "auth",
            });

            result.Id.Should().BeGreaterThan(0);
            context.PushSubscriptions.Should().ContainSingle(s => s.Endpoint == "https://push.test/new");
        }

        [Fact]
        public async Task UpsertAsync_ShouldUpdateAndEnable_WhenEndpointAlreadyExists()
        {
            await using var context = GetInMemoryDbContext();
            context.PushSubscriptions.Add(new PushSubscription
            {
                UserId = 1,
                Endpoint = "https://push.test/existing",
                P256dh = "old-key",
                Auth = "old-auth",
                IsEnabled = false,
            });
            await context.SaveChangesAsync();

            var repo = new PushSubscriptionRepository(context);

            var result = await repo.UpsertAsync(new PushSubscription
            {
                UserId = 2,
                Endpoint = "https://push.test/existing",
                P256dh = "new-key",
                Auth = "new-auth",
            });

            result.UserId.Should().Be(2);
            result.P256dh.Should().Be("new-key");
            result.Auth.Should().Be("new-auth");
            result.IsEnabled.Should().BeTrue();
            context.PushSubscriptions.Should().ContainSingle(s => s.Endpoint == "https://push.test/existing");
        }

        [Fact]
        public async Task DisableAsync_ShouldDisableMatchingUserSubscription()
        {
            await using var context = GetInMemoryDbContext();
            context.PushSubscriptions.Add(new PushSubscription
            {
                UserId = 1,
                Endpoint = "https://push.test/matching",
                P256dh = "key",
                Auth = "auth",
                IsEnabled = true,
            });
            await context.SaveChangesAsync();

            var repo = new PushSubscriptionRepository(context);

            await repo.DisableAsync(1, "https://push.test/matching");

            var subscription = await context.PushSubscriptions.SingleAsync();
            subscription.IsEnabled.Should().BeFalse();
        }

        [Fact]
        public async Task DisableAsync_ShouldDoNothing_WhenSubscriptionDoesNotExist()
        {
            await using var context = GetInMemoryDbContext();
            var repo = new PushSubscriptionRepository(context);

            Func<Task> act = async () => await repo.DisableAsync(1, "https://push.test/missing");

            await act.Should().NotThrowAsync();
            context.PushSubscriptions.Should().BeEmpty();
        }

        [Fact]
        public async Task DisableByEndpointAsync_ShouldDisableMatchingSubscription()
        {
            await using var context = GetInMemoryDbContext();
            context.PushSubscriptions.Add(new PushSubscription
            {
                UserId = 1,
                Endpoint = "https://push.test/by-endpoint",
                P256dh = "key",
                Auth = "auth",
                IsEnabled = true,
            });
            await context.SaveChangesAsync();

            var repo = new PushSubscriptionRepository(context);

            await repo.DisableByEndpointAsync("https://push.test/by-endpoint");

            var subscription = await context.PushSubscriptions.SingleAsync();
            subscription.IsEnabled.Should().BeFalse();
        }
    }
}
