using System;
using IntroductionToEntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;

namespace IntroductionToEntityFrameworkCore.Data;

/*
 * FILE ROLE: Minimal DbContext preview - the EF Core session that exposes DbSet<T>
 *            collections and coordinates queries and SaveChanges.
 *
 * SECTIONS IN THIS FILE:
 *   8. DbContext preview - gateway to the database
 */

/*
 * =========================================================================
 * SECTION 8: DbContext PREVIEW - GATEWAY TO THE DATABASE
 * =========================================================================
 *
 * DbContext is the root type you work with in EF Core applications:
 *
 *   - DbSet<TEntity> properties represent tables (Products -> dbo.Products)
 *   - LINQ queries run against DbSet<T> (translated to SQL by the provider)
 *   - SaveChanges() persists inserts/updates/deletes tracked in memory
 *
 * Typical construction (SQL Server - used in later chapters):
 *
 *   var options = new DbContextOptionsBuilder<AppDbContext>()
 *       .UseSqlServer(connectionString)
 *       .Options;
 *   using var db = new AppDbContext(options);
 *
 * This chapter uses the In-Memory provider instead (SECTION 9) so the demo
 * runs without LocalDB. In-Memory is not a substitute for integration tests
 * against real SQL - it approximates EF behavior in process memory.
 *
 * COVERED IN DETAIL LATER -> 02. DbContext & DbSet
 *   - OnConfiguring vs DbContextOptions injection
 *   - DbSet<T> members, EnsureCreated vs migrations
 *   - DI registration in ASP.NET Core apps
 * -------------------------------------------------------------------------
 */
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>(); // table mapped from Product entity
}
