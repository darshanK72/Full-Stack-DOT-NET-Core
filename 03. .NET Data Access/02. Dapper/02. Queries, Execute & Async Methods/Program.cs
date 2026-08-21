using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using QueriesExecuteAndAsyncMethods.Models;
using QueriesExecuteAndAsyncMethods.Repositories;
using QueriesExecuteAndAsyncMethods.Utils;

namespace QueriesExecuteAndAsyncMethods;

/*
 * =============================================================================
 * TOPIC: Dapper Query*, Execute, ExecuteScalar, and async extension methods on
 *        IDbConnection - the core read/write API of the micro-ORM.
 *
 * WHY IT MATTERS:
 *   After SqlConnection and SqlCommand (ADO.NET), Dapper removes boilerplate:
 *   one line maps rows to objects, Execute replaces ExecuteNonQuery, ExecuteScalar
 *   replaces ExecuteScalar. Knowing QueryFirst vs QuerySingle prevents subtle bugs
 *   (empty result vs duplicate rows). Buffered vs unbuffered controls memory vs
 *   streaming. Async variants mirror ADO.NET ch08 for scalable web apps.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Query<T> - IEnumerable of strongly typed rows
 *   2.  QueryFirst / QueryFirstOrDefault / QuerySingle / QuerySingleOrDefault
 *   3.  Query with dynamic - ad-hoc rows without a DTO
 *   4.  Execute - rows affected for INSERT/UPDATE/DELETE
 *   5.  ExecuteScalar - COUNT, AVG, new key after INSERT (SCOPE_IDENTITY or MAX+1)
 *   6.  Async - QueryAsync, ExecuteAsync, ExecuteScalarAsync, QueryFirst*Async
 *   7.  Buffered (default) vs unbuffered Query - memory and connection lifetime
 *   8.  IEnumerable deferred execution - connection open rules
 *   9.  PREVIEW: parameters depth, CommandDefinition -> ch03
 *
 * CHAPTER MAP:
 *   1.  Product row type                         -> Models/Product.cs
 *   2.  Connection string                        -> Utils/ConnectionHelper.cs
 *   3.  Query<T>                                 -> Repositories/ProductRepository.cs
 *   4.  QueryFirst / QuerySingle family          -> Repositories/ProductRepository.cs
 *   5.  Query with dynamic                       -> Repositories/ProductRepository.cs
 *   6.  Execute                                  -> Repositories/ProductRepository.cs
 *   7.  ExecuteScalar + identity after INSERT    -> Repositories/ProductRepository.cs
 *   8.  Async Query / Execute / ExecuteScalar    -> Repositories/ProductRepository.cs
 *   9.  Buffered vs unbuffered                   -> Repositories/ProductRepository.cs
 *  10.  Demonstration                            -> Program.cs Main (below)
 *  11.  Runtime bootstrap                        -> Utils/AdoNetTutorialBootstrap.cs
 *
 * DATABASE SETUP - AdoNetTutorial on (localdb)\MSSQLLocalDB (ADO.NET ch.04 schema).
 * Main calls AdoNetTutorialBootstrap.EnsureReady() or run manually in SSMS:
 *
 *   CREATE TABLE dbo.Products (
 *       ProductId        INT            NOT NULL PRIMARY KEY,
 *       ProductName      NVARCHAR(100)  NOT NULL,
 *       UnitPrice        DECIMAL(10, 2) NOT NULL,
 *       StockQuantity    INT            NOT NULL,
 *       DiscontinuedDate DATE           NULL
 *   );
 *
 * Connection string:
 *   Server=(localdb)\MSSQLLocalDB;Database=AdoNetTutorial;Integrated Security=true;TrustServerCertificate=true
 * =============================================================================
 */
public class Program
{
    /*
     * SECTION 10: DEMONSTRATION - async Main orchestrates the chapter demo
     */
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== 02. Dapper - Queries, Execute & Async Methods ===");
        Console.WriteLine();

        ProductRepository repository = new ProductRepository();

