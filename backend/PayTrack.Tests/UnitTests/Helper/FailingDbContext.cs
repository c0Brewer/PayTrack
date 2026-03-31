using Microsoft.EntityFrameworkCore;
using PayTrack.Data;

namespace PayTrack.Tests.UnitTests.Helper
{
    /// <summary>
    /// Fake DbContext that throws on SaveChangesAsync to simulate failure
    /// </summary>
    public class FailingDbContext(string name, int _countOfSuccessBeforeFailure = 0) : AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name).Options)
    {
        // This is an important variable because sometimes we need to actually 
        // save an entity in our db before we want to simulate the failure
        private int countOfSuccessBeforeFailure = _countOfSuccessBeforeFailure;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (this.countOfSuccessBeforeFailure > 0) {
                this.countOfSuccessBeforeFailure--;
                return base.SaveChangesAsync(cancellationToken);
            }
            return Task.FromResult(0); // Simulate failure
        }
    }
}
