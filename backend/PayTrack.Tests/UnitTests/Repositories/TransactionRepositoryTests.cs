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

            context.Transactions.AddRange(
                new PaymentRequestByUser { Id = 1, Amount = 100, CreatedAt = DateTime.UtcNow, InvoiceNumber = "123" },
                new PaymentRequestByUser { Id = 2, Amount = 200, CreatedAt = DateTime.UtcNow, InvoiceNumber = "123" }
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
