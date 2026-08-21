using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ParametersStoredProceduresAndQueryMultiple.Models;

namespace ParametersStoredProceduresAndQueryMultiple.Repositories;

/*
 * FILE ROLE: Executing SQL Server stored procedures through Dapper with CommandType.StoredProcedure.
 *
 * SECTIONS IN THIS FILE:
 *   5. Stored procedures
 */

/*
 * SECTION 5: STORED PROCEDURES
 *
 * Dapper passes commandType: CommandType.StoredProcedure to the underlying IDbCommand.
 * Same semantic as ADO.NET SqlCommand.CommandType (ch.07):
 *
 *   | Field / argument   | Stored procedure value              | Inline SQL value
 *   |--------------------|-------------------------------------|------------------
 *   | Command text       | "dbo.usp_UpdateProductPrice"        | "UPDATE ... WHERE Id=@id"
 *   | commandType        | CommandType.StoredProcedure         | CommandType.Text (default)
 *   | Parameters         | anonymous, DynamicParameters, etc.  | same
 *
 * Rules:
 *   • Command text = procedure name only — no EXEC, no parentheses.
 *   • Schema-qualify: dbo.usp_Name avoids wrong-object resolution.
 *   • Input params: anonymous object or dp.Add — same as parameterized Text commands.
 *   • Output/Return: DynamicParameters only (SECTION 4).
 *   • Query<T> on a proc that SELECTs rows maps to T — same as inline SQL.
 *   • Execute on a proc that UPDATE/INSERT/DELETE returns rows affected (when NOCOUNT OFF).
 *
 * COVERED IN ADO.NET ch.07 → Stored Procedures & Output Parameters (T-SQL and SqlCommand)
 */
public sealed class StoredProcedureRepository
{
    private readonly string _connectionString;

    public StoredProcedureRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public int UpdateProductPrice(int productId, decimal newPrice)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        // Input-only proc — anonymous params + StoredProcedure command type
        return connection.Execute(
            "dbo.usp_UpdateProductPrice",
            new { ProductId = productId, NewPrice = newPrice },
            commandType: CommandType.StoredProcedure);
    }

    public IEnumerable<Product> GetAllViaInlineBatch()
    {
        // Contrast: Text commandType is default — shown for comparison with proc below
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            ORDER BY ProductId;
            """;

        using SqlConnection connection = new SqlConnection(_connectionString);
        return connection.Query<Product>(sql); // CommandType.Text — default
    }

    public IEnumerable<Product> GetAllViaStoredProcedure()
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        // Proc that only SELECTs — Query<T> works; no DynamicParameters needed for input-only
        return connection.Query<Product>(
            "dbo.usp_GetProductDashboard",
            commandType: CommandType.StoredProcedure);
        // Note: usp_GetProductDashboard returns multiple result sets — Query reads the FIRST only.
        // For all sets use QueryMultiple (SECTION 6).
    }
}
