using System.Diagnostics;
using Microsoft.Data.SqlClient;
using TransactionsAndConnectionPooling.Utils;

namespace TransactionsAndConnectionPooling.Services;

/*
 * FILE ROLE: Connection pooling timing demo and ClearPool preview.
 *
 * SECTIONS IN THIS FILE:
 *   9. Connection pooling — why Open/Close is cheap
 *  10. Clearing pools — SqlConnection.ClearPool / ClearAllPools (preview)
 */

/*
 * SECTION 9: CONNECTION POOLING - why Open/Close is cheap
 *
 * SqlConnection.Close() (and Dispose) does NOT necessarily destroy the TCP session.
 * The provider returns the physical connection to a pool keyed by the exact connection
 * string. The next Open() with the same string reuses a warm connection - skipping
 * login handshake latency.
 *
 * Rules:
 *   - Identical connection string text -> same pool (watch spaces/casing in keywords)
 *   - Connection must be Closed/Disposed to return to pool
 *   - Do not cache SqlConnection in static fields across requests - borrow, use, return
 *
 * Demonstration: open/close many times; elapsed time stays low once pool is warm.
 */
internal static class ConnectionPoolingDemo
{
    public static (long ElapsedMs, string Explanation) MeasurePooledOpenClose(int iterations)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            using SqlConnection connection = new SqlConnection(ConnectionStrings.AdoNetTutorial);
            connection.Open();  // first call may create pool entry; later calls reuse
            connection.Close(); // returns connection to pool - not destroyed
        }

        stopwatch.Stop();
        string explanation =
            $"{iterations} Open/Close cycles in {stopwatch.ElapsedMilliseconds} ms - pooled reuse keeps this low.";
        return (stopwatch.ElapsedMilliseconds, explanation);
    }

    /*
     * SECTION 10: CLEARING POOLS - SqlConnection.ClearPool / ClearAllPools (preview)
     *
     * ClearPool(connection)  - drain pool for that connection's string after Close
     * ClearAllPools()        - drain every pool in the AppDomain (rare; tests, failover)
     *
     * Use when credentials change, failover forces new endpoints, or tests need a cold pool.
     * Production services rarely call these - prefer letting the pool manage lifecycle.
     */
    public static string PreviewClearPool()
    {
        using (SqlConnection connection = new SqlConnection(ConnectionStrings.AdoNetTutorial))
        {
            connection.Open();
            connection.Close();
            SqlConnection.ClearPool(connection); // preview - drains pool for this connection string
        }

        return "SqlConnection.ClearPool(connection) preview - pool drained for AdoNetTutorial string.";
    }
}
