using Microsoft.Data.SqlClient;

namespace ParametersStoredProceduresAndQueryMultiple.Utils;

/*
 * FILE ROLE: Connection string for AdoNetTutorial LocalDB and runtime bootstrap
 *            (database, Products table, seed rows, stored procedures for this chapter).
 *
 * SECTIONS IN THIS FILE:
 *   7. Runtime bootstrap — EnsureReady()
 */

/*
 * SECTION 7: RUNTIME BOOTSTRAP — EnsureReady()
 *
 * Creates AdoNetTutorial on (localdb)\MSSQLLocalDB if missing, ensures the Products
 * table matches the schema from ADO.NET ch.04 SqlDataReader, seeds sample rows,
 * and creates or alters the stored procedures documented in Program.cs.
 *
 * One-time manual setup: run the CREATE PROC batch in Program.cs comments in SSMS
 * if you prefer not to let the demo create objects at runtime.
 *
 * COVERED IN ADO.NET ch.02 → SqlConnection & Connection Strings (connection strings).
 * COVERED IN ADO.NET ch.07 → Stored Procedures & Output Parameters (T-SQL proc patterns).
 */
public static class AdoNetTutorialBootstrap
{
    public const string DatabaseName = "AdoNetTutorial";

    public static string ConnectionString { get; } =
        "Server=(localdb)\\MSSQLLocalDB;" +
        $"Database={DatabaseName};" +
        "Integrated Security=true;" +
        "TrustServerCertificate=true;";

    private static string MasterConnectionString { get; } =
        "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;";

    public static void EnsureReady()
    {
        using (var master = new SqlConnection(MasterConnectionString))
        {
            master.Open();
            using var createDb = new SqlCommand(
                """
                IF DB_ID(@dbName) IS NULL
                    CREATE DATABASE [AdoNetTutorial];
                """,
                master);
            createDb.Parameters.AddWithValue("@dbName", DatabaseName);
            createDb.ExecuteNonQuery();
        }

        using var connection = new SqlConnection(ConnectionString);
        connection.Open();

        EnsureProductsTable(connection);
        EnsureSeedData(connection);
        EnsureStoredProcedures(connection);
    }

    private static void EnsureProductsTable(SqlConnection connection)
    {
        using var cmd = new SqlCommand(
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

    private static void EnsureSeedData(SqlConnection connection)
    {
        using var countCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Products;", connection);
        int existing = (int)countCmd.ExecuteScalar()!;
        if (existing > 0)
        {
            return;
        }

        using var seed = new SqlCommand(
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
            CREATE OR ALTER PROCEDURE dbo.usp_UpdateProductPrice
                @ProductId INT,
                @NewPrice  DECIMAL(10, 2)
            AS
            BEGIN
                SET NOCOUNT ON;
                UPDATE dbo.Products
                SET UnitPrice = @NewPrice
                WHERE ProductId = @ProductId;
            END;
            """);

        ExecuteBatch(connection, """
            CREATE OR ALTER PROCEDURE dbo.usp_InsertProduct
                @ProductName   NVARCHAR(100),
                @UnitPrice     DECIMAL(10, 2),
                @StockQuantity INT,
                @NewProductId  INT OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
                DECLARE @NextId INT = (SELECT ISNULL(MAX(ProductId), 0) + 1 FROM dbo.Products);
                INSERT INTO dbo.Products (ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate)
                VALUES (@NextId, @ProductName, @UnitPrice, @StockQuantity, NULL);
                SET @NewProductId = @NextId;
            END;
            """);

        ExecuteBatch(connection, """
            CREATE OR ALTER PROCEDURE dbo.usp_GetProductName
                @ProductId   INT,
                @ProductName NVARCHAR(100) OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT @ProductName = ProductName
                FROM dbo.Products
                WHERE ProductId = @ProductId;

                IF @ProductName IS NULL
                    SET @ProductName = N'';
            END;
            """);

        ExecuteBatch(connection, """
            CREATE OR ALTER PROCEDURE dbo.usp_GetProductCount
            AS
            BEGIN
                SET NOCOUNT ON;
                DECLARE @Count INT = (SELECT COUNT(*) FROM dbo.Products);
                RETURN @Count;
            END;
            """);

        ExecuteBatch(connection, """
            CREATE OR ALTER PROCEDURE dbo.usp_TryGetProductName
                @ProductId   INT,
                @ProductName NVARCHAR(100) OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT @ProductName = ProductName
                FROM dbo.Products
                WHERE ProductId = @ProductId;

                IF @ProductName IS NULL
                BEGIN
                    SET @ProductName = N'';
                    RETURN 1;
                END
                RETURN 0;
            END;
            """);

        ExecuteBatch(connection, """
            CREATE OR ALTER PROCEDURE dbo.usp_GetProductDashboard
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
                FROM dbo.Products
                ORDER BY ProductId;

                SELECT COUNT(*) AS TotalCount FROM dbo.Products;

                SELECT TOP 1 ProductName AS MostExpensive
                FROM dbo.Products
                ORDER BY UnitPrice DESC;
            END;
            """);
    }

    private static void ExecuteBatch(SqlConnection connection, string sql)
    {
        using var cmd = new SqlCommand(sql, connection);
        cmd.ExecuteNonQuery();
    }
}
