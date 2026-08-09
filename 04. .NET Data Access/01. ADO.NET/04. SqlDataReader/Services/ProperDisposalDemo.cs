using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using SqlClientReader = Microsoft.Data.SqlClient.SqlDataReader;

namespace SqlDataReader;

/*
 * FILE ROLE: Loads all products with nested using blocks and maps rows via ProductMapper.
 *
 * SECTIONS IN THIS FILE:
 *   10. Proper disposal — using with ExecuteReader
 */

/*
 * =========================================================================
 * SECTION 10: PROPER DISPOSAL — using WITH ExecuteReader
 * =========================================================================
 *
 * SqlDataReader implements IDisposable. Always dispose it — typically with
 * using — so server-side cursor handles are released promptly.
 *
 * Nested using pattern (connection → command → reader):
 *
 *   using var connection = new SqlConnection(...);
 *   connection.Open();
 *   using var cmd = new SqlCommand(..., connection);
 *   using var reader = cmd.ExecuteReader();
 *   while (reader.Read()) { … }
 *
 * Dispose order is LIFO: reader first, then command, then connection.
 * -------------------------------------------------------------------------
 */
public static class ProperDisposalDemo
{
    public static List<ProductRow> Run(string connectionString)
    {
        Console.WriteLine("--- SECTION 10: using + ExecuteReader disposal ---");

        var products = new List<ProductRow>();

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            using (var command = new SqlCommand(
                "SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate FROM dbo.Products ORDER BY ProductId;",
                connection))
            {
                using SqlClientReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    products.Add(ProductMapper.MapFromReader(reader));
                }
            }
        }

        foreach (ProductRow product in products)
        {
            Console.WriteLine($"  {product}");
        }

        return products;
    }
}
