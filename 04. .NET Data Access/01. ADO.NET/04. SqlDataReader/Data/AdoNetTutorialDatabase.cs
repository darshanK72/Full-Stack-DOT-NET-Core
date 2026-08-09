using Microsoft.Data.SqlClient;

namespace SqlDataReader;

/*
 * FILE ROLE: Creates and seeds the AdoNetTutorial LocalDB sample database used by every
 *            SqlDataReader demo in this chapter.
 *
 * SECTIONS IN THIS FILE:
 *   1. Sample database — AdoNetTutorial on LocalDB
 */

/*
 * =========================================================================
 * SECTION 1: SAMPLE DATABASE — AdoNetTutorial ON LOCALDB
 * =========================================================================
 *
 * Hands-on demos use a small Products table in AdoNetTutorial on
 * (localdb)\MSSQLLocalDB. EnsureSampleData creates the database, table,
 * and seed rows on first run.
 *
 * ch.02 SqlConnection & Connection Strings covers connection strings in depth.
 * ch.03 SqlCommand & Parameters covers SqlCommand and parameters in depth.
 * Here we assume an open connection and focus on reading rows.
 * -------------------------------------------------------------------------
 */
public static class AdoNetTutorialDatabase
{
    public const string Server = @"(localdb)\MSSQLLocalDB";
    public const string DatabaseName = "AdoNetTutorial";

    public static string MasterConnectionString { get; } =
        $"Server={Server};Database=master;Integrated Security=true;TrustServerCertificate=true;";

    public static string ConnectionString { get; } =
        $"Server={Server};Database={DatabaseName};Integrated Security=true;TrustServerCertificate=true;";

    public static void EnsureSampleData()
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

        if (!TableHasExpectedSchema(connection))
        {
            using var reset = new SqlCommand(
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
            reset.ExecuteNonQuery();
        }

        using (var countCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Products;", connection))
        {
            int existing = (int)countCmd.ExecuteScalar()!;
            if (existing > 0)
            {
                return;
            }
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

    private static bool TableHasExpectedSchema(SqlConnection connection)
    {
        using var cmd = new SqlCommand(
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
}
