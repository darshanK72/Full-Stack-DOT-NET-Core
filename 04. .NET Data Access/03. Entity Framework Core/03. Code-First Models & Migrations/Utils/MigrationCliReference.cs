using System;

namespace CodeFirstModelsAndMigrations.Utils;

/*
 * FILE ROLE: Documents the CLI and Package Manager Console workflow for creating
 *            and applying migrations - concepts you run outside Main.
 *
 * SECTIONS IN THIS FILE:
 *   5. dotnet ef migrations add / Add-Migration
 *   6. dotnet ef database update / Update-Database
 *   9. EnsureCreated vs Migrate (preview)
 */

/*
 * =========================================================================
 * SECTION 5: DOTNET EF MIGRATIONS ADD / ADD-MIGRATION
 * =========================================================================
 *
 * After entity classes and DbContext exist, generate a migration from the
 * current model diff:
 *
 *   CLI (cross-platform, recommended):
 *     dotnet tool install --global dotnet-ef          # once per machine
 *     dotnet ef migrations add InitialCreate \
 *       --project "03. Code-First Models & Migrations/CodeFirstModelsAndMigrations.csproj"
 *
 *   Package Manager Console (Visual Studio):
 *     Add-Migration InitialCreate
 *     (Default project = CodeFirstModelsAndMigrations, Startup project = same)
 *
 * What happens:
 *   1. EF builds the project and creates StoreDbContext at design time
 *   2. Compares the model to the previous snapshot (none on first migration)
 *   3. Writes timestamped files under Migrations/:
 *        {Timestamp}_InitialCreate.cs         - Up()/Down() DDL operations
 *        {Timestamp}_InitialCreate.Designer.cs - metadata for this migration
 *        StoreDbContextModelSnapshot.cs       - full model after this migration
 *
 * Migration class names are PascalCase; the file prefix is UTC timestamp.
 * Each new model change -> new migration -> snapshot updated in place.
 *
 * Pitfall: edit entity, forget to add migration -> runtime model no longer matches
 * database; queries may fail or silently mis-map columns.
 * -------------------------------------------------------------------------
 */
public static class MigrationCliReference
{
    public static void ExplainAddMigration()
    {
        Console.WriteLine("--- SECTION 5: dotnet ef migrations add / Add-Migration ---");
        Console.WriteLine("  dotnet ef migrations add InitialCreate");
        Console.WriteLine("  Add-Migration InitialCreate   (Package Manager Console)");
        Console.WriteLine("  Output: Migrations/*_InitialCreate.cs + ModelSnapshot");
    }

    /*
     * =========================================================================
     * SECTION 6: DOTNET EF DATABASE UPDATE / UPDATE-DATABASE
     * =========================================================================
     *
     * Applies pending migrations to the database:
     *
     *   CLI:
     *     dotnet ef database update
     *     dotnet ef database update InitialCreate   # migrate to a specific migration
     *     dotnet ef database update 0               # revert all (drop to empty)
     *
     *   Package Manager Console:
     *     Update-Database
     *     Update-Database -Migration InitialCreate
     *
     * Each migration's Up() method runs in order. EF records applied migration
     * IDs in dbo.__EFMigrationsHistory (see MigrationHistoryReader).
     *
     * At runtime (this chapter demo):
     *     context.Database.Migrate();
     *   Equivalent to Update-Database for pending migrations only - common in
     *   dev/test and some deployment scripts. Production often runs dotnet ef
     *   database update from CI/CD instead of Migrate() in app startup.
     * -------------------------------------------------------------------------
     */
    public static void ExplainDatabaseUpdate()
    {
        Console.WriteLine("--- SECTION 6: dotnet ef database update / Update-Database ---");
        Console.WriteLine("  dotnet ef database update");
        Console.WriteLine("  Update-Database   (Package Manager Console)");
        Console.WriteLine("  Runtime equivalent: context.Database.Migrate()");
    }

    /*
     * =========================================================================
     * SECTION 9: ENSURECREATED VS MIGRATE (PREVIEW)
     * =========================================================================
     *
     *   Database.EnsureCreated()
     *     - Creates database + tables from current model if missing
     *     - Does NOT use migration files; no __EFMigrationsHistory
     *     - Cannot evolve schema with migrations later on same database
     *     - OK for quick throwaway demos and tests (InMemory/SQLite previews)
     *
     *   Database.Migrate()
     *     - Applies migration Up() scripts in order
     *     - Maintains __EFMigrationsHistory
     *     - Required for team workflows and production schema evolution
     *
     * Rule: pick migrations for real apps. EnsureCreated is a shortcut only.
     * -------------------------------------------------------------------------
     */
    public static void ExplainEnsureCreatedVsMigrate()
    {
        Console.WriteLine("--- EnsureCreated vs Migrate ---");
        Console.WriteLine("  EnsureCreated() - create schema without migration history (throwaway demos)");
        Console.WriteLine("  Migrate()       - apply Migrations/ Up() scripts + __EFMigrationsHistory");
    }
}
