using DbContextAndDbSet.Models;
using DbContextAndDbSet.Utils;
using Microsoft.EntityFrameworkCore;

namespace DbContextAndDbSet.Data;

/*
 * FILE ROLE: Defines AppDbContext - the EF Core session that owns DbSet<T> collections,
 *            coordinates queries, tracks changes, and talks to SQL Server through the provider.
 *
 * SECTIONS IN THIS FILE:
 *   4. DbContext role and responsibilities
 *   5. DbSet<T> properties and Set<T>()
 *   6. Constructor injection with DbContextOptions<AppDbContext>
 *   4b. OnConfiguring fallback when options are not supplied
 */

/*
 * SECTION 4: DbContext - GATEWAY TO THE DATABASE
 *
 * DbContext is the main EF Core type your application code interacts with. One instance
 * typically represents one unit of work (one connection scope, one change tracker).
 *
 * Responsibilities (this chapter focuses on the first two; depth in later chapters):
 *
 *   Responsibility          | API surface (preview)              | Full depth in
 *   ------------------------|------------------------------------|---------------------------
 *   Expose entity sets      | DbSet<T>, Set<T>()                 | this chapter (section 5)
 *   Query / persist         | LINQ on DbSet, SaveChanges          | ch.05 CRUD, ch.08 LINQ
 *   Model configuration     | OnModelCreating, conventions       | ch.03, ch.07
 *   Change tracking         | Entry(), EntityState               | ch.11 Change Tracking
 *   Transactions            | Database.BeginTransaction          | ch.11
 *
 * Lifetime rules:
 *   - DbContext implements IDisposable / IAsyncDisposable - dispose when done (using).
 *   - Web apps register DbContext as SCOPED (one per HTTP request) via AddDbContext.
 *   - Do not share one DbContext instance across threads.
 *
 * Pitfall: Creating many long-lived DbContext instances leaks connections and grows the
 * change tracker. Create, use, dispose - or resolve scoped from DI per unit of work.
 */
public sealed class AppDbContext : DbContext
{
    /*
     * SECTION 6: CONSTRUCTOR INJECTION WITH DbContextOptions<AppDbContext>
     *
     * Preferred pattern: pass pre-built DbContextOptions<T> from DI or ConnectionOptions.
     * The base DbContext constructor stores options; OnConfiguring runs only when the provider
     * was not already configured through those options.
     *
     * Generic DbContextOptions<AppDbContext> (not non-generic DbContextOptions) gives EF Core
     * compile-time type safety for which context the options belong to.
     */
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /*
     * Parameterless constructor exists so OnConfiguring fallback (section 4b) can run in
     * quick demos. Production and DI always use the options constructor above.
     */
    public AppDbContext()
    {
    }

    /*
     * SECTION 5: DbSet<T> - TYPED TABLE COLLECTIONS
     *
     * Each DbSet<T> property maps to a database table (by convention: DbSet<Product> Products
     * -> dbo.Products). DbSet<T> implements IQueryable<T>, so LINQ queries translate to SQL.
     *
     *   Member                    | Purpose
     *   --------------------------|-------------------------------------------------
     *   DbSet<Product> Products   | Strongly typed access; preferred in application code
     *   Set<Product>()            | Same set without a declared property - useful in generic code
     *   DbSet.Local               | Entities already tracked in memory (change tracker)
     *   Add / AddRange            | Stage inserts (persisted on SaveChanges -> ch.05)
     *   Find / FindAsync          | PK lookup against tracked entities first, then database
     *
     * Table naming: class Product + property name Products -> table "Products" (pluralized).
     * Override table names in OnModelCreating -> ch.07.
     *
     * Pitfall: DbSet queries are deferred (IQueryable) until enumeration or ToList/ToListAsync.
     * Full LINQ-to-Entities behavior -> ch.08 LINQ to Entities & Query Patterns.
     */
    public DbSet<Product> Products => Set<Product>(); // expression-bodied DbSet backed by Set<T>()

    /*
     * SECTION 4b: OnConfiguring FALLBACK WHEN OPTIONS ARE NOT SUPPLIED
     *
     * Runs when the context was constructed without provider configuration in
     * DbContextOptions (e.g. `new AppDbContext()` with the parameterless ctor).
     *
     * if (!optionsBuilder.IsConfigured) prevents overwriting options already set by DI or
     * DbContextOptionsBuilder.UseSqlServer in ConnectionOptions.BuildAppDbContextOptions().
     *
     * In ASP.NET Core, connection strings come from configuration and AddDbContext - you rarely
     * override OnConfiguring. Keep connection text out of the context in real apps; this fallback
     * exists so the tutorial can show both manual and DI paths without duplicating classes.
     */
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(ConnectionOptions.LocalDbConnectionString);
        }
    }
}
