// <copyright file="AppDbContext.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PayTrack.Data.Entities;

namespace PayTrack.Data;

/// <summary>
/// Database Context. Contains all important Database objects and sets up the relations.
/// </summary>
[ExcludeFromCodeCoverage]
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Database Set for all Team objects.
    /// </summary>
    public DbSet<Team> Teams => this.Set<Team>();

    /// <summary>
    /// Database Set for all Team objects.
    /// </summary>
    public DbSet<User> User => this.Set<User>();

    /// <summary>
    /// Database set for all CostCentres.
    /// </summary>
    public DbSet<CostCentre> CostCentres => this.Set<CostCentre>();

    /// <summary>
    /// Database set for all Budgets.
    /// </summary>
    public DbSet<Budget> Budgets => this.Set<Budget>();

    /// <summary>
    /// Database set for all Seasons.
    /// </summary>
    public DbSet<Season> Seasons => this.Set<Season>();

    /// <summary>
    /// Database set for all BankAccounts.
    /// </summary>
    public DbSet<BankAccount> BankAccounts => this.Set<BankAccount>();

    /// <summary>
    /// Database set for all Transaction.
    /// </summary>
    public DbSet<Transaction> Transactions => this.Set<Transaction>();

    /// <summary>
    /// Database set for all PaymentManuals.
    /// </summary>
    public DbSet<PaymentManual> PaymentManuals => this.Set<PaymentManual>();

    /// <summary>
    /// Database set for all PaymentRequestsByUser.
    /// </summary>
    public DbSet<PaymentRequestByUser> PaymentRequestsByUser => this.Set<PaymentRequestByUser>();

    /// <summary>
    /// Database set for all PaymentRequestsByTeam.
    /// </summary>
    public DbSet<PaymentRequestByTeam> PaymentRequestsByTeam => this.Set<PaymentRequestByTeam>();

    /// <summary>
    /// Database set for all TransactionStatusHistories.
    /// </summary>
    public DbSet<TransactionStatusHistory> TransactionStatusHistories => this.Set<TransactionStatusHistory>();

    /// <summary>
    /// Overrides OnModelCreating to manually adjust the connections and constraints.
    /// </summary>
    /// <param name="modelBuilder">Model Builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // -------------------------------------------------------
        // TPH Inheritance — single Transactions table with a discriminator column
        // -------------------------------------------------------
        modelBuilder.Entity<Transaction>()
            .HasDiscriminator<string>("TransactionType")
            .HasValue<PaymentManual>("PaymentManual")
            .HasValue<PaymentRequestByUser>("PaymentRequestByUser")
            .HasValue<PaymentRequestByTeam>("PaymentRequestByTeam");

        // -------------------------------------------------------
        // User
        // -------------------------------------------------------
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();

            e.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(50);

            // User belongs to exactly one Team
            e.HasOne(u => u.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(u => u.TeamId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // User owns many BankAccounts
            e.HasMany(u => u.BankAccounts)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User initiates many Transactions (base FK)
            e.HasMany(u => u.Transactions)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User appears in status history as the changer
            e.HasMany(u => u.StatusHistoryChanges)
                .WithOne(h => h.ChangedBy)
                .HasForeignKey(h => h.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);

            // User as requester on PaymentRequestByTeam
            e.HasMany(u => u.RequestedByTeamPayments)
                .WithOne(p => p.RequestedBy)
                .HasForeignKey(p => p.RequestedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // -------------------------------------------------------
        // Team
        // -------------------------------------------------------
        modelBuilder.Entity<Team>(e =>
        {
            e.HasIndex(t => t.Name).IsUnique();

            e.HasMany(t => t.Budgets)
                .WithOne(b => b.Team)
                .HasForeignKey(b => b.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(t => t.Transactions)
                .WithOne(tx => tx.Team)
                .HasForeignKey(tx => tx.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // -------------------------------------------------------
        // CostCentre
        // -------------------------------------------------------
        modelBuilder.Entity<CostCentre>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();

            e.HasMany(c => c.Budgets)
                .WithOne(b => b.CostCentre)
                .HasForeignKey(b => b.CostCentreId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // -------------------------------------------------------
        // Season
        // -------------------------------------------------------
        modelBuilder.Entity<Season>(e =>
        {
            e.HasIndex(s => s.Name).IsUnique();

            e.HasMany(s => s.Budgets)
                .WithOne(b => b.Season)
                .HasForeignKey(b => b.SeasonId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // -------------------------------------------------------
        // Budget
        // -------------------------------------------------------
        modelBuilder.Entity<Budget>(e =>
        {
            e.Property(b => b.TargetAmount).HasColumnType("decimal(18,2)");

            e.HasMany(b => b.Transactions)
                .WithOne(tx => tx.Budget)
                .HasForeignKey(tx => tx.BudgetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: one budget per team+costcentre+season+period
            e.HasIndex(b => new { b.TeamId, b.CostCentreId, b.SeasonId, b.PeriodStart, b.PeriodEnd })
                .IsUnique();
        });

        // -------------------------------------------------------
        // BankAccount
        // -------------------------------------------------------
        modelBuilder.Entity<BankAccount>(e =>
        {
            // IBAN should be unique per user (same account not duplicated)
            e.HasIndex(b => new { b.UserId, b.Iban }).IsUnique();
        });

        // -------------------------------------------------------
        // Transaction (base)
        // -------------------------------------------------------
        modelBuilder.Entity<Transaction>(e =>
        {
            e.Property(t => t.Amount).HasColumnType("decimal(18,2)");

            e.Property(t => t.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            e.Property(t => t.PaymentDirection)
                .HasConversion<string>()
                .HasMaxLength(10);

            e.HasMany(t => t.StatusHistory)
                .WithOne(h => h.Transaction)
                .HasForeignKey(h => h.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------------------------------------------
        // PaymentRequestByUser
        // -------------------------------------------------------
        modelBuilder.Entity<PaymentRequestByUser>(e =>
        {
            e.Property(p => p.PayoutType)
                .HasConversion<string>()
                .HasMaxLength(20);

            // BankAccount is optional
            e.HasOne(p => p.BankAccount)
                .WithMany()
                .HasForeignKey(p => p.BankAccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // -------------------------------------------------------
        // TransactionStatusHistory
        // -------------------------------------------------------
        modelBuilder.Entity<TransactionStatusHistory>(e =>
        {
            e.Property(h => h.FromStatus)
                .HasConversion<string>()
                .HasMaxLength(50);

            e.Property(h => h.ToStatus)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Index for fast lookup of a transaction's history ordered by time
            e.HasIndex(h => new { h.TransactionId, h.ChangedAt });
        });
    }
}
