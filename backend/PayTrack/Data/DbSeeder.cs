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
                Description = "Responsible for aero package, CFD, and composite surfaces.",
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

        var budgetStart = new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var budgetEnd = new DateTime(DateTime.UtcNow.Year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        if (!await db.Budgets.AnyAsync(b =>
                b.Team == chassisTeam &&
                b.CostCentre == manufacturingCostCentre &&
                b.PeriodStart == budgetStart &&
                b.PeriodEnd == budgetEnd))
        {
            db.Budgets.Add(new Budget
            {
                Team = chassisTeam,
                CostCentre = manufacturingCostCentre,
                TargetAmount = 15000m,
                PeriodStart = budgetStart,
                PeriodEnd = budgetEnd,
            });
        }

        if (!await db.Budgets.AnyAsync(b =>
                b.Team == electronicsTeam &&
                b.CostCentre == electronicsCostCentre &&
                b.PeriodStart == budgetStart &&
                b.PeriodEnd == budgetEnd))
        {
            db.Budgets.Add(new Budget
            {
                Team = electronicsTeam,
                CostCentre = electronicsCostCentre,
                TargetAmount = 8000m,
                PeriodStart = budgetStart,
                PeriodEnd = budgetEnd,
            });
        }

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

        await db.SaveChangesAsync();
    }
}
