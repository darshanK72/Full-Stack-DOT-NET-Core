using System;
using System.Data;

namespace DataSetDataTableAndSqlDataAdapter;

/*
 * FILE ROLE: Demonstrates DataRow cell access, NewRow + Add, and ItemArray snapshots.
 *
 * SECTIONS IN THIS FILE:
 *   3. DataRow and the Rows collection
 */

/*
 * =========================================================================
 * SECTION 3: DATAROW AND THE ROWS COLLECTION
 * =========================================================================
 *
 * DataRow is one record. Access cells by column name or index:
 *
 *   row["ProductName"] = "Updated Name";
 *   row[1]             same as second column
 *
 * Rows collection:
 *
 *   table.Rows.Add(row)          append
 *   table.Rows.Remove(row)       detach (different from row.Delete())
 *   table.Rows.Find(keyValues)   lookup by PrimaryKey (section 7)
 *   table.NewRow()               blank row matching schema - edit, then Add
 *
 * ItemArray - object?[] snapshot of all column values (handy for cloning/debug).
 * -------------------------------------------------------------------------
 */
public static class DataRowDemo
{
    public static void DemonstrateRowAccess(DataTable products)
    {
        Console.WriteLine("=== DataRow and Rows collection ===");

        DataRow first = products.Rows[0]!; // Rows[0] is object? - cast/use !
        Console.WriteLine($"First product: {first["ProductName"]} @ {first["UnitPrice"]:C}");

        // NewRow → edit → Add (preferred when building rows programmatically)
        DataRow draft = products.NewRow();
        draft["ProductId"] = 4;
        draft["ProductName"] = "Water Bottle";
        draft["UnitPrice"] = 12.00m;
        draft["IsDiscontinued"] = false;
        products.Rows.Add(draft);

        // ItemArray - all values as object?[]
        object?[] snapshot = first.ItemArray;
        Console.WriteLine($"ItemArray length: {snapshot.Length}, first cell: {snapshot[0]}");
        Console.WriteLine();
    }
}
