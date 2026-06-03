using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using PayTrack.Application.Dto.PaymentRequestByTeam;
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

            var (transaction, totalCount) = await repo.GetAllAsync(new Application.Dto.Transaction.GetTransactionQuery());

            transaction.Should().HaveCount(2);
            totalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllTransactionsWithAllParameters_ShouldReturnData()
        {
            await using var context = GetInMemoryDbContext("GetAllTransactionsWithParams");

            context.User.Add(new User { Id = 1, Email = "test@123", Name = "test123" });
            context.Teams.Add(new Team { Id = 1, Name = "test123" });

            context.Transactions.AddRange(
                new PaymentRequestByUser { Id = 1, PurposeOfPayment = "123", Amount = 100, CreatedAt = DateTime.UtcNow, InvoiceNumber = "123", PayoutType = PayoutType.External, BankAccountId = 1, PaymentReference = "123", Status = TransactionStatus.Submitted, UserId = 1, TeamId = 1, PaymentDirection = PaymentDirection.Out },
                new PaymentRequestByUser { Id = 2, PurposeOfPayment = "123", Amount = 200, CreatedAt = DateTime.UtcNow, InvoiceNumber = "123", PayoutType = PayoutType.External, BankAccountId = 1, PaymentReference = "123", Status = TransactionStatus.Submitted, UserId = 1, TeamId = 1, PaymentDirection = PaymentDirection.Out }
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
                TeamId = 1,
                PaymentDirection = PaymentDirection.Out,
                Offset = 0,
                Limit = 20,
                MinCreatedAt = DateTime.Now.AddDays(-2),
                MaxCreatedAt = DateTime.Now.AddDays(2)
                // In order to test these we would have to insert teams, etc as well
                // IncludeTeam = true,
                // IncludeStatusHistory = true
            });

            transaction.Should().HaveCount(2);
            totalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllPaymentRequestByTeam_ShouldReturnData()
        {
            await using var context = GetInMemoryDbContext("GetAllTeam");

            context.User.Add(new User { Id = 1, Email = "test@123", Name = "test123" });
            context.Teams.Add(new Team { Id = 1, Name = "test123" });

            context.PaymentRequestsByTeam.AddRange(
                new PaymentRequestByTeam { Id = 1, UserId = 1, TeamId = 1, Amount = 100, CreatedAt = DateTime.UtcNow },
                new PaymentRequestByTeam { Id = 2, UserId = 1, TeamId = 1, Amount = 200, CreatedAt = DateTime.UtcNow }
            );

            await context.SaveChangesAsync();

            var fileRepo = new Mock<IFileRepository>();
            var repo = new TransactionRepository(context, fileRepo.Object);

            var (result, count) = await repo.GetAllAsync(new GetPaymentRequestByTeamQuery());

            result.Should().HaveCount(2);
            count.Should().Be(2);
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
        public async Task GetAllTransactions_ShouldFilterByAmount()
        {
            await using var context = GetInMemoryDbContext("FilterAmount");

            context.User.Add(new User { Id = 1, Email = "test@123", Name = "test123" });
            context.Teams.Add(new Team { Id = 1, Name = "test123" });

            context.Transactions.AddRange(
                new PaymentRequestByUser { Id = 1, UserId = 1, TeamId = 1, Amount = 50, CreatedAt = DateTime.UtcNow, InvoiceNumber = "123" },
                new PaymentRequestByUser { Id = 2, UserId = 1, TeamId = 1, Amount = 200, CreatedAt = DateTime.UtcNow, InvoiceNumber = "123" }
            );

            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, Mock.Of<IFileRepository>());

            var (result, count) = await repo.GetAllAsync(new Application.Dto.Transaction.GetTransactionQuery
            {
                MinAmount = 100
            });

            result.Should().HaveCount(1);
            result.First().Amount.Should().Be(200);
            count.Should().Be(1);
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

        [Fact]
        public async Task GetAllPaymentRequests_ShouldMarkPotentialDuplicates()
        {
            await using var context = GetInMemoryDbContext("MarkPotentialDuplicates");

            context.User.AddRange(
                new User { Id = 1, Email = "user1@paytrack.dev", Name = "User 1" },
                new User { Id = 2, Email = "user2@paytrack.dev", Name = "User 2" });
            context.Teams.AddRange(
                new Team { Id = 1, Name = "Team 1" },
                new Team { Id = 2, Name = "Team 2" });

            var paidAt = new DateTime(2026, 1, 5, 12, 0, 0, DateTimeKind.Utc);

            context.PaymentRequestsByUser.AddRange(
                new PaymentRequestByUser { Id = 1, UserId = 1, TeamId = 1, Amount = 100, PaidAt = paidAt, InvoiceNumber = "DUP-1" },
                new PaymentRequestByUser { Id = 2, UserId = 2, TeamId = 1, Amount = 100, PaidAt = paidAt.AddHours(2), InvoiceNumber = "DUP-2" },
                new PaymentRequestByUser { Id = 3, UserId = 1, TeamId = 2, Amount = 50, PaidAt = paidAt, InvoiceNumber = "OTHER-AMOUNT" },
                new PaymentRequestByUser { Id = 4, UserId = 2, TeamId = 1, Amount = 100, PaidAt = paidAt.AddDays(1), InvoiceNumber = "OTHER-DAY" },
                new PaymentRequestByUser { Id = 5, UserId = 1, TeamId = 1, Amount = 100, PaidAt = null, InvoiceNumber = "NO-DAY" });

            await context.SaveChangesAsync();

            var fileRepo = new Mock<IFileRepository>();
            var repo = new TransactionRepository(context, fileRepo.Object);

            var (transactions, totalCount) = await repo.GetAllAsync(new GetPaymentRequestByUserQuery());

            totalCount.Should().Be(5);
            transactions.Single(t => t.Id == 1).HasPotentialDuplicate.Should().BeTrue();
            transactions.Single(t => t.Id == 2).HasPotentialDuplicate.Should().BeTrue();
            transactions.Single(t => t.Id == 3).HasPotentialDuplicate.Should().BeFalse();
            transactions.Single(t => t.Id == 4).HasPotentialDuplicate.Should().BeFalse();
            transactions.Single(t => t.Id == 5).HasPotentialDuplicate.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllPaymentRequests_ShouldIgnoreDismissedDuplicatePairs()
        {
            await using var context = GetInMemoryDbContext("IgnoreDismissedDuplicates");

            context.User.AddRange(
                new User { Id = 1, Email = "user1@paytrack.dev", Name = "User 1" },
                new User { Id = 2, Email = "user2@paytrack.dev", Name = "User 2" });
            context.Teams.Add(new Team { Id = 1, Name = "Team 1" });

            var paidAt = new DateTime(2026, 1, 5, 12, 0, 0, DateTimeKind.Utc);
            context.PaymentRequestsByUser.AddRange(
                new PaymentRequestByUser { Id = 1, UserId = 1, TeamId = 1, Amount = 100, PaidAt = paidAt, InvoiceNumber = "DUP-1" },
                new PaymentRequestByUser { Id = 2, UserId = 2, TeamId = 1, Amount = 100, PaidAt = paidAt, InvoiceNumber = "DUP-2" });
            context.DismissedDuplicatePaymentRequestsByUser.Add(
                new DismissedDuplicatePaymentRequestByUser
                {
                    FirstPaymentRequestByUserId = 1,
                    SecondPaymentRequestByUserId = 2,
                });
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, new Mock<IFileRepository>().Object);

            var (transactions, _) = await repo.GetAllAsync(new GetPaymentRequestByUserQuery());

            transactions.Single(t => t.Id == 1).HasPotentialDuplicate.Should().BeFalse();
            transactions.Single(t => t.Id == 2).HasPotentialDuplicate.Should().BeFalse();
        }

        [Fact]
        public async Task GetPotentialDuplicatesAsync_ShouldExcludeDismissedPairsForSource()
        {
            await using var context = GetInMemoryDbContext("PotentialDuplicatesDismissed");

            context.User.AddRange(
                new User { Id = 1, Email = "user1@paytrack.dev", Name = "User 1" },
                new User { Id = 2, Email = "user2@paytrack.dev", Name = "User 2" },
                new User { Id = 3, Email = "user3@paytrack.dev", Name = "User 3" });
            context.Teams.Add(new Team { Id = 1, Name = "Team 1" });

            var paidAt = new DateTime(2026, 1, 5, 12, 0, 0, DateTimeKind.Utc);
            context.PaymentRequestsByUser.AddRange(
                new PaymentRequestByUser { Id = 1, UserId = 1, TeamId = 1, Amount = 100, PaidAt = paidAt, InvoiceNumber = "SRC" },
                new PaymentRequestByUser { Id = 2, UserId = 2, TeamId = 1, Amount = 100, PaidAt = paidAt, InvoiceNumber = "DISMISSED" },
                new PaymentRequestByUser { Id = 3, UserId = 3, TeamId = 1, Amount = 100, PaidAt = paidAt, InvoiceNumber = "VISIBLE" });
            context.DismissedDuplicatePaymentRequestsByUser.Add(
                new DismissedDuplicatePaymentRequestByUser
                {
                    FirstPaymentRequestByUserId = 1,
                    SecondPaymentRequestByUserId = 2,
                });
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, new Mock<IFileRepository>().Object);

            var result = await repo.GetPotentialDuplicatesAsync(1, 1, 100, paidAt, null, 1);

            result.Select(t => t.Id).Should().Equal(3);
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

            var result = await repo.GetByIdAsync(42, (GetPaymentRequestByUserQueryById?)null);

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

            var result = await repo.GetByIdAsync(999, (GetPaymentRequestByUserQueryById?)null);

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

        [Fact]
        public async Task GetByIdAsync_ShouldIncludeStatusHistoryChangedBy_WhenRequested()
        {
            await using var context = GetInMemoryDbContext("GetByIdAsync_WithStatusHistoryChangedBy");

            context.User.Add(new User
            {
                Id = 7,
                Email = "finance@example.com",
                Name = "Finance User",
            });
            context.PaymentRequestsByUser.Add(new PaymentRequestByUser
            {
                Id = 1,
                InvoiceNumber = "INV-1",
                CreatedAt = DateTime.UtcNow,
                StatusHistory =
                [
                    new TransactionStatusHistory
                    {
                        ChangedById = 7,
                        Comment = "approved",
                        FromStatus = TransactionStatus.Submitted,
                        ToStatus = TransactionStatus.Approved,
                        ChangedAt = DateTime.UtcNow,
                    },
                ],
            });
            await context.SaveChangesAsync();

            var fileRepo = new Mock<IFileRepository>();
            var repo = new TransactionRepository(context, fileRepo.Object);

            var result = await repo.GetByIdAsync(1, new GetPaymentRequestByUserQueryById { IncludeStatusHistory = true });

            result.Should().NotBeNull();
            result!.StatusHistory.Should().ContainSingle();
            result.StatusHistory.Single().ChangedBy.Should().NotBeNull();
            result.StatusHistory.Single().ChangedBy.Name.Should().Be("Finance User");
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

        [Fact]
        public async Task DeletePaymentRequestByUserAsync_ShouldRemoveInvoice_WhenFound()
        {
            await using var context = GetInMemoryDbContext("DeletePaymentRequestByUser");
            context.PaymentRequestsByUser.Add(new PaymentRequestByUser { Id = 1, InvoiceNumber = "INV-1" });
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, new Mock<IFileRepository>().Object);

            var result = await repo.DeletePaymentRequestByUserAsync(1);

            result.Should().BeTrue();
            (await context.PaymentRequestsByUser.FindAsync(1)).Should().BeNull();
        }

        [Fact]
        public async Task DeletePaymentRequestByUserAsync_ShouldReturnFalse_WhenMissing()
        {
            await using var context = GetInMemoryDbContext("DeletePaymentRequestByUserMissing");
            var repo = new TransactionRepository(context, new Mock<IFileRepository>().Object);

            var result = await repo.DeletePaymentRequestByUserAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DismissDuplicatePaymentRequestByUserAsync_ShouldStoreNormalizedPair()
        {
            await using var context = GetInMemoryDbContext("DismissDuplicatePair");
            context.PaymentRequestsByUser.AddRange(
                new PaymentRequestByUser { Id = 1, InvoiceNumber = "INV-1" },
                new PaymentRequestByUser { Id = 2, InvoiceNumber = "INV-2" });
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, new Mock<IFileRepository>().Object);

            await repo.DismissDuplicatePaymentRequestByUserAsync(2, 1);
            await repo.DismissDuplicatePaymentRequestByUserAsync(1, 2);

            context.DismissedDuplicatePaymentRequestsByUser.Should().ContainSingle();
            var dismissal = await context.DismissedDuplicatePaymentRequestsByUser.SingleAsync();
            dismissal.FirstPaymentRequestByUserId.Should().Be(1);
            dismissal.SecondPaymentRequestByUserId.Should().Be(2);
        }


        // ----------------------------
        // GET BY ID (PaymentRequestByTeam)
        // ----------------------------
        [Fact]
        public async Task GetByIdAsyncTeam_ShouldReturnEntity_WhenExists()
        {
            await using var context = GetInMemoryDbContext("GetByIdTeam_Found");

            context.User.Add(new User { Id = 1, Email = "test@123", Name = "test123" });
            context.User.Add(new User { Id = 2, Email = "admin@123", Name = "admin" });
            context.Teams.Add(new Team { Id = 1, Name = "Chassis" });
            context.PaymentRequestsByTeam.Add(new PaymentRequestByTeam
            {
                Id = 10,
                UserId = 1,
                RequestedById = 2,
                TeamId = 1,
                Amount = 150,
                CreatedAt = DateTime.UtcNow,
            });
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, Mock.Of<IFileRepository>());

            var result = await repo.GetByIdAsync(10, (GetPaymentRequestByTeamQueryById?)null);

            result.Should().NotBeNull();
            result!.Id.Should().Be(10);
            result.Amount.Should().Be(150);
        }

        [Fact]
        public async Task GetByIdAsyncTeam_ShouldReturnNull_WhenNotExists()
        {
            await using var context = GetInMemoryDbContext("GetByIdTeam_NotFound");

            var repo = new TransactionRepository(context, Mock.Of<IFileRepository>());

            var result = await repo.GetByIdAsync(999, (GetPaymentRequestByTeamQueryById?)null);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllPaymentRequestByTeam_FilterByDueDate_ShouldReturnMatchingEntries()
        {
            await using var context = GetInMemoryDbContext("FilterByDueDate");

            context.User.Add(new User { Id = 1, Email = "test@123", Name = "test123" });
            context.Teams.Add(new Team { Id = 1, Name = "test123" });

            var cutoff = new DateTime(2025, 8, 1, 0, 0, 0, DateTimeKind.Utc);

            context.PaymentRequestsByTeam.AddRange(
                new PaymentRequestByTeam { Id = 1, UserId = 1, TeamId = 1, Amount = 100, DueDate = cutoff.AddDays(10) },
                new PaymentRequestByTeam { Id = 2, UserId = 1, TeamId = 1, Amount = 200, DueDate = cutoff.AddDays(-10) },
                new PaymentRequestByTeam { Id = 3, UserId = 1, TeamId = 1, Amount = 300, DueDate = null }
            );
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, Mock.Of<IFileRepository>());

            var (results, count) = await repo.GetAllAsync(new GetPaymentRequestByTeamQuery { MinDueDate = cutoff });

            count.Should().Be(1);
            results.Should().ContainSingle(t => t.Id == 1);
        }

        [Fact]
        public async Task GetAllPaymentRequestByTeam_FilterByPurpose_ShouldBeCaseInsensitive()
        {
            await using var context = GetInMemoryDbContext("FilterPurposeCaseInsensitive");

            context.User.Add(new User { Id = 1, Email = "test@123", Name = "test123" });
            context.Teams.Add(new Team { Id = 1, Name = "test123" });

            context.PaymentRequestsByTeam.AddRange(
                new PaymentRequestByTeam { Id = 1, UserId = 1, TeamId = 1, Amount = 100, PurposeOfPayment = "Engine repair" },
                new PaymentRequestByTeam { Id = 2, UserId = 1, TeamId = 1, Amount = 200, PurposeOfPayment = "Chassis work" },
                new PaymentRequestByTeam { Id = 3, UserId = 1, TeamId = 1, Amount = 300, PurposeOfPayment = null }
            );
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, Mock.Of<IFileRepository>());

            var (results, count) = await repo.GetAllAsync(new GetPaymentRequestByTeamQuery { PurposeOfPayment = "engine" });

            count.Should().Be(1);
            results.Should().ContainSingle(t => t.Id == 1);
        }

        [Fact]
        public async Task GetAllTransactions_ShouldApplyOffsetAndLimit()
        {
            await using var context = GetInMemoryDbContext("OffsetLimit");

            context.User.Add(new User { Id = 1, Email = "test@123", Name = "test123" });
            context.Teams.Add(new Team { Id = 1, Name = "test123" });

            for (int i = 1; i <= 5; i++)
            {
                context.Transactions.Add(new PaymentRequestByUser
                {
                    UserId = 1,
                    TeamId = 1,
                    Amount = i * 10,
                    CreatedAt = DateTime.UtcNow.AddMinutes(i),
                    InvoiceNumber = "123"
                });
            }

            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, Mock.Of<IFileRepository>());

            var (result, count) = await repo.GetAllAsync(new Application.Dto.Transaction.GetTransactionQuery
            {
                Offset = 1,
                Limit = 2
            });

            count.Should().Be(5);
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllTransactions_ShouldFilterByCostCentre()
        {
            await using var context = GetInMemoryDbContext("FilterByCostCentre");

            context.User.Add(new User { Id = 1, Email = "test@123", Name = "test123" });
            context.Teams.Add(new Team { Id = 1, Name = "test123" });
            context.Seasons.Add(new Season
            {
                Id = 1,
                Name = "2026",
            });
            context.CostCentres.AddRange(
                new CostCentre { Id = 1, Name = "Engine" },
                new CostCentre { Id = 2, Name = "Chassis" });
            context.Budgets.AddRange(
                new Budget
                {
                    Id = 1,
                    Name = "Engine Budget",
                    TeamId = 1,
                    CostCentreId = 1,
                    SeasonId = 1,
                    TargetAmount = 1000,
                    PeriodStart = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    PeriodEnd = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                },
                new Budget
                {
                    Id = 2,
                    Name = "Chassis Budget",
                    TeamId = 1,
                    CostCentreId = 2,
                    SeasonId = 1,
                    TargetAmount = 1000,
                    PeriodStart = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    PeriodEnd = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                });
            context.Transactions.AddRange(
                new PaymentRequestByUser
                {
                    Id = 1,
                    UserId = 1,
                    TeamId = 1,
                    BudgetId = 1,
                    Amount = 100,
                    InvoiceNumber = "INV-1",
                },
                new PaymentRequestByUser
                {
                    Id = 2,
                    UserId = 1,
                    TeamId = 1,
                    BudgetId = 2,
                    Amount = 200,
                    InvoiceNumber = "INV-2",
                },
                new PaymentRequestByUser
                {
                    Id = 3,
                    UserId = 1,
                    TeamId = 1,
                    BudgetId = null,
                    Amount = 300,
                    InvoiceNumber = "INV-3",
                });
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, Mock.Of<IFileRepository>());

            var (result, count) = await repo.GetAllAsync(new Application.Dto.Transaction.GetTransactionQuery
            {
                CostCentreId = 1,
                IncludeBudget = true,
            });

            count.Should().Be(1);
            result.Should().ContainSingle(t => t.Id == 1);
            result.Single().Budget.Should().NotBeNull();
            result.Single().Budget!.CostCentre.Name.Should().Be("Engine");
        }

        // ----------------------------
        // ADD PaymentRequestByTeam
        // ----------------------------
        [Fact]
        public async Task AddPaymentRequestByTeam_ShouldPersistEntity()
        {
            await using var context = GetInMemoryDbContext("AddPaymentRequestByTeam");

            var repo = new TransactionRepository(context, Mock.Of<IFileRepository>());

            var entity = new PaymentRequestByTeam
            {
                Amount = 500,
                CreatedAt = DateTime.UtcNow,
            };

            var result = await repo.AddAsync(entity);

            result.Should().NotBeNull();
            result.Id.Should().NotBe(0);

            var db = await context.PaymentRequestsByTeam.FindAsync(result.Id);
            db.Should().NotBeNull();
            db!.Amount.Should().Be(500);
        }

        [Fact]
        public async Task AddPaymentRequestByTeam_ShouldThrow_WhenSaveFails()
        {
            var context = new FailingDbContext("FailAddTeam");
            var repo = new TransactionRepository(context, Mock.Of<IFileRepository>());

            var entity = new PaymentRequestByTeam { Amount = 100 };

            async Task act() => await repo.AddAsync(entity);

            var ex = await Assert.ThrowsAsync<InternalErrorException>(act);
            ex.Message.Should().Contain("Transaction");
        }

        // ----------------------------
        // UPDATE PaymentRequestByTeam
        // ----------------------------
        [Fact]
        public async Task UpdatePaymentRequestByTeam_ShouldPersistChanges()
        {
            await using var context = GetInMemoryDbContext("UpdatePaymentRequestByTeam");

            var existing = new PaymentRequestByTeam { Id = 1, Amount = 100 };
            context.PaymentRequestsByTeam.Add(existing);
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, Mock.Of<IFileRepository>());

            existing.Amount = 750;
            var result = await repo.UpdateAsync(existing);

            result.Amount.Should().Be(750);

            var db = await context.PaymentRequestsByTeam.FindAsync(1);
            db!.Amount.Should().Be(750);
        }

        [Fact]
        public async Task UpdatePaymentRequestByTeam_ShouldThrow_WhenSaveFails()
        {
            var context = new FailingDbContext("FailUpdateTeam");
            var repo = new TransactionRepository(context, Mock.Of<IFileRepository>());

            var entity = new PaymentRequestByTeam { Id = 1, Amount = 100 };

            async Task act() => await repo.UpdateAsync(entity);

            var ex = await Assert.ThrowsAsync<InternalErrorException>(act);
            ex.Message.Should().Contain("Transaction");
        }

        // ----------------------------
        // GET ALL PaymentRequestByTeam – remaining filters
        // ----------------------------
        [Fact]
        public async Task GetAllPaymentRequestByTeam_FilterByRequestById_ShouldReturnMatchingEntries()
        {
            await using var context = GetInMemoryDbContext("FilterByRequestById");

            context.User.AddRange(
                new User { Id = 1, Email = "a@a.com", Name = "A" },
                new User { Id = 2, Email = "b@b.com", Name = "B" });
            context.Teams.Add(new Team { Id = 1, Name = "T" });

            context.PaymentRequestsByTeam.AddRange(
                new PaymentRequestByTeam { Id = 1, UserId = 1, TeamId = 1, RequestedById = 1 },
                new PaymentRequestByTeam { Id = 2, UserId = 1, TeamId = 1, RequestedById = 2 },
                new PaymentRequestByTeam { Id = 3, UserId = 1, TeamId = 1, RequestedById = 1 });
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, Mock.Of<IFileRepository>());

            var (results, count) = await repo.GetAllAsync(new GetPaymentRequestByTeamQuery { RequestById = 1 });

            count.Should().Be(2);
            results.Should().OnlyContain(t => t.RequestedById == 1);
        }

        [Fact]
        public async Task GetAllPaymentRequestByTeam_FilterByMaxDueDate_ShouldReturnMatchingEntries()
        {
            await using var context = GetInMemoryDbContext("FilterByMaxDueDate");

            context.User.Add(new User { Id = 1, Email = "test@test.com", Name = "T" });
            context.Teams.Add(new Team { Id = 1, Name = "T" });

            var cutoff = new DateTime(2025, 8, 1, 0, 0, 0, DateTimeKind.Utc);

            context.PaymentRequestsByTeam.AddRange(
                new PaymentRequestByTeam { Id = 1, UserId = 1, TeamId = 1, Amount = 100, DueDate = cutoff.AddDays(-1) },
                new PaymentRequestByTeam { Id = 2, UserId = 1, TeamId = 1, Amount = 200, DueDate = cutoff.AddDays(10) },
                new PaymentRequestByTeam { Id = 3, UserId = 1, TeamId = 1, Amount = 300, DueDate = null });
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, Mock.Of<IFileRepository>());

            var (results, count) = await repo.GetAllAsync(new GetPaymentRequestByTeamQuery { MaxDueDate = cutoff });

            count.Should().Be(1);
            results.Should().ContainSingle(t => t.Id == 1);
        }

        // ----------------------------
        // GET BY ID PaymentRequestByTeam – includes
        // ----------------------------
        [Fact]
        public async Task GetByIdAsyncTeam_ShouldIncludeNavigationProperties_WhenRequested()
        {
            await using var context = GetInMemoryDbContext("GetByIdTeam_Includes");

            context.User.Add(new User { Id = 1, Email = "u@u.com", Name = "User" });
            context.User.Add(new User { Id = 2, Email = "r@r.com", Name = "Requester" });
            context.Teams.Add(new Team { Id = 1, Name = "Team" });
            context.Budgets.Add(new Budget { Id = 1, Name = "CC" });

            context.PaymentRequestsByTeam.Add(new PaymentRequestByTeam
            {
                Id = 5,
                UserId = 1,
                RequestedById = 2,
                TeamId = 1,
                BudgetId = 1,
                Amount = 200,
                CreatedAt = DateTime.UtcNow,
            });
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, Mock.Of<IFileRepository>());

            var result = await repo.GetByIdAsync(5, new GetPaymentRequestByTeamQueryById
            {
                IncludeUser = true,
                IncludeTeam = true,
            });

            result.Should().NotBeNull();
            result!.User.Should().NotBeNull();
            result.User!.Id.Should().Be(1);
            result.Team.Should().NotBeNull();
            result.Team!.Id.Should().Be(1);
            result.Budget.Should().NotBeNull();
            result.Budget!.Id.Should().Be(1);
            result.RequestedBy.Should().NotBeNull();
            result.RequestedBy!.Id.Should().Be(2);
        }
    }
}
