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
    }
}
