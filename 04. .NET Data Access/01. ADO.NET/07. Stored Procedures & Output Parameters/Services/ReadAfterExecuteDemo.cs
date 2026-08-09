using System;

namespace StoredProceduresAndOutputParameters.Services;

/*
 * FILE ROLE: Live workflow demonstrating reading Output and ReturnValue after ExecuteNonQuery.
 *
 * SECTIONS IN THIS FILE:
 *   6. Reading Output and Return after ExecuteNonQuery
 */

/*
 * SECTION 6: READING OUTPUT AND RETURN AFTER ExecuteNonQuery
 *
 * ExecuteNonQuery return value = rows affected (when SET NOCOUNT OFF) or -1
 * when NOCOUNT ON - do not confuse with OUTPUT or RETURN parameters.
 *
 *   Source                         | Where to read
 *   -------------------------------|------------------------------------------
 *   Rows affected (UPDATE/DELETE)  | int from ExecuteNonQuery() when NOCOUNT OFF
 *   OUTPUT parameter               | sqlParameter.Value after ExecuteNonQuery
 *   RETURN code                    | ReturnValue parameter .Value after execute
 *
 * Read parameters only AFTER the command finishes. Same applies to
 * ExecuteScalar and ExecuteReader - Output/Return populate when the reader closes
 * if the procedure did not leave an open result set blocking completion.
 *
 * Pitfall: Reading .Value before ExecuteNonQuery -> stale or null.
 * Pitfall: Casting DBNull.Value directly to int -> InvalidCastException - check first.
 */
public static class ReadAfterExecuteDemo
{
    public static void DemonstrateAllParameterReads()
    {
        Console.WriteLine();
        Console.WriteLine("=== Live demo: Input + Output + Return on one workflow ===");

        int newId = OutputParameterDemo.InsertProduct("Ergonomic Stand", 79.00m);
        Console.WriteLine("  Insert OUTPUT @NewProductId -> {0}", newId);

        string? name = OutputParameterDemo.GetProductName(newId);
        Console.WriteLine("  GetProductName OUTPUT @ProductName -> {0}", name ?? "(not found)");

        int count = ReturnValueDemo.GetProductCountViaReturnValue();
        Console.WriteLine("  GetProductCount RETURN value -> {0}", count);

        (string? missingName, int status) = ReturnValueDemo.TryGetProductName(99999);
        Console.WriteLine("  TryGetProductName missing id - RETURN={0}, Name={1}", status, missingName ?? "(null)");
    }
}
