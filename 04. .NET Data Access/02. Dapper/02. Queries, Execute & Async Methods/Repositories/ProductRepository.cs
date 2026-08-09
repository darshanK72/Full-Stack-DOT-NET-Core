using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using QueriesExecuteAndAsyncMethods.Models;

namespace QueriesExecuteAndAsyncMethods.Repositories;

/*
 * FILE ROLE:
 *   Dapper Query*, Execute, ExecuteScalar, async variants, dynamic reads, and
 *   buffered vs unbuffered streaming against dbo.Products.
 *
 * SECTIONS IN THIS FILE:
 *   3.  Query<T> — IEnumerable of mapped objects
 *   4.  QueryFirst / QueryFirstOrDefault / QuerySingle / QuerySingleOrDefault
 *   5.  Query with dynamic
 *   6.  Execute — INSERT / UPDATE / DELETE (rows affected)
 *   7.  ExecuteScalar — aggregates and SCOPE_IDENTITY() after INSERT
 *   8.  Async — QueryAsync, ExecuteAsync, ExecuteScalarAsync, QueryFirst*Async
 *   9.  Buffered vs unbuffered Query<T>
 */

public sealed class ProductRepository
{
    /*
     * =========================================================================
     * SECTION 3: Query<T> — IEnumerable OF MAPPED OBJECTS
     * =========================================================================
     *
     * connection.Query<Product>(sql) runs the SQL and maps each row to Product.
     * Return type is IEnumerable<Product> — Dapper uses a micro-ORM pipeline
     * instead of manual SqlDataReader.GetInt32 loops (ADO.NET ch04).
     *
     * Default buffered: true — Dapper reads the entire result set into a List<T>
     * before returning. Safe to close the connection after Query returns.
     *
     * Parameters: anonymous object new { id = 1 } binds @id. Full parameter
     * patterns (DynamicParameters, output params) → ch03 Parameters.
     *
     * Equivalent ADO.NET: ExecuteReader + loop + manual mapping.
     * -------------------------------------------------------------------------
     */
    public IEnumerable<Product> GetAllActive(SqlConnection connection)
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE DiscontinuedDate IS NULL
            ORDER BY ProductName;
            """;

        IEnumerable<Product> products = connection.Query<Product>(sql); // buffered by default
        return products; // caller can foreach — data already in memory when buffered
    }

    public IEnumerable<Product> GetByMinimumPrice(SqlConnection connection, decimal minPrice)
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE DiscontinuedDate IS NULL AND UnitPrice >= @minPrice
            ORDER BY UnitPrice;
            """;

