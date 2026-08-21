using System;

namespace DataSetDataTableAndSqlDataAdapter;

/*
 * FILE ROLE: Compares the connected SqlDataReader model (ch.04) with the disconnected
 *            DataTable/DataSet model taught in this chapter.
 *
 * SECTIONS IN THIS FILE:
 *   1. Connected vs disconnected model
 */

/*
 * =========================================================================
 * SECTION 1: CONNECTED VS DISCONNECTED MODEL
 * =========================================================================
 *
 * ADO.NET offers two ways to work with query results:
 *
 *   CONNECTED (ch.04 SqlDataReader - FULL depth there)
 *   ----------------------------------------------------
 *   - SqlConnection stays open while you read.
 *   - SqlDataReader is forward-only, read-only, one row at a time.
 *   - Low memory for huge result sets; you process each row immediately.
 *   - You cannot rewind or edit rows in the reader.
 *
 *   DISCONNECTED (this chapter - FULL depth here)
 *   ---------------------------------------------
 *   - SqlDataAdapter.Fill pulls rows into a DataTable, then closes the connection.
 *   - DataTable / DataSet live entirely in RAM - edit, sort, filter offline.
 *   - DataRowState tracks Added / Modified / Deleted for batch Update.
 *   - Higher memory for large snapshots; ideal for grids and cached reports.
 *
 *   Typical disconnected flow:
 *
 *     using var connection = new SqlConnection(connString);
 *     using var adapter = new SqlDataAdapter(selectSql, connection);
 *     var table = new DataTable();
 *     adapter.Fill(table);          // connection opens briefly, then closes
 *     // ... user edits rows in table ...
 *     adapter.Update(table);        // pushes changes back (overview in section 9)
 *
 * ch.04 SqlDataReader covers ExecuteReader, Read(), typed getters, and
 * multiple result sets. This chapter assumes you know SqlConnection and
 * SqlCommand from chapters 02-03.
 * -------------------------------------------------------------------------
 */
public static class DisconnectedModelNotes
{
    public static void PrintComparison()
    {
        Console.WriteLine("=== Connected vs disconnected (see ch.04 for SqlDataReader) ===");
        Console.WriteLine("  SqlDataReader     forward-only stream, connection held open");
        Console.WriteLine("  DataTable/DataSet in-memory snapshot, connection closed after Fill");
        Console.WriteLine();
    }
}
