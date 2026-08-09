using System;
using System.Data;
using Microsoft.Data.SqlClient;
using StoredProceduresAndOutputParameters.Utils;

namespace StoredProceduresAndOutputParameters.Services;

/*
 * FILE ROLE: ReturnValue parameter and combined OUTPUT + RETURN status patterns.
 *
 * SECTIONS IN THIS FILE:
 *   5. Return value parameter
 */

/*
 * SECTION 5: RETURN VALUE PARAMETER
 *
 * T-SQL: RETURN @SomeInt;  (must be int - not string, not decimal)
 * ADO.NET: one SqlParameter with Direction = ParameterDirection.ReturnValue
 *
 *   cmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int)
 *   {
 *       Direction = ParameterDirection.ReturnValue
 *   });
 *
 * The ParameterName (@ReturnValue) is conventional; SqlClient maps it to the
 * procedure's RETURN code. Add the return parameter before or after others -
 * order does not matter when ParameterName matches.
 *
 * RETURN is not the same as OUTPUT:
 *   RETURN     -> single int status/count via ReturnValue parameter
 *   OUTPUT     -> any SqlDbType, named parameter, can be multiple per procedure
 *
 * Common pattern: RETURN 0 success, non-zero error/status (like usp_TryGetProductName).
 */
public static class ReturnValueDemo
{
    public static int GetProductCountViaReturnValue()
    {
        using SqlConnection connection = new SqlConnection(LocalDbConnection.ConnectionString);
        using SqlCommand command = new SqlCommand("dbo.usp_GetProductCount", connection);
        command.CommandType = CommandType.StoredProcedure;

        SqlParameter returnParameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
        {
            Direction = ParameterDirection.ReturnValue
        };
        command.Parameters.Add(returnParameter);

        connection.Open();
        command.ExecuteNonQuery();

        return Convert.ToInt32(returnParameter.Value); // row count returned by T-SQL RETURN
    }

    public static (string? Name, int StatusCode) TryGetProductName(int productId)
    {
        using SqlConnection connection = new SqlConnection(LocalDbConnection.ConnectionString);
        using SqlCommand command = new SqlCommand("dbo.usp_TryGetProductName", connection);
        command.CommandType = CommandType.StoredProcedure;

        SqlParameter returnParameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
        {
            Direction = ParameterDirection.ReturnValue
        };
        command.Parameters.Add(returnParameter);

        command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });

        SqlParameter nameParameter = new SqlParameter("@ProductName", SqlDbType.NVarChar, 100)
        {
            Direction = ParameterDirection.Output,
            Size = 100
        };
        command.Parameters.Add(nameParameter);

        connection.Open();
        command.ExecuteNonQuery();

        int statusCode = Convert.ToInt32(returnParameter.Value);
        string nameText = nameParameter.Value == DBNull.Value ? string.Empty : (string)nameParameter.Value;
        string? name = string.IsNullOrEmpty(nameText) ? null : nameText;
        return (name, statusCode);
    }
}
