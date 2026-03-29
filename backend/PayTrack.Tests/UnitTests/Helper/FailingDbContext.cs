using Microsoft.EntityFrameworkCore;
using PayTrack.Data;

namespace PayTrack.Tests.UnitTests.Helper
{
    /// <summary>
    /// Fake DbContext that throws on SaveChangesAsync to simulate failure
    /// </summary>
    public class FailingDbContext(string name) : AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name).Options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0); // Simulate failure
        }
    }
}
