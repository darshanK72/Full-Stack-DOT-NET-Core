using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace DataSetDataTableAndSqlDataAdapter;

/*
 * FILE ROLE: Loads rows from BikeStores on LocalDB into a DataTable via SqlDataAdapter.Fill.
 *
 * SECTIONS IN THIS FILE:
 *   8. SqlDataAdapter — SelectCommand and Fill
 */

/*
 * =========================================================================
 * SECTION 8: SQLDATAADAPTER - SELECTCOMMAND AND FILL
 * =========================================================================
 *
 * SqlDataAdapter bridges SqlConnection/SqlCommand and DataTable/DataSet:
 *
 *   adapter.SelectCommand   - usually a SELECT (required for Fill)
 *   adapter.Fill(table)     - opens connection, runs SELECT, loads rows, closes
 *   adapter.Fill(dataSet)   - can load multiple result sets into named tables
 *
 * Fill adds/ refreshes rows. Schema (columns) is created on first Fill if empty.
 *
 * Optional demo below uses BikeStores on LocalDB (see 04. .NET Data Access
 * README). If LocalDB or BikeStores is missing, the rest of this chapter
 * still runs - offline sections do not depend on SQL Server.
 * -------------------------------------------------------------------------
 */
public static class LocalDbFillDemo
{
    public static bool TryFillCategories(out DataTable? categories, out string message)
    {
        categories = null;
        try
        {
            using var connection = new SqlConnection(LocalDbConnection.BikeStoresConnectionString);
            const string sql = """
                SELECT TOP 5
                    category_id   AS CategoryId,
                    category_name AS CategoryName
                FROM production.categories
                ORDER BY category_id
                """;

            using var adapter = new SqlDataAdapter(sql, connection);
            categories = new DataTable("CategoriesFromDb");
            int rowCount = adapter.Fill(categories); // opens connection, Fill, closes
            message = $"Fill loaded {rowCount} rows from production.categories.";
            return true;
        }
        catch (DbException ex)
        {
            message = $"LocalDB/BikeStores unavailable - skipping Fill ({ex.Message}).";
            return false;
        }
    }

    public static void PrintFilledTable(DataTable categories)
    {
        Console.WriteLine($"=== SqlDataAdapter.Fill - {categories.TableName} ===");
        foreach (DataRow row in categories.Rows)
        {
            Console.WriteLine($"  {row["CategoryId"]}: {row["CategoryName"]}");
        }

        Console.WriteLine();
    }
}
