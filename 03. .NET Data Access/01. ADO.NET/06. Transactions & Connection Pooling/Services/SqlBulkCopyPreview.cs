namespace TransactionsAndConnectionPooling.Services;

/*
 * FILE ROLE: Preview of SqlBulkCopy high-volume insert API (full coverage deferred).
 *
 * SECTIONS IN THIS FILE:
 *  11. SqlBulkCopy — preview
 */

/*
 * SECTION 11: SqlBulkCopy - PREVIEW
 *
 * COVERED IN DETAIL LATER -> dedicated bulk-import scenarios / performance chapters
 *
 * SqlBulkCopy streams many rows to a table without row-by-row INSERT overhead.
 * Typical shape:
 *
 *   using var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
 *   bulk.DestinationTableName = "dbo.StagingAccounts";
 *   bulk.WriteToServer(dataTable); // or IDataReader
 *
 * Can enlist in an existing SqlTransaction (third ctor argument) so bulk load
 * rolls back with the rest of the unit of work.
 */
internal static class SqlBulkCopyPreview
{
    public static string DescribeUsage()
    {
        return "SqlBulkCopy - high-volume insert into dbo.Accounts; enlist via SqlBulkCopy(connection, options, transaction).";
    }
}
