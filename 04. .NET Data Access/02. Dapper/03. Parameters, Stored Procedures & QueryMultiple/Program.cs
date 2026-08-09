/*
 * TOPIC: Dapper parameters ù anonymous objects, DynamicParameters, stored procedures,
 *        QueryMultiple/GridReader, and CommandDefinition.
 *
 * WHY IT MATTERS:
 *   Production data access rarely uses parameterless SQL. You pass filters, call stored
 *   procedures with OUTPUT/RETURN codes, and consume multiple result sets from one round
 *   trip. Dapper mirrors ADO.NET parameter semantics without SqlParameter boilerplate for
 *   simple Input cases ù but DynamicParameters is required for Output and ReturnValue.
 *
 * WHAT YOU WILL LEARN:
 *   1. Product row type and column-to-property mapping
 *   2. Parameter naming ù @ conventions in SQL vs C# property names
 *   3. Anonymous object parameters ù new { Id = 1, Name = "x" }
 *   4. DynamicParameters ù Add, direction, Output, ReturnValue, AddDynamicParams
 *   5. Stored procedures ù commandType: CommandType.StoredProcedure
 *   6. QueryMultiple + GridReader ù multiple result sets in one round trip
 *   7. CommandDefinition ù encapsulate sql, params, timeout, commandType, flags
 *   8. Preview: table-valued parameters (TVPs)
 *
 * Prerequisites: Dapper ch.01ù02; ADO.NET ch.03 (parameters), ch.07 (stored procedures).
 * Database: AdoNetTutorial on (localdb)\MSSQLLocalDB ù bootstrap in Utils/AdoNetTutorialBootstrap.cs.
 *
 * CHAPTER MAP:
 *   1. Product row type                 ? Models/Product.cs
 *   2. @ naming conventions             ? Repositories/AnonymousParameterRepository.cs
 *   3. Anonymous object parameters      ? Repositories/AnonymousParameterRepository.cs
 *   4. DynamicParameters                ? Repositories/DynamicParameterRepository.cs
 *   5. Stored procedures                ? Repositories/StoredProcedureRepository.cs
 *   6. QueryMultiple / GridReader       ? Repositories/QueryMultipleRepository.cs
 *   7. CommandDefinition                ? Repositories/CommandDefinitionRepository.cs
 *   8. TVP preview                      ? Repositories/DynamicParameterRepository.cs
 *   9. Runtime bootstrap                ? Utils/AdoNetTutorialBootstrap.cs
 *  10. Demonstration                     ? Program.cs Main
 *
 * -------------------------------------------------------------------------
 * DATABASE SETUP ù STORED PROCEDURES (run once in SSMS or sqlcmd if not using bootstrap)
 *
 * Connection: Server=(localdb)\MSSQLLocalDB;Database=AdoNetTutorial;Integrated Security=true;TrustServerCertificate=true
 *
 * Requires dbo.Products from ADO.NET SqlDataReader chapter (ProductId, ProductName,
 * UnitPrice, StockQuantity, DiscontinuedDate). Then create procedures:
 *
 * CREATE OR ALTER PROCEDURE dbo.usp_UpdateProductPrice
 *     @ProductId INT,
 *     @NewPrice  DECIMAL(10, 2)
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     UPDATE dbo.Products SET UnitPrice = @NewPrice WHERE ProductId = @ProductId;
 * END;
 * GO
 *
 * CREATE OR ALTER PROCEDURE dbo.usp_InsertProduct
 *     @ProductName   NVARCHAR(100),
 *     @UnitPrice     DECIMAL(10, 2),
 *     @StockQuantity INT,
 *     @NewProductId  INT OUTPUT
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     DECLARE @NextId INT = (SELECT ISNULL(MAX(ProductId), 0) + 1 FROM dbo.Products);
 *     INSERT INTO dbo.Products (ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate)
 *     VALUES (@NextId, @ProductName, @UnitPrice, @StockQuantity, NULL);
 *     SET @NewProductId = @NextId;
 * END;
 * GO
 *
 * CREATE OR ALTER PROCEDURE dbo.usp_GetProductName
 *     @ProductId   INT,
 *     @ProductName NVARCHAR(100) OUTPUT
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     SELECT @ProductName = ProductName FROM dbo.Products WHERE ProductId = @ProductId;
 *     IF @ProductName IS NULL SET @ProductName = N'';
 * END;
 * GO
 *
 * CREATE OR ALTER PROCEDURE dbo.usp_GetProductCount
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     DECLARE @Count INT = (SELECT COUNT(*) FROM dbo.Products);
 *     RETURN @Count;
 * END;
 * GO
 *
 * CREATE OR ALTER PROCEDURE dbo.usp_TryGetProductName
 *     @ProductId   INT,
 *     @ProductName NVARCHAR(100) OUTPUT
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     SELECT @ProductName = ProductName FROM dbo.Products WHERE ProductId = @ProductId;
 *     IF @ProductName IS NULL BEGIN SET @ProductName = N''; RETURN 1; END
 *     RETURN 0;
 * END;
 * GO
 *
 * CREATE OR ALTER PROCEDURE dbo.usp_GetProductDashboard
 * AS
 * BEGIN
 *     SET NOCOUNT ON;
 *     SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
 *     FROM dbo.Products ORDER BY ProductId;
 *     SELECT COUNT(*) AS TotalCount FROM dbo.Products;
 *     SELECT TOP 1 ProductName AS MostExpensive FROM dbo.Products ORDER BY UnitPrice DESC;
 * END;
 * GO
 * -------------------------------------------------------------------------
 */

