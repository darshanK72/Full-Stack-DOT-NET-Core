using IntroductionToAdoNet.Models;

namespace IntroductionToAdoNet.Utils;

/*
 * FILE ROLE: DbCommand execute method descriptions (Section 4 preview).
 * SECTIONS IN THIS FILE:
 *   4. DbCommand — send SQL or stored procedures (preview)
 */

/*
 * =========================================================================
 * SECTION 4: DbCommand — SEND SQL OR STORED PROCEDURES (PREVIEW)
 * =========================================================================
 *
 * DbCommand carries CommandText, CommandType, Parameters, and Timeout.
 * You attach it to an open DbConnection (or DbTransaction for atomic batches).
 *
 * Execute methods (FULL detail → ch.03 SqlCommand & Parameters):
 *
 *   Method              | Returns              | Typical use
 *   --------------------|----------------------|----------------------------------
 *   ExecuteNonQuery()   | int (rows affected)  | INSERT, UPDATE, DELETE
 *   ExecuteScalar()     | object? (first cell) | SELECT COUNT(*), identity value
 *   ExecuteReader()     | DbDataReader         | SELECT many rows — stream
 *
 * Always use parameters (@Name) instead of string concatenation — prevents SQL
 * injection. ch.03 covers SqlParameter, types, and DBNull.
 * -------------------------------------------------------------------------
 */
public static class CommandConcept
{
    public static string DescribeExecuteMethod(CommandExecuteMethod method)
    {
        return method switch
        {
            CommandExecuteMethod.NonQuery => "ExecuteNonQuery() → rows affected (INSERT/UPDATE/DELETE)",
            CommandExecuteMethod.Scalar => "ExecuteScalar() → first column of first row (aggregates)",
            CommandExecuteMethod.Reader => "ExecuteReader() → forward-only DbDataReader (SELECT)",
            _ => string.Empty,
        };
    }
}
