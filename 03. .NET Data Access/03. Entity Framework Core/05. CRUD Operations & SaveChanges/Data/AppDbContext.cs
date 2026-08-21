using CrudOperationsAndSaveChanges.Models;
using CrudOperationsAndSaveChanges.Utils;
using Microsoft.EntityFrameworkCore;

namespace CrudOperationsAndSaveChanges.Data;

/*
 * FILE ROLE: DbContext — the EF Core session that exposes DbSet<T> collections,
 *            tracks entity changes, and translates SaveChanges into SQL.
 *
 * SECTIONS IN THIS FILE:
 *   2. DbContext and DbSet<T>
 */

/*
 * =========================================================================
 * SECTION 2: DbContext AND DbSet<T>
 * =========================================================================
 *
 * DbContext is the unit-of-work boundary for EF Core:
 *   - DbSet<Product> Products  -> gateway to the Products table
 *   - Change tracker           -> remembers Added/Modified/Deleted/Unchanged
 *   - SaveChanges()            -> flushes pending changes as SQL (SECTION 7)
 *
 * DbSet<T> implements IQueryable<T> (LINQ -> ch08) and CRUD helpers:
 *   Add, AddRange, Update, Remove, RemoveRange, Find, Attach, ...
 *
 * Lifetime: create one context per unit of work (request, transaction, or
 * demo step). Dispose when done — it closes the underlying connection.
 *
 * Configuration paths (both valid):
 *   A) Pass DbContextOptions<AppDbContext> from the caller (shown in Program.cs)
 *   B) Override OnConfiguring when options were not supplied (fallback below)
 *
 * DbContext and DI registration patterns -> ch02 DbContext & DbSet.
 * Migrations and schema evolution -> ch03 Code-First Models & Migrations.
 * -------------------------------------------------------------------------
 */
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>(); // typed table accessor

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(ConnectionHelper.LocalDbConnectionString);
        }
    }
}
