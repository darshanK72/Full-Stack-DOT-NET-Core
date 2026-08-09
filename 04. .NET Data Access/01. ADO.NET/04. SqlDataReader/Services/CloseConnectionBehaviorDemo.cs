using System;
using System.Data;
using Microsoft.Data.SqlClient;
using SqlClientReader = Microsoft.Data.SqlClient.SqlDataReader;

namespace SqlDataReader;

/*
 * FILE ROLE: Demonstrates CommandBehavior.CloseConnection — disposing the reader closes the connection.
 *
 * SECTIONS IN THIS FILE:
 *   8. CommandBehavior.CloseConnection
 */

/*
 * =========================================================================
 * SECTION 8: CommandBehavior.CloseConnection
 * =========================================================================
 *
 * Default: closing/disposing the reader leaves the connection open.
 * CommandBehavior.CloseConnection: when the reader is closed, the associated
 * SqlConnection closes automatically — useful when the caller only holds
 * the reader reference.
 *
 * Typical pattern:
 *   connection.Open();
 *   var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
 *   // return reader to caller; caller disposes reader → connection closes
 *
 * ch.02 covers connection lifetime; here we verify State after dispose.
 * -------------------------------------------------------------------------
 */
public static class CloseConnectionBehaviorDemo
{
    public static void Run(string connectionString)
    {
        Console.WriteLine("--- SECTION 8: CommandBehavior.CloseConnection ---");

        var connection = new SqlConnection(connectionString);
        connection.Open();
        Console.WriteLine($"  Connection open: {connection.State}");

        using (var command = new SqlCommand(
            "SELECT COUNT(*) FROM dbo.Products;",
            connection))
        {
            using SqlClientReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            if (reader.Read())
            {
                int count = reader.GetInt32(0);
                Console.WriteLine($"  Product count via reader: {count}");
            }
        }

        Console.WriteLine($"  After reader disposed: {connection.State} (Closed — CloseConnection)");
        connection.Dispose();
    }
}
