using System;
using System.Data;
using Microsoft.Data.SqlClient;
using StoredProceduresAndOutputParameters.Utils;

namespace StoredProceduresAndOutputParameters.Services;

/*
 * FILE ROLE: Output parameters — identity after INSERT and string OUTPUT with Size.
 *
 * SECTIONS IN THIS FILE:
 *   4. Output parameters
 */

/*
 * SECTION 4: OUTPUT PARAMETERS
 *
 * T-SQL declares: @NewProductId INT OUTPUT  or  @Name NVARCHAR(100) OUTPUT
 * ADO.NET:
 *
 *   param.Direction = ParameterDirection.Output;
 *   param.Value is ignored before execute - read param.Value AFTER ExecuteNonQuery.
 *
 * --- 4a. Size is mandatory for string OUTPUT parameters ---
 *
 * SqlClient must allocate a buffer for NVARCHAR/VARCHAR output. Set Size to
 * the same (or larger) size as the T-SQL parameter. Missing Size -> runtime error
 * or truncated data.
 *
 * --- 4b. DBNull vs null ---
 *
 * After execute, check param.Value == DBNull.Value before casting (nullable types).
 *
 * --- 4c. InputOutput (related) ---
 *
 * ParameterDirection.InputOutput sends a value in and receives an updated value
 * back (T-SQL @x INT OUTPUT that you also pass in). Same Size rules for strings.
 */
public static class OutputParameterDemo
{
    public static int InsertProduct(string productName, decimal unitPrice)
    {
        using SqlConnection connection = new SqlConnection(LocalDbConnection.ConnectionString);
        using SqlCommand command = new SqlCommand("dbo.usp_InsertProduct", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.NVarChar, 100) { Value = productName });
        command.Parameters.Add(new SqlParameter("@UnitPrice", SqlDbType.Decimal)
        {
            Value = unitPrice,
            Precision = 10,
            Scale = 2
        });

        SqlParameter newIdParameter = new SqlParameter("@NewProductId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output // no Value needed before execute
        };
        command.Parameters.Add(newIdParameter);

        connection.Open();
        command.ExecuteNonQuery(); // also works with ExecuteScalar when no result set - Output still populated

        object rawId = newIdParameter.Value; // read AFTER execute
        return rawId == DBNull.Value ? 0 : Convert.ToInt32(rawId);
    }

    public static string? GetProductName(int productId)
    {
        using SqlConnection connection = new SqlConnection(LocalDbConnection.ConnectionString);
        using SqlCommand command = new SqlCommand("dbo.usp_GetProductName", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });

        SqlParameter nameParameter = new SqlParameter("@ProductName", SqlDbType.NVarChar, 100)
        {
            Direction = ParameterDirection.Output,
            Size = 100 // must match T-SQL NVARCHAR(100) - required for string OUTPUT
        };
        command.Parameters.Add(nameParameter);

        connection.Open();
        command.ExecuteNonQuery();

        if (nameParameter.Value == DBNull.Value)
        {
            return null;
        }

        string name = (string)nameParameter.Value;
        return string.IsNullOrEmpty(name) ? null : name;
    }

    public static void ShowOutputParameterSetup()
    {
        Console.WriteLine();
        Console.WriteLine("=== Output parameter setup (no database required) ===");

        SqlParameter newId = new SqlParameter("@NewProductId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        SqlParameter productName = new SqlParameter("@ProductName", SqlDbType.NVarChar, 100)
        {
            Direction = ParameterDirection.Output,
            Size = 100
        };

        Console.WriteLine("  {0}: Direction={1}, Size={2}", newId.ParameterName, newId.Direction, newId.Size);
        Console.WriteLine("  {0}: Direction={1}, Size={2} (string OUTPUT requires Size)", productName.ParameterName, productName.Direction, productName.Size);
    }
}
