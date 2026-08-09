using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using ParametersStoredProceduresAndQueryMultiple.Models;

namespace ParametersStoredProceduresAndQueryMultiple.Repositories;

/*
 * FILE ROLE: QueryMultiple and GridReader — multiple result sets in one round trip.
 *
 * SECTIONS IN THIS FILE:
 *   6. QueryMultiple and GridReader
 */

/*
 * SECTION 6: QueryMultiple AND GridReader
 *
 * One command can return multiple SELECT result sets (inline batch or stored procedure).
 * ADO.NET uses SqlDataReader.NextResult() (ch.04 SqlDataReader). Dapper wraps that in
 * GridReader returned from QueryMultiple.
 *
 *   Step | API call                         | Result
 *   -----|----------------------------------|------------------------------------------
 *   1    | connection.QueryMultiple(...)    | Opens reader; returns GridReader ( IDisposable )
 *   2    | multi.Read<Product>()            | Maps first result set to IEnumerable<Product>
 *   3    | multi.Read<int>()                | Maps second result set (single column → int)
 *   4    | multi.ReadFirst<string>()        | Single row from third set — or Read<string>().First()
 *
 * Rules:
 *   • Read result sets IN ORDER — GridReader is forward-only across sets (like NextResult).
 *   • Each Read<T>() must match the shape of the current result set columns.
 *   • Dispose GridReader (using var multi = ...) to close the reader and connection work.
 *   • QueryMultiple works with Text, StoredProcedure, and CommandDefinition.
 *
 * One round trip vs three separate Query calls — less latency, one execution plan context.
 *
 * COVERED IN ADO.NET ch.04 → SqlDataReader (NextResult / multiple result sets)
 */
public sealed class QueryMultipleRepository
{
    private readonly string _connectionString;

    public QueryMultipleRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public (IReadOnlyList<Product> Products, int TotalCount, string MostExpensive) GetDashboardFromProcedure()
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlMapper.GridReader multi = connection.QueryMultiple(
            "dbo.usp_GetProductDashboard",
            commandType: CommandType.StoredProcedure);

        IReadOnlyList<Product> products = multi.Read<Product>().AsList(); // result set 1 — all products
        int totalCount = multi.Read<int>().Single();                      // result set 2 — COUNT(*)
        string mostExpensive = multi.ReadFirst<string>();                 // result set 3 — TOP 1 name

        return (products, totalCount, mostExpensive);
    }

    public (IReadOnlyList<Product> Products, int TotalCount, string MostExpensive) GetDashboardFromInlineBatch()
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            ORDER BY ProductId;

            SELECT COUNT(*) FROM dbo.Products;

            SELECT TOP 1 ProductName FROM dbo.Products ORDER BY UnitPrice DESC;
            """;

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlMapper.GridReader multi = connection.QueryMultiple(sql); // Text — default command type

        IReadOnlyList<Product> products = multi.Read<Product>().AsList();
        int totalCount = multi.Read<int>().Single();
        string mostExpensive = multi.ReadFirst<string>();

        return (products, totalCount, mostExpensive);
    }
}
