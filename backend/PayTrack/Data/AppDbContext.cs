// <copyright file="AppDbContext.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Data.Entities;

namespace PayTrack.Data;

/// <summary>
/// Database Context. Contains all important Database objects and sets up the relations.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Database Set for all Team objects.
    /// </summary>
    public DbSet<Team> Teams => this.Set<Team>();
}
