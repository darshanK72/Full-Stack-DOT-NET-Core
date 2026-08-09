using System;

namespace DataSetDataTableAndSqlDataAdapter;

/*
 * FILE ROLE: Preview of SqlBulkCopy for high-volume inserts — full treatment in chapter 06.
 *
 * SECTIONS IN THIS FILE:
 *   10. Preview — SqlBulkCopy (high-volume inserts)
 */

/*
 * =========================================================================
 * SECTION 10: PREVIEW - SQLBULKCOPY (HIGH-VOLUME INSERTS)
 * =========================================================================
 *
 * SqlBulkCopy streams many rows into one table quickly - bypasses per-row
 * InsertCommand overhead from adapter.Update. Typical use: ETL, import jobs.
 *
 *   using var bulk = new SqlBulkCopy(connection);
 *   bulk.DestinationTableName = "staging.Products";
 *   bulk.WriteToServer(dataTable);   // or IDataReader
 *
 * COVERED IN DETAIL LATER -> 06. Transactions & Connection Pooling
 * (bulk load + transactions, batch size, SqlRowsCopied event).
 * -------------------------------------------------------------------------
 */
public static class SqlBulkCopyPreview
{
    public static void PrintPreview()
    {
        Console.WriteLine("=== PREVIEW: SqlBulkCopy (full treatment in ch.06) ===");
        Console.WriteLine("  SqlBulkCopy.WriteToServer(DataTable) - fast append to one SQL table");
        Console.WriteLine("  Pair with SqlTransaction when all-or-nothing load matters.");
        Console.WriteLine();
    }
}