        try
        {
            AdoNetTutorialBootstrap.EnsureReady();

            await using SqlConnection connection = new SqlConnection(ConnectionHelper.AdoNetTutorial);
            await connection.OpenAsync();
            Console.WriteLine($"Connected: {connection.DataSource} / {connection.Database}");
            Console.WriteLine();

            Console.WriteLine("--- Query<T> - available products ---");
            foreach (Product product in repository.GetAllActive(connection))
            {
                Console.WriteLine($"  [{product.ProductId}] {product.ProductName} @ ${product.UnitPrice:F2}, stock {product.StockQuantity}");
            }

            Console.WriteLine();
            Console.WriteLine("--- Query<T> with @minPrice ---");
            foreach (Product priced in repository.GetByMinimumPrice(connection, minPrice: 10m))
            {
                Console.WriteLine($"  {priced.ProductName} @ ${priced.UnitPrice:F2}");
            }

            Console.WriteLine();

            Product widget = repository.GetById(connection, productId: 1);
            Console.WriteLine($"QuerySingle by PK: {widget.ProductName} (stock {widget.StockQuantity})");

            Product? missing = repository.FindByIdOrDefault(connection, productId: 9999);
            Console.WriteLine($"QuerySingleOrDefault missing id: {(missing is null ? "null" : missing.ProductName)}");

            Product cheapest = repository.GetCheapestActive(connection);
            Console.WriteLine($"QueryFirst cheapest: {cheapest.ProductName} @ ${cheapest.UnitPrice:F2}");
            Console.WriteLine();

            repository.DemonstrateSingleRowSemantics(connection);
            repository.PrintActiveProductNamesDynamic(connection);

            int inserted = repository.InsertProduct(connection, "Webcam", 79.99m, 15);
            Console.WriteLine($"Execute INSERT - rows affected: {inserted}");

            int updated = repository.UpdateStock(connection, productId: 1, newStockQuantity: 30);
            Console.WriteLine($"Execute UPDATE ProductId=1 - rows affected: {updated}");
            Console.WriteLine();

            int activeCount = repository.GetActiveCount(connection);
            decimal avgPrice = repository.GetAverageUnitPrice(connection);
            Console.WriteLine($"ExecuteScalar - available: {activeCount}, avg price: ${avgPrice:F2}");

            int newId = repository.InsertProductReturningId(connection, "USB Hub", 24.99m, 40);
            Console.WriteLine($"ExecuteScalar after INSERT - new ProductId: {newId}");
            Console.WriteLine();

            Console.WriteLine("--- Async methods ---");
            int asyncCount = await repository.GetActiveCountAsync(connection);
            Console.WriteLine($"  ExecuteScalarAsync - available count: {asyncCount}");

            int asyncInserted = await repository.InsertProductAsync(connection, "Headset", 89.99m, 12);
            Console.WriteLine($"  ExecuteAsync INSERT - rows affected: {asyncInserted}");

            int asyncNewId = await repository.InsertProductReturningIdAsync(connection, "Dock", 149.99m, 5);
            Console.WriteLine($"  ExecuteScalarAsync after INSERT - ProductId: {asyncNewId}");

            Product asyncProduct = await repository.GetByIdAsync(connection, productId: 1);
            Console.WriteLine($"  QuerySingleAsync - ProductId=1: {asyncProduct.ProductName}");

            IReadOnlyList<Product> asyncList = await repository.GetAllActiveAsync(connection);
            Console.WriteLine($"  QueryAsync - {asyncList.Count} available product(s) loaded.");
            Console.WriteLine();

            repository.DemonstrateBufferedVsUnbuffered(connection);
            repository.DemonstrateUnbufferedConnectionRequirement(connection);
        }
        catch (SqlException ex)
        {
            Console.WriteLine("Database unavailable or setup incomplete.");
            Console.WriteLine($"  SqlException: {ex.Message}");
            Console.WriteLine("  Ensure LocalDB is installed: Server=(localdb)\\MSSQLLocalDB");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Operation error: {ex.Message}");
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE - DAPPER QUERY / EXECUTE / ASYNC
 * =========================================================================
 *
 * --- Query (SELECT -> objects) ---
 *
 *   IEnumerable<T> rows = conn.Query<T>(sql, param);
 *   IEnumerable<T> rows = conn.Query<T>(sql, param, buffered: false);
 *
 *   T row  = conn.QueryFirst<T>(sql, param);
 *   T? row = conn.QueryFirstOrDefault<T>(sql, param);
 *   T row  = conn.QuerySingle<T>(sql, param);
 *   T? row = conn.QuerySingleOrDefault<T>(sql, param);
 *
 * --- Execute (DML - rows affected) ---
 *
 *   int rows = conn.Execute(sql, param);
 *
 * --- ExecuteScalar (one value) ---
 *
 *   int n = conn.ExecuteScalar<int>("SELECT COUNT(*) ...");
 *   int id = conn.ExecuteScalar<int>("INSERT ...; SELECT CAST(SCOPE_IDENTITY() AS INT);", param);
 *   // AdoNetTutorial uses explicit ProductId: SELECT @NextId after INSERT batch
 *
 * --- Async ---
 *
 *   await conn.QueryAsync<T>(sql, param);
 *   await conn.ExecuteAsync(sql, param);
 *   await conn.ExecuteScalarAsync<int>(sql, param);
 *
 * --- Buffered flag ---
 *
 *   buffered: true  (default) - full result in memory
 *   buffered: false           - lazy reader; conn open until foreach completes
 *
 * --- Related chapters ---
 *
 *   Dapper 03. Parameters - parameters, CommandDefinition, QueryMultiple
 *   Dapper 04. Mapping - column aliases, splitOn, multi-map
 *
 * =========================================================================
 */
