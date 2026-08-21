using System;
using System.Data;

namespace DataSetDataTableAndSqlDataAdapter;

/*
 * FILE ROLE: Builds an in-memory Products DataTable with schema and seed rows — no database required.
 *
 * SECTIONS IN THIS FILE:
 *   2. DataTable and DataColumn — schema in memory
 */

/*
 * =========================================================================
 * SECTION 2: DATATABLE AND DATACOLUMN - SCHEMA IN MEMORY
 * =========================================================================
 *
 * DataTable is one relational table: Columns (schema) + Rows (data).
 * DataColumn defines name, CLR type, constraints, and defaults.
 *
 * Create schema two ways:
 *
 *   A) adapter.Fill(table)     - infer columns from a SELECT (section 8)
 *   B) Columns.Add(...)        - build entirely offline (section 7 demo below)
 *
 * Important members:
 *
 *   table.Columns.Add("Name", typeof(string))
 *   table.Columns["Name"].AllowDBNull = false
 *   table.Rows.Add(...)          - append a row
 *   table.NewRow()               - create row matching schema, not yet in Rows
 * -------------------------------------------------------------------------
 */
public static class InMemoryCatalogBuilder
{
    /*
     * Builds a product catalog DataTable with no database call.
     * Every later section that needs a table starts here or clones this shape.
     */
    public static DataTable CreateProductTable()
    {
        var products = new DataTable("Products"); // TableName used by adapter.Update

        // DataColumn - name, CLR type, optional expression/default
        products.Columns.Add("ProductId", typeof(int));
        products.Columns.Add("ProductName", typeof(string));
        products.Columns.Add("UnitPrice", typeof(decimal));
        products.Columns.Add("IsDiscontinued", typeof(bool));

        products.Columns["ProductName"]!.AllowDBNull = false; // NOT NULL in memory too
        products.Columns["UnitPrice"]!.DefaultValue = 0m;     // NewRow() picks this up

        // Rows.Add(object?[] values) - order must match column ordinal
        products.Rows.Add(1, "Trail Helmet", 59.99m, false);
        products.Rows.Add(2, "City Lights", 24.50m, false);
        products.Rows.Add(3, "Carbon Frame Kit", 899.00m, false);

        return products;
    }

    public static void PrintTable(DataTable table, string caption)
    {
        Console.WriteLine($"--- {caption} ({table.TableName}, {table.Rows.Count} rows) ---");
        foreach (DataColumn column in table.Columns)
        {
            Console.Write($"{column.ColumnName}\t");
        }

        Console.WriteLine();
        foreach (DataRow row in table.Rows)
        {
            if (row.RowState == DataRowState.Deleted)
            {
                continue; // Deleted rows still sit in Rows until AcceptChanges
            }

            Console.WriteLine($"{row["ProductId"]}\t{row["ProductName"]}\t{row["UnitPrice"]}\t{row["IsDiscontinued"]}");
        }

        Console.WriteLine();
    }
}
