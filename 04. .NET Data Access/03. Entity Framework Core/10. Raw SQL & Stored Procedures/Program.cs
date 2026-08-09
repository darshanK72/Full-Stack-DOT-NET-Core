using System;
using System.Collections.Generic;
using RawSqlAndStoredProcedures.Data;
using RawSqlAndStoredProcedures.Models;
using RawSqlAndStoredProcedures.Repositories;
using RawSqlAndStoredProcedures.Utils;

namespace RawSqlAndStoredProcedures;

/*
 * TOPIC: Raw SQL and stored procedures in EF Core ù FromSqlRaw, ExecuteSqlRaw, and
 *        SqlQuery<T> (EF Core 8) for when LINQ is not enough.
 *
 * WHY IT MATTERS:
 *   EF Core translates LINQ to SQL for most CRUD, but production apps still hit legacy
 *   stored procedures, tuned SQL, reporting aggregates, and vendor-specific syntax. Raw APIs
 *   let you drop to SQL while keeping connection management, mapping, and (optionally)
 *   change tracking inside the same DbContext you use for LINQ.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Product entity and column mapping expectations
 *   2.  InventorySummary and LowStockRow ù non-entity SqlQuery shapes
 *   3.  DbContext registration and fluent mapping
 *   4.  EfCoreTutorial LocalDB bootstrap and sample stored procedures
 *   5.  FromSqlRaw / FromSqlInterpolated ù entity SELECT with parameters
 *   6.  ExecuteSqlRaw / ExecuteSqlInterpolated ù UPDATE and EXEC without results
 *   7.  Stored procedures mapped to Product via FromSqlRaw
 *   8.  Database.SqlQuery / SqlQueryRaw (EF Core 8) ù arbitrary result types
 *   9.  How raw EF Core APIs relate to ADO.NET and Dapper
 *  10.  Runnable demonstration wiring
 *
 * Prerequisites: ADO.NET ch.03ù04, ch.07 (parameters, reader, stored procedures);
 *                Dapper ch.01ù03; EF Core ch.02 (DbContext/DbSet).
 *
 * Database: EfCoreTutorial on (localdb)\MSSQLLocalDB ù bootstrap in Data/EfCoreTutorialBootstrap.cs.
 * Connection: Server=(localdb)\MSSQLLocalDB;Database=EfCoreTutorial;Integrated Security=true;TrustServerCertificate=true
 *
 * CHAPTER MAP:
 *   1.  Product entity                     -> Models/Product.cs
 *   2.  InventorySummary (SqlQuery)        -> Models/InventorySummary.cs
 *   3.  LowStockRow (proc report)           -> Models/LowStockRow.cs
 *   4.  DbContext and mapping                -> Data/EfCoreTutorialDbContext.cs
 *   5.  LocalDB bootstrap + procs            -> Data/EfCoreTutorialBootstrap.cs
 *   6.  FromSqlRaw / FromSqlInterpolated     -> Repositories/RawSqlQueryRepository.cs
 *   7.  ExecuteSqlRaw / ExecuteSqlInterpolated -> Repositories/RawSqlExecuteRepository.cs
 *   8.  Stored procedures -> entities        -> Repositories/StoredProcedureRepository.cs
 *   9.  SqlQuery / SqlQueryRaw (EF Core 8)   -> Repositories/SqlQueryRepository.cs
 *  10.  Connection string helper             -> Utils/ConnectionHelper.cs
 *  11.  Demonstration                        -> Program.cs Main (below)
 */

