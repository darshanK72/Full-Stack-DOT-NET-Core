/*
 * =============================================================================
 * 05. DATASET, DATATABLE & SQLDATAADAPTER - COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: ADO.NET's disconnected model - hold query results in memory as
 *        DataTable / DataSet objects, manipulate rows offline, then sync back
 *        with SqlDataAdapter.Fill and Update.
 *
 * WHY IT MATTERS:
 *   Web forms, WinForms, and reporting tools often load a snapshot of data,
 *   let the user edit grids offline, and save changes in a batch. DataSet and
 *   DataTable are the in-memory relational containers for that workflow.
 *   Even when you move to EF Core or Dapper, understanding row state, keys,
 *   and adapter-based sync clarifies how change tracking works everywhere.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Connected vs disconnected model (SqlDataReader preview from ch.04)
 *   2.  DataTable, DataColumn, DataRow, and the Rows collection
 *   3.  DataSet as a multi-table in-memory database
 *   4.  DataRowState - Added, Modified, Deleted, Unchanged
 *   5.  AcceptChanges and RejectChanges
 *   6.  Primary keys and basic Constraints (Unique, ForeignKey)
 *   7.  Building and editing a DataTable entirely in memory (no database)
 *   8.  SqlDataAdapter - SelectCommand, Fill, Update overview
 *   9.  Optional Fill from SQL Server LocalDB (BikeStores) when available
 *  10.  SqlBulkCopy preview - high-volume inserts (ch.06)
 *
 * CHAPTER MAP:
 *   1.  Connected vs disconnected       → Services/DisconnectedModelNotes.cs
 *   2.  DataTable schema in memory      → Services/InMemoryCatalogBuilder.cs
 *   3.  DataRow and Rows collection     → Services/DataRowDemo.cs
 *   4.  DataSet multi-table container   → Services/CatalogDataSetBuilder.cs
 *   5.  DataRowState lifecycle          → Services/RowStateDemo.cs
 *   6.  AcceptChanges / RejectChanges   → Services/AcceptRejectDemo.cs
 *   7.  Primary keys and constraints    → Services/ConstraintsDemo.cs
 *   8.  SqlDataAdapter.Fill             → Services/LocalDbFillDemo.cs
 *   9.  SqlDataAdapter.Update overview  → Services/AdapterUpdateOverview.cs
 *  10.  SqlBulkCopy preview             → Services/SqlBulkCopyPreview.cs
 *       LocalDB connection string        → Utils/LocalDbConnection.cs
 *  11.  Demo wiring                     → Program.cs Main (below)
 *
 * =============================================================================
 */

using System;
using System.Data;

namespace DataSetDataTableAndSqlDataAdapter;

public class Program
{
    /*
     * =========================================================================
     * SECTION 11: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     *
     * Order: offline concepts first (always succeed), then optional LocalDB.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        DisconnectedModelNotes.PrintComparison();

        DataTable products = InMemoryCatalogBuilder.CreateProductTable();
        InMemoryCatalogBuilder.PrintTable(products, "In-memory Products (offline)");

        DataRowDemo.DemonstrateRowAccess(products);
        InMemoryCatalogBuilder.PrintTable(products, "After NewRow + Add");

        DataSet catalog = CatalogDataSetBuilder.CreateCatalogDataSet();
        CatalogDataSetBuilder.PrintDataSetSummary(catalog);

        RowStateDemo.DemonstrateRowStates();
        AcceptRejectDemo.DemonstrateAcceptAndReject();
        ConstraintsDemo.DemonstrateKeysAndConstraints();

        bool filled = LocalDbFillDemo.TryFillCategories(out DataTable? dbCategories, out string fillMessage);
        Console.WriteLine(fillMessage);
        if (filled && dbCategories is not null)
        {
            LocalDbFillDemo.PrintFilledTable(dbCategories);
        }
        else
        {
            Console.WriteLine();
        }

        AdapterUpdateOverview.DemonstrateUpdatePipeline(filled);
        SqlBulkCopyPreview.PrintPreview();

        Console.WriteLine("Chapter 05 complete - disconnected ADO.NET fundamentals.");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE - DATASET, DATATABLE, SQLDATAADAPTER
 * =========================================================================
 *
 * --- Core types ---
 *
 *   DataSet          in-memory database; .Tables, .Relations
 *   DataTable        one table; .Columns, .Rows, .PrimaryKey, .Constraints
 *   DataColumn       schema cell; .DataType, .AllowDBNull, .DefaultValue
 *   DataRow          one record; row["Col"], .RowState, .ItemArray
 *   DataRowState     Unchanged | Added | Modified | Deleted | Detached
 *
 * --- Row lifecycle ---
 *
 *   table.NewRow();  edit values;  table.Rows.Add(row);
 *   row["Col"] = value;           // Modified (if was Unchanged)
 *   row.Delete();                 // Deleted (still in Rows until AcceptChanges)
 *   row.AcceptChanges();          // commit pending state
 *   row.RejectChanges();          // undo pending state
 *   table.Rows.Find(keyValues);   // needs PrimaryKey set
 *
 * --- SqlDataAdapter ---
 *
 *   var adapter = new SqlDataAdapter(selectSql, connection);
 *   adapter.Fill(table);                    // disconnected load
 *   new SqlCommandBuilder(adapter);         // auto Insert/Update/Delete
 *   adapter.Update(table);                  // push RowState changes
 *   table.AcceptChanges();                  // after successful Update
 *
 * --- Connected vs disconnected ---
 *
 *   SqlDataReader (ch.04)   stream, read-only, connection open
 *   DataTable + Adapter     snapshot, editable, connection closed after Fill
 *
 * --- Constraints ---
 *
 *   table.PrimaryKey = new[] { table.Columns["Id"]! };
 *   table.Constraints.Add(new UniqueConstraint(...));
 *   dataSet.Relations.Add(name, parentCol, childCol);  // adds FK constraint
 *
 * --- Common mistakes ---
 *
 *  Mistake                           | Result
 *  ----------------------------------|----------------------------------
 *  No PrimaryKey before Update       | CommandBuilder / Update cannot match rows
 *  row.Delete() expecting removal    | Row hidden until AcceptChanges
 *  Fill huge table on slow network   | Memory spike; prefer paging or reader
 *  Update without transaction        | Partial saves on failure (use ch.06)
 *  Assuming Fill merges by key       | Fill adds/appends; use LoadOption for merge rules
 *
 * --- Related chapters ---
 *
 *   02. SqlConnection & Connection Strings
 *   03. SqlCommand & Parameters
 *   04. SqlDataReader                    connected forward-only reads
 *   06. Transactions & Connection Pooling  SqlTransaction, SqlBulkCopy
 *   08. Async ADO.NET                    FillAsync / UpdateAsync
 *
 * =========================================================================
 */
