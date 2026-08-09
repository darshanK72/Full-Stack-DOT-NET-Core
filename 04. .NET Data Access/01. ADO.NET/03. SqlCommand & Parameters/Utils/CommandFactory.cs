using System.Data;
using Microsoft.Data.SqlClient;

namespace SqlCommandAndParameters.Utils;

/*
 * FILE ROLE:
 *   Factory helper for creating SqlCommand instances bound to an open connection.
 *
 * SECTIONS IN THIS FILE:
 *   1. SqlCommand core properties and CreateTextCommand factory
 */

/*
 * =========================================================================
 * SECTION 1: SqlCommand CORE PROPERTIES
 * =========================================================================
 *
 * SqlCommand is the object that carries SQL text (or a procedure name) to
 * SQL Server on an open SqlConnection.
 *
 * Key properties:
 *
 *   Property       | Role
 *   ---------------|-------------------------------------------------------
 *   CommandText    | SQL statement or stored procedure name
 *   CommandType    | CommandType.Text (default) or StoredProcedure
 *   Connection     | Open SqlConnection the command runs on (required)
 *   Transaction    | Optional SqlTransaction — all commands in one unit
 *   CommandTimeout | Seconds before cancel (default 30); 0 = no limit
 *   Parameters     | SqlParameterCollection — @Name placeholders in SQL
 *
 * Typical construction:
 *
 *   using var cmd = connection.CreateCommand();   // inherits Connection
 *   cmd.CommandText = "UPDATE dbo.Products SET Stock = @stock WHERE ProductId = @id";
 *   cmd.Parameters.Add(new SqlParameter("@stock", SqlDbType.Int) { Value = 5 });
 *
 * Or:
 *
 *   using var cmd = new SqlCommand(sql, connection);
 *
 * CommandType defaults to Text — you only set StoredProcedure when CommandText
 * holds a procedure name (full depth in ch07).
 *
 * ch.02 SqlConnection covers opening/closing the connection; this chapter
 * assumes Connection is open before Execute*.
 * -------------------------------------------------------------------------
 */
public static class CommandFactory
{
    public static SqlCommand CreateTextCommand(SqlConnection connection, string sql)
    {
        SqlCommand command = connection.CreateCommand(); // SqlCommand bound to this connection
        command.CommandText = sql;                         // ad-hoc SQL (CommandType.Text is default)
        command.CommandType = CommandType.Text;            // explicit for clarity; Text is the default
        return command;
    }
}
