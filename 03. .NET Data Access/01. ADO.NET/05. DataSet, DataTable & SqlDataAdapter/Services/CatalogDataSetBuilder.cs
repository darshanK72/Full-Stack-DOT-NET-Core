using System;
using System.Data;

namespace DataSetDataTableAndSqlDataAdapter;

/*
 * FILE ROLE: Builds a multi-table DataSet with Categories, Products, and a DataRelation.
 *
 * SECTIONS IN THIS FILE:
 *   4. DataSet — multiple tables in one container
 */

/*
 * =========================================================================
 * SECTION 4: DATASET - MULTIPLE TABLES IN ONE CONTAINER
 * =========================================================================
 *
 * DataSet is an in-memory relational database:
 *
 *   - Tables collection - many DataTable instances
 *   - Relations - parent/child links (DataRelation)
 *   - Can serialize to XML (legacy); EF Core replaced most greenfield uses
 *
 * SqlDataAdapter can Fill an entire DataSet and map each result set to a table
 * (multiple SELECTs or stored procedure result sets - advanced).
 *
 * Here: Products + Categories with a ForeignKeyConstraint (section 7).
 * -------------------------------------------------------------------------
 */
public static class CatalogDataSetBuilder
{
    public static DataSet CreateCatalogDataSet()
    {
        var catalog = new DataSet("BikeCatalog");

        var categories = new DataTable("Categories");
        categories.Columns.Add("CategoryId", typeof(int));
        categories.Columns.Add("CategoryName", typeof(string));
        categories.Rows.Add(1, "Components");
        categories.Rows.Add(2, "Accessories");

        DataTable products = InMemoryCatalogBuilder.CreateProductTable();
        products.Columns.Add("CategoryId", typeof(int));
        foreach (DataRow row in products.Rows)
        {
            row["CategoryId"] = 2; // Accessories for demo
        }

        catalog.Tables.Add(categories);
        catalog.Tables.Add(products);

        // DataRelation names the link; ForeignKeyConstraint enforces it (section 7)
        catalog.Relations.Add(
            "CategoryProducts",
            categories.Columns["CategoryId"]!,
            products.Columns["CategoryId"]!);

        return catalog;
    }

    public static void PrintDataSetSummary(DataSet catalog)
    {
        Console.WriteLine($"=== DataSet '{catalog.DataSetName}' - {catalog.Tables.Count} tables ===");
        foreach (DataTable table in catalog.Tables)
        {
            Console.WriteLine($"  {table.TableName}: {table.Rows.Count} rows, {table.Columns.Count} columns");
        }

        Console.WriteLine();
    }
}
