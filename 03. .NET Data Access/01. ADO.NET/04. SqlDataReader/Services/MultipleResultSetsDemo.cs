using System;
using Microsoft.Data.SqlClient;
using SqlClientReader = Microsoft.Data.SqlClient.SqlDataReader;

namespace SqlDataReader;

/*
 * FILE ROLE: Consumes multiple SELECT result sets from one batch using NextResult().
 *
 * SECTIONS IN THIS FILE:
 *   7. Multiple result sets — NextResult()
 */

/*
 * =========================================================================
 * SECTION 7: MULTIPLE RESULT SETS — NextResult()
 * =========================================================================
 *
 * One SqlCommand can run a batch (multiple SELECT statements separated by
 * semicolons). ExecuteReader opens the first result set. After the Read()
 * loop finishes, call NextResult() to advance to the next set.
 *
 * Returns true when another result set exists, false when done.
 * Stored procedures (ch.07) often return multiple sets — same API.
 * -------------------------------------------------------------------------
 */
public static class MultipleResultSetsDemo
{
    public static void Run(string connectionString)
    {
        Console.WriteLine("--- SECTION 7: NextResult() / multiple result sets ---");

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        using var command = new SqlCommand(
            """
            SELECT ProductId, ProductName FROM dbo.Products ORDER BY ProductId;
            SELECT COUNT(*) AS ProductCount FROM dbo.Products;
            SELECT TOP 1 ProductName AS MostExpensive FROM dbo.Products ORDER BY UnitPrice DESC;
            """,
            connection);

        using SqlClientReader reader = command.ExecuteReader();
        int setNumber = 0;

        do
        {
            setNumber++;
            Console.WriteLine($"  Result set {setNumber}:");

            while (reader.Read())
            {
                if (setNumber == 1)
                {
                    Console.WriteLine($"    {reader.GetInt32(0)} — {reader.GetString(1)}");
                }
                else if (setNumber == 2)
                {
                    Console.WriteLine($"    ProductCount = {reader.GetInt32(0)}");
                }
                else
                {
                    Console.WriteLine($"    MostExpensive = {reader.GetString(0)}");
                }
            }
        }
        while (reader.NextResult());

        Console.WriteLine($"  Total result sets consumed: {setNumber}");
    }
}
