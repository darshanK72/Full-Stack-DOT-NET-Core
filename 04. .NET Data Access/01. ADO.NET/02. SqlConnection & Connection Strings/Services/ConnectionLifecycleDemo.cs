using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SqlConnectionAndConnectionStrings.Services;

/*
 * FILE ROLE:
 *   SqlConnection metadata, Open/Close, ConnectionState, StateChange event,
 *   and using / await using disposal patterns.
 *
 * SECTIONS IN THIS FILE:
 *   4. SqlConnection — constructor and metadata properties
 *   5. Open, Close, and ConnectionState
 *   6. StateChange event — observe open and close transitions
 *   7. using and await using — IDisposable pattern
 */

/*
 * =========================================================================
 * SECTION 4: SqlConnection — CONSTRUCTOR AND METADATA PROPERTIES
 * =========================================================================
 *
 * SqlConnection implements IDbConnection and IDisposable (and IAsyncDisposable).
 *
 * Construction patterns:
 *   new SqlConnection()                        — set ConnectionString before Open
 *   new SqlConnection(connectionString)          — most common
 *
 * Read-only after open (examples):
 *   connection.DataSource   — server from connection string
 *   connection.Database     — current database (may change after USE db)
 *   connection.ServerVersion — SQL Server version string (requires open connection)
 *   connection.State        — Closed, Open, Connecting, …
 *
 * SqlCommand, SqlDataReader, and transactions attach to an open SqlConnection —
 * COVERED IN DETAIL LATER → 03. SqlCommand & Parameters, 04. SqlDataReader.
 * -------------------------------------------------------------------------
 */
public static class ConnectionMetadataDemo
{
    public static void ShowClosedMetadata(SqlConnection connection)
    {
        Console.WriteLine($"  State (before Open) = {connection.State}");
        Console.WriteLine($"  DataSource          = {connection.DataSource}");
        Console.WriteLine($"  Database            = {connection.Database}");
    }

    public static void ShowOpenMetadata(SqlConnection connection)
    {
        Console.WriteLine($"  State (after Open)  = {connection.State}");
        Console.WriteLine($"  ServerVersion       = {connection.ServerVersion}");
        Console.WriteLine($"  WorkstationId       = {connection.WorkstationId}");
    }
}

/*
 * =========================================================================
 * SECTION 5: Open, Close, AND ConnectionState
 * =========================================================================
 *
 * ConnectionState enum (System.Data):
 *
 *   Closed      — default; no server session (socket may still be pooled — ch.06)
 *   Open        — session established; commands allowed
 *   Connecting  — transient while Open() / OpenAsync() runs
 *   Broken      — fatal error; close and reopen
 *   Executing   — command running (visible on some providers)
 *   Fetching    — reader consuming rows
 *
 * Always Close() (or dispose) when finished. Prefer using / await using so
 * Dispose calls Close even when an exception escapes the block.
 *
 * Open() is synchronous; OpenAsync() is PREVIEW here —
 * COVERED IN DETAIL LATER → 08. Async ADO.NET.
 * -------------------------------------------------------------------------
 */
public static class ConnectionOpenCloseDemo
{
    public static bool TryOpenAndReport(string connectionString, out string? serverVersion)
    {
        serverVersion = null;

        try
        {
            using var connection = new SqlConnection(connectionString);
            ConnectionMetadataDemo.ShowClosedMetadata(connection);

            connection.Open(); // throws SqlException if server unreachable
            serverVersion = connection.ServerVersion;
            ConnectionMetadataDemo.ShowOpenMetadata(connection);

            connection.Close(); // optional inside using — Dispose also closes
            Console.WriteLine($"  State (after Close) = {connection.State}");
            return true;
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"  SqlException (number {ex.Number}): {ex.Message}");
            return false;
        }
    }
}

/*
 * =========================================================================
 * SECTION 6: StateChange EVENT — OBSERVE OPEN AND CLOSE TRANSITIONS
 * =========================================================================
 *
 * SqlConnection.StateChange fires when State moves between Closed and Open
 * (and other transitions). Signature:
 *
 *   void Handler(object sender, StateChangeEventArgs e)
 *   e.OriginalState / e.CurrentState
 *
 * Useful for logging, diagnostics, or UI status indicators. Unsubscribe when
 * done if the connection object lives a long time (rare for SqlConnection).
 * -------------------------------------------------------------------------
 */
public static class StateChangeLogger
{
    public static void Attach(SqlConnection connection)
    {
        connection.StateChange += OnStateChange;
    }

    public static void Detach(SqlConnection connection)
    {
        connection.StateChange -= OnStateChange;
    }

    private static void OnStateChange(object sender, StateChangeEventArgs e)
    {
        Console.WriteLine(
            $"  [StateChange] {e.OriginalState} ? {e.CurrentState}");
    }
}

/*
 * =========================================================================
 * SECTION 7: using AND await using — IDisposable PATTERN
 * =========================================================================
 *
 * SqlConnection.Dispose():
 *   • Calls Close() if still open
 *   • Returns the physical connection to the pool (pooling PREVIEW → ch.06)
 *
 * Patterns:
 *
 *   using (var conn = new SqlConnection(cs)) { conn.Open(); … }
 *
 *   using var conn = new SqlConnection(cs);   // C# 8 — disposes at end of scope
 *   conn.Open();
 *
 *   await using var conn = new SqlConnection(cs);   // IAsyncDisposable
 *   await conn.OpenAsync();                         // full async in ch.08
 *
 * CS1674: 'SqlConnection' does not implement System.IDisposable in very old
 * samples — Microsoft.Data.SqlClient always implements IDisposable.
 * -------------------------------------------------------------------------
 */
public static class DisposalPatternDemo
{
    public static void UsingBlockPattern(string connectionString)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            StateChangeLogger.Attach(connection);
            try
            {
                connection.Open();
                Console.WriteLine($"  using-block: opened, State = {connection.State}");
            }
            finally
            {
                StateChangeLogger.Detach(connection);
            }
        } // Dispose ? Close even if Open threw

        Console.WriteLine("  using-block: connection disposed (State not readable on disposed object)");
    }

    public static async Task AwaitUsingPatternAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(); // cancellation overload in ch.08
        Console.WriteLine($"  await using: State = {connection.State}");
    } // DisposeAsync at end of scope
}
