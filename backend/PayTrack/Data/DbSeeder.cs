// <copyright file="DbSeeder.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

/*
 * Disclaimer:
 * This seeder is AI-Generated
 */
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PayTrack.Data.Entities;

namespace PayTrack.Data;

/// <summary>
/// Seeds development data into the database.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DbSeeder
{
    /// <summary>
    /// Adds demo data when it is missing.
    /// </summary>
    /// <param name="db">Application database context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SeedAsync(AppDbContext db)
    {
        var chassisTeam = await db.Teams.FirstOrDefaultAsync(t => t.Name == "Chassis");
        if (chassisTeam is null)
        {
            chassisTeam = new Team
            {
                Name = "Chassis",
                Description = "Responsible for frame, suspension, and mechanical structure.",
                DisplayColor = "#F97316",
            };

            db.Teams.Add(chassisTeam);
        }

        var electronicsTeam = await db.Teams.FirstOrDefaultAsync(t => t.Name == "Electronics");
        if (electronicsTeam is null)
        {
            electronicsTeam = new Team
            {
                Name = "Electronics",
                Description = "Responsible for wiring, sensors, and embedded systems.",
                DisplayColor = "#2563EB",
            };

            db.Teams.Add(electronicsTeam);
        }

        var suspensionTeam = await db.Teams.FirstOrDefaultAsync(t => t.Name == "Suspension");
        if (suspensionTeam is null)
        {
            suspensionTeam = new Team
            {
                Name = "Suspension",
                Description = "Responsible for dampers, kinematics, and handling setup.",
                DisplayColor = "#0EA5E9",
            };

            db.Teams.Add(suspensionTeam);
        }

        var aerodynamicsTeam = await db.Teams.FirstOrDefaultAsync(t => t.Name == "Aerodynamics");
        if (aerodynamicsTeam is null)
        {
            aerodynamicsTeam = new Team
            {
                Name = "Aerodynamics",
                DisplayColor = "#14B8A6",
            };

            db.Teams.Add(aerodynamicsTeam);
        }

        var powertrainTeam = await db.Teams.FirstOrDefaultAsync(t => t.Name == "Powertrain");
        if (powertrainTeam is null)
        {
            powertrainTeam = new Team
            {
                Name = "Powertrain",
                Description = "Responsible for drivetrain, cooling, and propulsion systems.",
                DisplayColor = "#DC2626",
            };

            db.Teams.Add(powertrainTeam);
        }

        var batteryTeam = await db.Teams.FirstOrDefaultAsync(t => t.Name == "Battery");
        if (batteryTeam is null)
        {
            batteryTeam = new Team
            {
                Name = "Battery",
                Description = "Responsible for accumulator, cells, and high-voltage safety.",
                DisplayColor = "#EAB308",
            };

            db.Teams.Add(batteryTeam);
        }

        var softwareTeam = await db.Teams.FirstOrDefaultAsync(t => t.Name == "Software");
        if (softwareTeam is null)
        {
            softwareTeam = new Team
            {
                Name = "Software",
                Description = "Responsible for telemetry, tools, and vehicle software.",
                DisplayColor = "#6366F1",
            };

            db.Teams.Add(softwareTeam);
        }

        var financeTeam = await db.Teams.FirstOrDefaultAsync(t => t.Name == "Finance");
        if (financeTeam is null)
        {
            financeTeam = new Team
            {
                Name = "Finance",
                Description = "Responsible for budget planning, payments, and controlling.",
                DisplayColor = "#22C55E",
            };

            db.Teams.Add(financeTeam);
        }

        var operationsTeam = await db.Teams.FirstOrDefaultAsync(t => t.Name == "Operations");
        if (operationsTeam is null)
        {
            operationsTeam = new Team
            {
                Name = "Operations",
                Description = "Responsible for logistics, events, and workshop coordination.",
                DisplayColor = "#64748B",
            };

            db.Teams.Add(operationsTeam);
        }

        var marketingTeam = await db.Teams.FirstOrDefaultAsync(t => t.Name == "Marketing");
        if (marketingTeam is null)
        {
            marketingTeam = new Team
            {
                Name = "Marketing",
                Description = "Responsible for sponsors, media, and public communication.",
                DisplayColor = "#EC4899",
            };

            db.Teams.Add(marketingTeam);
        }

        var driverlessTeam = await db.Teams.FirstOrDefaultAsync(t => t.Name == "Driverless");
        if (driverlessTeam is null)
        {
            driverlessTeam = new Team
            {
                Name = "Driverless",
                Description = "Responsible for perception, planning, and autonomous driving.",
                DisplayColor = "#8B5CF6",
            };

            db.Teams.Add(driverlessTeam);
        }

        var legacyCombustionTeam = await db.Teams.FirstOrDefaultAsync(t => t.Name == "Legacy Combustion");
        if (legacyCombustionTeam is null)
        {
            legacyCombustionTeam = new Team
            {
                Name = "Legacy Combustion",
                Description = "Inactive team kept for old combustion-era accounting records.",
                DisplayColor = "#92400E",
                IsActive = false,
            };

            db.Teams.Add(legacyCombustionTeam);
        }

        var archivedKartingTeam = await db.Teams.FirstOrDefaultAsync(t => t.Name == "Archived Karting");
        if (archivedKartingTeam is null)
        {
            archivedKartingTeam = new Team
            {
                Name = "Archived Karting",
                Description = "Inactive team without current members or budgets.",
                DisplayColor = "#475569",
                IsActive = false,
            };

            db.Teams.Add(archivedKartingTeam);
        }

        var currentSeason = await db.Seasons.FirstOrDefaultAsync(c => c.Name == "S25/26");
        if (currentSeason is null)
        {
            currentSeason = new Season
            {
                Name = "S25/26",
            };

            db.Seasons.Add(currentSeason);
        }

        var manufacturingBudget = await db.Budgets.FirstOrDefaultAsync(c => c.Name == "Manufacturing");
        if (manufacturingBudget is null)
        {
            manufacturingBudget = new Budget
            {
                Name = "Manufacturing",
                Description = "Parts, machining, and production costs.",
                SeasonId = currentSeason.Id,
                Season = currentSeason,
            };

            db.Budgets.Add(manufacturingBudget);
        }

        var electronicsBudget = await db.Budgets.FirstOrDefaultAsync(c => c.Name == "Electronics");
        if (electronicsBudget is null)
        {
            electronicsBudget = new Budget
            {
                Name = "Electronics",
                Description = "Electrical components and prototype hardware.",
                SeasonId = currentSeason.Id,
                Season = currentSeason,
            };

            db.Budgets.Add(electronicsBudget);
        }

        var compositesBudget = await db.Budgets.FirstOrDefaultAsync(c => c.Name == "Composites");
        if (compositesBudget is null)
        {
            compositesBudget = new Budget
            {
                Name = "Composites",
                Description = "#0EA5E9",
                SeasonId = currentSeason.Id,
                Season = currentSeason,
            };

            db.Budgets.Add(compositesBudget);
        }

        var manufacturingCostCentre = await db.CostCentres.FirstOrDefaultAsync(c => c.Name == "Manufacturing");
        if (manufacturingCostCentre is null)
        {
            manufacturingCostCentre = new CostCentre
            {
                Name = "Manufacturing",
                Description = "Parts, machining, and production costs.",
                DisplayColor = "#16A34A",
                IsActive = true,
            };

            db.CostCentres.Add(manufacturingCostCentre);
        }

        var electronicsCostCentre = await db.CostCentres.FirstOrDefaultAsync(c => c.Name == "Electronics");
        if (electronicsCostCentre is null)
        {
            electronicsCostCentre = new CostCentre
            {
                Name = "Electronics",
                Description = "Electrical components and prototype hardware.",
                DisplayColor = "#7C3AED",
                IsActive = true,
            };

            db.CostCentres.Add(electronicsCostCentre);
        }

        var compositesCostCentre = await db.CostCentres.FirstOrDefaultAsync(c => c.Name == "Composites");
        if (compositesCostCentre is null)
        {
            compositesCostCentre = new CostCentre
            {
                Name = "Composites",
                DisplayColor = "#0EA5E9",
                IsActive = true,
            };

            db.CostCentres.Add(compositesCostCentre);
        }

        var legacyToolingCostCentre = await db.CostCentres.FirstOrDefaultAsync(c => c.Name == "Legacy Tooling");
        if (legacyToolingCostCentre is null)
        {
            legacyToolingCostCentre = new CostCentre
            {
                Name = "Legacy Tooling",
                Description = "Deprecated workshop tools and retired production fixtures.",
                DisplayColor = "#78716C",
                IsActive = false,
            };

            db.CostCentres.Add(legacyToolingCostCentre);
        }

        var oldAccumulatorCostCentre = await db.CostCentres.FirstOrDefaultAsync(c => c.Name == "Old Accumulator Program");
        if (oldAccumulatorCostCentre is null)
        {
            oldAccumulatorCostCentre = new CostCentre
            {
                Name = "Old Accumulator Program",
                Description = "Closed high-voltage accumulator development budget.",
                DisplayColor = "#B91C1C",
                IsActive = false,
            };

            db.CostCentres.Add(oldAccumulatorCostCentre);
        }

        var archivedSponsoringCostCentre = await db.CostCentres.FirstOrDefaultAsync(c => c.Name == "Archived Sponsoring");
        if (archivedSponsoringCostCentre is null)
        {
            archivedSponsoringCostCentre = new CostCentre
            {
                Name = "Archived Sponsoring",
                DisplayColor = "#A855F7",
                IsActive = false,
            };

            db.CostCentres.Add(archivedSponsoringCostCentre);
        }

        await AddBudgetIfMissingAsync(db, chassisTeam, manufacturingCostCentre, 15000m);
        await AddBudgetIfMissingAsync(db, electronicsTeam, electronicsCostCentre, 8000m);
        await AddBudgetIfMissingAsync(db, suspensionTeam, compositesCostCentre, 9500m);
        await AddBudgetIfMissingAsync(db, aerodynamicsTeam, compositesCostCentre, 18000m);
        await AddBudgetIfMissingAsync(db, powertrainTeam, manufacturingCostCentre, 22000m);
        await AddBudgetIfMissingAsync(db, batteryTeam, oldAccumulatorCostCentre, 14000m);
        await AddBudgetIfMissingAsync(db, softwareTeam, electronicsCostCentre, 7000m);
        await AddBudgetIfMissingAsync(db, financeTeam, legacyToolingCostCentre, 3000m);
        await AddBudgetIfMissingAsync(db, operationsTeam, manufacturingCostCentre, 6500m);
        await AddBudgetIfMissingAsync(db, marketingTeam, electronicsCostCentre, 4500m);
        await AddBudgetIfMissingAsync(db, legacyCombustionTeam, legacyToolingCostCentre, 1200m);

        await AddPresenterInvoicesIfUserExistsAsync(
            db,
            chassisTeam,
            electronicsTeam,
            suspensionTeam,
            operationsTeam);

        await AddPresenterTeamRequestsIfUserExistsAsync(
            db,
            chassisTeam,
            electronicsTeam,
            suspensionTeam,
            batteryTeam,
            powertrainTeam,
            operationsTeam,
            manufacturingBudget,
            electronicsBudget,
            compositesBudget);

        await db.SaveChangesAsync();
    }

    private static async Task AddBudgetIfMissingAsync(
        AppDbContext db,
        Team team,
        CostCentre costCentre,
        decimal targetAmount)
    {
        var budgetStart = new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var budgetEnd = new DateTime(DateTime.UtcNow.Year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        if (await db.Budgets.AnyAsync(b =>
                b.Team == team &&
                b.CostCentre == costCentre &&
                b.PeriodStart == budgetStart &&
                b.PeriodEnd == budgetEnd))
        {
            return;
        }

        db.Budgets.Add(new Budget
        {
            Team = team,
            CostCentre = costCentre,
            TargetAmount = targetAmount,
            PeriodStart = budgetStart,
            PeriodEnd = budgetEnd,
        });
    }

    private static async Task AddPresenterInvoicesIfUserExistsAsync(
        AppDbContext db,
        Team chassisTeam,
        Team electronicsTeam,
        Team suspensionTeam,
        Team operationsTeam)
    {
        var presenterUser = await db.User
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync(u => u.Email.Contains("gmail"));

        if (presenterUser is null)
        {
            return;
        }

        var presenterBankAccount = await GetOrCreatePresenterBankAccountAsync(db, presenterUser);

        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-PRES-SELF-PDF-001",
            presenterUser,
            presenterBankAccount,
            chassisTeam,
            248.80m,
            "Workshop fasteners and carbon repair consumables",
            "Self-paid reimbursement for workshop material.",
            "uploads/presentation-invoices/invoice-consulting-2026.pdf",
            PayoutType.User,
            TransactionStatus.Approved,
            2);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-PRES-SUPPLIER-PNG-001",
            presenterUser,
            null,
            electronicsTeam,
            736.42m,
            "Sensor connectors from electronics supplier",
            "External supplier should be paid directly.",
            "uploads/presentation-invoices/invoice-techstore-2026.png",
            PayoutType.External,
            TransactionStatus.Paid,
            4);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-PRES-SELF-JPG-001",
            presenterUser,
            presenterBankAccount,
            suspensionTeam,
            119.95m,
            "Hotel booking for supplier visit",
            "Self-paid travel expense for project coordination.",
            "uploads/presentation-invoices/invoice-hotel-booking-2026.jpg",
            PayoutType.User,
            TransactionStatus.ChangesRequested,
            6);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-PRES-SUPPLIER-PDF-002",
            presenterUser,
            null,
            operationsTeam,
            1580.00m,
            "Workshop machine service invoice",
            "External workshop invoice submitted for finance processing.",
            "uploads/presentation-invoices/invoice-consulting-2026.pdf",
            PayoutType.External,
            TransactionStatus.Declined,
            8);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-PRES-SUBMITTED-PNG-001",
            presenterUser,
            null,
            electronicsTeam,
            87.30m,
            "Prototype cable labels",
            "Freshly submitted and waiting for finance review.",
            "uploads/presentation-invoices/invoice-techstore-2026.png",
            PayoutType.External,
            TransactionStatus.Submitted,
            1);
    }

    private static async Task<BankAccount> GetOrCreatePresenterBankAccountAsync(AppDbContext db, User presenterUser)
    {
        const string presenterIban = "AT611904300234573299";
        var bankAccount = await db.BankAccounts.FirstOrDefaultAsync(b =>
            b.User == presenterUser &&
            b.Iban == presenterIban);

        if (bankAccount is not null)
        {
            return bankAccount;
        }

        bankAccount = new BankAccount
        {
            User = presenterUser,
            Iban = presenterIban,
            Bic = "BKAUATWW",
            AccountHolder = presenterUser.Name,
        };

        db.BankAccounts.Add(bankAccount);
        return bankAccount;
    }

    private static async Task AddPresenterInvoiceIfMissingAsync(
        AppDbContext db,
        string invoiceNumber,
        User presenterUser,
        BankAccount? bankAccount,
        Team team,
        decimal amount,
        string purposeOfPayment,
        string comment,
        string receiptUrl,
        PayoutType payoutType,
        TransactionStatus status,
        int createdDaysAgo)
    {
        var paidAt = DateTime.UtcNow.AddDays(-createdDaysAgo).ToUniversalTime();
        var existingPaymentRequest = await db.PaymentRequestsByUser.FirstOrDefaultAsync(p => p.InvoiceNumber == invoiceNumber);

        if (existingPaymentRequest is not null)
        {
            existingPaymentRequest.User = presenterUser;
            existingPaymentRequest.Team = team;
            existingPaymentRequest.Budget = null!;
            existingPaymentRequest.BudgetId = null;
            existingPaymentRequest.Amount = amount;
            existingPaymentRequest.PurposeOfPayment = purposeOfPayment;
            existingPaymentRequest.PaymentReference = string.Empty;
            existingPaymentRequest.PaymentDirection = PaymentDirection.Out;
            existingPaymentRequest.Status = status;
            existingPaymentRequest.PaidAt = paidAt;
            existingPaymentRequest.InvoiceNumber = invoiceNumber;
            existingPaymentRequest.Comment = comment;
            existingPaymentRequest.ReceiptUrl = receiptUrl;
            existingPaymentRequest.PayoutType = payoutType;
            existingPaymentRequest.BankAccount = payoutType == PayoutType.User ? bankAccount : null;

            var existingStatusHistory = await db.TransactionStatusHistories
                .Where(h => h.TransactionId == existingPaymentRequest.Id)
                .ToListAsync();
            db.TransactionStatusHistories.RemoveRange(existingStatusHistory);
            AddPresenterStatusHistoryIfNeeded(db, presenterUser, existingPaymentRequest, status);
            return;
        }

        var paymentRequest = new PaymentRequestByUser
        {
            User = presenterUser,
            Team = team,
            Amount = amount,
            PurposeOfPayment = purposeOfPayment,
            PaymentReference = string.Empty,
            PaymentDirection = PaymentDirection.Out,
            Status = status,
            PaidAt = paidAt,
            InvoiceNumber = invoiceNumber,
            Comment = comment,
            ReceiptUrl = receiptUrl,
            PayoutType = payoutType,
            BankAccount = payoutType == PayoutType.User ? bankAccount : null,
        };

        db.PaymentRequestsByUser.Add(paymentRequest);
        AddPresenterStatusHistoryIfNeeded(db, presenterUser, paymentRequest, status);
    }

    private static void AddPresenterStatusHistoryIfNeeded(
        AppDbContext db,
        User presenterUser,
        PaymentRequestByUser paymentRequest,
        TransactionStatus status)
    {
        if (status == TransactionStatus.Submitted)
        {
            return;
        }

        if (status == TransactionStatus.Paid)
        {
            db.TransactionStatusHistories.Add(new TransactionStatusHistory
            {
                Transaction = paymentRequest,
                ChangedBy = presenterUser,
                FromStatus = TransactionStatus.Submitted,
                ToStatus = TransactionStatus.Approved,
                Comment = "Approved during presentation setup.",
                ChangedAt = paymentRequest.PaidAt?.AddDays(1) ?? DateTime.UtcNow,
            });
            db.TransactionStatusHistories.Add(new TransactionStatusHistory
            {
                Transaction = paymentRequest,
                ChangedBy = presenterUser,
                FromStatus = TransactionStatus.Approved,
                ToStatus = TransactionStatus.Paid,
                Comment = "Marked as paid during presentation setup.",
                ChangedAt = paymentRequest.PaidAt?.AddDays(2) ?? DateTime.UtcNow,
            });
            return;
        }

        db.TransactionStatusHistories.Add(new TransactionStatusHistory
        {
            Transaction = paymentRequest,
            ChangedBy = presenterUser,
            FromStatus = TransactionStatus.Submitted,
            ToStatus = status,
            Comment = $"Moved to {status} during presentation setup.",
            ChangedAt = paymentRequest.PaidAt?.AddDays(1) ?? DateTime.UtcNow,
        });
    }

    private static async Task AddPresenterTeamRequestsIfUserExistsAsync(
        AppDbContext db,
        Team chassisTeam,
        Team electronicsTeam,
        Team suspensionTeam,
        Team batteryTeam,
        Team powertrainTeam,
        Team operationsTeam,
        Budget manufacturingBudget,
        Budget electronicsBudget,
        Budget compositesBudget)
    {
        var presenterUser = await db.User
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync(u => u.Email.Contains("gmail"));

        if (presenterUser is null)
        {
            return;
        }

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            chassisTeam,
            manufacturingBudget,
            150.00m,
            "Workshop tool deposit – spring season",
            TransactionStatus.Submitted,
            DateTime.UtcNow.AddDays(30),
            5);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            electronicsTeam,
            electronicsBudget,
            320.50m,
            "CAN bus hardware contribution",
            TransactionStatus.Submitted,
            DateTime.UtcNow.AddDays(10),
            10);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            suspensionTeam,
            compositesBudget,
            89.00m,
            "Damper test rig maintenance fee",
            TransactionStatus.Submitted,
            DateTime.UtcNow.AddDays(-5),
            14);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            powertrainTeam,
            manufacturingBudget,
            2800.00m,
            "Engine testbench booking – Q2",
            TransactionStatus.Paid,
            null,
            20);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            operationsTeam,
            null,
            45.00m,
            "Event transport cost share – FSAE Austria",
            TransactionStatus.Paid,
            DateTime.UtcNow.AddDays(-15),
            18);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            batteryTeam,
            electronicsBudget,
            560.00m,
            "High-voltage safety training fee",
            TransactionStatus.Submitted,
            DateTime.UtcNow.AddDays(45),
            2);
    }

    private static async Task AddTeamRequestIfMissingAsync(
        AppDbContext db,
        User requestedBy,
        User targetUser,
        Team team,
        Budget? budget,
        decimal amount,
        string purposeOfPayment,
        TransactionStatus status,
        DateTime? dueDate,
        int createdDaysAgo)
    {
        var createdAt = DateTime.UtcNow.AddDays(-createdDaysAgo).ToUniversalTime();
        var paidAt = status == TransactionStatus.Paid
            ? createdAt.AddDays(3)
            : (DateTime?)null;

        var existingRequest = await db.PaymentRequestsByTeam
            .FirstOrDefaultAsync(r => r.PurposeOfPayment == purposeOfPayment);

        if (existingRequest is not null)
        {
            existingRequest.RequestedBy = requestedBy;
            existingRequest.User = targetUser;
            existingRequest.Team = team;
            existingRequest.Amount = amount;
            existingRequest.PurposeOfPayment = purposeOfPayment;
            existingRequest.PaymentDirection = PaymentDirection.In;
            existingRequest.Status = status;
            existingRequest.DueDate = dueDate;
            existingRequest.PaidAt = paidAt;

            if (budget is null)
            {
                existingRequest.Budget = null!;
                existingRequest.BudgetId = null;
            }
            else
            {
                existingRequest.Budget = budget;
            }

            var existingHistory = await db.TransactionStatusHistories
                .Where(h => h.TransactionId == existingRequest.Id)
                .ToListAsync();
            db.TransactionStatusHistories.RemoveRange(existingHistory);
            AddTeamRequestStatusHistoryIfNeeded(db, requestedBy, existingRequest, status, createdAt);
            return;
        }

        var teamRequest = new PaymentRequestByTeam
        {
            RequestedBy = requestedBy,
            User = targetUser,
            Team = team,
            Amount = amount,
            PurposeOfPayment = purposeOfPayment,
            PaymentReference = string.Empty,
            PaymentDirection = PaymentDirection.In,
            Status = status,
            CreatedAt = createdAt,
            DueDate = dueDate,
            PaidAt = paidAt,
        };

        if (budget is not null)
        {
            teamRequest.Budget = budget;
        }

        db.PaymentRequestsByTeam.Add(teamRequest);
        AddTeamRequestStatusHistoryIfNeeded(db, requestedBy, teamRequest, status, createdAt);
    }

    private static void AddTeamRequestStatusHistoryIfNeeded(
        AppDbContext db,
        User changedBy,
        PaymentRequestByTeam teamRequest,
        TransactionStatus status,
        DateTime createdAt)
    {
        if (status == TransactionStatus.Submitted)
        {
            return;
        }

        if (status == TransactionStatus.Paid)
        {
            db.TransactionStatusHistories.Add(new TransactionStatusHistory
            {
                Transaction = teamRequest,
                ChangedBy = changedBy,
                FromStatus = TransactionStatus.Submitted,
                ToStatus = TransactionStatus.Approved,
                Comment = "Approved during presentation setup.",
                ChangedAt = createdAt.AddDays(1),
            });
            db.TransactionStatusHistories.Add(new TransactionStatusHistory
            {
                Transaction = teamRequest,
                ChangedBy = changedBy,
                FromStatus = TransactionStatus.Approved,
                ToStatus = TransactionStatus.Paid,
                Comment = "Marked as paid during presentation setup.",
                ChangedAt = createdAt.AddDays(2),
            });
            return;
        }

        db.TransactionStatusHistories.Add(new TransactionStatusHistory
        {
            Transaction = teamRequest,
            ChangedBy = changedBy,
            FromStatus = TransactionStatus.Submitted,
            ToStatus = status,
            Comment = $"Moved to {status} during presentation setup.",
            ChangedAt = createdAt.AddDays(1),
        });
    }
}
