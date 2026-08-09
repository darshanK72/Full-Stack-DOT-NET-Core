using System;
using Microsoft.Data.SqlClient;

namespace DapperMappingAndAdvancedPatterns.Data;

/*
 * FILE ROLE:
 *   Creates AdoNetTutorial schema and seed data for mapping and multi-mapping demos.
 *
 * SECTIONS IN THIS FILE:
 *   1. Database bootstrap — DDL and seed SQL (documented below)
 */

/*
 * SECTION 1: DATABASE BOOTSTRAP — DDL AND SEED SQL
 *
 * Products table schema matches ADO.NET ch.04 / Dapper ch.01-03 (ProductName, StockQuantity).
 * EnsureProductsTable creates or resets dbo.Products when missing or on an older schema.
 * This chapter adds Customers, Orders, and OrderLines for JOIN demos.
 *
 *   CREATE TABLE dbo.Customers (
 *       CustomerId  INT IDENTITY(1,1) PRIMARY KEY,
 *       Name        NVARCHAR(100) NOT NULL,
 *       Email       NVARCHAR(200) NOT NULL
 *   );
 *
 *   CREATE TABLE dbo.Orders (
 *       OrderId     INT IDENTITY(1,1) PRIMARY KEY,
 *       CustomerId  INT NOT NULL REFERENCES dbo.Customers(CustomerId),
 *       OrderDate   DATE NOT NULL,
 *       TotalAmount DECIMAL(10,2) NOT NULL
 *   );
 *
 *   CREATE TABLE dbo.OrderLines (
 *       OrderLineId INT IDENTITY(1,1) PRIMARY KEY,
 *       OrderId     INT NOT NULL REFERENCES dbo.Orders(OrderId),
 *       ProductId   INT NOT NULL,
 *       Quantity    INT NOT NULL,
 *       LineTotal   DECIMAL(10,2) NOT NULL
 *   );
 * -------------------------------------------------------------------------
 */
public static class DatabaseBootstrap
{
    private const string MasterConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;";

    public static bool TryEnsureSchema(string appConnectionString)
    {
        try
        {
            using (SqlConnection master = new SqlConnection(MasterConnectionString))
            {
                master.Open();
                using SqlCommand createDb = master.CreateCommand();
                createDb.CommandText =
                    """
                    IF DB_ID(N'AdoNetTutorial') IS NULL
                        CREATE DATABASE AdoNetTutorial;
                    """;
                createDb.ExecuteNonQuery();
            }

            using SqlConnection app = new SqlConnection(appConnectionString);
            app.Open();

            EnsureProductsTable(app);

            ExecuteBatch(app, """
                IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Customers
                    (
                        CustomerId INT IDENTITY(1,1) PRIMARY KEY,
                        Name       NVARCHAR(100) NOT NULL,
                        Email      NVARCHAR(200) NOT NULL
                    );
                END
                """);

            ExecuteBatch(app, """
                IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Orders
                    (
                        OrderId     INT IDENTITY(1,1) PRIMARY KEY,
                        CustomerId  INT NOT NULL,
                        OrderDate   DATE NOT NULL,
                        TotalAmount DECIMAL(10,2) NOT NULL,
                        CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerId)
                            REFERENCES dbo.Customers(CustomerId)
                    );
                END
                """);

            ExecuteBatch(app, """
                IF OBJECT_ID(N'dbo.OrderLines', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.OrderLines
                    (
                        OrderLineId INT IDENTITY(1,1) PRIMARY KEY,
                        OrderId     INT NOT NULL,
                        ProductId   INT NOT NULL,
                        Quantity    INT NOT NULL,
                        LineTotal   DECIMAL(10,2) NOT NULL,
                        CONSTRAINT FK_OrderLines_Orders FOREIGN KEY (OrderId)
                            REFERENCES dbo.Orders(OrderId)
                    );
                END
                """);

            ExecuteBatch(app, """
                IF NOT EXISTS (SELECT 1 FROM dbo.Customers)
                BEGIN
                    INSERT INTO dbo.Customers (Name, Email) VALUES
                        (N'Alice Chen', N'alice@example.com'),
                        (N'Bob Rivera', N'bob@example.com');
                END
                """);

            ExecuteBatch(app, """
                IF NOT EXISTS (SELECT 1 FROM dbo.Orders)
                BEGIN
                    INSERT INTO dbo.Orders (CustomerId, OrderDate, TotalAmount) VALUES
                        (1, '2024-06-01', 24.49),
                        (1, '2024-07-15', 9.99),
                        (2, '2024-08-02', 14.50);
                END
                """);

            ExecuteBatch(app, """
                IF NOT EXISTS (SELECT 1 FROM dbo.OrderLines)
                BEGIN
                    INSERT INTO dbo.OrderLines (OrderId, ProductId, Quantity, LineTotal) VALUES
                        (1, 1, 1, 9.99),
                        (1, 2, 1, 14.50),
                        (2, 1, 1, 9.99),
                        (3, 2, 1, 14.50);
                END
                """);

            return TableExists(app);
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"  [Bootstrap] LocalDB unavailable: {ex.Message}");
            return false;
        }
    }

    private static void EnsureProductsTable(SqlConnection connection)
    {
        if (!ProductsTableHasExpectedSchema(connection))
        {
            ExecuteBatch(connection, """
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
                """);
        }

        using SqlCommand countCmd = connection.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM dbo.Products;";
        if ((int)countCmd.ExecuteScalar()! > 0)
        {
            return;
        }

        ExecuteBatch(connection, """
            INSERT INTO dbo.Products (ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate)
            VALUES
                (1, N'Widget A',  9.99,  100, NULL),
                (2, N'Widget B', 14.50,   50, NULL),
                (3, N'Gadget C', 29.99,   25, '2023-06-01'),
                (4, N'Gadget D', 45.00,    0, '2024-01-15'),
                (5, N'Part E',    2.50,  500, NULL),
                (6, N'Part F',    3.75,  300, NULL);
            """);
    }

    private static bool ProductsTableHasExpectedSchema(SqlConnection connection)
    {
        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT COUNT(*)
            FROM sys.columns c
            INNER JOIN sys.tables t ON c.object_id = t.object_id
            WHERE t.name = N'Products'
              AND t.schema_id = SCHEMA_ID(N'dbo')
              AND c.name IN (N'ProductId', N'ProductName', N'UnitPrice', N'StockQuantity', N'DiscontinuedDate');
            """;
        return (int)command.ExecuteScalar()! == 5;
    }

    private static bool TableExists(SqlConnection connection)
    {
        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT CASE WHEN OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL THEN 1 ELSE 0 END;
            """;
        return (int)command.ExecuteScalar()! == 1;
    }

    private static void ExecuteBatch(SqlConnection connection, string sql)
    {
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
