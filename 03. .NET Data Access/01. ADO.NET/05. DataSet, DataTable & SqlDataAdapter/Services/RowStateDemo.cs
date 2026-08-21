using System;
using System.Data;

namespace DataSetDataTableAndSqlDataAdapter;

/*
 * FILE ROLE: Walks through DataRowState transitions — Unchanged, Modified, Added, Deleted.
 *
 * SECTIONS IN THIS FILE:
 *   5. DataRowState — tracking in-memory changes
 */

/*
 * =========================================================================
 * SECTION 5: DATAROWSTATE - TRACKING IN-MEMORY CHANGES
 * =========================================================================
 *
 * Each DataRow has a RowState while detached from the database (after Fill
 * or before Update):
 *
 *   State        | Meaning
 *   -------------|--------------------------------------------------------
 *   Unchanged    | Loaded or last AcceptChanges - no pending edits
 *   Added        | New row via Rows.Add / NewRow+Add - not yet in DB
 *   Modified     | At least one column changed since load/AcceptChanges
 *   Deleted      | row.Delete() called - marked for DELETE on Update
 *   Detached     | Row removed from Rows or created but not yet Added
 *
 * row.Delete() does NOT remove the row from Rows - it hides it from default
 * views and sets RowState to Deleted until AcceptChanges purges it.
 *
 * Modified is detected on row version "Current" vs "Original" internally.
 * -------------------------------------------------------------------------
 */
public static class RowStateDemo
{
    public static void DemonstrateRowStates()
    {
        Console.WriteLine("=== DataRowState lifecycle ===");

        DataTable table = InMemoryCatalogBuilder.CreateProductTable();
        table.AcceptChanges(); // simulate post-Fill snapshot - in-memory Adds start as Added until this

        DataRow row = table.Rows[1]!;
        Console.WriteLine($"After load: RowState = {row.RowState}"); // Unchanged

        row["UnitPrice"] = 19.99m;
        Console.WriteLine($"After price edit: RowState = {row.RowState}"); // Modified

        DataRow newRow = table.NewRow();
        newRow["ProductId"] = 99;
        newRow["ProductName"] = "Spare Tube";
        newRow["UnitPrice"] = 8.50m;
        newRow["IsDiscontinued"] = false;
        table.Rows.Add(newRow);
        Console.WriteLine($"New row: RowState = {newRow.RowState}"); // Added

        row.Delete();
        Console.WriteLine($"After Delete(): RowState = {row.RowState}, visible count = {CountVisibleRows(table)}"); // Deleted

        PrintStates(table);
        Console.WriteLine();
    }

    private static int CountVisibleRows(DataTable table)
    {
        int count = 0;
        foreach (DataRow row in table.Rows)
        {
            if (row.RowState != DataRowState.Deleted)
            {
                count++;
            }
        }

        return count;
    }

    private static void PrintStates(DataTable table)
    {
        foreach (DataRow row in table.Rows)
        {
            string name = row.RowState == DataRowState.Deleted
                ? "(deleted row)"
                : row["ProductName"]?.ToString() ?? "?";
            Console.WriteLine($"  {name,-20} {row.RowState}");
        }
    }
}
