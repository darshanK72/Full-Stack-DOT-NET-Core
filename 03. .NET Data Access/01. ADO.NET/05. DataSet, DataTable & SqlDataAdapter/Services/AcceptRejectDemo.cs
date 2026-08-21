using System;
using System.Data;

namespace DataSetDataTableAndSqlDataAdapter;

/*
 * FILE ROLE: Demonstrates AcceptChanges and RejectChanges on rows and tables.
 *
 * SECTIONS IN THIS FILE:
 *   6. AcceptChanges and RejectChanges
 */

/*
 * =========================================================================
 * SECTION 6: ACCEPTCHANGES AND REJECTCHANGES
 * =========================================================================
 *
 * AcceptChanges() - commit the in-memory edit session:
 *   - Added / Modified → Unchanged (Original values updated to Current)
 *   - Deleted rows → removed from Rows collection
 *
 * RejectChanges() - undo pending edits:
 *   - Modified → revert to Original, Unchanged
 *   - Added → removed from Rows
 *   - Deleted → restored to Unchanged
 *
 * Call on DataRow, DataTable, or entire DataSet ( cascades to tables/rows ).
 *
 * After adapter.Update(table), many apps call table.AcceptChanges() so row
 * states reset before the next edit cycle.
 * -------------------------------------------------------------------------
 */
public static class AcceptRejectDemo
{
    public static void DemonstrateAcceptAndReject()
    {
        Console.WriteLine("=== AcceptChanges vs RejectChanges ===");

        DataTable acceptTable = InMemoryCatalogBuilder.CreateProductTable();
        acceptTable.AcceptChanges(); // treat as loaded data
        DataRow edited = acceptTable.Rows[0]!;
        edited["ProductName"] = "Trail Helmet Pro";
        Console.WriteLine($"Before AcceptChanges: {edited.RowState}");
        edited.AcceptChanges();
        Console.WriteLine($"After AcceptChanges:  {edited.RowState}, name = {edited["ProductName"]}");

        DataTable rejectTable = InMemoryCatalogBuilder.CreateProductTable();
        rejectTable.AcceptChanges();
        DataRow revert = rejectTable.Rows[0]!;
        string originalName = revert["ProductName"]!.ToString()!;
        revert["ProductName"] = "Temporary Name";
        Console.WriteLine($"Before RejectChanges: {revert.RowState}, name = {revert["ProductName"]}");
        revert.RejectChanges();
        Console.WriteLine($"After RejectChanges:  {revert.RowState}, name = {revert["ProductName"]} (restored '{originalName}')");

        // Added rows: RejectChanges removes them from the table entirely
        DataTable addedReject = InMemoryCatalogBuilder.CreateProductTable();
        addedReject.AcceptChanges();
        DataRow brandNew = addedReject.NewRow();
        brandNew["ProductId"] = 100;
        brandNew["ProductName"] = "Will Be Discarded";
        brandNew["UnitPrice"] = 5m;
        brandNew["IsDiscontinued"] = false;
        addedReject.Rows.Add(brandNew);
        Console.WriteLine($"Added row before RejectChanges: RowState = {brandNew.RowState}, count = {addedReject.Rows.Count}");
        brandNew.RejectChanges();
        Console.WriteLine($"Added row after RejectChanges: removed from table, count = {addedReject.Rows.Count}");
        Console.WriteLine();
    }
}
