using Microsoft.Data.SqlClient;

namespace IntroductionToDapper.Utils;

/*
 * FILE ROLE: Ensures AdoNetTutorial LocalDB exists with the Products schema from
 *            ADO.NET ch.04 SqlDataReader (shared across all Dapper chapters).
 *
 * SECTIONS IN THIS FILE:
 *   9. Runtime bootstrap — EnsureReady()
 */

/*
 * SECTION 9: RUNTIME BOOTSTRAP — EnsureReady()
 *
 * Creates the database and dbo.Products if missing, resets the table when an older
 * schema is detected (e.g. Name/Stock/IsActive from ADO.NET ch.03), and seeds rows.
 *
 * Manual setup: run the DATABASE SETUP block in Program.cs comments in SSMS/sqlcmd.
 */
public static class AdoNetTutorialBootstrap
{
    public const string DatabaseName = "AdoNetTutorial";

    public static string ConnectionString { get; } = ConnectionHelper.LocalDbConnectionString;

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
            if ((int)countCmd.ExecuteScalar()! > 0)
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
