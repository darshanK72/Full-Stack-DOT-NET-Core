using System;
using System.Data;
using Microsoft.Data.SqlClient;
using SqlClientReader = Microsoft.Data.SqlClient.SqlDataReader;

namespace SqlDataReader;

/*
 * FILE ROLE: Shows IsDBNull and DBNull.Value when reading nullable SQL columns.
 *
 * SECTIONS IN THIS FILE:
 *   6. NULL values — IsDBNull and DBNull.Value
 */

/*
 * =========================================================================
 * SECTION 6: NULL VALUES — IsDBNull AND DBNull.Value
 * =========================================================================
 *
 * SQL NULL maps to DBNull.Value in ADO.NET — not C# null on value types.
 *
 *   reader.IsDBNull(ordinal)     true when column is SQL NULL
 *   reader.GetValue(i) == DBNull.Value   alternative check on boxed value
 *
 * Calling GetDateTime/GetInt32 on a NULL column throws SqlNullValueException.
 * For nullable reference/value targets, branch on IsDBNull first.
 *
 * When writing parameters (ch.03), assign DBNull.Value to mean SQL NULL.
 * -------------------------------------------------------------------------
 */
public static class NullHandlingDemo
{
    public static void Run(string connectionString)
    {
        Console.WriteLine("--- SECTION 6: IsDBNull / DBNull.Value ---");

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        using var command = new SqlCommand(
            "SELECT ProductName, DiscontinuedDate FROM dbo.Products ORDER BY ProductId;",
            connection);

        using SqlClientReader reader = command.ExecuteReader();
        int ordName = reader.GetOrdinal("ProductName");
        int ordDisc = reader.GetOrdinal("DiscontinuedDate");

        while (reader.Read())
        {
            string name = reader.GetString(ordName);
            string status;

            if (reader.IsDBNull(ordDisc))
            {
                status = "still active (NULL in database)";
            }
            else
            {
                DateTime discontinued = reader.GetDateTime(ordDisc);
                status = $"discontinued {discontinued:yyyy-MM-dd}";
            }

            object raw = reader.GetValue(ordDisc);
            bool isDbNull = raw == DBNull.Value;
            Console.WriteLine($"  {name,-10} → {status}  [GetValue == DBNull.Value: {isDbNull}]");
        }
    }
}
