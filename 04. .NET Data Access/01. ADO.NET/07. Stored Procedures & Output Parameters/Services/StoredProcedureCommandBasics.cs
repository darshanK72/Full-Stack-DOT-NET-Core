using System.Data;
using Microsoft.Data.SqlClient;

namespace StoredProceduresAndOutputParameters.Services;

/*
 * FILE ROLE: CommandType.StoredProcedure basics and creating an update-price command.
 *
 * SECTIONS IN THIS FILE:
 *   2. CommandType.StoredProcedure
 */

/*
 * SECTION 2: CommandType.StoredProcedure
 *
 * SqlCommand defaults to CommandType.Text (inline SQL). For a procedure:
 *
 *   cmd.CommandText = "dbo.usp_UpdateProductPrice";  // name only - no EXEC
 *   cmd.CommandType = CommandType.StoredProcedure;
 *
 *   | Approach              | CommandText              | CommandType
 *   |-----------------------|--------------------------|---------------------------
 *   | Inline SQL (ch 03)    | "UPDATE ... WHERE Id=@id"  | Text (default)
 *   | Stored procedure      | "dbo.usp_UpdateProductPrice" | StoredProcedure
 *
 * Use schema-qualified names (dbo.usp_...) so SQL Server does not resolve the
 * wrong object. Do not append parentheses or EXEC - ADO.NET handles execution.
 *
 * COVERED IN ch 03 -> SqlCommand & Parameters (Text + Input parameters).
 * COVERED IN ch 04 -> SqlDataReader (reading result sets from procedures).
 */
public static class StoredProcedureCommandBasics
{
    public static SqlCommand CreateUpdatePriceCommand(SqlConnection connection, int productId, decimal newPrice)
    {
        SqlCommand command = new SqlCommand("dbo.usp_UpdateProductPrice", connection);
        command.CommandType = CommandType.StoredProcedure; // required - default Text would treat name as SQL

        command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });
        command.Parameters.Add(new SqlParameter("@NewPrice", SqlDbType.Decimal)
        {
            Value = newPrice,
            Precision = 10,
            Scale = 2
        });

        return command;
    }
}