public class Program
{
    /*
     * SECTION 11: DEMONSTRATION ù Main orchestrates the chapter demo
     *
     * Bootstraps EfCoreTutorial, then runs each repository in reading order and prints
     * concise output so you can compare EF raw SQL with prior ADO.NET/Dapper chapters.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 10. Raw SQL & Stored Procedures (EF Core) ===");
        Console.WriteLine();

        PrintAdoNetDapperComparison();

        if (!EfCoreTutorialBootstrap.TryEnsureReady(out string? bootstrapFailure))
        {
            Console.WriteLine("Database demos skipped ù LocalDB unavailable or bootstrap failed.");
            Console.WriteLine($"  Reason: {bootstrapFailure}");
            Console.WriteLine("  Ensure SQL Server LocalDB: Server=(localdb)\\MSSQLLocalDB");
            Console.WriteLine("  Manual proc scripts: Data/EfCoreTutorialBootstrap.cs and chapter intro above.");
            return;
        }

        Console.WriteLine("EfCoreTutorial ready on LocalDB.");
        Console.WriteLine();

        using EfCoreTutorialDbContext context = new EfCoreTutorialDbContext();

        RawSqlQueryRepository queryRepo = new RawSqlQueryRepository(context);
        RawSqlExecuteRepository executeRepo = new RawSqlExecuteRepository(context);
        StoredProcedureRepository spRepo = new StoredProcedureRepository(context);
        SqlQueryRepository sqlQueryRepo = new SqlQueryRepository(context);

        DemonstrateFromSql(queryRepo);
        DemonstrateExecuteSql(executeRepo, queryRepo);
        DemonstrateStoredProcedures(spRepo);
        DemonstrateSqlQuery(sqlQueryRepo);
    }

    private static void PrintAdoNetDapperComparison()
    {
        /*
         * SECTION 9 (inline): ADO.NET / DAPPER / EF CORE RAW SQL
         *
         *   Task                    | ADO.NET (prior)        | Dapper (prior)           | EF Core (this chapter)
         *   ------------------------|------------------------|--------------------------|----------------------------
         *   Parameterized SELECT    | SqlCommand + reader    | Query<T>(sql, params)    | FromSqlRaw / SqlQueryRaw
         *   Non-query DML           | ExecuteNonQuery        | Execute(sql, params)     | ExecuteSqlRaw
         *   Stored proc -> rows     | CommandType = SP       | Query<T>(..., SP)        | FromSqlRaw("EXEC ...")
         *   Arbitrary DTO           | Manual reader mapping  | Query<Dto>               | SqlQueryRaw<Dto> (EF 8)
         *   Change tracking         | N/A                    | N/A                      | FromSqlRaw on DbSet (default)
         */
        Console.WriteLine("--- Cross-stack quick map (read sections 6ù9 in Repositories/) ---");
        Console.WriteLine("  ADO.NET SqlCommand + SqlDataReader -> manual column mapping (ch.03ù04, ch.07).");
        Console.WriteLine("  Dapper Query/Execute -> convention-based mapping (Dapper ch.01ù03).");
        Console.WriteLine("  EF Core FromSqlRaw / SqlQuery -> DbContext + optional change tracking (this chapter).");
        Console.WriteLine();
    }

    private static void DemonstrateFromSql(RawSqlQueryRepository queryRepo)
    {
        Console.WriteLine("--- SECTION 6: FromSqlRaw / FromSqlInterpolated ---");

        int linqCount = queryRepo.CountActiveProducts();
        Console.WriteLine($"  Active products (LINQ baseline): {linqCount}");

        foreach (Product product in queryRepo.GetActiveProductsAbovePrice(10m))
        {
            Console.WriteLine($"  Above $10 (raw SQL): {product.ProductId} {product.ProductName} @ {product.UnitPrice:C}");
        }

        foreach (Product product in queryRepo.GetProductsByNamePrefix("Widget"))
        {
            Console.WriteLine($"  Prefix 'Widget' (interpolated SQL): {product.ProductId} {product.ProductName}");
        }

        Console.WriteLine();
    }

    private static void DemonstrateExecuteSql(RawSqlExecuteRepository executeRepo, RawSqlQueryRepository queryRepo)
    {
        Console.WriteLine("--- SECTION 7: ExecuteSqlRaw / ExecuteSqlInterpolated ---");

        int restocked = executeRepo.RestockProduct(productId: 4, additionalUnits: 10);
        Console.WriteLine($"  Restock ProductId 4 (+10 units): {restocked} row(s) affected");

        int discounted = executeRepo.ApplyBulkDiscountViaStoredProcedure("Widget", discountPercent: 5m);
        Console.WriteLine(
            $"  Bulk discount via EXEC proc (Widget* 5%): {discounted} row(s) reported (-1 means NOCOUNT ON in proc)");

        int marked = executeRepo.MarkDiscontinued(productId: 6, discontinuedOn: new DateTime(2025, 1, 1));
        Console.WriteLine($"  Mark Part F discontinued: {marked} row(s) affected");

        int activeAfterUpdates = queryRepo.CountActiveProducts();
        Console.WriteLine($"  Active products after ExecuteSqlRaw batch: {activeAfterUpdates}");
        Console.WriteLine();
    }

    private static void DemonstrateStoredProcedures(StoredProcedureRepository spRepo)
    {
        Console.WriteLine("--- SECTION 8: Stored procedures -> Product entities ---");

        List<Product> all = spRepo.GetAllViaStoredProcedure();
        Console.WriteLine($"  EXEC usp_GetAllProducts: {all.Count} row(s)");

        List<Product> stocked = spRepo.GetByMinStock(minStock: 50);
        Console.WriteLine($"  EXEC usp_GetProductsByMinStock @MinStock=50: {stocked.Count} row(s)");
        foreach (Product product in stocked)
        {
            Console.WriteLine($"    {product.ProductId} {product.ProductName} qty={product.StockQuantity}");
        }

        Console.WriteLine();
    }

    private static void DemonstrateSqlQuery(SqlQueryRepository sqlQueryRepo)
    {
        Console.WriteLine("--- SECTION 9: SqlQuery / SqlQueryRaw (EF Core 8) ---");

        InventorySummary summary = sqlQueryRepo.GetInventorySummary();
        Console.WriteLine(
            $"  Inventory: {summary.ProductCount} products, {summary.TotalUnits} units, avg {summary.AveragePrice:C}");

        InventorySummary filtered = sqlQueryRepo.GetInventorySummaryInterpolated(minPrice: 5m);
        Console.WriteLine(
            $"  Filtered (UnitPrice >= 5): {filtered.ProductCount} products, avg {filtered.AveragePrice:C}");

        foreach (LowStockRow row in sqlQueryRepo.GetLowStockReport(threshold: 50))
        {
            Console.WriteLine($"  Low stock: {row.ProductId} {row.ProductName} qty={row.StockQuantity} [{row.Status}]");
        }

        Console.WriteLine();
    }
}

/*
 * QUICK REFERENCE
 *
 * Entity SELECT (mapped type, DbSet required):
 *   context.Products.FromSqlRaw("SELECT ... WHERE Id = {0}", id)
 *   context.Products.FromSqlInterpolated($"SELECT ... WHERE Id = {id}")
 *
 * Non-query (rows affected):
 *   context.Database.ExecuteSqlRaw("UPDATE ... WHERE Id = {0}", id)
 *   context.Database.ExecuteSqlInterpolated($"UPDATE ... WHERE Id = {id}")
 *
 * Stored procedure -> entity rows:
 *   context.Products.FromSqlRaw("EXEC dbo.usp_Name @P={0}", p).AsNoTracking()
 *
 * Arbitrary result type (EF Core 8+, no DbSet):
 *   context.Database.SqlQueryRaw<MyDto>("SELECT Col AS Prop FROM ...")
 *   context.Database.SqlQuery<MyDto>($"SELECT ... WHERE x = {value}")
 *
 * Safety: never concatenate user input into SQL strings ù use {0} or FormattableString APIs.
 *
 * When to stay on LINQ: composable filters, provider translation, global query filters (ch.08).
 * When to use raw SQL: legacy procs, hints, reporting SQL, shapes that are not entities.
 */
