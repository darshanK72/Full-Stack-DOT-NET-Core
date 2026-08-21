using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ParametersStoredProceduresAndQueryMultiple.Models;

namespace ParametersStoredProceduresAndQueryMultiple.Repositories;

/*
 * FILE ROLE: CommandDefinition — encapsulate SQL, parameters, timeout, commandType for reuse.
 *
 * SECTIONS IN THIS FILE:
 *   7. CommandDefinition
 */

/*
 * SECTION 7: CommandDefinition
 *
 * CommandDefinition bundles everything IDbConnection extension methods need into one
 * immutable-ish struct — useful when the same command is reused with different connections,
 * passed to helpers, or executed with consistent timeout/flags.
 *
 * Constructor parameters (common):
 *
 *   | Parameter      | Role
 *   |----------------|----------------------------------------------------------
 *   | commandText    | SQL string or procedure name
 *   | parameters     | anonymous object, DynamicParameters, or null
 *   | transaction    | IDbTransaction for enlisted commands (ch. ADO.NET transactions)
 *   | commandTimeout | Seconds — overrides connection default; null = provider default
 *   | commandType    | Text (default) or StoredProcedure
 *   | flags          | CommandFlags.Buffered, None, etc. (buffered vs unbuffered — ch.02)
 *
 * Any Dapper Query/Execute/QueryMultiple overload accepts CommandDefinition instead of
 * separate sql/params/commandType arguments:
 *
 *   var cmd = new CommandDefinition(
 *       "dbo.usp_UpdateProductPrice",
 *       new { ProductId = 1, NewPrice = 19.99m },
 *       commandType: CommandType.StoredProcedure,
 *       commandTimeout: 30);
 *   connection.Execute(cmd);
 *
 * --- 7a. Reuse across methods ---
 *
 * Build once in a repository field or factory; pass to Query<T>(commandDefinition) from
 * sync or async overloads (ExecuteAsync(cmd) — full async in ch.02).
 *
 * --- 7b. With DynamicParameters ---
 *
 * CommandDefinition accepts DynamicParameters for Output/Return procs — same as Execute(sql, dp, ...).
 */
public sealed class CommandDefinitionRepository
{
    private readonly string _connectionString;

    public CommandDefinitionRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Product? GetProductById(int productId)
    {
        CommandDefinition command = new CommandDefinition(
            """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE ProductId = @ProductId;
            """,
            new { ProductId = productId },
            commandTimeout: 30); // seconds — null would use SqlConnection default

        using SqlConnection connection = new SqlConnection(_connectionString);
        return connection.QuerySingleOrDefault<Product>(command);
    }

    public int UpdatePriceViaCommandDefinition(int productId, decimal newPrice)
    {
        CommandDefinition command = new CommandDefinition(
            "dbo.usp_UpdateProductPrice",
            new { ProductId = productId, NewPrice = newPrice },
            commandType: CommandType.StoredProcedure,
            commandTimeout: 15);

        using SqlConnection connection = new SqlConnection(_connectionString);
        return connection.Execute(command);
    }

    public IEnumerable<Product> GetActiveProductsViaCommandDefinition()
    {
        // Factory pattern — same definition reused by multiple callers
        CommandDefinition command = BuildActiveProductsCommand(minStock: 1);

        using SqlConnection connection = new SqlConnection(_connectionString);
        return connection.Query<Product>(command);
    }

    public static CommandDefinition BuildActiveProductsCommand(int minStock)
    {
        return new CommandDefinition(
            """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE StockQuantity >= @MinStock AND DiscontinuedDate IS NULL
            ORDER BY ProductName;
            """,
            new { MinStock = minStock },
            commandType: CommandType.Text,
            flags: CommandFlags.Buffered); // default for Query — materialize before reader closes
    }
}
