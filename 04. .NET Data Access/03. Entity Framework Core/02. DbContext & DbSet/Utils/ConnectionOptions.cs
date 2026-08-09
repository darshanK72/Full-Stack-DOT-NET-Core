using Microsoft.EntityFrameworkCore;

namespace DbContextAndDbSet.Utils;

/*
 * FILE ROLE: Centralizes the EfCoreTutorial LocalDB connection string and shows how to build
 *            DbContextOptions<TContext> for manual context creation or DI registration.
 *
 * SECTIONS IN THIS FILE:
 *   2. Connection string constant
 *   3. DbContextOptionsBuilder -> DbContextOptions<TContext>
 */

/*
 * SECTION 2: CONNECTION STRING CONSTANT
 *
 * Same LocalDB instance used across ADO.NET and Dapper tutorials; this chapter uses its own
 * database name so EF Core demos do not collide with hand-written SQL scripts.
 *
 *   Server=(localdb)\MSSQLLocalDB  - SQL Server Express LocalDB on Windows
 *   Database=EfCoreTutorial        - created by EnsureCreated in EfCoreTutorialBootstrap
 *   Integrated Security=true       - Windows authentication (no SQL login in connection text)
 *   TrustServerCertificate=true    - skip cert validation for local dev connections
 *
 * Production apps store connection strings in configuration (appsettings.json, secrets,
 * environment variables) - not hard-coded constants. ASP.NET wiring -> Web API modules.
 */
public static class ConnectionOptions
{
    public const string LocalDbConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=EfCoreTutorial;Integrated Security=true;TrustServerCertificate=true";

    /*
     * SECTION 3: DbContextOptionsBuilder -> DbContextOptions<TContext>
     *
     * DbContextOptions<TContext> is an immutable snapshot of provider + connection settings.
     * DbContext reads it at construction time; you pass the same options shape to DI and to
     * `new AppDbContext(options)`.
     *
     * Build pattern:
     *   var builder = new DbContextOptionsBuilder<AppDbContext>();
     *   builder.UseSqlServer(connectionString);   // selects SqlServer provider + conn text
     *   DbContextOptions<AppDbContext> options = builder.Options;
     *
     * UseSqlServer lives in Microsoft.EntityFrameworkCore.SqlServer (already referenced).
     * Other providers use parallel extensions: UseNpgsql, UseSqlite, UseInMemoryDatabase, etc.
     *
     * Pitfall: DbContextOptionsBuilder is mutable while building; call .Options once and treat
     * the result as read-only. Reuse the same options instance for many context instances.
     */
    public static DbContextOptions<Data.AppDbContext> BuildAppDbContextOptions()
    {
        var builder = new DbContextOptionsBuilder<Data.AppDbContext>();
        builder.UseSqlServer(LocalDbConnectionString); // registers SqlServer as the provider
        return builder.Options;                          // freeze configuration for AppDbContext
    }
}
