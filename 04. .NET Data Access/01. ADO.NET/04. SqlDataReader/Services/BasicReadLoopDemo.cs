using System;
using Microsoft.Data.SqlClient;
using SqlClientReader = Microsoft.Data.SqlClient.SqlDataReader;

namespace SqlDataReader;

/*
 * FILE ROLE: Demonstrates the connected forward-only Read() loop and typed column accessors.
 *
 * SECTIONS IN THIS FILE:
 *   4. The connected cursor — Read() and typed accessors
 */

/*
 * =========================================================================
 * SECTION 4: THE CONNECTED CURSOR — Read() AND TYPED ACCESSORS
 * =========================================================================
 *
 * SqlDataReader characteristics:
 *
 *   Forward-only   — no MovePrevious; only Read() advances
 *   Read-only      — cannot INSERT/UPDATE through the reader
 *   Connected      — SqlConnection must stay open while reading
 *
 * ExecuteReader returns a live cursor. Read() returns true for each row,
 * false when the current result set is exhausted.
 *
 * Typed getters (by ordinal — fastest when cached):
 *   GetInt32, GetString, GetDecimal, GetDateTime, GetBoolean, …
 *
 * Wrong getter on a column ? InvalidCastException at runtime. Use
 * GetFieldType(i) or reader.GetDataTypeName(i) to inspect schema first.
 * -------------------------------------------------------------------------
 */
public static class BasicReadLoopDemo
{
    public static void Run(string connectionString)
    {
        Console.WriteLine("--- SECTION 4: Read() loop + typed accessors ---");

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        using var command = new SqlCommand(
            "SELECT ProductId, ProductName, UnitPrice FROM dbo.Products ORDER BY ProductId;",
            connection);

        using SqlClientReader reader = command.ExecuteReader();
        int rowNumber = 0;

        while (reader.Read())
        {
            rowNumber++;
            int id = reader.GetInt32(0);
            string name = reader.GetString(1);
            decimal price = reader.GetDecimal(2);
            Console.WriteLine($"  Row {rowNumber}: {id} — {name} @ {price:F2}");
        }

        Console.WriteLine($"  Rows read: {rowNumber}");
    }
}
