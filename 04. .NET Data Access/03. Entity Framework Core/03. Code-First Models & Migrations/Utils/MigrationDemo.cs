using System;
using System.Collections.Generic;
using System.Linq;
using CodeFirstModelsAndMigrations.Data;
using CodeFirstModelsAndMigrations.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CodeFirstModelsAndMigrations.Utils;

/*
 * FILE ROLE: Runtime helpers for applying migrations, seeding sample rows, and
 *            reading __EFMigrationsHistory during the chapter demo.
 *
 * SECTIONS IN THIS FILE:
 *   7. Database.Migrate() demonstration
 *   8. __EFMigrationsHistory table
 */

public static class MigrationDemo
{
    /*
     * =========================================================================
     * SECTION 7: DATABASE.MIGRATE() DEMONSTRATION
     * =========================================================================
     *
     * Migrate() connects to EfCoreTutorial on LocalDB and:
     *   1. Creates the database if it does not exist
     *   2. Ensures __EFMigrationsHistory exists
     *   3. Runs Up() for any migration not yet recorded in history
     *
     * Returns true when the database is reachable; false when LocalDB is down.
     * -------------------------------------------------------------------------
     */
    public static bool TryApplyMigrations(out string? failureMessage)
    {
        failureMessage = null;
        try
        {
            DbContextOptions<StoreDbContext> options = new DbContextOptionsBuilder<StoreDbContext>()
                .UseSqlServer(ConnectionHelper.LocalDbConnectionString)
                .Options;

            using StoreDbContext context = new StoreDbContext(options);

            if (!context.Database.CanConnect())
            {
                context.Database.Migrate(); // creates EfCoreTutorial on first connect + applies migrations
                return true;
            }

            IEnumerable<string> pending = context.Database.GetPendingMigrations();
            if (!pending.Any())
            {
                return true; // already up to date - Migrate() is a no-op
            }

            context.Database.Migrate(); // applies pending migrations from Migrations/
            return true;
        }
        catch (SqlException ex) when (ex.Message.Contains("already an object named", StringComparison.OrdinalIgnoreCase))
        {
            // EfCoreTutorial exists with tables but no __EFMigrationsHistory (EnsureCreated, manual script, etc.)
            failureMessage =
                "Tables exist without migration history. Drop and recreate: dotnet ef database drop --force; dotnet ef database update";
            return CanUseExistingSchema();
        }
        catch (SqlException ex)
        {
            failureMessage = ex.Message;
            return false;
        }
        catch (InvalidOperationException ex)
        {
            failureMessage = ex.Message;
            return false;
        }
    }

    private static bool CanUseExistingSchema()
    {
        try
        {
            DbContextOptions<StoreDbContext> options = new DbContextOptionsBuilder<StoreDbContext>()
                .UseSqlServer(ConnectionHelper.LocalDbConnectionString)
                .Options;

            using StoreDbContext context = new StoreDbContext(options);
            return context.Database.CanConnect() && context.Products.Any();
        }
        catch (SqlException)
        {
            return false;
        }
    }

    public static void SeedSampleDataIfEmpty()
    {
        DbContextOptions<StoreDbContext> options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseSqlServer(ConnectionHelper.LocalDbConnectionString)
            .Options;

        using StoreDbContext context = new StoreDbContext(options);

        if (context.Products.Any())
        {
            return; // already seeded from a prior run
        }

        Category office = new Category { Name = "Office" };
        Category field = new Category { Name = "Field" };
        context.Categories.AddRange(office, field);
        context.SaveChanges(); // INSERT Categories; EF assigns CategoryId values

        context.Products.AddRange(
            new Product { Name = "Notebook", UnitPrice = 4.50m, CategoryId = office.CategoryId },
            new Product { Name = "Pen Pack", UnitPrice = 6.25m, CategoryId = office.CategoryId },
            new Product { Name = "Hard Hat", UnitPrice = 18.00m, CategoryId = field.CategoryId });
        context.SaveChanges();
    }

    public static IReadOnlyList<Product> ReadAllProducts()
    {
        DbContextOptions<StoreDbContext> options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseSqlServer(ConnectionHelper.LocalDbConnectionString)
            .Options;

        using StoreDbContext context = new StoreDbContext(options);
        return context.Products
            .OrderBy(product => product.ProductId)
            .AsNoTracking() // read-only list - change tracking preview -> ch11
            .ToList();
    }

    /*
     * =========================================================================
     * SECTION 8: __EFMIGRATIONSHISTORY TABLE
     * =========================================================================
     *
     * SQL Server stores one row per applied migration:
     *
     *   Table: dbo.__EFMigrationsHistory
     *   Columns:
     *     MigrationId   nvarchar(150) PK  - e.g. 20250809120000_InitialCreate
     *     ProductVersion nvarchar(32)     - EF Core version that generated the migration
     *
     * EF checks this table before running Up(). Already-applied IDs are skipped.
     * If you delete rows manually, EF may try to re-apply migrations and fail on
     * existing objects. If you drop the database, history is recreated on Migrate().
     *
     * Query it like any table - below uses raw SQL (FromSqlRaw preview -> ch10).
     * -------------------------------------------------------------------------
     */
    public static IReadOnlyList<(string MigrationId, string ProductVersion)> ReadMigrationHistory()
    {
        DbContextOptions<StoreDbContext> options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseSqlServer(ConnectionHelper.LocalDbConnectionString)
            .Options;

        using StoreDbContext context = new StoreDbContext(options);

        List<(string, string)> rows = new List<(string, string)>();
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT MigrationId, ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;";
        context.Database.OpenConnection();
        try
        {
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add((reader.GetString(0), reader.GetString(1)));
            }
        }
        finally
        {
            context.Database.CloseConnection();
        }

        return rows;
    }
}
