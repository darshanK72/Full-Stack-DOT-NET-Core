using System;
using System.Collections.Generic;
using IntroductionToDapper.Models;
using IntroductionToDapper.Repositories;
using IntroductionToDapper.Utils;
using Microsoft.Data.SqlClient;

namespace IntroductionToDapper;

/*
 * =============================================================================
 * TOPIC: Introduction to Dapper - the micro-ORM that maps SQL result sets to
 *        .NET objects through IDbConnection extension methods. You keep writing
 *        SQL; Dapper removes SqlDataReader mapping boilerplate.
 *
 * WHY IT MATTERS:
 *   After ADO.NET you can manually map every column in a Read() loop - workable
 *   but repetitive. Dapper gives you that mapping in one Query<T> call with
 *   almost no overhead. Teams use it for hand-tuned reads, reporting, and
 *   microservices where EF Core's change tracker is unnecessary weight.
 *
 * WHAT YOU WILL LEARN:
 *   1.  What Dapper is - micro-ORM, extension methods on IDbConnection
 *   2.  NuGet setup - Microsoft.Data.SqlClient + Dapper (already in .csproj)
 *   3.  First Query<T> - map rows to a POCO (compare to ADO.NET ch04 reader loop)
 *   4.  Dapper vs raw ADO.NET vs EF Core - when to pick each
 *   5.  IDbConnection requirement - SqlConnection works; open or closed
 *   6.  Minimal repository pattern with Dapper
 *   7.  Column-to-property mapping by convention (preview advanced mapping -> ch04)
 *
 * CHAPTER MAP:
 *   1.  What Dapper is                    -> Utils/DapperConcepts.cs
 *   2.  NuGet setup                       -> Utils/DapperConcepts.cs
 *   3.  First Query<T>                    -> Repositories/ProductRepository.cs
 *   4.  Dapper vs ADO.NET vs EF Core      -> Utils/DapperConcepts.cs
 *   5.  IDbConnection open vs closed      -> Utils/ConnectionBehaviorDemo.cs
 *   6.  Minimal repository pattern        -> Repositories/ProductRepository.cs
 *   7.  Convention column mapping         -> Models/Product.cs
 *   8.  Demonstration                     -> Program.cs Main (below)
 *   9.  Runtime bootstrap                 -> Utils/AdoNetTutorialBootstrap.cs
 *
 * DATABASE SETUP - run in SSMS or sqlcmd against (localdb)\MSSQLLocalDB
 * (same AdoNetTutorial schema as ADO.NET ch.04 SqlDataReader and Dapper ch.03+):
 *
 *   CREATE DATABASE AdoNetTutorial;
 *   GO
 *   USE AdoNetTutorial;
 *   GO
 *   CREATE TABLE dbo.Products (
 *       ProductId        INT            NOT NULL PRIMARY KEY,
 *       ProductName      NVARCHAR(100)  NOT NULL,
 *       UnitPrice        DECIMAL(10, 2) NOT NULL,
 *       StockQuantity    INT            NOT NULL,
 *       DiscontinuedDate DATE           NULL
 *   );
 *   GO
 *   INSERT INTO dbo.Products (ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate)
 *   VALUES (1, N'Widget A', 9.99, 100, NULL);
 *   GO
 *
 * Or call AdoNetTutorialBootstrap.EnsureReady() at startup (Main does this).
 *
 * Connection string used below:
 *   Server=(localdb)\MSSQLLocalDB;Database=AdoNetTutorial;Integrated Security=true;TrustServerCertificate=true
 * =============================================================================
 */
