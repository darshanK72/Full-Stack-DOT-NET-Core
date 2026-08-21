using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace StoredProceduresAndOutputParameters.Services;

/*
 * FILE ROLE: When to prefer stored procedures vs inline parameterized SQL.
 *
 * SECTIONS IN THIS FILE:
 *   7. Stored procedures vs inline SQL
 */

/*
 * SECTION 7: STORED PROCEDURES VS INLINE SQL
 *
 * Prefer stored procedures when:
 *   - Same operation is called from many apps/services (single source of truth)
 *   - You grant EXEC on procedures without exposing base tables
 *   - Complex multi-step logic, temp tables, or plan stability matters
 *   - OUTPUT/RETURN contracts are part of a stable API surface
 *
 * Prefer inline parameterized SQL when:
 *   - Simple one-off SELECT/UPDATE and ORM/query builder generates SQL
 *   - Reporting with dynamic WHERE clauses (still parameterized - never concat)
 *   - Migrations and schema change frequently during early development
 *
 * ADO.NET cost is similar - CommandType differs; parameter binding is the same.
 * Dapper: CommandType.StoredProcedure + DynamicParameters for Output (see Dapper ch 03).
 * EF Core: FromSqlRaw / ExecuteSqlRaw with SqlParameter (see EF Core ch 09).
 */
public static class StoredProcedureVsInlineSql
{
    public static void PrintDecisionGuide()
    {
        Console.WriteLine();
        Console.WriteLine("=== Stored procedures vs inline SQL ===");
        Console.WriteLine("  Use procedures for reusable writes, OUTPUT/RETURN contracts, and EXEC-only security.");
        Console.WriteLine("  Use inline parameterized Text for simple reads and ORM-generated queries.");
        Console.WriteLine("  Both require SqlParameter - never embed user input in CommandText.");
    }

    public static SqlCommand CreateInlineUpdateCommand(SqlConnection connection, int productId, decimal newPrice)
    {
        SqlCommand command = new SqlCommand(
            "UPDATE dbo.Products SET UnitPrice = @NewPrice WHERE ProductId = @ProductId",
            connection);
        command.CommandType = CommandType.Text; // default - shown for contrast with section 2

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
