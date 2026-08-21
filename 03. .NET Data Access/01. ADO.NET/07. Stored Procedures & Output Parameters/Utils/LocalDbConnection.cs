namespace StoredProceduresAndOutputParameters.Utils;

/*
 * FILE ROLE: LocalDB connection string and one-time setup scripts for AdoNetSpDemo.
 *
 * SECTIONS IN THIS FILE:
 *   1. LocalDB setup scripts (run once in SSMS or sqlcmd)
 */

/*
 * SECTION 1: LOCALDB SETUP SCRIPTS (RUN ONCE IN SSMS OR SQLCMD)
 *
 * Connection string used by this chapter (Integrated Security, LocalDB):
 *
 *   Server=(localdb)\MSSQLLocalDB;Database=AdoNetSpDemo;Integrated Security=true;TrustServerCertificate=true;
 *
 * Run the batch below once to create the sample database, table, and procedures.
 * Each GO separates T-SQL batches - required after CREATE DATABASE.
 *
 * -------------------------------------------------------------------------
 * -- 1) Create database
 * CREATE DATABASE AdoNetSpDemo;
 * GO
 *
 * USE AdoNetSpDemo;
 * GO
 *
 * -- 2) Sample table
 * CREATE TABLE dbo.Products
 * (
 *     ProductId   INT            IDENTITY(1, 1) NOT NULL PRIMARY KEY,
 *     ProductName NVARCHAR(100)  NOT NULL,
 *     UnitPrice   DECIMAL(10, 2) NOT NULL
 * );
 * GO
 *
 * INSERT INTO dbo.Products (ProductName, UnitPrice)
 * VALUES (N'Wireless Mouse', 29.99),
 *        (N'USB-C Hub', 49.50),
 *        (N'Mechanical Keyboard', 129.00);
 * GO
 *
 * -- 3) Input-only procedure - updates price, returns rows affected via ExecuteNonQuery
 * CREATE OR ALTER PROCEDURE dbo.usp_UpdateProductPrice
 *     @ProductId INT,
 *     @NewPrice  DECIMAL(10, 2)
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     UPDATE dbo.Products
 *     SET UnitPrice = @NewPrice
 *     WHERE ProductId = @ProductId;
 * END;
 * GO
 *
 * -- 4) Output parameter - returns new identity value after INSERT
 * CREATE OR ALTER PROCEDURE dbo.usp_InsertProduct
 *     @ProductName  NVARCHAR(100),
 *     @UnitPrice    DECIMAL(10, 2),
 *     @NewProductId INT OUTPUT
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     INSERT INTO dbo.Products (ProductName, UnitPrice)
 *     VALUES (@ProductName, @UnitPrice);
 *     SET @NewProductId = SCOPE_IDENTITY();
 * END;
 * GO
 *
 * -- 5) Output string parameter - Size in ADO.NET must be >= NVARCHAR length
 * CREATE OR ALTER PROCEDURE dbo.usp_GetProductName
 *     @ProductId   INT,
 *     @ProductName NVARCHAR(100) OUTPUT
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     SELECT @ProductName = ProductName
 *     FROM dbo.Products
 *     WHERE ProductId = @ProductId;
 *
 *     IF @ProductName IS NULL
 *         SET @ProductName = N'';  -- not found - client checks empty or rows affected
 * END;
 * GO
 *
 * -- 6) Return value - T-SQL RETURN sends an int to ParameterDirection.ReturnValue
 * CREATE OR ALTER PROCEDURE dbo.usp_GetProductCount
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     DECLARE @Count INT = (SELECT COUNT(*) FROM dbo.Products);
 *     RETURN @Count;  -- RETURN must be int; separate from OUTPUT parameters
 * END;
 * GO
 *
 * -- 7) Combined - OUTPUT + RETURN status code (0 = found, 1 = missing)
 * CREATE OR ALTER PROCEDURE dbo.usp_TryGetProductName
 *     @ProductId   INT,
 *     @ProductName NVARCHAR(100) OUTPUT
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     SELECT @ProductName = ProductName
 *     FROM dbo.Products
 *     WHERE ProductId = @ProductId;
 *
 *     IF @ProductName IS NULL
 *     BEGIN
 *         SET @ProductName = N'';
 *         RETURN 1;
 *     END
 *     RETURN 0;
 * END;
 * GO
 * -------------------------------------------------------------------------
 *
 * Pitfall: Forgetting SET NOCOUNT ON makes ExecuteNonQuery return extra counts
 * from internal SELECT statements - confusing when you expect one result.
 * -------------------------------------------------------------------------
 */
public static class LocalDbConnection
{
    public const string DatabaseName = "AdoNetSpDemo";

    public static string ConnectionString { get; } =
        "Server=(localdb)\\MSSQLLocalDB;" +
        $"Database={DatabaseName};" +
        "Integrated Security=true;" +
        "TrustServerCertificate=true;";
}
