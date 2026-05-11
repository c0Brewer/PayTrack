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
            operationsTeam,
            manufacturingCostCentre,
            electronicsCostCentre,
            compositesCostCentre);

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
        Team operationsTeam,
        CostCentre manufacturingCostCentre,
        CostCentre electronicsCostCentre,
        CostCentre compositesCostCentre)
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
            manufacturingCostCentre,
            248.80m,
            "Workshop fasteners and carbon repair consumables",
            "Self-paid reimbursement for workshop material.",
            "uploads/presentation-invoices/invoice-consulting-2026.pdf",
            PayoutType.User,
            TransactionStatus.Submitted,
            2);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-PRES-SUPPLIER-PNG-001",
            presenterUser,
            null,
            electronicsTeam,
            electronicsCostCentre,
            736.42m,
            "Sensor connectors from electronics supplier",
            "External supplier should be paid directly.",
            "uploads/presentation-invoices/invoice-techstore-2026.png",
            PayoutType.External,
            TransactionStatus.Submitted,
            4);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-PRES-SELF-JPG-001",
            presenterUser,
            presenterBankAccount,
            suspensionTeam,
            compositesCostCentre,
            119.95m,
            "Hotel booking for supplier visit",
            "Self-paid travel expense for project coordination.",
            "uploads/presentation-invoices/invoice-hotel-booking-2026.jpg",
            PayoutType.User,
            TransactionStatus.Submitted,
            6);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-PRES-SUPPLIER-PDF-002",
            presenterUser,
            null,
            operationsTeam,
            manufacturingCostCentre,
            1580.00m,
            "Workshop machine service invoice",
            "External workshop invoice submitted for finance processing.",
            "uploads/presentation-invoices/invoice-consulting-2026.pdf",
            PayoutType.External,
            TransactionStatus.Submitted,
            8);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-PRES-SUBMITTED-PNG-001",
            presenterUser,
            null,
            electronicsTeam,
            compositesCostCentre,
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
        CostCentre costCentre,
        decimal amount,
        string purposeOfPayment,
        string comment,
        string receiptUrl,
        PayoutType payoutType,
        TransactionStatus status,
        int createdDaysAgo)
    {
        if (await db.PaymentRequestsByUser.AnyAsync(p => p.InvoiceNumber == invoiceNumber))
        {
            return;
        }

        var paymentRequest = new PaymentRequestByUser
        {
            User = presenterUser,
            Team = team,
            CostCentre = costCentre,
            Amount = amount,
            PurposeOfPayment = purposeOfPayment,
            PaymentReference = string.Empty,
            PaymentDirection = PaymentDirection.Out,
            Status = status,
            CreatedAt = DateTime.UtcNow.AddDays(-createdDaysAgo),
            PaidAt = null,
            InvoiceNumber = invoiceNumber,
            Comment = comment,
            ReceiptUrl = receiptUrl,
            PayoutType = payoutType,
            BankAccount = payoutType == PayoutType.User ? bankAccount : null,
        };

        db.PaymentRequestsByUser.Add(paymentRequest);
    }
}
