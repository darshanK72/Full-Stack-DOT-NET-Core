using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DataSetDataTableAndSqlDataAdapter;

/*
 * FILE ROLE: Overview of SqlDataAdapter.Update and SqlCommandBuilder-generated commands.
 *
 * SECTIONS IN THIS FILE:
 *   9. SqlDataAdapter.Update — overview (batch sync back to SQL)
 */

/*
 * =========================================================================
 * SECTION 9: SQLDATAADAPTER.UPDATE - OVERVIEW (BATCH SYNC BACK TO SQL)
 * =========================================================================
 *
 * Fill is read; Update is write. Update inspects each row's RowState and
 * runs the matching command:
 *
 *   RowState   | Command used
 *   -----------|-------------------------------------------------
 *   Added      | InsertCommand
 *   Modified   | UpdateCommand
 *   Deleted    | DeleteCommand
 *   Unchanged  | skipped
 *
 * You must configure InsertCommand / UpdateCommand / DeleteCommand, OR use
 * SqlCommandBuilder to generate them from SelectCommand (works for simple
 * single-table SELECTs without joins).
 *
 *   var builder = new SqlCommandBuilder(adapter);
 *   adapter.Update(table);   // returns rows affected; may throw on conflict
 *   table.AcceptChanges();   // reset RowState after successful save
 *
 * Production code often wraps Update in SqlTransaction (ch.06). Parameterized
 * commands from ch.03 apply to every generated statement.
 *
 * Demo: when LocalDB is available, show CommandBuilder-generated UPDATE text
 * without mutating BikeStores. Offline path explains the same pipeline in text.
 * -------------------------------------------------------------------------
 */
public static class AdapterUpdateOverview
{
    public static void DemonstrateUpdatePipeline(bool localDbAvailable)
    {
        Console.WriteLine("=== SqlDataAdapter.Update overview ===");
        Console.WriteLine("  1. Fill loads rows (RowState = Unchanged)");
        Console.WriteLine("  2. UI/code edits rows (Added / Modified / Deleted)");
        Console.WriteLine("  3. adapter.Update(table) runs INSERT/UPDATE/DELETE per state");
        Console.WriteLine("  4. table.AcceptChanges() clears pending flags");

        if (!localDbAvailable)
        {
            Console.WriteLine("  (CommandBuilder demo skipped - LocalDB not available.)");
            Console.WriteLine();
            return;
        }

        using var connection = new SqlConnection(LocalDbConnection.BikeStoresConnectionString);
        const string selectSql = """
            SELECT category_id, category_name
            FROM production.categories
            WHERE category_id = @id
            """;

        using var adapter = new SqlDataAdapter(selectSql, connection);
        adapter.SelectCommand!.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = 1 });

        // SqlCommandBuilder reflects SelectCommand and assigns Insert/Update/Delete
        using var commandBuilder = new SqlCommandBuilder(adapter);
        adapter.UpdateCommand = commandBuilder.GetUpdateCommand();
        adapter.InsertCommand = commandBuilder.GetInsertCommand();
        adapter.DeleteCommand = commandBuilder.GetDeleteCommand();

        Console.WriteLine("  CommandBuilder generated UpdateCommand (not executed):");
        Console.WriteLine($"    {adapter.UpdateCommand.CommandText}");
        Console.WriteLine("  In production: modify rows, then adapter.Update(table) inside a transaction.");
        Console.WriteLine();
    }
}
