/*
 * TOPIC: Calling SQL Server stored procedures from ADO.NET with SqlCommand,
 *        CommandType.StoredProcedure, and SqlParameter directions (Input,
 *        Output, ReturnValue).
 *
 * WHY IT MATTERS:
 *   Stored procedures bundle validated, reusable T-SQL on the server. Production
 *   apps call them for inserts with generated keys, status codes, and business
 *   rules you do not want duplicated in every client. ADO.NET maps procedure
 *   parameters through SqlParameter.Direction - the same pattern Dapper and EF
 *   Core build on when they execute procedures.
 *
 * WHAT YOU WILL LEARN:
 *   1. CommandType.StoredProcedure and procedure naming (no EXEC prefix)
 *   2. Input parameters - @Name in T-SQL, ParameterDirection.Input (default)
 *   3. Output parameters - Direction Output, Size required for strings
 *   4. Return value parameter - Direction ReturnValue and T-SQL RETURN
 *   5. Reading Output and ReturnValue from SqlParameter.Value after ExecuteNonQuery
 *   6. LocalDB setup scripts (run once in SSMS or sqlcmd)
 *   7. When stored procedures beat inline SQL - and when they do not
 *   8. Preview: ExecuteNonQueryAsync (full async coverage in chapter 08)
 *
 * Prerequisites: ADO.NET chapters 02-03 (SqlConnection, SqlCommand, parameters).
 * Database: LocalDB - scripts in Utils/LocalDbConnection.cs. Demo uses try/catch if unavailable.
 *
 * CHAPTER MAP:
 *   1. LocalDB setup scripts              → Utils/LocalDbConnection.cs
 *   2. CommandType.StoredProcedure        → Services/StoredProcedureCommandBasics.cs
 *   3. Input parameters                   → Services/InputParameterDemo.cs
 *   4. Output parameters                  → Services/OutputParameterDemo.cs
 *   5. Return value parameter             → Services/ReturnValueDemo.cs
 *   6. Reading Output/Return after execute → Services/ReadAfterExecuteDemo.cs
 *   7. Stored procedures vs inline SQL    → Services/StoredProcedureVsInlineSql.cs
 *   8. ExecuteNonQueryAsync preview       → Services/AsyncPreview.cs
 *   9. Demonstration                      → Program.cs Main
 */

using System;
using Microsoft.Data.SqlClient;
using StoredProceduresAndOutputParameters.Services;
using StoredProceduresAndOutputParameters.Utils;

namespace StoredProceduresAndOutputParameters;

public class Program
{
    /*
     * SECTION 9: DEMONSTRATION - Main orchestrates the chapter demo
     * Live database calls are wrapped in try/catch so parameter setup sections
     * still run when LocalDB or AdoNetSpDemo is unavailable.
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("07. Stored Procedures & Output Parameters");
        Console.WriteLine("Connection: {0}", LocalDbConnection.ConnectionString);
        Console.WriteLine();

        InputParameterDemo.ShowInputParameterSetup();
        OutputParameterDemo.ShowOutputParameterSetup();
        StoredProcedureVsInlineSql.PrintDecisionGuide();
        AsyncPreview.ShowAsyncSignaturePreview();

        try
        {
            int rowsUpdated = InputParameterDemo.RunUpdateProductPrice(1, 34.99m);
            Console.WriteLine();
            Console.WriteLine("usp_UpdateProductPrice (Input only) - rows affected: {0}", rowsUpdated);

            ReadAfterExecuteDemo.DemonstrateAllParameterReads();
        }
        catch (SqlException ex)
        {
            Console.WriteLine();
            Console.WriteLine("Database demo skipped - LocalDB unavailable or setup scripts not run.");
            Console.WriteLine("  SqlException: {0}", ex.Message);
            Console.WriteLine("  Run SECTION 1 scripts in SSMS, then re-run this project.");
        }
    }
}

/*
 * QUICK REFERENCE
 *
 * cmd.CommandText = "dbo.usp_Name";
 * cmd.CommandType = CommandType.StoredProcedure;
 *
 * Input:    new SqlParameter("@P", SqlDbType.Int) { Value = x }
 * Output:   new SqlParameter("@P", SqlDbType.Int) { Direction = Output }
 * Output string: - NVarChar, size) { Direction = Output, Size = size }
 * Return:   new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ReturnValue }
 *
 * connection.Open();
 * command.ExecuteNonQuery();
 * int outVal = (int)outParam.Value;           // after execute
 * int retVal = (int)returnParam.Value;        // T-SQL RETURN int
 *
 * Async preview: await command.ExecuteNonQueryAsync(); -> chapter 08
 */
