namespace CodeFirstModelsAndMigrations.Utils;

/*
 * FILE ROLE: Centralizes the EfCoreTutorial LocalDB connection string for runtime
 *            and design-time EF Core tooling (dotnet ef migrations add).
 *
 * SECTIONS IN THIS FILE:
 *   4. LocalDB connection string for Code-First
 */

/*
 * =========================================================================
 * SECTION 4: LOCALDB CONNECTION STRING FOR CODE-FIRST
 * =========================================================================
 *
 * Code-First starts from C# entity classes. EF Core still needs a real SQL
 * Server connection when you:
 *   - run dotnet ef migrations add (design-time model diff)
 *   - run dotnet ef database update or context.Database.Migrate() (apply DDL)
 *
 * This chapter uses a dedicated tutorial database separate from AdoNetTutorial:
 *
 *   Server=(localdb)\MSSQLLocalDB;Database=EfCoreTutorial;Integrated Security=true;TrustServerCertificate=true
 *
 * Keywords (same family as ADO.NET ch02):
 *   Server / Data Source     host\instance - here (localdb)\MSSQLLocalDB
 *   Database / Initial Catalog  logical database name created on first Migrate()
 *   Integrated Security=true    Windows auth - typical for LocalDB dev boxes
 *   TrustServerCertificate=true dev-only TLS shortcut - not for production
 *
 * At runtime, pass this string to UseSqlServer(...) or store it in configuration
 * (ASP.NET Core appsettings.json - deferred to Web API modules).
 * -------------------------------------------------------------------------
 */
public static class ConnectionHelper
{
    public const string LocalDbConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=EfCoreTutorial;Integrated Security=true;TrustServerCertificate=true";
}
