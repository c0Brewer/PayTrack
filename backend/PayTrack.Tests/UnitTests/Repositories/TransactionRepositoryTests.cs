using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using PayTrack.Application.Dto.PaymentRequestByUser;
using PayTrack.Application.Exceptions;
using PayTrack.Data;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Implementation;
using PayTrack.Data.Repositories.Model;
using PayTrack.Tests.UnitTests.Helper;

namespace PayTrack.Tests.UnitTests.Repositories
{
    public class TransactionRepositoryTests
    {
        // ----------------------------
        // Helper: InMemory DB
        // ----------------------------
        private static AppDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName + Guid.NewGuid())
                .Options;

            return new AppDbContext(options);
        }

        // ----------------------------
        // GET ALL (Transaction)
        // ----------------------------
        [Fact]
        public async Task GetAllTransactions_ShouldReturnData()
        {
            await using var context = GetInMemoryDbContext("GetAllTransactions");

            context.User.Add(new User { Id = 1, Email = "test@123", Name = "test123" });
            context.Teams.Add(new Team { Id = 1, Name = "test123" });

            context.Transactions.AddRange(
                new PaymentRequestByUser { Id = 1, Amount = 100, UserId = 1, TeamId = 1, CreatedAt = DateTime.UtcNow, InvoiceNumber = "123" },
                new PaymentRequestByUser { Id = 2, Amount = 200, UserId = 1, TeamId = 1, CreatedAt = DateTime.UtcNow, InvoiceNumber = "123" }
            );

            await context.SaveChangesAsync();

            var fileRepo = new Mock<IFileRepository>();
            var repo = new TransactionRepository(context, fileRepo.Object);

            var (transaction, totalCount) = await repo.GetAllAsync(null);

            transaction.Should().HaveCount(2);
            totalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllTransactionsWithAllParameters_ShouldReturnData()
        {
            await using var context = GetInMemoryDbContext("GetAllTransactions");

            context.User.Add(new User { Id = 1, Email = "test@123", Name = "test123" });
            context.Teams.Add(new Team { Id = 1, Name = "test123" });

            context.Transactions.AddRange(
                new PaymentRequestByUser { Id = 1, PurposeOfPayment = "123", Amount = 100, CreatedAt = DateTime.UtcNow, InvoiceNumber = "123", PayoutType = PayoutType.External, BankAccountId = 1, PaymentReference = "123", Status = TransactionStatus.Submitted, CostCentreId = 1, UserId = 1, TeamId = 1, PaymentDirection = PaymentDirection.Out },
                new PaymentRequestByUser { Id = 2, PurposeOfPayment = "123", Amount = 200, CreatedAt = DateTime.UtcNow, InvoiceNumber = "123", PayoutType = PayoutType.External, BankAccountId = 1, PaymentReference = "123", Status = TransactionStatus.Submitted, CostCentreId = 1, UserId = 1, TeamId = 1, PaymentDirection = PaymentDirection.Out }
            );

            await context.SaveChangesAsync();

            var fileRepo = new Mock<IFileRepository>();
            var repo = new TransactionRepository(context, fileRepo.Object);

            var (transaction, totalCount) = await repo.GetAllAsync(new GetPaymentRequestByUserQuery
            {
                InvoiceNumber = "12",
                PayoutType = PayoutType.External,
                BankAccountId = 1,
                IncludeBankAccount = true,

                // Transaction queries:
                UserId = 1,
                MinAmount = 50,
                MaxAmount = 250,
                PurposeOfPayment = "12",
                PaymentReference = "12",
                Status = TransactionStatus.Submitted,
                CostCentreId = 1,
                TeamId = 1,
                PaymentDirection = PaymentDirection.Out,
                Offset = 0,
                Limit = 20,
                MinCreatedAt = DateTime.Now.AddDays(-2),
                MaxCreatedAt = DateTime.Now.AddDays(2)
                // In order to test these we would have to insert teams, cost centres, etc as well
                // IncludeCostCentre = true,
                // IncludeTeam = true,
                // IncludeStatusHistory = true
            });

            transaction.Should().HaveCount(2);
            totalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllTransactions_FilterByDecimalAmount_ShouldReturnMatchingData()
        {
            await using var context = GetInMemoryDbContext("FilterByDecimalAmount");

            context.User.Add(new User { Id = 1, Email = "test@123", Name = "test123" });
            context.Teams.Add(new Team { Id = 1, Name = "test123" });

            context.Transactions.AddRange(
                new PaymentRequestByUser { Id = 1, Amount = 105.30m, UserId = 1, TeamId = 1, CreatedAt = DateTime.UtcNow, InvoiceNumber = "1" },
                new PaymentRequestByUser { Id = 2, Amount = 105.40m, UserId = 1, TeamId = 1, CreatedAt = DateTime.UtcNow, InvoiceNumber = "2" },
                new PaymentRequestByUser { Id = 3, Amount = 105.50m, UserId = 1, TeamId = 1, CreatedAt = DateTime.UtcNow, InvoiceNumber = "3" });

            await context.SaveChangesAsync();

            var fileRepo = new Mock<IFileRepository>();
            var repo = new TransactionRepository(context, fileRepo.Object);

            var (transactions, totalCount) = await repo.GetAllAsync(new GetPaymentRequestByUserQuery
            {
                MinAmount = 105.4m,
                MaxAmount = 105.4m,
            });

            totalCount.Should().Be(1);
            transactions.Should().ContainSingle(t => t.Id == 2);
        }

        [Fact]
        public async Task GetAllTransactions_FilterByMinPaidAt_ShouldExcludeUnpaidAndEarlier()
        {
            await using var context = GetInMemoryDbContext("FilterByMinPaidAt");

            context.User.Add(new User { Id = 1, Email = "test@123", Name = "test123" });
            context.Teams.Add(new Team { Id = 1, Name = "test123" });

            var cutoff = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            context.Transactions.AddRange(
                new PaymentRequestByUser { Id = 1, Amount = 100, UserId = 1, TeamId = 1, InvoiceNumber = "1", PaidAt = cutoff.AddDays(1) },
                new PaymentRequestByUser { Id = 2, Amount = 200, UserId = 1, TeamId = 1, InvoiceNumber = "2", PaidAt = cutoff.AddDays(-1) },
                new PaymentRequestByUser { Id = 3, Amount = 300, UserId = 1, TeamId = 1, InvoiceNumber = "3", PaidAt = null }
            );

            await context.SaveChangesAsync();

            var fileRepo = new Mock<IFileRepository>();
            var repo = new TransactionRepository(context, fileRepo.Object);

            var (transactions, totalCount) = await repo.GetAllAsync(new GetPaymentRequestByUserQuery { MinPaidAt = cutoff });

            totalCount.Should().Be(1);
            transactions.Should().ContainSingle(t => t.Id == 1);
        }

        [Fact]
        public async Task GetAllTransactions_FilterByMaxPaidAt_ShouldExcludeLaterAndUnpaid()
        {
            await using var context = GetInMemoryDbContext("FilterByMaxPaidAt");

            context.User.Add(new User { Id = 1, Email = "test@123", Name = "test123" });
            context.Teams.Add(new Team { Id = 1, Name = "test123" });

            var cutoff = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            context.Transactions.AddRange(
                new PaymentRequestByUser { Id = 1, Amount = 100, UserId = 1, TeamId = 1, InvoiceNumber = "1", PaidAt = cutoff.AddDays(-1) },
                new PaymentRequestByUser { Id = 2, Amount = 200, UserId = 1, TeamId = 1, InvoiceNumber = "2", PaidAt = cutoff.AddDays(1) },
                new PaymentRequestByUser { Id = 3, Amount = 300, UserId = 1, TeamId = 1, InvoiceNumber = "3", PaidAt = null }
            );

            await context.SaveChangesAsync();

            var fileRepo = new Mock<IFileRepository>();
            var repo = new TransactionRepository(context, fileRepo.Object);

            var (transactions, totalCount) = await repo.GetAllAsync(new GetPaymentRequestByUserQuery { MaxPaidAt = cutoff });

            totalCount.Should().Be(1);
            transactions.Should().ContainSingle(t => t.Id == 1);
        }

        // ----------------------------
        // ADD TRANSACTION
        // ----------------------------
        [Fact]
        public async Task AddTransaction_ShouldPersistEntity()
        {
            await using var context = GetInMemoryDbContext("AddTransaction");

            var fileRepo = new Mock<IFileRepository>();
            var repo = new TransactionRepository(context, fileRepo.Object);

            var transaction = new PaymentRequestByUser
            {
                Amount = 123,
                CreatedAt = DateTime.UtcNow,
                InvoiceNumber = "123"
            };

            var result = await repo.AddAsync(transaction);

            result.Should().NotBeNull();
            result.Id.Should().NotBe(0);

            var dbEntity = await context.Transactions.FindAsync(result.Id);
            dbEntity.Should().NotBeNull();
            dbEntity.Amount.Should().Be(123);
        }

        // ----------------------------
        // ADD PAYMENT REQUEST BY USER
        // ----------------------------
        [Fact]
        public async Task AddPaymentRequest_ShouldSaveFileAndEntity()
        {
            await using var context = GetInMemoryDbContext("AddPaymentRequest");

            var fileRepo = new Mock<IFileRepository>();

            fileRepo
                .Setup(f => f.SaveFile(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync("stored/path/file.pdf");

            var repo = new TransactionRepository(context, fileRepo.Object);

            var file = new FormFile(Stream.Null, 0, 0, "file", "test.pdf");

            var entity = new PaymentRequestByUser
            {
                InvoiceNumber = "INV-1",
                CreatedAt = DateTime.UtcNow
            };

            var result = await repo.AddAsync(entity, file);

            result.Should().NotBeNull();
            result.ReceiptUrl.Should().Be("stored/path/file.pdf");

            var db = await context.PaymentRequestsByUser.FindAsync(result.Id);
            db.Should().NotBeNull();
        }

        // ----------------------------
        // ADD FAILURE CASE
        // ----------------------------
        [Fact]
        public async Task AddTransaction_ShouldThrow_WhenSaveFails()
        {
            var context = new FailingDbContext("FailTransaction");
            var fileRepo = new Mock<IFileRepository>();

            var repo = new TransactionRepository(context, fileRepo.Object);

            var entity = new PaymentRequestByUser() { InvoiceNumber = "123" };

            async Task act() => await repo.AddAsync(entity);

            var ex = await Assert.ThrowsAsync<InternalErrorException>(act);

            ex.Message.Should().Contain("Transaction");
        }

        // ----------------------------
        // UPDATE PAYMENT REQUEST
        // ----------------------------
        [Fact]
        public async Task UpdatePaymentRequest_ShouldPersistChanges()
        {
            await using var context = GetInMemoryDbContext("UpdatePaymentRequest");

            var fileRepo = new Mock<IFileRepository>();

            var existing = new PaymentRequestByUser
            {
                Id = 1,
                Amount = 100,
                InvoiceNumber = "123"
            };

            context.PaymentRequestsByUser.Add(existing);
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, fileRepo.Object);

            existing.Amount = 999;

            var result = await repo.UpdateAsync(existing);

            result.Amount.Should().Be(999);

            var db = await context.PaymentRequestsByUser.FindAsync(1);
            db!.Amount.Should().Be(999);
        }

        // ----------------------------
        // GET BY ID (PaymentRequestByUser)
        // ----------------------------
        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
        {
            await using var context = GetInMemoryDbContext("GetByIdAsync_Found");

            context.PaymentRequestsByUser.Add(new PaymentRequestByUser
            {
                Id = 42,
                InvoiceNumber = "INV-42",
                Amount = 500,
                CreatedAt = DateTime.UtcNow,
            });
            await context.SaveChangesAsync();

            var fileRepo = new Mock<IFileRepository>();
            var repo = new TransactionRepository(context, fileRepo.Object);

            var result = await repo.GetByIdAsync(42, null);

            result.Should().NotBeNull();
            result!.Id.Should().Be(42);
            result.InvoiceNumber.Should().Be("INV-42");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            await using var context = GetInMemoryDbContext("GetByIdAsync_NotFound");

            var fileRepo = new Mock<IFileRepository>();
            var repo = new TransactionRepository(context, fileRepo.Object);

            var result = await repo.GetByIdAsync(999, null);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldIncludeBankAccount_WhenRequested()
        {
            await using var context = GetInMemoryDbContext("GetByIdAsync_WithBankAccount");

            var bankAccount = new BankAccount
            {
                Id = 1,
                UserId = 99,
                Iban = "AT611904300234573201",
                Bic = "BKAUATWW",
                AccountHolder = "Test User",
            };
            context.BankAccounts.Add(bankAccount);
            context.PaymentRequestsByUser.Add(new PaymentRequestByUser
            {
                Id = 1,
                InvoiceNumber = "INV-1",
                BankAccountId = 1,
                CreatedAt = DateTime.UtcNow,
            });
            await context.SaveChangesAsync();

            var fileRepo = new Mock<IFileRepository>();
            var repo = new TransactionRepository(context, fileRepo.Object);

            var result = await repo.GetByIdAsync(1, new GetPaymentRequestByUserQueryById { IncludeBankAccount = true });

            result.Should().NotBeNull();
            result!.BankAccount.Should().NotBeNull();
            result.BankAccount!.Iban.Should().Be("AT611904300234573201");
        }

        // ----------------------------
        // UPDATE FAILURE
        // ----------------------------
        [Fact]
        public async Task Update_ShouldThrow_WhenSaveFails()
        {
            var context = new FailingDbContext("FailUpdate");
            var fileRepo = new Mock<IFileRepository>();

            var repo = new TransactionRepository(context, fileRepo.Object);

            var entity = new PaymentRequestByUser { Id = 1, InvoiceNumber = "123" };

            async Task act() => await repo.UpdateAsync(entity);

            var ex = await Assert.ThrowsAsync<InternalErrorException>(act);

            ex.Message.Should().Contain("Transaction");
        }
    }
}
