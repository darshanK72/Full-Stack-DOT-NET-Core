using System;
using System.Data;
using Microsoft.Data.SqlClient;
using SqlClientReader = Microsoft.Data.SqlClient.SqlDataReader;

namespace SqlDataReader;

/*
 * FILE ROLE: Preview of CommandBehavior.SequentialAccess for streaming large columns in ordinal order.
 *
 * SECTIONS IN THIS FILE:
 *   9. CommandBehavior.SequentialAccess — preview
 */

/*
 * =========================================================================
 * SECTION 9: CommandBehavior.SequentialAccess — PREVIEW
 * =========================================================================
 *
 * For rows with large binary/text columns (varbinary(max), varchar(max)),
 * SequentialAccess tells the provider to read columns in ordinal order and
 * stream large values in chunks via GetBytes/GetChars — without buffering
 * the entire LOB in memory.
 *
 * Rules when combined:
 *   — Read columns in increasing ordinal order
 *   — Do not re-read an earlier ordinal after moving forward
 *
 * Full LOB streaming patterns → future dedicated topic; async variant in
 * COVERED IN DETAIL LATER → 08. Async ADO.NET
 * -------------------------------------------------------------------------
 */
public static class SequentialAccessPreviewDemo
{
    public static void Run(string connectionString)
    {
        Console.WriteLine("--- SECTION 9: SequentialAccess preview ---");

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        using var command = new SqlCommand(
            "SELECT ProductId, ProductName FROM dbo.Products ORDER BY ProductId;",
            connection);

        CommandBehavior behavior =
            CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection;

        using SqlClientReader reader = command.ExecuteReader(behavior);
        int rows = 0;

        while (reader.Read())
        {
            rows++;
            int id = reader.GetInt32(0);
            string name = reader.GetString(1);
            if (rows <= 2)
            {
                Console.WriteLine($"  Sequential row {rows}: {id} — {name}");
            }
        }

        Console.WriteLine($"  Read {rows} rows with SequentialAccess | CloseConnection flags set.");
        Console.WriteLine("  LOB chunk reads (GetBytes/GetChars) deferred — see quick reference.");
    }
}
