namespace FluentApiAndDataAnnotations.Utils;

/*
 * FILE ROLE: Centralizes the EfCoreTutorial LocalDB connection string for runtime
 *            and design-time EF Core tooling.
 *
 * SECTIONS IN THIS FILE:
 *   7. LocalDB connection string
 */

/*
 * =========================================================================
 * SECTION 7: LOCALDB CONNECTION STRING
 * =========================================================================
 *
 * Fluent API and data annotations configure the *model*; EF Core still needs
 * a SQL Server connection to materialize that model as tables, indexes, and
 * constraints when you call EnsureCreated(), Migrate(), or run queries.
 *
 * This chapter shares EfCoreTutorial with other EF Core chapters (see ch03):
 *
 *   Server=(localdb)\MSSQLLocalDB;Database=EfCoreTutorial;Integrated Security=true;TrustServerCertificate=true
 *
 * Keywords (same family as ADO.NET ch02):
 *   Server / Data Source        host\instance — here (localdb)\MSSQLLocalDB
 *   Database / Initial Catalog  logical database name
 *   Integrated Security=true    Windows auth — typical for LocalDB dev boxes
 *   TrustServerCertificate=true dev-only TLS shortcut — not for production
 *
 * Pass this string to UseSqlServer(...) in DbContextOptionsBuilder (see Program.cs).
 * -------------------------------------------------------------------------
 */
public static class ConnectionHelper
{
    public const string LocalDbConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=EfCoreTutorial;Integrated Security=true;TrustServerCertificate=true";

    public const string MasterConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true";
}