using System;
using Microsoft.Data.SqlClient;
using ParametersStoredProceduresAndQueryMultiple.Models;
using ParametersStoredProceduresAndQueryMultiple.Repositories;
using ParametersStoredProceduresAndQueryMultiple.Utils;

namespace ParametersStoredProceduresAndQueryMultiple;

public class Program
{
    /*
     * SECTION 10: DEMONSTRATION ù Main orchestrates the chapter demo
     * Bootstrap creates database objects when LocalDB is available; live calls are
     * wrapped in try/catch so parameter setup sections still run when offline.
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("03. Parameters, Stored Procedures & QueryMultiple");
        Console.WriteLine("Connection: {0}", AdoNetTutorialBootstrap.ConnectionString);
        Console.WriteLine();

        string connectionString = AdoNetTutorialBootstrap.ConnectionString;

        AnonymousParameterRepository anonymousRepo = new AnonymousParameterRepository(connectionString);
        DynamicParameterRepository dynamicRepo = new DynamicParameterRepository(connectionString);
        StoredProcedureRepository storedProcRepo = new StoredProcedureRepository(connectionString);
        QueryMultipleRepository queryMultipleRepo = new QueryMultipleRepository(connectionString);
        CommandDefinitionRepository commandDefRepo = new CommandDefinitionRepository(connectionString);

        anonymousRepo.ShowAnonymousBindingConcept();
        dynamicRepo.ShowDynamicParameterSetup();
        DynamicParameterRepository.ShowTvpPreview();

        try
        {
            AdoNetTutorialBootstrap.EnsureReady();

            Product? widget = anonymousRepo.GetById(1);
            Console.WriteLine();
            Console.WriteLine("=== Anonymous parameters (live) ===");
            Console.WriteLine("  GetById(1) -> {0}, ${1:F2}", widget?.ProductName ?? "(null)", widget?.UnitPrice ?? 0m);

            int rowsUpdated = anonymousRepo.UpdatePrice(2, 15.99m);
            Console.WriteLine("  UpdatePrice(2, 15.99) -> rows affected: {0}", rowsUpdated);

            int restored = storedProcRepo.UpdateProductPrice(2, 14.50m);
            Console.WriteLine("  usp_UpdateProductPrice via StoredProcedure -> rows: {0}", restored);

            int newId = dynamicRepo.InsertProduct("Ergonomic Stand", 79.00m, 40);
            Console.WriteLine();
            Console.WriteLine("=== DynamicParameters ù Output / Return (live) ===");
            Console.WriteLine("  InsertProduct OUTPUT @NewProductId -> {0}", newId);

            string? name = dynamicRepo.GetProductName(newId);
            Console.WriteLine("  GetProductName OUTPUT -> {0}", name ?? "(not found)");

            int count = dynamicRepo.GetProductCountViaReturnValue();
            Console.WriteLine("  GetProductCount RETURN -> {0}", count);

            (string? missingName, int status) = dynamicRepo.TryGetProductName(99999);
            Console.WriteLine("  TryGetProductName missing id -> RETURN={0}, Name={1}", status, missingName ?? "(null)");

            (var products, int total, string topName) = queryMultipleRepo.GetDashboardFromProcedure();
            Console.WriteLine();
            Console.WriteLine("=== QueryMultiple / GridReader (live) ===");
            Console.WriteLine("  usp_GetProductDashboard -> {0} products, count={1}, most expensive={2}", products.Count, total, topName);

            Product? viaCmd = commandDefRepo.GetProductById(3);
            Console.WriteLine();
            Console.WriteLine("=== CommandDefinition (live) ===");
            Console.WriteLine("  GetProductById(3) via CommandDefinition -> {0}", viaCmd?.ProductName ?? "(null)");

            int activeCount = 0;
            foreach (var _ in commandDefRepo.GetActiveProductsViaCommandDefinition())
            {
                activeCount++;
            }
            Console.WriteLine("  GetActiveProductsViaCommandDefinition -> {0} in-stock active products", activeCount);
        }
        catch (SqlException ex)
        {
            Console.WriteLine();
            Console.WriteLine("Database demo skipped ù LocalDB unavailable or setup failed.");
            Console.WriteLine("  SqlException: {0}", ex.Message);
            Console.WriteLine("  Run CREATE PROC batch above in SSMS, then re-run this project.");
        }
    }
}

/*
 * QUICK REFERENCE
 *
 * Anonymous:     connection.Query<T>(sql, new { ProductId = 1 });
 *                Property names match @ProductId in SQL (no @ on C# side).
 *
 * DynamicParameters:
 *   var dp = new DynamicParameters();
 *   dp.Add("@ProductId", 1);
 *   dp.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);
 *   dp.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
 *   connection.Execute("dbo.usp_Name", dp, commandType: CommandType.StoredProcedure);
 *   int id = dp.Get<int>("@NewId");
 *
 * Stored proc:   connection.Query<T>("dbo.usp_Name", param, commandType: CommandType.StoredProcedure);
 *
 * QueryMultiple:
 *   using var multi = connection.QueryMultiple("dbo.usp_Dashboard", commandType: CommandType.StoredProcedure);
 *   var rows = multi.Read<Product>().ToList();
 *   int count = multi.Read<int>().Single();
 *
 * CommandDefinition:
 *   var cmd = new CommandDefinition(sql, param, commandTimeout: 30, commandType: CommandType.Text);
 *   connection.Query<T>(cmd);
 *
 * Output/Return detail -> ADO.NET ch.07 Stored Procedures & Output Parameters
 * NextResult detail    -> ADO.NET ch.04 SqlDataReader
 * TVPs                 -> preview only; AsTableValuedParameter when type exists on server
 */