        return connection.Query<Product>(sql, new { minPrice }); // @minPrice from anonymous object
    }

    /*
     * =========================================================================
     * SECTION 4: QueryFirst / QueryFirstOrDefault / QuerySingle / QuerySingleOrDefault
     * =========================================================================
     *
     * Convenience methods when you expect one row (or zero-or-one) instead of
     * calling Query<T> and using LINQ First/Single yourself.
     *
     * Method                  | 0 rows              | 1 row   | 2+ rows
     * ------------------------|---------------------|---------|---------------------------
     * QueryFirst              | InvalidOperationEx  | row     | first row (no throw)
     * QueryFirstOrDefault     | default(T)          | row     | first row (no throw)
     * QuerySingle             | InvalidOperationEx  | row     | InvalidOperationEx
     * QuerySingleOrDefault    | default(T)          | row     | InvalidOperationEx
     *
     * InvalidOperationException messages mirror LINQ ("Sequence contains no
     * elements" / "Sequence contains more than one element").
     *
     * Use QuerySingle when the WHERE clause guarantees uniqueness (PK lookup).
     * Use QueryFirst when you only need any matching row (TOP 1 semantics).
     * -------------------------------------------------------------------------
     */
    public Product GetById(SqlConnection connection, int productId)
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE ProductId = @productId;
            """;

        // PK lookup — exactly one row expected; throws if ProductId missing
        Product product = connection.QuerySingle<Product>(sql, new { productId });
        return product;
    }

    public Product? FindByIdOrDefault(SqlConnection connection, int productId)
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE ProductId = @productId;
            """;

        return connection.QuerySingleOrDefault<Product>(sql, new { productId }); // null when not found
    }

    public Product GetCheapestActive(SqlConnection connection)
    {
        const string sql = """
            SELECT TOP 1 ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE DiscontinuedDate IS NULL
            ORDER BY UnitPrice;
            """;

        // TOP 1 guarantees at most one row; QueryFirst throws if table empty
        return connection.QueryFirst<Product>(sql);
    }

    public Product? FindCheapestActiveOrDefault(SqlConnection connection)
    {
        const string sql = """
            SELECT TOP 1 ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE DiscontinuedDate IS NULL
            ORDER BY UnitPrice;
            """;

        return connection.QueryFirstOrDefault<Product>(sql); // null when no active products
    }

    /*
     * Educational helpers — demonstrate exception semantics without crashing the demo.
     */
    public void DemonstrateSingleRowSemantics(SqlConnection connection)
    {
        const string duplicateNameSql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE DiscontinuedDate IS NULL;
            """;

        Console.WriteLine("--- QueryFirst / QuerySingle semantics ---");

        try
        {
            connection.QuerySingle<Product>(duplicateNameSql); // multiple rows → throw
            Console.WriteLine("  QuerySingle: unexpected success on multi-row set.");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"  QuerySingle on many rows: {ex.Message}");
        }

        Product first = connection.QueryFirst<Product>(duplicateNameSql); // takes first, no throw
        Console.WriteLine($"  QueryFirst on many rows: took '{first.ProductName}' (first row only).");

        Product? missing = connection.QuerySingleOrDefault<Product>(
            "SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate FROM dbo.Products WHERE ProductId = -1;");
        Console.WriteLine($"  QuerySingleOrDefault missing PK: {(missing is null ? "null" : missing.ProductName)}");

        try
        {
            connection.QueryFirst<Product>(
                "SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate FROM dbo.Products WHERE ProductId = -1;");
            Console.WriteLine("  QueryFirst: unexpected success on empty set.");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"  QueryFirst on empty set: {ex.Message}");
        }

        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 5: Query WITH dynamic
     * =========================================================================
     *
     * Query without a generic type (or Query<dynamic>) returns rows as dynamic
     * objects — Dapper wraps each row in DapperRow (IDictionary<string, object>).
     *
     * Useful for ad-hoc reports, prototyping, or columns that vary at runtime.
     * Trade-offs:
     *
     *   Pros                          | Cons
     *   ------------------------------|----------------------------------------
     *   No DTO class per query        | No compile-time property checks
     *   Quick exploration             | Runtime errors on typos (row.Nmae)
     *   Good for one-off admin SQL    | Harder to refactor and test
     *
     * Prefer strongly typed Query<T> in application code; dynamic for scripts.
     * -------------------------------------------------------------------------
     */
    public void PrintActiveProductNamesDynamic(SqlConnection connection)
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice
            FROM dbo.Products
            WHERE DiscontinuedDate IS NULL
            ORDER BY ProductName;
            """;

        IEnumerable<dynamic> rows = connection.Query(sql); // non-generic → dynamic rows

        Console.WriteLine("--- Query with dynamic ---");
        foreach (dynamic row in rows)
        {
            int id = row.ProductId;           // runtime binding — typo fails at runtime
            string name = row.ProductName;
            decimal price = row.UnitPrice;
            Console.WriteLine($"  [{id}] {name} @ ${price:F2}");
        }

        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 6: Execute — INSERT / UPDATE / DELETE (ROWS AFFECTED)
     * =========================================================================
     *
     * Execute is Dapper's counterpart to SqlCommand.ExecuteNonQuery (ADO.NET ch03).
     * Returns int — rows affected. Does not materialize a result set.
     *
     * Use for DML and DDL. For SELECT rows use Query<T>; for one scalar cell
     * use ExecuteScalar (SECTION 7).
     *
     * Parameter binding uses the same anonymous object / expando patterns as Query.
     * COVERED IN DETAIL LATER → 03. Parameters, Stored Procedures & QueryMultiple
     * -------------------------------------------------------------------------
     */
    public int InsertProduct(SqlConnection connection, string productName, decimal unitPrice, int stockQuantity)
    {
        const string sql = """
            DECLARE @NextId INT = (SELECT ISNULL(MAX(ProductId), 0) + 1 FROM dbo.Products);
            INSERT INTO dbo.Products (ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate)
            VALUES (@NextId, @productName, @unitPrice, @stockQuantity, NULL);
            """;

        int rowsAffected = connection.Execute(sql, new { productName, unitPrice, stockQuantity });
        return rowsAffected;
    }

    public int UpdateStock(SqlConnection connection, int productId, int newStockQuantity)
    {
        const string sql = """
            UPDATE dbo.Products
            SET StockQuantity = @newStockQuantity
            WHERE ProductId = @productId;
            """;

        return connection.Execute(sql, new { productId, newStockQuantity });
    }

    public int DiscontinueProduct(SqlConnection connection, int productId)
    {
        const string sql = """
            UPDATE dbo.Products
            SET DiscontinuedDate = CAST(GETDATE() AS DATE)
            WHERE ProductId = @productId;
            """;

        return connection.Execute(sql, new { productId });
    }

    /*
     * =========================================================================
     * SECTION 7: ExecuteScalar — AGGREGATES AND SCOPE_IDENTITY() AFTER INSERT
     * =========================================================================
     *
     * ExecuteScalar runs SQL and returns the first column of the first row as
     * object? — same role as SqlCommand.ExecuteScalar (ADO.NET ch03).
     *
     * Common uses:
     *   SELECT COUNT(*), SELECT AVG(UnitPrice), SELECT MAX(ProductId)
     *
     * Identity after INSERT — two common patterns:
     *
     *   IDENTITY column (ADO.NET ch.03):
     *     INSERT ...; SELECT CAST(SCOPE_IDENTITY() AS INT);
     *
     *   Explicit ProductId (AdoNetTutorial — no IDENTITY):
     *     DECLARE @NextId INT = (SELECT ISNULL(MAX(ProductId), 0) + 1 FROM dbo.Products);
     *     INSERT ... VALUES (@NextId, ...); SELECT @NextId;
     *
     * SCOPE_IDENTITY() returns the last identity inserted in the current scope.
     * Prefer over @@IDENTITY (can reflect triggers).
     *
     * Dapper overload ExecuteScalar<T> casts/converts the scalar for you.
     * -------------------------------------------------------------------------
     */
    public int GetActiveCount(SqlConnection connection)
    {
        const string sql = "SELECT COUNT(*) FROM dbo.Products WHERE DiscontinuedDate IS NULL;";

        int count = connection.ExecuteScalar<int>(sql); // generic cast from object
        return count;
    }

    public decimal GetAverageUnitPrice(SqlConnection connection)
    {
        const string sql = "SELECT AVG(UnitPrice) FROM dbo.Products WHERE DiscontinuedDate IS NULL;";

        decimal? average = connection.ExecuteScalar<decimal?>(sql); // NULL if no rows match AVG
        return average ?? 0m;
    }

    public int InsertProductReturningId(SqlConnection connection, string productName, decimal unitPrice, int stockQuantity)
    {
        const string sql = """
            DECLARE @NextId INT = (SELECT ISNULL(MAX(ProductId), 0) + 1 FROM dbo.Products);
            INSERT INTO dbo.Products (ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate)
            VALUES (@NextId, @productName, @unitPrice, @stockQuantity, NULL);
            SELECT @NextId;
            """;

        int newId = connection.ExecuteScalar<int>(sql, new { productName, unitPrice, stockQuantity });
        return newId;
    }

    /*
     * =========================================================================
     * SECTION 8: ASYNC — QueryAsync, ExecuteAsync, ExecuteScalarAsync, QueryFirst*Async
     * =========================================================================
     *
     * Every sync method has an *Async sibling on IDbConnection (Dapper extension).
     * They return Task or Task<T> and release the thread during network I/O —
     * same motivation as async ADO.NET (ADO.NET ch08).
     *
     *   QueryAsync<T>, QueryFirstAsync, QueryFirstOrDefaultAsync
     *   QuerySingleAsync, QuerySingleOrDefaultAsync
     *   ExecuteAsync, ExecuteScalarAsync
     *
     * Pattern: await connection.QueryAsync<Product>(sql, param);
     *
     * CancellationToken and CommandDefinition (timeout, flags) are passed via
     * CommandDefinition — not the simple string overloads shown here.
     *
     * COVERED IN DETAIL LATER → 03. Parameters … (CommandDefinition section)
     *
     * Use async end-to-end in ASP.NET Core: async controller → async repo → await QueryAsync.
     * -------------------------------------------------------------------------
     */
    public async Task<IReadOnlyList<Product>> GetAllActiveAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE DiscontinuedDate IS NULL
            ORDER BY ProductName;
            """;

        IEnumerable<Product> rows = await connection.QueryAsync<Product>(sql);
        return rows.ToList(); // materialize before connection may close in caller
    }

    public async Task<Product> GetByIdAsync(SqlConnection connection, int productId)
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE ProductId = @productId;
            """;

        return await connection.QuerySingleAsync<Product>(sql, new { productId });
    }

    public async Task<int> InsertProductAsync(SqlConnection connection, string productName, decimal unitPrice, int stockQuantity)
    {
        const string sql = """
            DECLARE @NextId INT = (SELECT ISNULL(MAX(ProductId), 0) + 1 FROM dbo.Products);
            INSERT INTO dbo.Products (ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate)
            VALUES (@NextId, @productName, @unitPrice, @stockQuantity, NULL);
            """;

        return await connection.ExecuteAsync(sql, new { productName, unitPrice, stockQuantity });
    }

    public async Task<int> GetActiveCountAsync(SqlConnection connection)
    {
        const string sql = "SELECT COUNT(*) FROM dbo.Products WHERE DiscontinuedDate IS NULL;";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> InsertProductReturningIdAsync(
        SqlConnection connection,
        string productName,
        decimal unitPrice,
        int stockQuantity)
    {
        const string sql = """
            DECLARE @NextId INT = (SELECT ISNULL(MAX(ProductId), 0) + 1 FROM dbo.Products);
            INSERT INTO dbo.Products (ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate)
            VALUES (@NextId, @productName, @unitPrice, @stockQuantity, NULL);
            SELECT @NextId;
            """;

        return await connection.ExecuteScalarAsync<int>(sql, new { productName, unitPrice, stockQuantity });
    }

    /*
     * =========================================================================
     * SECTION 9: BUFFERED VS UNBUFFERED Query<T>
     * =========================================================================
     *
     * Query<T>(sql, param, transaction, buffered, commandTimeout, commandType)
     *
     *   buffered: true  (default) — reads entire result into List<T> before return.
     *                               Connection can close before you iterate.
     *                               Higher memory for large sets; simpler lifetime.
     *
     *   buffered: false — returns deferred IEnumerable<T> backed by an open
     *                     SqlDataReader. Rows stream one at a time (lower memory).
     *                     Connection MUST stay open until enumeration completes.
     *
     * IEnumerable deferred execution:
     *   Dapper does not run SQL until you start iterating (both modes start
     *   the reader on first MoveNext). With buffered:true, the internal loop
     *   reads all rows immediately on first enumeration and caches them.
     *
     * Pitfall: buffered:false + using(connection) { foreach (...) } is correct;
     *          closing the connection before foreach finishes throws on read.
     *
     * CommandDefinition overloads (flags, cancellation) → ch03.
     * -------------------------------------------------------------------------
     */
    public void DemonstrateBufferedVsUnbuffered(SqlConnection connection)
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE DiscontinuedDate IS NULL
            ORDER BY ProductId;
            """;

        Console.WriteLine("--- Buffered vs unbuffered Query<T> ---");

        // buffered: true (default) — safe to consume after connection closes if already materialized
        List<Product> buffered = connection.Query<Product>(sql, buffered: true).ToList();
        Console.WriteLine($"  buffered:true — {buffered.Count} row(s) loaded into memory immediately on enumerate.");

        // buffered: false — stream rows; connection must remain open during foreach
        IEnumerable<Product> unbuffered = connection.Query<Product>(sql, buffered: false);
        int streamed = 0;
        foreach (Product product in unbuffered) // reader active here — do not close connection yet
        {
            streamed++;
            if (streamed == 1)
            {
                Console.WriteLine($"  buffered:false - streaming first row: {product.ProductName}");
            }
        }

        Console.WriteLine($"  buffered:false — streamed {streamed} row(s) with lazy reader.");
        Console.WriteLine("  Rule: unbuffered queries require an open connection for the full foreach.");
        Console.WriteLine();
    }

    public void DemonstrateUnbufferedConnectionRequirement(SqlConnection connection)
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE DiscontinuedDate IS NULL;
            """;

        Console.WriteLine("--- Unbuffered + closed connection (pitfall) ---");

        IEnumerable<Product> deferred;
        using (SqlConnection shortLived = new SqlConnection(connection.ConnectionString))
        {
            shortLived.Open();
            deferred = shortLived.Query<Product>(sql, buffered: false); // returns deferred enumerable
        } // connection closed — reader not yet consumed

        try
        {
            int count = deferred.Count(); // forces enumeration after connection closed
            Console.WriteLine($"  Unexpected: read {count} rows after close.");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"  Expected failure: {ex.Message}");
        }

        Console.WriteLine();
    }
}
