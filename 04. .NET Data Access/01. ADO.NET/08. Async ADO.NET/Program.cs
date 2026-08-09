/*
 * TOPIC: Async ADO.NET -- non-blocking database I/O with Microsoft.Data.SqlClient.
 *
 * WHY IT MATTERS:
 *   Database calls are I/O-bound: the thread waits on the network and SQL Server,
 *   not on CPU. Synchronous ADO.NET blocks a thread for the entire round-trip.
 *   Async methods (OpenAsync, ExecuteReaderAsync, ...) release the thread while
 *   waiting, so ASP.NET Core, worker services, and desktop apps scale to more
 *   concurrent requests without growing the thread pool.
 *
 * PREREQUISITE:
 *   You should already understand async/await, Task, and CancellationToken from
 *   02. C# & LINQ -> 06. Multithreading & Async Programming -> 04. Async and Await.
 *   This chapter applies those C# patterns to SqlConnection, SqlCommand, and
 *   SqlDataReader -- it does not re-teach the async language keywords.
 *
 * WHAT YOU WILL LEARN:
 *   1.  When async ADO.NET helps (I/O-bound work, server scalability)
 *   2.  async Main -- entry point for an async console program
 *   3.  OpenAsync on SqlConnection
 *   4.  ExecuteNonQueryAsync, ExecuteScalarAsync, ExecuteReaderAsync
 *   5.  ReadAsync and NextResultAsync on SqlDataReader
 *   6.  CancellationToken on async ADO.NET methods
 *   7.  ConfigureAwait(false) in library/repository code
 *   8.  await using with SqlConnection and SqlDataReader
 *   9.  Graceful fallback when LocalDB is unavailable
 *  10.  PREVIEW: IAsyncEnumerable streaming over a reader
 *
 * CHAPTER MAP:
 *   1.  When async ADO.NET helps + ProductRow     -> Models/ProductRow.cs
 *   2.  Connection strings and bootstrap            -> Data/DatabaseBootstrap.cs
 *   3.  ConfigureAwait(false) repository helpers    -> Repositories/ProductRepository.cs
 *   4.  PREVIEW: IAsyncEnumerable streaming         -> Services/ProductStream.cs
 *   5.  async Main entry point                      -> Program.cs (below)
 *   6.  OpenAsync                                   -> Program.cs DemonstrateOpenAsync
 *   7.  ExecuteNonQueryAsync                        -> Program.cs DemonstrateExecuteNonQueryAsync
 *   8.  ExecuteScalarAsync                          -> Program.cs DemonstrateExecuteScalarAsync
 *   9.  ExecuteReaderAsync / ReadAsync / NextResult -> Program.cs DemonstrateExecuteReaderReadAsyncAndNextResultAsync
 *  10.  await using (IAsyncDisposable)              -> Program.cs DemonstrateAwaitUsingAsync
 *  11.  CancellationToken                           -> Program.cs DemonstrateCancellationOnOpenAsync
 *  12.  Repository demo (ConfigureAwait)            -> Program.cs DemonstrateRepositoryWithConfigureAwaitAsync
 *  13.  IAsyncEnumerable preview demo               -> Program.cs DemonstrateIAsyncEnumerablePreviewAsync
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using AsyncAdoNet.Data;
using AsyncAdoNet.Models;
using AsyncAdoNet.Repositories;
using AsyncAdoNet.Services;
using Microsoft.Data.SqlClient;

namespace AsyncAdoNet;

public class Program
{
    /*
     * SECTION 5: async MAIN -- ENTRY POINT FOR ASYNC CONSOLE APPS
     *
     * C# allows the entry point to return Task or Task<int> instead of void.
     * The runtime awaits it before the process exits -- no blocking .GetAwaiter().GetResult().
     *
     * Top-level statements (C# 9+) can also use bare await at file scope:
     *   await RunChapterAsync();
     * This file uses explicit static async Task Main for clarity in a textbook layout.
     * -------------------------------------------------------------------------
     */
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== 08. Async ADO.NET ===");
        Console.WriteLine();

        // Demos that do not require a live database run first.
        await DemonstrateCancellationOnOpenAsync();
        Console.WriteLine();

        bool databaseReady = await DatabaseBootstrap.TryEnsureSchemaAsync(CancellationToken.None);
        if (!databaseReady)
        {
            Console.WriteLine("Database demos skipped -- start LocalDB and re-run to see OpenAsync,");
            Console.WriteLine("ExecuteNonQueryAsync, ExecuteScalarAsync, ExecuteReaderAsync, ReadAsync,");
            Console.WriteLine("NextResultAsync, and await using against AdoNetTutorial.");
            return;
        }

        Console.WriteLine("AdoNetTutorial ready on LocalDB.");
        Console.WriteLine();

        await DemonstrateOpenAsync();
        await DemonstrateExecuteNonQueryAsync();
        await DemonstrateExecuteScalarAsync();
        await DemonstrateExecuteReaderReadAsyncAndNextResultAsync();
        await DemonstrateAwaitUsingAsync();
        await DemonstrateRepositoryWithConfigureAwaitAsync();
        await DemonstrateIAsyncEnumerablePreviewAsync();
    }

    /*
     * SECTION 6: OpenAsync
     *
     * Opens the physical connection asynchronously. Prefer await using so
     * DisposeAsync runs even when exceptions occur.
     * -------------------------------------------------------------------------
     */
    private static async Task DemonstrateOpenAsync()
    {
        Console.WriteLine("--- OpenAsync ---");

        await using SqlConnection connection = new SqlConnection(DatabaseBootstrap.AppConnectionString);
        Console.WriteLine($"  State before OpenAsync: {connection.State}");

        await connection.OpenAsync(); // non-blocking open; thread free while waiting
        Console.WriteLine($"  State after OpenAsync:  {connection.State}");
        Console.WriteLine();
    }

    /*
     * SECTION 7: ExecuteNonQueryAsync
     *
     * Returns the number of rows affected (INSERT, UPDATE, DELETE, DDL).
     * Same semantics as ExecuteNonQuery -- only the wait is async.
     * -------------------------------------------------------------------------
     */
    private static async Task DemonstrateExecuteNonQueryAsync()
    {
        Console.WriteLine("--- ExecuteNonQueryAsync ---");

        int rows = await ProductRepository.InsertProductAsync(
            DatabaseBootstrap.AppConnectionString,
            "Demo Webcam",
            59.99m,
            15);

        Console.WriteLine($"  InsertProductAsync affected {rows} row(s).");
        Console.WriteLine();
    }

    /*
     * SECTION 8: ExecuteScalarAsync
     *
     * Returns the first column of the first row (aggregates, SELECT COUNT, etc.).
     * -------------------------------------------------------------------------
     */
    private static async Task DemonstrateExecuteScalarAsync()
    {
        Console.WriteLine("--- ExecuteScalarAsync ---");

        await using SqlConnection connection = new SqlConnection(DatabaseBootstrap.AppConnectionString);
        await connection.OpenAsync();

        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT AVG(UnitPrice) FROM dbo.Products;";

        object? average = await command.ExecuteScalarAsync();
        decimal avgPrice = average is decimal d ? d : Convert.ToDecimal(average);
        Console.WriteLine($"  Average unit price: {avgPrice:F2}");
        Console.WriteLine();
    }

    /*
     * SECTION 9: ExecuteReaderAsync, ReadAsync, NextResultAsync
     *
     * ExecuteReaderAsync -- start a forward-only read (prefer CommandBehavior.CloseConnection
     * when the connection should close with the reader).
     *
     * ReadAsync -- advance to the next row without blocking the thread.
     *
     * NextResultAsync -- advance to the next result set when the batch returns multiple
     *   (e.g. two SELECT statements in one CommandText).
     *
     * Pitfall: do not mix sync Read() and async ReadAsync() on the same reader.
     * -------------------------------------------------------------------------
     */
    private static async Task DemonstrateExecuteReaderReadAsyncAndNextResultAsync()
    {
        Console.WriteLine("--- ExecuteReaderAsync / ReadAsync / NextResultAsync ---");

        await using SqlConnection connection = new SqlConnection(DatabaseBootstrap.AppConnectionString);
        await connection.OpenAsync();

        await using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT ProductId, Name, UnitPrice, Stock
            FROM dbo.Products
            ORDER BY ProductId;

            SELECT COUNT(*) AS TotalProducts, SUM(Stock) AS TotalStock
            FROM dbo.Products;
            """;

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

        Console.WriteLine("  Result set 1 -- products:");
        while (await reader.ReadAsync())
        {
            int id = reader.GetInt32(0);
            string name = reader.GetString(1);
            decimal price = reader.GetDecimal(2);
            int stock = reader.GetInt32(3);
            Console.WriteLine($"    #{id} {name} @ {price:F2} (stock {stock})");
        }

        bool hasSecondSet = await reader.NextResultAsync();
        if (hasSecondSet && await reader.ReadAsync())
        {
            int totalProducts = reader.GetInt32(0);
            int totalStock = reader.GetInt32(1);
            Console.WriteLine($"  Result set 2 -- {totalProducts} product(s), {totalStock} total stock.");
        }

        Console.WriteLine();
    }

    /*
     * SECTION 10: await using WITH SqlConnection AND SqlDataReader
     *
     * SqlConnection and SqlDataReader implement IAsyncDisposable in modern
     * Microsoft.Data.SqlClient. await using calls DisposeAsync, which can flush
     * async cleanup without blocking.
     *
     * Pattern:
     *   await using SqlConnection conn = new SqlConnection(...);
     *   await conn.OpenAsync();
     *   await using SqlCommand cmd = conn.CreateCommand();
     *   await using SqlDataReader reader = await cmd.ExecuteReaderAsync(...);
     * -------------------------------------------------------------------------
     */
    private static async Task DemonstrateAwaitUsingAsync()
    {
        Console.WriteLine("--- await using (IAsyncDisposable) ---");

        await using (SqlConnection connection = new SqlConnection(DatabaseBootstrap.AppConnectionString))
        {
            await connection.OpenAsync();

            await using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT TOP 1 Name FROM dbo.Products ORDER BY ProductId;";

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                Console.WriteLine($"  First product (await using path): {reader.GetString(0)}");
            }
        } // DisposeAsync on connection, command, reader -- async cleanup chain

        Console.WriteLine("  Connection disposed via await using.");
        Console.WriteLine();
    }

    /*
     * SECTION 11: CancellationToken ON ASYNC ADO.NET METHODS
     *
     * SqlConnection.OpenAsync, SqlCommand.Execute*Async, SqlDataReader.ReadAsync,
     * and NextResultAsync accept an optional CancellationToken.
     *
     * Passing an already-cancelled token throws OperationCanceledException --
     * useful for timeouts and user cancel -- and works even when the database is down.
     * -------------------------------------------------------------------------
     */
    private static async Task DemonstrateCancellationOnOpenAsync()
    {
        Console.WriteLine("--- CancellationToken (works without database) ---");

        using CancellationTokenSource cts = new CancellationTokenSource();
        cts.Cancel(); // simulate user cancel or linked timeout firing

        await using SqlConnection connection = new SqlConnection(DatabaseBootstrap.AppConnectionString);

        try
        {
            await connection.OpenAsync(cts.Token);
            Console.WriteLine("  Unexpected: OpenAsync completed after cancel.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("  OpenAsync cancelled cooperatively (OperationCanceledException).");
        }

        Console.WriteLine();
    }

    private static async Task DemonstrateRepositoryWithConfigureAwaitAsync()
    {
        Console.WriteLine("--- ConfigureAwait(false) via ProductRepository ---");

        int count = await ProductRepository.GetProductCountAsync(DatabaseBootstrap.AppConnectionString);
        Console.WriteLine($"  Product count from library helper: {count}");
        Console.WriteLine();
    }

    private static async Task DemonstrateIAsyncEnumerablePreviewAsync()
    {
        Console.WriteLine("--- PREVIEW: IAsyncEnumerable + ReadAsync ---");

        List<ProductRow> streamed = new List<ProductRow>();

        await using SqlConnection connection = new SqlConnection(DatabaseBootstrap.AppConnectionString);
        await connection.OpenAsync();

        await using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            "SELECT ProductId, Name, UnitPrice, Stock FROM dbo.Products ORDER BY ProductId;";

        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        await foreach (ProductRow row in ProductStream.ReadProductsAsync(reader))
        {
            streamed.Add(row);
        }

        Console.WriteLine($"  await foreach streamed {streamed.Count} row(s) without loading a List first.");
        Console.WriteLine();
    }
}

/*
 * QUICK REFERENCE -- Async ADO.NET (Microsoft.Data.SqlClient)
 *
 * WHEN TO USE
 *   I/O-bound DB access in servers and UI -- async end-to-end.
 *   Not for CPU-bound work on rows already in memory.
 *
 * CONNECTION
 *   await using var conn = new SqlConnection(cs);
 *   await conn.OpenAsync(cancellationToken);
 *
 * COMMAND
 *   await cmd.ExecuteNonQueryAsync(ct);   // rows affected
 *   await cmd.ExecuteScalarAsync(ct);     // first cell
 *   await cmd.ExecuteReaderAsync(behavior, ct);
 *
 * READER
 *   while (await reader.ReadAsync(ct)) { ... }
 *   while (await reader.NextResultAsync(ct)) { ... }
 *
 * CANCELLATION
 *   Pass CancellationToken to every *Async call; handle OperationCanceledException.
 *
 * LIBRARY CODE
 *   await something.ConfigureAwait(false);  // avoid context capture
 *
 * DISPOSAL
 *   await using for SqlConnection, SqlCommand, SqlDataReader (IAsyncDisposable).
 *
 * ENTRY POINT
 *   public static async Task Main(string[] args) { ... }
 *
 * PREVIEW
 *   IAsyncEnumerable + await foreach over ReadAsync -> async streams chapter.
 */
