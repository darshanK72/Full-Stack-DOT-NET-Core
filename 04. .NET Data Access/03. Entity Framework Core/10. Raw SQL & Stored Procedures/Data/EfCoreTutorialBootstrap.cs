using System;
using Microsoft.Data.SqlClient;
using RawSqlAndStoredProcedures.Utils;

namespace RawSqlAndStoredProcedures.Data;

/*
 * FILE ROLE: Creates EfCoreTutorial on LocalDB, seeds dbo.Products, and installs stored
 *            procedures documented in Program.cs comments.
 *
 * SECTIONS IN THIS FILE:
 *   5. Runtime bootstrap — TryEnsureReady()
 */

/*
 * SECTION 5: RUNTIME BOOTSTRAP — TryEnsureReady()
 *
 * Mirrors ADO.NET AdoNetTutorialDatabase and Dapper AdoNetTutorialBootstrap patterns, but
 * targets database EfCoreTutorial so EF demos stay isolated from AdoNetTutorial.
 *
 * Returns false when LocalDB is unavailable — Main prints a skip message and exits cleanly.
 *
 * --- Manual setup (SSMS / sqlcmd) — same objects as EnsureStoredProcedures below ---
 *
 * USE EfCoreTutorial;
 * GO
 *
 * CREATE OR ALTER PROCEDURE dbo.usp_GetProductsByMinStock
 *     @MinStock INT
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
 *     FROM dbo.Products
 *     WHERE StockQuantity >= @MinStock
 *     ORDER BY ProductId;
 * END;
 * GO
 *
 * CREATE OR ALTER PROCEDURE dbo.usp_GetAllProducts
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
 *     FROM dbo.Products
 *     ORDER BY ProductId;
 * END;
 * GO
 *
 * CREATE OR ALTER PROCEDURE dbo.usp_GetLowStockReport
 *     @Threshold INT
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     SELECT
 *         ProductId,
 *         ProductName,
 *         StockQuantity,
 *         CASE WHEN StockQuantity = 0 THEN N'Out of stock' ELSE N'Low stock' END AS Status
 *     FROM dbo.Products
 *     WHERE StockQuantity <= @Threshold
 *     ORDER BY StockQuantity, ProductId;
 * END;
 * GO
 *
 * CREATE OR ALTER PROCEDURE dbo.usp_ApplyBulkDiscount
 *     @CategoryPrefix NVARCHAR(20),
 *     @DiscountPct    DECIMAL(5, 2)
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     UPDATE dbo.Products
 *     SET UnitPrice = ROUND(UnitPrice * (1 - @DiscountPct / 100.0), 2)
 *     WHERE ProductName LIKE @CategoryPrefix + N'%';
 * END;
 * GO
 */
public static class EfCoreTutorialBootstrap
{
    public static bool TryEnsureReady(out string? failureMessage)
    {
        try
        {
            EnsureDatabase();
        using SqlConnection connection = new SqlConnection(ConnectionHelper.ConnectionString);
        connection.Open();

        if (!TableHasExpectedSchema(connection))
        {
            ResetProductsTable(connection);
        }
        else
        {
            EnsureProductsTable(connection);
        }

        EnsureSeedData(connection);
        EnsureStoredProcedures(connection);
            failureMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            failureMessage = ex.Message;
            return false;
        }
    }

    private static void EnsureDatabase()
    {
        using SqlConnection master = new SqlConnection(ConnectionHelper.MasterConnectionString);
        master.Open();
        using SqlCommand createDb = new SqlCommand(
            """
            IF DB_ID(@dbName) IS NULL
                CREATE DATABASE [EfCoreTutorial];
            """,
            master);
        createDb.Parameters.AddWithValue("@dbName", ConnectionHelper.DatabaseName);
        createDb.ExecuteNonQuery();
    }

    private static void EnsureProductsTable(SqlConnection connection)
    {
        using SqlCommand cmd = new SqlCommand(
            """
            IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Products
                (
                    ProductId        INT            NOT NULL PRIMARY KEY,
                    ProductName      NVARCHAR(100)  NOT NULL,
                    UnitPrice        DECIMAL(10, 2) NOT NULL,
                    StockQuantity    INT            NOT NULL,
                    DiscontinuedDate DATE           NULL
                );
            END
            """,
            connection);
        cmd.ExecuteNonQuery();
    }

