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

        var adminUser = await db.User.FirstOrDefaultAsync(u => u.Email == "admin@paytrack.local");
        if (adminUser is null)
        {
            adminUser = new User
            {
                Name = "Admin User",
                Email = "admin@paytrack.local",
                Role = Role.Admin,
                Team = chassisTeam,
                IsActive = true,
            };

            db.User.Add(adminUser);
        }

        var financeAdminUser = await db.User.FirstOrDefaultAsync(u => u.Email == "finance.admin@paytrack.local");
        if (financeAdminUser is null)
        {
            financeAdminUser = new User
            {
                Name = "Finance Admin",
                Email = "finance.admin@paytrack.local",
                Role = Role.Admin,
                Team = financeTeam,
                IsActive = true,
            };

            db.User.Add(financeAdminUser);
        }

        var teamLeadUser = await db.User.FirstOrDefaultAsync(u => u.Email == "lead@paytrack.local");
        if (teamLeadUser is null)
        {
            teamLeadUser = new User
            {
                Name = "Team Lead",
                Email = "lead@paytrack.local",
                Role = Role.TeamLead,
                Team = electronicsTeam,
                IsActive = true,
            };

            db.User.Add(teamLeadUser);
        }

        var chassisLeadUser = await db.User.FirstOrDefaultAsync(u => u.Email == "chassis.lead@paytrack.local");
        if (chassisLeadUser is null)
        {
            chassisLeadUser = new User
            {
                Name = "Chassis Lead",
                Email = "chassis.lead@paytrack.local",
                Role = Role.TeamLead,
                Team = chassisTeam,
                IsActive = true,
            };

            db.User.Add(chassisLeadUser);
        }

        var chassisMemberUser = await db.User.FirstOrDefaultAsync(u => u.Email == "chassis.member@paytrack.local");
        if (chassisMemberUser is null)
        {
            chassisMemberUser = new User
            {
                Name = "Chassis Member",
                Email = "chassis.member@paytrack.local",
                Role = Role.RegularUser,
                Team = chassisTeam,
                IsActive = true,
            };

            db.User.Add(chassisMemberUser);
        }

        var electronicsMemberUser = await db.User.FirstOrDefaultAsync(u => u.Email == "electronics.member@paytrack.local");
        if (electronicsMemberUser is null)
        {
            electronicsMemberUser = new User
            {
                Name = "Electronics Member",
                Email = "electronics.member@paytrack.local",
                Role = Role.RegularUser,
                Team = electronicsTeam,
                IsActive = true,
            };

            db.User.Add(electronicsMemberUser);
        }

        var suspensionMemberUser = await db.User.FirstOrDefaultAsync(u => u.Email == "suspension.member@paytrack.local");
        if (suspensionMemberUser is null)
        {
            suspensionMemberUser = new User
            {
                Name = "Suspension Member",
                Email = "suspension.member@paytrack.local",
                Role = Role.RegularUser,
                Team = suspensionTeam,
                IsActive = true,
            };

            db.User.Add(suspensionMemberUser);
        }

        var aeroMemberUser = await db.User.FirstOrDefaultAsync(u => u.Email == "aero.member@paytrack.local");
        if (aeroMemberUser is null)
        {
            aeroMemberUser = new User
            {
                Name = "Aero Member",
                Email = "aero.member@paytrack.local",
                Role = Role.RegularUser,
                Team = aerodynamicsTeam,
                IsActive = true,
            };

            db.User.Add(aeroMemberUser);
        }

        var embeddedMemberUser = await db.User.FirstOrDefaultAsync(u => u.Email == "embedded.member@paytrack.local");
        if (embeddedMemberUser is null)
        {
            embeddedMemberUser = new User
            {
                Name = "Embedded Member",
                Email = "embedded.member@paytrack.local",
                Role = Role.RegularUser,
                Team = softwareTeam,
                IsActive = true,
            };

            db.User.Add(embeddedMemberUser);
        }

        var batteryMemberUser = await db.User.FirstOrDefaultAsync(u => u.Email == "battery.member@paytrack.local");
        if (batteryMemberUser is null)
        {
            batteryMemberUser = new User
            {
                Name = "Battery Member",
                Email = "battery.member@paytrack.local",
                Role = Role.RegularUser,
                Team = batteryTeam,
                IsActive = true,
            };

            db.User.Add(batteryMemberUser);
        }

        var inactiveUser = await db.User.FirstOrDefaultAsync(u => u.Email == "inactive@paytrack.local");
        if (inactiveUser is null)
        {
            inactiveUser = new User
            {
                Name = "Inactive User",
                Email = "inactive@paytrack.local",
                Role = Role.RegularUser,
                Team = chassisTeam,
                IsActive = false,
            };

            db.User.Add(inactiveUser);
        }

        var unassignedUser = await db.User.FirstOrDefaultAsync(u => u.Email == "unassigned@paytrack.local");
        if (unassignedUser is null)
        {
            unassignedUser = new User
            {
                Name = "Unassigned User",
                Email = "unassigned@paytrack.local",
                Role = Role.RegularUser,
                IsActive = true,
            };

            db.User.Add(unassignedUser);
        }

        var e2eFirstLoginUser = await db.User.FirstOrDefaultAsync(u => u.Email == "e2e.first-login@paytrack.local");
        if (e2eFirstLoginUser is null)
        {
            e2eFirstLoginUser = new User
            {
                Name = "E2E First Login User",
                Email = "e2e.first-login@paytrack.local",
                Role = Role.RegularUser,
                IsActive = true,
            };

            db.User.Add(e2eFirstLoginUser);
        }

        var e2eSkipBankInformationUserEmails = new[]
        {
            "e2e.skip-bank-information-chromium@paytrack.local",
            "e2e.skip-bank-information-firefox@paytrack.local",
            "e2e.skip-bank-information-webkit@paytrack.local",
        };

        foreach (var email in e2eSkipBankInformationUserEmails)
        {
            var e2eSkipBankInformationUser = await db.User.FirstOrDefaultAsync(u => u.Email == email);
            if (e2eSkipBankInformationUser is null)
            {
                e2eSkipBankInformationUser = new User
                {
                    Name = "E2E Skip Bank Information User",
                    Email = email,
                    Role = Role.RegularUser,
                    IsActive = true,
                };

                db.User.Add(e2eSkipBankInformationUser);
            }
        }

        // Duplicate-name pair — used to test ambiguous CSV matching.
        var alexTaylor1 = await db.User.FirstOrDefaultAsync(u => u.Email == "alex.taylor@paytrack.local");
        if (alexTaylor1 is null)
        {
            alexTaylor1 = new User
            {
                Name = "Alex Taylor",
                Email = "alex.taylor@paytrack.local",
                Role = Role.RegularUser,
                Team = powertrainTeam,
                IsActive = true,
            };

            db.User.Add(alexTaylor1);
        }

        var alexTaylor2 = await db.User.FirstOrDefaultAsync(u => u.Email == "alex.taylor2@paytrack.local");
        if (alexTaylor2 is null)
        {
            alexTaylor2 = new User
            {
                Name = "Alex Taylor",
                Email = "alex.taylor2@paytrack.local",
                Role = Role.RegularUser,
                Team = driverlessTeam,
                IsActive = true,
            };

            db.User.Add(alexTaylor2);
        }

        await db.SaveChangesAsync();

        if (!await db.BankAccounts.AnyAsync(b => b.User == adminUser && b.Iban == "AT611904300234573201"))
        {
            db.BankAccounts.Add(new BankAccount
            {
                User = adminUser,
                Iban = "AT611904300234573201",
                Bic = "BKAUATWW",
                AccountHolder = "Admin User",
            });
        }

        if (!await db.BankAccounts.AnyAsync(b => b.User == teamLeadUser && b.Iban == "AT483200000012345864"))
        {
            db.BankAccounts.Add(new BankAccount
            {
                User = teamLeadUser,
                Iban = "AT483200000012345864",
                Bic = "RLNWATWW",
                AccountHolder = "Team Lead",
            });
        }

        if (!await db.BankAccounts.AnyAsync(b => b.User == chassisMemberUser && b.Iban == "AT026000000012345678"))
        {
            db.BankAccounts.Add(new BankAccount
            {
                User = chassisMemberUser,
                Iban = "AT026000000012345678",
                Bic = "OPSKATWW",
                AccountHolder = "Chassis Member",
            });
        }

        if (!await db.BankAccounts.AnyAsync(b => b.User == electronicsMemberUser && b.Iban == "AT611904300234573202"))
        {
            db.BankAccounts.Add(new BankAccount
            {
                User = electronicsMemberUser,
                Iban = "AT611904300234573202",
                Bic = "BKAUATWW",
                AccountHolder = "Electronics Member",
            });
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

        await AddBudgetIfMissingAsync("Electric tools", db, chassisTeam, manufacturingCostCentre, 15000m, currentSeason);
        await AddBudgetIfMissingAsync("Cables", db, electronicsTeam, electronicsCostCentre, 8000m, currentSeason);
        await AddBudgetIfMissingAsync("Screws", db, suspensionTeam, compositesCostCentre, 9500m, currentSeason);
        await AddBudgetIfMissingAsync("Wheels", db, aerodynamicsTeam, compositesCostCentre, 18000m, currentSeason);
        await AddBudgetIfMissingAsync("Rearwing Parts", db, powertrainTeam, manufacturingCostCentre, 22000m, currentSeason);
        await AddBudgetIfMissingAsync("Engine Parts", db, batteryTeam, oldAccumulatorCostCentre, 14000m, currentSeason);
        await AddBudgetIfMissingAsync("Screw Driver", db, softwareTeam, electronicsCostCentre, 7000m, currentSeason);
        await AddBudgetIfMissingAsync("Tools", db, financeTeam, legacyToolingCostCentre, 3000m, currentSeason);
        await AddBudgetIfMissingAsync("Combustion Parts", db, operationsTeam, manufacturingCostCentre, 6500m, currentSeason);
        await AddBudgetIfMissingAsync("Charger", db, marketingTeam, electronicsCostCentre, 4500m, currentSeason);
        await AddBudgetIfMissingAsync("Iron Parts", db, legacyCombustionTeam, legacyToolingCostCentre, 1200m, currentSeason);

        var sponsoringCostCentre = await db.CostCentres.FirstOrDefaultAsync(c => c.Name == "Sponsoring");
        if (sponsoringCostCentre is null)
        {
            sponsoringCostCentre = new CostCentre
            {
                Name = "Sponsoring",
                Description = "Sponsorship income and partner contributions.",
                DisplayColor = "#F59E0B",
                IsActive = true,
            };

            db.CostCentres.Add(sponsoringCostCentre);
        }

        var chassisIncomeBudget = await AddBudgetIfMissingAsync("Sponsor Revenue", db, chassisTeam, sponsoringCostCentre, null, currentSeason, BudgetType.Income);
        var electronicsIncomeBudget = await AddBudgetIfMissingAsync("Sponsor Revenue", db, electronicsTeam, sponsoringCostCentre, null, currentSeason, BudgetType.Income);
        var suspensionIncomeBudget = await AddBudgetIfMissingAsync("Sponsor Revenue", db, suspensionTeam, sponsoringCostCentre, null, currentSeason, BudgetType.Income);
        var powertrainIncomeBudget = await AddBudgetIfMissingAsync("Sponsor Revenue", db, powertrainTeam, sponsoringCostCentre, null, currentSeason, BudgetType.Income);
        var batteryIncomeBudget = await AddBudgetIfMissingAsync("Sponsor Revenue", db, batteryTeam, sponsoringCostCentre, null, currentSeason, BudgetType.Income);
        var operationsIncomeBudget = await AddBudgetIfMissingAsync("Sponsor Revenue", db, operationsTeam, sponsoringCostCentre, null, currentSeason, BudgetType.Income);

        await AddPresenterInvoicesIfUserExistsAsync(
            db,
            chassisTeam,
            electronicsTeam,
            suspensionTeam,
            operationsTeam,
            powertrainTeam,
            batteryTeam,
            aerodynamicsTeam);

        await AddPresenterTeamRequestsIfUserExistsAsync(
            db,
            chassisTeam,
            electronicsTeam,
            suspensionTeam,
            batteryTeam,
            powertrainTeam,
            operationsTeam,
            chassisIncomeBudget,
            electronicsIncomeBudget,
            suspensionIncomeBudget,
            powertrainIncomeBudget,
            batteryIncomeBudget,
            operationsIncomeBudget,
            aerodynamicsTeam,
            softwareTeam,
            marketingTeam);

        await db.SaveChangesAsync();
    }

    private static async Task<Budget> AddBudgetIfMissingAsync(
        string name,
        AppDbContext db,
        Team team,
        CostCentre costCentre,
        decimal? targetAmount,
        Season season,
        BudgetType type = BudgetType.Expense)
    {
        var budgetStart = new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var budgetEnd = new DateTime(DateTime.UtcNow.Year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        var existing = await db.Budgets.FirstOrDefaultAsync(b =>
                b.Team == team &&
                b.CostCentre == costCentre &&
                b.Season == season &&
                b.PeriodStart == budgetStart &&
                b.PeriodEnd == budgetEnd);

        if (existing is not null)
        {
            return existing;
        }

        var budget = new Budget
        {
            Name = name,
            Team = team,
            CostCentre = costCentre,
            TargetAmount = targetAmount,
            PeriodStart = budgetStart,
            PeriodEnd = budgetEnd,
            Season = season,
            Type = type,
        };
        db.Budgets.Add(budget);
        return budget;
    }

    private static async Task AddPresenterInvoicesIfUserExistsAsync(
        AppDbContext db,
        Team chassisTeam,
        Team electronicsTeam,
        Team suspensionTeam,
        Team operationsTeam,
        Team powertrainTeam,
        Team batteryTeam,
        Team aerodynamicsTeam)
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
            PayoutType.NotYetPaid,
            TransactionStatus.Paid,
            4);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-PRES-SUPPLIER-PNG-001-2",
            presenterUser,
            null,
            electronicsTeam,
            736.42m,
            "Test Duplicate",
            "External supplier should be paid directly.",
            "uploads/presentation-invoices/invoice-techstore-2026.png",
            PayoutType.NotYetPaid,
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
            PayoutType.NotYetPaid,
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
            PayoutType.NotYetPaid,
            TransactionStatus.Submitted,
            1);

        // Budget-linked invoices — gives the budget utilization bars real data.
        // Chassis "Electric tools" (15,000€): 70% utilized — healthy
        var chassisBudget = await db.Budgets.FirstOrDefaultAsync(b => b.TeamId == chassisTeam.Id);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-BUDGET-CHS-001",
            presenterUser,
            null,
            chassisTeam,
            4500.00m,
            "Carbon fibre raw material order",
            "Bulk carbon roll purchase for monocoque layup.",
            "uploads/presentation-invoices/invoice-consulting-2026.pdf",
            PayoutType.NotYetPaid,
            TransactionStatus.Paid,
            30,
            chassisBudget);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-BUDGET-CHS-002",
            presenterUser,
            null,
            chassisTeam,
            3200.00m,
            "CNC milling service batch",
            "External machining service for frame brackets.",
            "uploads/presentation-invoices/invoice-consulting-2026.pdf",
            PayoutType.NotYetPaid,
            TransactionStatus.Paid,
            22,
            chassisBudget);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-BUDGET-CHS-003",
            presenterUser,
            presenterBankAccount,
            chassisTeam,
            2800.00m,
            "Aluminium profiles and brackets",
            "Self-purchased structural material, reimbursement pending.",
            "uploads/presentation-invoices/invoice-consulting-2026.pdf",
            PayoutType.User,
            TransactionStatus.Approved,
            10,
            chassisBudget);

        // Electronics "Cables" (8,000€): 93.75% utilized — near limit
        var electronicsBudget = await db.Budgets.FirstOrDefaultAsync(b => b.TeamId == electronicsTeam.Id);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-BUDGET-ELEC-001",
            presenterUser,
            null,
            electronicsTeam,
            3500.00m,
            "Sensor harness and connector set",
            "Full wiring harness from specialist supplier.",
            "uploads/presentation-invoices/invoice-techstore-2026.png",
            PayoutType.NotYetPaid,
            TransactionStatus.Paid,
            28,
            electronicsBudget);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-BUDGET-ELEC-002",
            presenterUser,
            null,
            electronicsTeam,
            2200.00m,
            "PCB manufacturing batch",
            "Custom PCB order for sensor interface boards.",
            "uploads/presentation-invoices/invoice-techstore-2026.png",
            PayoutType.NotYetPaid,
            TransactionStatus.Paid,
            18,
            electronicsBudget);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-BUDGET-ELEC-003",
            presenterUser,
            null,
            electronicsTeam,
            1800.00m,
            "Microcontroller units and modules",
            "STM32 dev modules for embedded control system.",
            "uploads/presentation-invoices/invoice-techstore-2026.png",
            PayoutType.NotYetPaid,
            TransactionStatus.Approved,
            7,
            electronicsBudget);

        // Powertrain "Rearwing Parts" (22,000€): OVER BUDGET
        var powertrainBudget = await db.Budgets.FirstOrDefaultAsync(b => b.TeamId == powertrainTeam.Id);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-BUDGET-PT-001",
            presenterUser,
            null,
            powertrainTeam,
            14500.00m,
            "Drivetrain component procurement",
            "Gearbox internals and differential housing.",
            "uploads/presentation-invoices/invoice-consulting-2026.pdf",
            PayoutType.NotYetPaid,
            TransactionStatus.Paid,
            35,
            powertrainBudget);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-BUDGET-PT-002",
            presenterUser,
            null,
            powertrainTeam,
            9200.00m,
            "Cooling system assembly parts",
            "Water pump, radiator and hose set.",
            "uploads/presentation-invoices/invoice-consulting-2026.pdf",
            PayoutType.NotYetPaid,
            TransactionStatus.Approved,
            12,
            powertrainBudget);

        // Battery "Engine Parts" (14,000€): OVER BUDGET
        var batteryBudget = await db.Budgets.FirstOrDefaultAsync(b => b.TeamId == batteryTeam.Id);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-BUDGET-BAT-001",
            presenterUser,
            null,
            batteryTeam,
            10000.00m,
            "Battery cell module order",
            "Lithium pouch cells for accumulator pack.",
            "uploads/presentation-invoices/invoice-techstore-2026.png",
            PayoutType.NotYetPaid,
            TransactionStatus.Paid,
            40,
            batteryBudget);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-BUDGET-BAT-002",
            presenterUser,
            null,
            batteryTeam,
            5500.00m,
            "BMS electronics and contactors",
            "Battery management system boards and safety contactors.",
            "uploads/presentation-invoices/invoice-techstore-2026.png",
            PayoutType.NotYetPaid,
            TransactionStatus.Approved,
            15,
            batteryBudget);

        // Aerodynamics "Wheels" (18,000€): 87% utilized — near limit
        var aeroBudget = await db.Budgets.FirstOrDefaultAsync(b => b.TeamId == aerodynamicsTeam.Id);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-BUDGET-AERO-001",
            presenterUser,
            null,
            aerodynamicsTeam,
            8000.00m,
            "Wind tunnel session booking",
            "Full-day wind tunnel run at external facility.",
            "uploads/presentation-invoices/invoice-consulting-2026.pdf",
            PayoutType.NotYetPaid,
            TransactionStatus.Paid,
            45,
            aeroBudget);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-BUDGET-AERO-002",
            presenterUser,
            null,
            aerodynamicsTeam,
            4500.00m,
            "CFD simulation software licence",
            "Annual licence for aero simulation toolchain.",
            "uploads/presentation-invoices/invoice-consulting-2026.pdf",
            PayoutType.NotYetPaid,
            TransactionStatus.Paid,
            20,
            aeroBudget);
        await AddPresenterInvoiceIfMissingAsync(
            db,
            "INV-BUDGET-AERO-003",
            presenterUser,
            null,
            aerodynamicsTeam,
            3200.00m,
            "Front wing layup materials",
            "Prepreg carbon and core foam for front wing mould.",
            "uploads/presentation-invoices/invoice-consulting-2026.pdf",
            PayoutType.NotYetPaid,
            TransactionStatus.Approved,
            8,
            aeroBudget);
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
        int createdDaysAgo,
        Budget? budget = null)
    {
        var paidAt = DateTime.UtcNow.AddDays(-createdDaysAgo).ToUniversalTime();
        var existingPaymentRequest = await db.PaymentRequestsByUser.FirstOrDefaultAsync(p => p.InvoiceNumber == invoiceNumber);

        if (existingPaymentRequest is not null)
        {
            existingPaymentRequest.User = presenterUser;
            existingPaymentRequest.Team = team;
            existingPaymentRequest.Budget = budget;
            existingPaymentRequest.BudgetId = budget?.Id;
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
            Budget = budget,
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
        Budget chassisIncomeBudget,
        Budget electronicsIncomeBudget,
        Budget suspensionIncomeBudget,
        Budget powertrainIncomeBudget,
        Budget batteryIncomeBudget,
        Budget operationsIncomeBudget,
        Team aerodynamicsTeam,
        Team softwareTeam,
        Team marketingTeam)
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
            chassisIncomeBudget,
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
            electronicsIncomeBudget,
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
            suspensionIncomeBudget,
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
            powertrainIncomeBudget,
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
            operationsIncomeBudget,
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
            batteryIncomeBudget,
            560.00m,
            "High-voltage safety training fee",
            TransactionStatus.Submitted,
            DateTime.UtcNow.AddDays(45),
            2);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            aerodynamicsTeam,
            null,
            1200.00m,
            "Wind tunnel facility access – April slot",
            TransactionStatus.Submitted,
            DateTime.UtcNow.AddDays(7),
            12);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            aerodynamicsTeam,
            null,
            340.00m,
            "Aero test equipment rental – pitot tubes",
            TransactionStatus.Paid,
            null,
            25);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            softwareTeam,
            null,
            890.00m,
            "Telemetry server hosting – annual renewal",
            TransactionStatus.Paid,
            DateTime.UtcNow.AddDays(14),
            8);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            softwareTeam,
            null,
            75.00m,
            "GitHub Actions CI minutes – overage charge",
            TransactionStatus.Submitted,
            DateTime.UtcNow.AddDays(5),
            3);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            marketingTeam,
            null,
            2400.00m,
            "Sponsor presentation event catering",
            TransactionStatus.Paid,
            null,
            20);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            marketingTeam,
            null,
            650.00m,
            "Team photo shoot and print materials",
            TransactionStatus.Paid,
            DateTime.UtcNow.AddDays(3),
            6);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            chassisTeam,
            null,
            480.00m,
            "Composite repair consumables reorder",
            TransactionStatus.Submitted,
            DateTime.UtcNow.AddDays(10),
            4);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            suspensionTeam,
            null,
            1100.00m,
            "Shock absorber rebuild kit – full set",
            TransactionStatus.Paid,
            DateTime.UtcNow.AddDays(21),
            15);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            electronicsTeam,
            null,
            220.00m,
            "Oscilloscope calibration service",
            TransactionStatus.Submitted,
            null,
            9);

        await AddTeamRequestIfMissingAsync(
            db,
            presenterUser,
            presenterUser,
            operationsTeam,
            null,
            3200.00m,
            "Trailer rental – FSAE Germany transport",
            TransactionStatus.Paid,
            DateTime.UtcNow.AddDays(60),
            1);
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
