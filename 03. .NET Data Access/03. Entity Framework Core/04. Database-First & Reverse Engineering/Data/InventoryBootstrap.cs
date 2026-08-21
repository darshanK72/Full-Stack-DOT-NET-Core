using System;
using DatabaseFirstAndReverseEngineering.Utils;
using Microsoft.Data.SqlClient;

namespace DatabaseFirstAndReverseEngineering.Data;

/*
 * FILE ROLE:
 *   Creates EfScaffoldTutorial schema and seed data so reverse-engineering demos have a real database.
 *
 * SECTIONS IN THIS FILE:
 *   1. Database-First mindset - existing DB as source of truth
 *   2. Sample schema DDL (documented CREATE TABLE SQL)
 *   3. Bootstrap implementation
 */

/*
 * SECTION 1: DATABASE-FIRST MINDSET - EXISTING DB AS SOURCE OF TRUTH
 *
 * Code-First (ch.03): C# entity classes and migrations drive the schema.
 * Database-First (this chapter): a DBA or legacy application already owns the tables;
 *   EF Core reverse-engineers C# types from that schema.
 *
 * | Aspect              | Code-First              | Database-First                    |
 * |---------------------|-------------------------|-----------------------------------|
 * | Source of truth     | C# model + migrations   | SQL Server tables/views           |
 * | Schema changes      | Add-Migration / Update  | DBA scripts; then re-scaffold     |
 * | Typical scenario    | Greenfield apps         | Legacy DB, vendor schema, reports |
 * | EF tooling          | dotnet ef migrations    | dotnet ef dbcontext scaffold      |
 *
 * Pitfall: editing generated entity or DbContext files by hand. Re-scaffold overwrites them.
 *   Use partial classes (Models/*.Partial.cs) for custom members that survive re-scaffold.
 * -------------------------------------------------------------------------
 */

/*
 * SECTION 2: SAMPLE SCHEMA DDL (DOCUMENTED CREATE TABLE SQL)
 *
 * Run against EfScaffoldTutorial (or let TryEnsureSchema create equivalent objects).
 *
 *   CREATE TABLE dbo.Categories (
 *       CategoryId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
 *       Name         NVARCHAR(100)     NOT NULL,
 *       Description  NVARCHAR(500)     NULL
 *   );
 *
 *   CREATE TABLE dbo.Products (
 *       ProductId      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
 *       CategoryId     INT               NOT NULL,
 *       ProductName    NVARCHAR(100)     NOT NULL,
 *       UnitPrice      DECIMAL(18, 2)    NOT NULL,
 *       StockQuantity  INT               NOT NULL,
 *       CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId)
 *           REFERENCES dbo.Categories (CategoryId)
 *   );
 *
 * Extra schema for --schema filtering preview (not scaffolded in the tutorial project):
 *
 *   CREATE SCHEMA audit;
 *
 *   CREATE TABLE audit.ChangeLog (
 *       ChangeLogId  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
 *       TableName    NVARCHAR(128)     NOT NULL,
 *       ChangedAt    DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME()
 *   );
 * -------------------------------------------------------------------------
 */
public static class InventoryBootstrap
{
    private const string MasterConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;";

    /*
     * SECTION 3: BOOTSTRAP IMPLEMENTATION
     *
     * Creates database, dbo tables, audit.ChangeLog, and seed rows when LocalDB is available.
     * -------------------------------------------------------------------------
     */
    public static bool TryEnsureSchema()
    {
        try
        {
            using (SqlConnection master = new SqlConnection(MasterConnectionString))
            {
                master.Open();
                using SqlCommand createDb = master.CreateCommand();
                createDb.CommandText =
                    """
                    IF DB_ID(N'EfScaffoldTutorial') IS NULL
                        CREATE DATABASE EfScaffoldTutorial;
                    """;
                createDb.ExecuteNonQuery();
            }

            using SqlConnection app = new SqlConnection(ConnectionHelper.EfScaffoldTutorial);
            app.Open();

            ExecuteBatch(app, """
                IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Categories
                    (
                        CategoryId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        Name         NVARCHAR(100)     NOT NULL,
                        Description  NVARCHAR(500)     NULL
                    );
                END
                """);

            ExecuteBatch(app, """
                IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Products
                    (
                        ProductId      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        CategoryId     INT               NOT NULL,
                        ProductName    NVARCHAR(100)     NOT NULL,
                        UnitPrice      DECIMAL(18, 2)    NOT NULL,
                        StockQuantity  INT               NOT NULL,
                        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId)
                            REFERENCES dbo.Categories (CategoryId)
                    );
                END
                """);

            ExecuteBatch(app, """
                IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'audit')
                    EXEC(N'CREATE SCHEMA audit');
                """);

            ExecuteBatch(app, """
                IF OBJECT_ID(N'audit.ChangeLog', N'U') IS NULL
                BEGIN
                    CREATE TABLE audit.ChangeLog
                    (
                        ChangeLogId  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        TableName    NVARCHAR(128)     NOT NULL,
                        ChangedAt    DATETIME2         NOT NULL CONSTRAINT DF_ChangeLog_ChangedAt DEFAULT SYSUTCDATETIME()
                    );
                END
                """);

            ExecuteBatch(app, """
                IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
                BEGIN
                    INSERT INTO dbo.Categories (Name, Description) VALUES
                        (N'Widgets',  N'Standard widget line'),
                        (N'Gadgets',  N'Premium gadgets'),
                        (N'Parts',    N'Replacement parts');
                END
                """);

            ExecuteBatch(app, """
                IF NOT EXISTS (SELECT 1 FROM dbo.Products)
                BEGIN
                    INSERT INTO dbo.Products (CategoryId, ProductName, UnitPrice, StockQuantity) VALUES
                        (1, N'Widget A',  9.99, 100),
                        (1, N'Widget B', 14.50,  50),
                        (2, N'Gadget C', 29.99,  25),
                        (3, N'Part E',    2.50, 500);
                END
                """);

            return TableExists(app, "dbo", "Categories");
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"  [Bootstrap] LocalDB unavailable: {ex.Message}");
            return false;
        }
    }

    private static bool TableExists(SqlConnection connection, string schema, string table)
    {
        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT CASE WHEN OBJECT_ID(@FullName, N'U') IS NOT NULL THEN 1 ELSE 0 END;
            """;
        command.Parameters.AddWithValue("@FullName", $"{schema}.{table}");
        return (int)command.ExecuteScalar()! == 1;
    }

    private static void ExecuteBatch(SqlConnection connection, string sql)
    {
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
