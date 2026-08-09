using System.Linq;
using CrudOperationsAndSaveChanges.Data;
using CrudOperationsAndSaveChanges.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CrudOperationsAndSaveChanges.Utils;

/*
 * FILE ROLE: Creates the EfCoreTutorial LocalDB database, builds schema from
 *            the entity model, and seeds starter rows when the table is empty.
 *
 * SECTIONS IN THIS FILE:
 *   9. LocalDB bootstrap — EnsureReady()
 */

/*
 * =========================================================================
 * SECTION 9: LOCALDB BOOTSTRAP — EnsureReady()
 * =========================================================================
 *
 * This chapter uses a dedicated EfCoreTutorial database (separate from
 * AdoNetTutorial used in ADO.NET/Dapper) so EF-owned schema does not clash
 * with hand-written SQL scripts.
 *
 * Bootstrap steps:
 *   1. CREATE DATABASE EfCoreTutorial on (localdb)\MSSQLLocalDB if missing
 *   2. Drop Products when an older incompatible schema is detected
 *   3. context.Database.EnsureCreated() — create tables from the model
 *   4. Seed sample products when Products is empty
 *
 * EnsureCreated vs Migrate:
 *   EnsureCreated() — quick for tutorials; no migration history table
 *   Database.Migrate() — applies EF migrations (Add-Migration / ch03)
 *
 * Production apps use Migrate() or external migration pipelines; this demo
 * picks EnsureCreated for zero CLI setup. Swap to Migrate() once ch03
 * migrations exist in your solution.
 *
 * Manual alternative — run in SSMS/sqlcmd against master, then point the
 * connection string at EfCoreTutorial and call EnsureCreated from code.
 * -------------------------------------------------------------------------
 */
public static class EfCoreTutorialBootstrap
{
    public const string DatabaseName = "EfCoreTutorial";

    public static void EnsureReady(DbContextOptions<AppDbContext> options)
    {
        using (var master = new SqlConnection(ConnectionHelper.MasterConnectionString))
        {
            master.Open();
            using var createDb = new SqlCommand(
                """
                IF DB_ID(@dbName) IS NULL
                    CREATE DATABASE [EfCoreTutorial];
                """,
                master);
            createDb.Parameters.AddWithValue("@dbName", DatabaseName);
            createDb.ExecuteNonQuery();
        }

        using var context = new AppDbContext(options);

        if (!TableHasExpectedSchema(context))
        {
            // EnsureCreated does not patch an existing database — recreate when schema differs.
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
        else
        {
            context.Database.EnsureCreated(); // no-op when Products already matches the model
        }

        if (context.Products.Any())
        {
            return;
        }

        context.Products.AddRange(
            new Product { ProductName = "Widget A", UnitPrice = 9.99m, StockQuantity = 100 },
            new Product { ProductName = "Widget B", UnitPrice = 14.50m, StockQuantity = 50 },
            new Product { ProductName = "Gadget C", UnitPrice = 29.99m, StockQuantity = 25, IsDiscontinued = true });
        context.SaveChanges(); // INSERT seed rows — states return to Unchanged
    }

    private static bool TableHasExpectedSchema(AppDbContext context)
    {
        if (!context.Database.CanConnect())
        {
            return false;
        }

        try
        {
            int columnCount = context.Database
                .SqlQueryRaw<int>(
                    """
                    SELECT COUNT(*)
                    FROM sys.columns c
                    INNER JOIN sys.tables t ON c.object_id = t.object_id
                    WHERE t.name = N'Products'
                      AND t.schema_id = SCHEMA_ID(N'dbo')
                      AND c.name IN (N'ProductId', N'ProductName', N'UnitPrice', N'StockQuantity', N'IsDiscontinued');
                    """)
                .AsEnumerable()
                .FirstOrDefault();

            return columnCount == 5;
        }
        catch (SqlException)
        {
            return false;
        }
    }
}
