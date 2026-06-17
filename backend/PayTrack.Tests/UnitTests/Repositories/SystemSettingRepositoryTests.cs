using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Implementation;

namespace PayTrack.Tests.UnitTests.Repositories
{
    public class SystemSettingRepositoryTests
    {
        private static AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "SystemSettingDb_" + Guid.NewGuid())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetByKeyAsync_ShouldReturnNull_WhenKeyDoesNotExist()
        {
            await using var context = GetInMemoryDbContext();
            var repo = new SystemSettingRepository(context);

            var result = await repo.GetByKeyAsync("nonexistent.key");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByKeyAsync_ShouldReturnSetting_WhenKeyExists()
        {
            await using var context = GetInMemoryDbContext();
            context.SystemSettings.Add(new SystemSetting { Key = "csv.column.name", Value = "Name" });
            await context.SaveChangesAsync();

            var repo = new SystemSettingRepository(context);

            var result = await repo.GetByKeyAsync("csv.column.name");

            result.Should().NotBeNull();
            result!.Value.Should().Be("Name");
        }

        [Fact]
        public async Task UpsertAsync_ShouldInsert_WhenKeyDoesNotExist()
        {
            await using var context = GetInMemoryDbContext();
            var user = new User { Name = "Admin", Email = "admin@test.com", Role = Role.Admin };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repo = new SystemSettingRepository(context);

            await repo.UpsertAsync("csv.column.name", "Bezeichnung", user.Id);

            var row = await context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "csv.column.name");
            row.Should().NotBeNull();
            row!.Value.Should().Be("Bezeichnung");
            row.LastModifiedByUserId.Should().Be(user.Id);
            row.LastModifiedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task UpsertAsync_ShouldUpdate_WhenKeyAlreadyExists()
        {
            await using var context = GetInMemoryDbContext();
            var user = new User { Name = "Admin", Email = "admin@test.com", Role = Role.Admin };
            context.User.Add(user);
            context.SystemSettings.Add(new SystemSetting { Key = "csv.column.name", Value = "Name" });
            await context.SaveChangesAsync();

            var repo = new SystemSettingRepository(context);

            await repo.UpsertAsync("csv.column.name", "Bezeichnung", user.Id);

            var rows = await context.SystemSettings.Where(s => s.Key == "csv.column.name").ToListAsync();
            rows.Should().HaveCount(1);
            rows[0].Value.Should().Be("Bezeichnung");
        }

        [Fact]
        public async Task UpsertManyAsync_ShouldInsertAllKeys_WhenNoneExist()
        {
            await using var context = GetInMemoryDbContext();
            var user = new User { Name = "Admin", Email = "admin@test.com", Role = Role.Admin };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repo = new SystemSettingRepository(context);

            await repo.UpsertManyAsync(
                new Dictionary<string, string>
                {
                    ["key.a"] = "value-a",
                    ["key.b"] = "value-b",
                },
                user.Id);

            var rows = await context.SystemSettings.ToListAsync();
            rows.Should().HaveCount(2);
            rows.Should().Contain(r => r.Key == "key.a" && r.Value == "value-a");
            rows.Should().Contain(r => r.Key == "key.b" && r.Value == "value-b");
        }

        [Fact]
        public async Task UpsertManyAsync_ShouldUpdateExistingAndInsertNew_WhenSomeExist()
        {
            await using var context = GetInMemoryDbContext();
            var user = new User { Name = "Admin", Email = "admin@test.com", Role = Role.Admin };
            context.User.Add(user);
            context.SystemSettings.Add(new SystemSetting { Key = "key.a", Value = "old-value" });
            await context.SaveChangesAsync();

            var repo = new SystemSettingRepository(context);

            await repo.UpsertManyAsync(
                new Dictionary<string, string>
                {
                    ["key.a"] = "new-value",
                    ["key.b"] = "value-b",
                },
                user.Id);

            var rows = await context.SystemSettings.ToListAsync();
            rows.Should().HaveCount(2);
            rows.Should().Contain(r => r.Key == "key.a" && r.Value == "new-value");
            rows.Should().Contain(r => r.Key == "key.b" && r.Value == "value-b");
        }

        [Fact]
        public async Task UpsertManyAsync_ShouldSetLastModifiedFields_OnAllRows()
        {
            await using var context = GetInMemoryDbContext();
            var user = new User { Name = "Admin", Email = "admin@test.com", Role = Role.Admin };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repo = new SystemSettingRepository(context);
            var before = DateTime.UtcNow;

            await repo.UpsertManyAsync(
                new Dictionary<string, string> { ["key.a"] = "v" },
                user.Id);

            var row = await context.SystemSettings.FirstAsync(s => s.Key == "key.a");
            row.LastModifiedByUserId.Should().Be(user.Id);
            row.LastModifiedAt.Should().BeOnOrAfter(before);
        }
    }
}
