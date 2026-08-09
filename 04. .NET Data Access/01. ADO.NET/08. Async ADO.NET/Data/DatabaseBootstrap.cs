using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace AsyncAdoNet.Data;

/*
 * FILE ROLE:
 *   LocalDB connection strings and async schema bootstrap for AdoNetTutorial.
 *
 * SECTIONS IN THIS FILE:
 *   2. Connection strings and database bootstrap
 */

/*
 * SECTION 2: CONNECTION STRINGS AND DATABASE BOOTSTRAP
 *
 * Database: AdoNetTutorial on LocalDB (same name used across ADO.NET chapters).
 * Bootstrap runs once at startup; failures are caught so later sections still run
 * where they do not require a live database (e.g. cancellation demo).
 * -------------------------------------------------------------------------
 */
public static class DatabaseBootstrap
{
    public const string Server = @"(localdb)\MSSQLLocalDB";

    public static string MasterConnectionString { get; } =
        $"Server={Server};Integrated Security=true;TrustServerCertificate=true;";

    public static string AppConnectionString { get; } =
        $"Server={Server};Database=AdoNetTutorial;Integrated Security=true;TrustServerCertificate=true;";

    public static async Task<bool> TryEnsureSchemaAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using (SqlConnection master = new SqlConnection(MasterConnectionString))
            {
                await master.OpenAsync(cancellationToken);
                await using SqlCommand createDb = master.CreateCommand();
                createDb.CommandText =
                    """
                    IF DB_ID(N'AdoNetTutorial') IS NULL
                        CREATE DATABASE AdoNetTutorial;
                    """;
                await createDb.ExecuteNonQueryAsync(cancellationToken);
            }

            await using SqlConnection app = new SqlConnection(AppConnectionString);
            await app.OpenAsync(cancellationToken);

            await using (SqlCommand createTable = app.CreateCommand())
            {
                createTable.CommandText =
                    """
                    IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.Products
                        (
                            ProductId  INT IDENTITY(1,1) PRIMARY KEY,
                            Name       NVARCHAR(100) NOT NULL,
                            UnitPrice  DECIMAL(10,2) NOT NULL,
                            Stock      INT NOT NULL
                        );
                    END
                    """;
                await createTable.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (SqlCommand seed = app.CreateCommand())
            {
                seed.CommandText =
                    """
                    IF NOT EXISTS (SELECT 1 FROM dbo.Products)
                    BEGIN
                        INSERT INTO dbo.Products (Name, UnitPrice, Stock) VALUES
                            (N'Wireless Mouse', 29.99, 120),
                            (N'Mechanical Keyboard', 89.50, 45),
                            (N'USB-C Hub', 49.00, 80);
                    END
                    """;
                await seed.ExecuteNonQueryAsync(cancellationToken);
            }

            return true;
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"  [Bootstrap] LocalDB unavailable: {ex.Message}");
            return false;
        }
    }
}
