using System;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ParametersStoredProceduresAndQueryMultiple.Utils;

namespace ParametersStoredProceduresAndQueryMultiple.Repositories;

/*
 * FILE ROLE: DynamicParameters — Add, direction, output parameters, return values, TVP preview.
 *
 * SECTIONS IN THIS FILE:
 *   4. DynamicParameters
 *   8. Preview — table-valued parameters (TVPs)
 */

/*
 * SECTION 4: DynamicParameters
 *
 * Use DynamicParameters when anonymous objects are not enough:
 *   • ParameterDirection.Output or InputOutput
 *   • ParameterDirection.ReturnValue (T-SQL RETURN int)
 *   • Explicit DbType, size, precision/scale
 *   • Adding parameters incrementally in helper methods
 *
 *   API                          | Role
 *   -----------------------------|------------------------------------------------
 *   dp.Add(name, value)          | Input parameter (default direction)
 *   dp.Add(name, value, dbType, direction, size) | Full control — required for Output strings
 *   dp.Get<T>(name)              | Read Output/ReturnValue AFTER Execute/Query completes
 *   dp.AddDynamicParams(obj)     | Merge anonymous/typed object into existing dp
 *
 * --- 4a. Output parameters ---
 *
 * T-SQL: @NewProductId INT OUTPUT
 * Dapper:
 *
 *   dp.Add("@NewProductId", dbType: DbType.Int32, direction: ParameterDirection.Output);
 *   connection.Execute("dbo.usp_InsertProduct", dp, commandType: CommandType.StoredProcedure);
 *   int newId = dp.Get<int>("@NewProductId");  // read AFTER Execute
 *
 * String OUTPUT: pass size (same rule as ADO.NET SqlParameter.Size in ch.07):
 *
 *   dp.Add("@ProductName", dbType: DbType.String, size: 100, direction: ParameterDirection.Output);
 *
 * --- 4b. Return value ---
 *
 * T-SQL RETURN sends an int to a ReturnValue parameter — not the same as OUTPUT:
 *
 *   dp.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
 *   connection.Execute("dbo.usp_GetProductCount", dp, commandType: CommandType.StoredProcedure);
 *   int count = dp.Get<int>("@ReturnValue");
 *
 * --- 4c. InputOutput (related) ---
 *
 * dp.Add("@Stock", currentStock, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
 * Value is sent in; updated value read with dp.Get<int>("@Stock") after execute.
 *
 * --- 4d. AddDynamicParams ---
 *
 * Merge a filter object into dp, then add Output/Return parameters on top:
 *
 *   var dp = new DynamicParameters(new { ProductId = 1 });
 *   dp.Add("@ProductName", dbType: DbType.String, size: 100, direction: ParameterDirection.Output);
 *
 * Full SqlParameter direction tables and read-after-execute workflow:
 * COVERED IN ADO.NET ch.07 → Stored Procedures & Output Parameters
 */
public sealed class DynamicParameterRepository
{
    private readonly string _connectionString;

    public DynamicParameterRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public int InsertProduct(string productName, decimal unitPrice, int stockQuantity)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("@ProductName", productName);
        parameters.Add("@UnitPrice", unitPrice);
        parameters.Add("@StockQuantity", stockQuantity);
        parameters.Add("@NewProductId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        connection.Execute(
            "dbo.usp_InsertProduct",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<int>("@NewProductId"); // OUTPUT populated after Execute
    }

    public string? GetProductName(int productId)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("@ProductId", productId); // Input — default direction
        parameters.Add("@ProductName", dbType: DbType.String, size: 100, direction: ParameterDirection.Output);

        connection.Execute(
            "dbo.usp_GetProductName",
            parameters,
            commandType: CommandType.StoredProcedure);

        string name = parameters.Get<string>("@ProductName");
        return string.IsNullOrEmpty(name) ? null : name;
    }

    public int GetProductCountViaReturnValue()
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

        connection.Execute(
            "dbo.usp_GetProductCount",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<int>("@ReturnValue"); // T-SQL RETURN int — not OUTPUT
    }

    public (string? Name, int StatusCode) TryGetProductName(int productId)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        DynamicParameters parameters = new DynamicParameters();
        parameters.AddDynamicParams(new { ProductId = productId }); // merge Input from anonymous object
        parameters.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
        parameters.Add("@ProductName", dbType: DbType.String, size: 100, direction: ParameterDirection.Output);

        connection.Execute(
            "dbo.usp_TryGetProductName",
            parameters,
            commandType: CommandType.StoredProcedure);

        int statusCode = parameters.Get<int>("@ReturnValue");
        string nameText = parameters.Get<string>("@ProductName");
        string? name = string.IsNullOrEmpty(nameText) ? null : nameText;
        return (name, statusCode);
    }

    public void ShowDynamicParameterSetup()
    {
        Console.WriteLine();
        Console.WriteLine("=== DynamicParameters setup (no database) ===");

        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("@ProductId", 2);
        parameters.Add("@NewProductId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        parameters.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

        Console.WriteLine("  Input:    @ProductId = 2");
        Console.WriteLine("  Output:   @NewProductId (DbType.Int32, Direction=Output)");
        Console.WriteLine("  Return:   @ReturnValue (Direction=ReturnValue)");
        Console.WriteLine("  Read with dp.Get<T>(\"@Name\") after Execute completes");
    }

    /*
     * SECTION 8: PREVIEW — TABLE-VALUED PARAMETERS (TVPs)
     *
     * TVPs pass a whole table as one parameter (READONLY table type in T-SQL).
     * Dapper supports TVPs via DynamicParameters.Add with a DataTable or IEnumerable
     * and a structured SqlDbType — setup is verbose (user-defined table type on server).
     *
     * Typical pattern (not runnable here — requires CREATE TYPE dbo.IdList AS TABLE ...):
     *
     *   var table = new DataTable();
     *   table.Columns.Add("Id", typeof(int));
     *   table.Rows.Add(1); table.Rows.Add(2);
     *   dp.Add("@Ids", table.AsTableValuedParameter("dbo.IdList"));
     *
     * Bulk insert/update scenarios and full TVP wiring are advanced topics beyond this chapter.
     * COVERED IN DETAIL LATER → dedicated TVP / bulk patterns when added to curriculum
     */
    public static void ShowTvpPreview()
    {
        Console.WriteLine();
        Console.WriteLine("=== TVP preview (table-valued parameters) ===");
        Console.WriteLine("  Pass many keys/rows as one @Ids READONLY parameter via AsTableValuedParameter.");
        Console.WriteLine("  Requires CREATE TYPE on SQL Server — full wiring deferred.");
    }
}
