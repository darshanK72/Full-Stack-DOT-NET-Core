using System.Data.Common;

namespace IntroductionToAdoNet.Services;

/*
 * FILE ROLE: In-memory DbDataReader demo using DataTableReader (Section 5 preview).
 * SECTIONS IN THIS FILE:
 *   5. DbDataReader — forward-only, read-only stream (preview)
 */

/*
 * =========================================================================
 * SECTION 5: DbDataReader — FORWARD-ONLY, READ-ONLY STREAM (PREVIEW)
 * =========================================================================
 *
 * DbDataReader reads one row at a time — memory-efficient for large results.
 * You must read columns by ordinal or name and check IsDBNull before mapping.
 *
 * Rules:
 *   • Read() advances; returns false when no more rows
 *   • Only one active reader per connection unless MARS enabled (ch.04)
 *   • Close/dispose reader before running another command on same connection
 *   • Prefer GetInt32("Col") / GetString("Col") over GetValue + cast (ch.04)
 *
 * FULL detail → ch.04 SqlDataReader
 *
 * Demo below uses DataTable.CreateDataReader() — a real DbDataReader subclass
 * over in-memory rows — so you can practice Read() without SQL Server.
 * -------------------------------------------------------------------------
 */
public static class ReaderDemo
{
    public static int CountRowsForwardOnly(DbDataReader reader)
    {
        int count = 0;
        while (reader.Read()) // advance one row; false at end
        {
            count++;
        }

        return count;
    }

    public static decimal SumColumn(DbDataReader reader, string columnName)
    {
        decimal total = 0m;
        int ordinal = reader.GetOrdinal(columnName); // resolve name once before loop

        while (reader.Read())
        {
            if (!reader.IsDBNull(ordinal))
            {
                total += reader.GetDecimal(ordinal);
            }
        }

        return total;
    }
}
