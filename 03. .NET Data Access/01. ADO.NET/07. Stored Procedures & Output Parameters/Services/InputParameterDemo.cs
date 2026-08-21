using System;
using System.Data;
using Microsoft.Data.SqlClient;
using StoredProceduresAndOutputParameters.Utils;

namespace StoredProceduresAndOutputParameters.Services;

/*
 * FILE ROLE: Input parameter setup and executing an input-only stored procedure.
 *
 * SECTIONS IN THIS FILE:
 *   3. Input parameters
 */

/*
 * SECTION 3: INPUT PARAMETERS
 *
 * T-SQL: CREATE PROCEDURE - @ProductId INT, @NewPrice DECIMAL(10,2)
 * ADO.NET: SqlParameter with Direction = Input (the default).
 *
 *   Parameter property    | Role
 *   ------------------------|------------------------------------------------
 *   ParameterName           | "@ProductId" - @ prefix matches T-SQL (recommended)
 *   SqlDbType               | Int, Decimal, NVarChar, etc. - type sent to server
 *   Value                     | Value sent before ExecuteNonQuery / ExecuteReader
 *   Direction               | ParameterDirection.Input (default if omitted)
 *
 * Always use SqlParameter - never concatenate user input into CommandText.
 * Same injection protection as inline parameterized SQL from chapter 03.
 *
 * --- 3a. Add with object initializer vs AddWithValue ---
 *
 * AddWithValue("@Name", value) is convenient but can infer wrong SqlDbType/size.
 * Prefer explicit SqlDbType for production code (especially Decimal precision).
 */
public static class InputParameterDemo
{
    public static void ShowInputParameterSetup()
    {
        Console.WriteLine("=== Input parameter setup (no database required) ===");

        SqlParameter productId = new SqlParameter("@ProductId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Input, // default - shown for clarity
            Value = 2
        };

        SqlParameter newPrice = new SqlParameter("@NewPrice", SqlDbType.Decimal)
        {
            Value = 39.99m,
            Precision = 10,
            Scale = 2
        };

        Console.WriteLine("  {0}: Direction={1}, Value={2}", productId.ParameterName, productId.Direction, productId.Value);
        Console.WriteLine("  {0}: Direction={1}, Value={2}", newPrice.ParameterName, newPrice.Direction, newPrice.Value);
    }

    public static int RunUpdateProductPrice(int productId, decimal newPrice)
    {
        using SqlConnection connection = new SqlConnection(LocalDbConnection.ConnectionString);
        using SqlCommand command = StoredProcedureCommandBasics.CreateUpdatePriceCommand(connection, productId, newPrice);
        connection.Open();
        return command.ExecuteNonQuery(); // rows affected from UPDATE inside the procedure
    }
}