public class Program
{
    /*
     * SECTION 8: DEMONSTRATION - Main orchestrates the chapter demo
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 01. Introduction to Dapper ===");
        Console.WriteLine();

        DapperConcepts.ExplainWhatDapperIs();
        Console.WriteLine();
        DapperConcepts.ExplainNuGetSetup();
        Console.WriteLine();
        DapperConcepts.ExplainStackComparison();
        Console.WriteLine();

        var repository = new ProductRepository(ConnectionHelper.LocalDbConnectionString);

        try
        {
            AdoNetTutorialBootstrap.EnsureReady();
            ConnectionBehaviorDemo.DemonstrateClosedConnectionQuery(ConnectionHelper.LocalDbConnectionString);
            Console.WriteLine();
            ConnectionBehaviorDemo.DemonstrateOpenConnectionQuery(ConnectionHelper.LocalDbConnectionString);
            Console.WriteLine();

            Console.WriteLine("--- SECTION 3 & 6: Repository Query<T> ---");
            IReadOnlyList<Product> active = repository.GetActiveProducts();
            foreach (Product product in active)
            {
                Console.WriteLine($"  {product}");
            }
            Console.WriteLine($"  Active products: {active.Count} (scalar check: {repository.CountActiveProducts()})");
            Console.WriteLine();

            if (active.Count > 0)
            {
                Product? first = repository.GetById(active[0].ProductId);
                if (first is not null)
                {
                    Console.WriteLine($"  GetById({first.ProductId}): {first.ProductName} @ ${first.UnitPrice:F2}");
                }
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine("Database unavailable or setup incomplete.");
            Console.WriteLine($"  SqlException: {ex.Message}");
            Console.WriteLine("  Run the DATABASE SETUP block at the top of Program.cs in SSMS/sqlcmd.");
            Console.WriteLine("  Ensure LocalDB is installed: Server=(localdb)\\MSSQLLocalDB");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Connection error: {ex.Message}");
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE - INTRODUCTION TO DAPPER
 * =========================================================================
 *
 * --- Setup ---
 *
 *   NuGet: Microsoft.Data.SqlClient, Dapper
 *   using System.Data;
 *   using Dapper;
 *   using Microsoft.Data.SqlClient;
 *
 * --- Core idea ---
 *
 *   Dapper = extension methods on IDbConnection that run SQL and map rows to T.
 *   You write SQL; Dapper handles reader -> object mapping.
 *
 * --- First query ---
 *
 *   using IDbConnection db = new SqlConnection(connectionString);
 *   IEnumerable<Product> rows = db.Query<Product>("SELECT ... FROM dbo.Products;");
 *
 * --- Connection state ---
 *
 *   Closed -> Dapper opens before executing; connection stays open after Query.
 *   Open   -> Dapper uses the existing connection (typical inside transactions).
 *   Always dispose with using so the connection returns to the pool.
 *
 * --- Mapping ---
 *
 *   Column names match property names (case-insensitive) -> automatic map.
 *   Aliases, underscores, multi-table joins -> ch04 Mapping & Advanced Patterns.
 *
 * --- Repository sketch ---
 *
 *   public sealed class ProductRepository
 *   {
 *       public IReadOnlyList<Product> GetActiveProducts()
 *       {
 *           using IDbConnection db = new SqlConnection(_cs);
 *           return db.Query<Product>("SELECT ... WHERE DiscontinuedDate IS NULL").ToList();
 *       }
 *   }
 *
 * --- When to use ---
 *
 *   Dapper     - hand-tuned SQL, reports, low overhead POCO mapping
 *   ADO.NET    - full manual control, provider-specific streaming APIs
 *   EF Core    - LINQ, migrations, change tracking, rich relationships
 *
 * --- Forward references ---
 *
 *   ch02 Queries, Execute & Async Methods     Query*, Single*, First*, Execute*, async
 *   ch03 Parameters, Stored Procedures      anonymous/DynamicParameters, procs, QueryMultiple
 *   ch04 Mapping & Advanced Patterns          custom maps, splitOn, multi-mapping
 *   ADO.NET ch04 SqlDataReader                manual Read() loop Dapper replaces
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Dispose connection before reading    | ObjectDisposedException (unbuffered)
 *  Property name != column name         | Default property left default/null
 *  Missing using Dapper;                | CS1061 - Query not found on connection
 *  Expect EF-style change tracking      | Updates not persisted - Dapper is stateless
 *
 * =========================================================================
 */
