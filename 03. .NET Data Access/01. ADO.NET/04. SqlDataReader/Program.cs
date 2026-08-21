/*
 * =============================================================================
 * 04. SqlDataReader — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: SqlDataReader — the connected, forward-only, read-only cursor that
 *        streams rows from SQL Server while the SqlConnection stays open.
 *
 * WHY IT MATTERS:
 *   ExecuteNonQuery and ExecuteScalar answer "how many rows changed?" and
 *   "what is one value?". Most reporting, list screens, and export jobs need
 *   many rows streamed efficiently. SqlDataReader reads one row at a time
 *   without loading the entire result into memory — the foundation Dapper and
 *   hand-rolled repositories use before ORMs materialize entities.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Connected cursor model — forward-only, read-only, one row at a time
 *   2.  Read() loop and typed accessors (GetInt32, GetString, GetDecimal, …)
 *   3.  Column metadata — GetName, GetOrdinal, GetFieldType, FieldCount
 *   4.  Indexer syntax — reader["ColumnName"] and reader[ordinal]
 *   5.  NULL handling — IsDBNull and DBNull.Value
 *   6.  Multiple result sets — NextResult() after a batch query
 *   7.  CommandBehavior — CloseConnection; SequentialAccess preview
 *   8.  Proper disposal — using with ExecuteReader
 *   9.  Manual row-to-object mapping (before Dapper/EF Core)
 *  10.  ReadAsync preview — COVERED IN DETAIL LATER → 08. Async ADO.NET
 *
 * CHAPTER MAP:
 *   1.  Sample database                    → Data/AdoNetTutorialDatabase.cs
 *   2.  Product row type                   → Models/ProductRow.cs
 *   3.  Manual row mapper                  → Utils/ProductMapper.cs
 *   4.  Read() loop + typed accessors      → Services/BasicReadLoopDemo.cs
 *   5.  Column metadata + indexer          → Services/ColumnMetadataDemo.cs
 *   6.  NULL handling                      → Services/NullHandlingDemo.cs
 *   7.  Multiple result sets               → Services/MultipleResultSetsDemo.cs
 *   8.  CommandBehavior.CloseConnection    → Services/CloseConnectionBehaviorDemo.cs
 *   9.  SequentialAccess preview           → Services/SequentialAccessPreviewDemo.cs
 *  10.  Proper disposal + mapping          → Services/ProperDisposalDemo.cs
 *  11.  ReadAsync preview                  → Services/ReadAsyncPreviewDemo.cs
 *  12.  Demo wiring                        → Program.cs Main (below)
 *
 * =============================================================================
 */

using System;
using Microsoft.Data.SqlClient;

namespace SqlDataReader;

public class Program
{
    /*
     * =========================================================================
     * SECTION 12: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("04. SqlDataReader — connected forward-only reads");
        Console.WriteLine();

        try
        {
            AdoNetTutorialDatabase.EnsureSampleData();
            string cs = AdoNetTutorialDatabase.ConnectionString;

            BasicReadLoopDemo.Run(cs);
            Console.WriteLine();
            ColumnMetadataDemo.Run(cs);
            Console.WriteLine();
            NullHandlingDemo.Run(cs);
            Console.WriteLine();
            MultipleResultSetsDemo.Run(cs);
            Console.WriteLine();
            CloseConnectionBehaviorDemo.Run(cs);
            Console.WriteLine();
            SequentialAccessPreviewDemo.Run(cs);
            Console.WriteLine();
            ProperDisposalDemo.Run(cs);
            Console.WriteLine();
            ReadAsyncPreviewDemo.Run();
        }
        catch (SqlException ex)
        {
            Console.WriteLine();
            Console.WriteLine("Database demo skipped — SQL Server LocalDB not available.");
            Console.WriteLine($"  SqlException: {ex.Message}");
            Console.WriteLine("  Install SQL Server Express LocalDB or start (localdb)\\MSSQLLocalDB, then re-run.");
            Console.WriteLine("  Reader concepts above in source comments remain the learning path.");
            ReadAsyncPreviewDemo.Run();
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — SqlDataReader
 * =========================================================================
 *
 * --- Obtain a reader ---
 *
 *   using SqlDataReader reader = cmd.ExecuteReader();
 *   using SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
 *
 * --- Row loop ---
 *
 *   while (reader.Read()) { … }          // one row; false when done
 *
 * --- Typed accessors (prefer cached ordinal) ---
 *
 *   reader.GetInt32(i)     reader.GetString(i)     reader.GetDecimal(i)
 *   reader.GetDateTime(i)  reader.GetBoolean(i)    reader.GetDouble(i)
 *
 * --- Metadata ---
 *
 *   reader.FieldCount
 *   reader.GetName(i)           reader.GetFieldType(i)
 *   reader.GetOrdinal("Col")    reader.GetDataTypeName(i)
 *
 * --- Indexer (boxes to object) ---
 *
 *   reader["ColumnName"]        reader[i]
 *
 * --- NULL ---
 *
 *   if (reader.IsDBNull(i)) …
 *   reader.GetValue(i) == DBNull.Value
 *   // parameters: param.Value = DBNull.Value for SQL NULL
 *
 * --- Multiple result sets ---
 *
 *   while (reader.Read()) { … first set … }
 *   while (reader.NextResult()) {
 *       while (reader.Read()) { … next set … }
 *   }
 *
 * --- CommandBehavior flags ---
 *
 *   CloseConnection     dispose reader → connection closes
 *   SequentialAccess    stream LOBs in ordinal order; use GetBytes/GetChars
 *   SingleRow           hint: at most one row (optimization)
 *   SchemaOnly          columns only, no rows
 *   KeyInfo             include key columns in schema
 *
 * --- Disposal ---
 *
 *   using var reader = cmd.ExecuteReader();   // always dispose reader
 *
 * --- Manual mapping sketch ---
 *
 *   int o = reader.GetOrdinal("Id");
 *   while (reader.Read())
 *       list.Add(new Row { Id = reader.GetInt32(o), … });
 *
 * --- vs disconnected model ---
 *
 *   SqlDataReader     connected, streaming, read-only — ch.04 (this chapter)
 *   DataTable/Adapter  load all rows into memory — ch.05
 *
 * --- Async (preview) ---
 *
 *   await cmd.ExecuteReaderAsync()
 *   await reader.ReadAsync()
 *   → 08. Async ADO.NET
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Close connection before Read() done  | reader throws — connection must stay open
 *  GetInt32 on NULL column              | SqlNullValueException — use IsDBNull
 *  GetOrdinal inside every Read()       | works but slower — cache ordinals
 *  Forget to dispose reader             | leaked cursor handles on server/client
 *  SequentialAccess out of order        | undefined behavior / exceptions on LOBs
 *
 * --- Related chapters ---
 *
 *   02. SqlConnection & Connection Strings   open/close, connection strings
 *   03. SqlCommand & Parameters              ExecuteReader entry point
 *   05. DataSet, DataTable & SqlDataAdapter  disconnected alternative
 *   08. Async ADO.NET                        ReadAsync, ExecuteReaderAsync
 *
 * =========================================================================
 */
