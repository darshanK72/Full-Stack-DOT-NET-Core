using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChangeTrackingAsyncAndTransactions.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ChangeTrackingAsyncAndTransactions.Data;

/*
 * FILE ROLE:
 *   Creates EfCoreInventoryTutorial on LocalDB and seeds Products for chapter demos.
 *
 * SECTIONS IN THIS FILE:
 *   4. Database bootstrap (EnsureCreated + seed)
 */

/*
 * SECTION 4: DATABASE BOOTSTRAP (ENSURECREATED + SEED)
 *
 * TryEnsureSchemaAsync:
 *   1. Creates the database on LocalDB if missing (master connection)
 *   2. Calls context.Database.EnsureCreated() for dbo.Products
 *   3. Seeds sample rows when the table is empty
 *
 * Pitfall: EnsureCreated does not evolve an existing schema — use migrations in real apps.
 * -------------------------------------------------------------------------
 */
public static class DatabaseBootstrap
{
    private const string MasterConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;";

    public static async Task<bool> TryEnsureSchemaAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureDatabaseExistsAsync(cancellationToken).ConfigureAwait(false);

            DbContextOptions<InventoryDbContext> options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseSqlServer(InventoryDbContext.ConnectionString)
                .Options;

            await using InventoryDbContext context = new InventoryDbContext(options);
            await RecreateSchemaIfNeededAsync(context, cancellationToken).ConfigureAwait(false);
            await SeedProductsIfEmptyAsync(context, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"  [Bootstrap] LocalDB unavailable: {ex.Message}");
            return false;
        }
    }

    private static async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken)
    {
        await using SqlConnection master = new SqlConnection(MasterConnectionString);
        await master.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using SqlCommand createDb = master.CreateCommand();
        createDb.CommandText =
            """
            IF DB_ID(N'EfCoreInventoryTutorial') IS NULL
                CREATE DATABASE EfCoreInventoryTutorial;
            """;
        await createDb.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task RecreateSchemaIfNeededAsync(
        InventoryDbContext context,
        CancellationToken cancellationToken)
    {
        // EnsureCreated skips existing DB — drop when Products has IDENTITY (wrong for our seed).
        await using SqlConnection connection = new SqlConnection(InventoryDbContext.ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        bool tableExists = await ScalarIntAsync(
            connection,
            "SELECT CASE WHEN OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL THEN 1 ELSE 0 END;",
            cancellationToken).ConfigureAwait(false) == 1;

        if (tableExists)
        {
            bool productIdIsIdentity = await ScalarIntAsync(
                connection,
                """
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM sys.columns c
                    INNER JOIN sys.tables t ON c.object_id = t.object_id
                    WHERE t.name = N'Products' AND t.schema_id = SCHEMA_ID(N'dbo')
                      AND c.name = N'ProductId' AND c.is_identity = 1
                ) THEN 1 ELSE 0 END;
                """,
                cancellationToken).ConfigureAwait(false) == 1;

            if (productIdIsIdentity)
            {
                await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<int> ScalarIntAsync(
        SqlConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = sql;
        object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(result);
    }

    private static async Task SeedProductsIfEmptyAsync(
        InventoryDbContext context,
        CancellationToken cancellationToken)
    {
        bool hasProducts = await context.Products.AnyAsync(cancellationToken).ConfigureAwait(false);
        if (hasProducts)
        {
            return;
        }

        context.Products.AddRange(
            new Product { ProductId = 1, ProductName = "Widget A", UnitPrice = 9.99m, StockQuantity = 100 },
            new Product { ProductId = 2, ProductName = "Widget B", UnitPrice = 14.50m, StockQuantity = 50 },
            new Product { ProductId = 3, ProductName = "Gadget C", UnitPrice = 29.99m, StockQuantity = 25 });

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