    private static void ResetProductsTable(SqlConnection connection)
    {
        using SqlCommand cmd = new SqlCommand(
            """
            IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL
                DROP TABLE dbo.Products;

            CREATE TABLE dbo.Products
            (
                ProductId        INT            NOT NULL PRIMARY KEY,
                ProductName      NVARCHAR(100)  NOT NULL,
                UnitPrice        DECIMAL(10, 2) NOT NULL,
                StockQuantity    INT            NOT NULL,
                DiscontinuedDate DATE           NULL
            );
            """,
            connection);
        cmd.ExecuteNonQuery();
    }

    private static bool TableHasExpectedSchema(SqlConnection connection)
    {
        using SqlCommand cmd = new SqlCommand(
            """
            SELECT COUNT(*)
            FROM sys.columns c
            INNER JOIN sys.tables t ON c.object_id = t.object_id
            WHERE t.name = N'Products'
              AND t.schema_id = SCHEMA_ID(N'dbo')
              AND c.name IN (N'ProductId', N'ProductName', N'UnitPrice', N'StockQuantity', N'DiscontinuedDate');
            """,
            connection);
        return (int)cmd.ExecuteScalar()! == 5;
    }

    private static void EnsureSeedData(SqlConnection connection)
    {
        using SqlCommand countCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Products;", connection);
        int existing = (int)countCmd.ExecuteScalar()!;
        if (existing > 0)
        {
            return;
        }

        using SqlCommand seed = new SqlCommand(
            """
            INSERT INTO dbo.Products (ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate)
            VALUES
                (1, N'Widget A',  9.99,  100, NULL),
                (2, N'Widget B', 14.50,   50, NULL),
                (3, N'Gadget C', 29.99,   25, '2023-06-01'),
                (4, N'Gadget D', 45.00,    0, '2024-01-15'),
                (5, N'Part E',    2.50,  500, NULL),
                (6, N'Part F',    3.75,  300, NULL);
            """,
            connection);
        seed.ExecuteNonQuery();
    }

    private static void EnsureStoredProcedures(SqlConnection connection)
    {
        ExecuteBatch(connection, """
            CREATE OR ALTER PROCEDURE dbo.usp_GetProductsByMinStock
                @MinStock INT
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
                FROM dbo.Products
                WHERE StockQuantity >= @MinStock
                ORDER BY ProductId;
            END;
            """);

        ExecuteBatch(connection, """
            CREATE OR ALTER PROCEDURE dbo.usp_GetAllProducts
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
                FROM dbo.Products
                ORDER BY ProductId;
            END;
            """);

        ExecuteBatch(connection, """
            CREATE OR ALTER PROCEDURE dbo.usp_GetLowStockReport
                @Threshold INT
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT
                    ProductId,
                    ProductName,
                    StockQuantity,
                    CASE WHEN StockQuantity = 0 THEN N'Out of stock' ELSE N'Low stock' END AS Status
                FROM dbo.Products
                WHERE StockQuantity <= @Threshold
                ORDER BY StockQuantity, ProductId;
            END;
            """);

        ExecuteBatch(connection, """
            CREATE OR ALTER PROCEDURE dbo.usp_ApplyBulkDiscount
                @CategoryPrefix NVARCHAR(20),
                @DiscountPct    DECIMAL(5, 2)
            AS
            BEGIN
                SET NOCOUNT ON;
                UPDATE dbo.Products
                SET UnitPrice = ROUND(UnitPrice * (1 - @DiscountPct / 100.0), 2)
                WHERE ProductName LIKE @CategoryPrefix + N'%';
            END;
            """);
    }

    private static void ExecuteBatch(SqlConnection connection, string sql)
    {
        using SqlCommand cmd = new SqlCommand(sql, connection);
        cmd.ExecuteNonQuery();
    }
}
