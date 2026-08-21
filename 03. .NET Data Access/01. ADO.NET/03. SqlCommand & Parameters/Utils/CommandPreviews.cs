using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SqlCommandAndParameters.Utils;

/*
 * FILE ROLE:
 *   Forward references to SqlTransaction and stored procedure CommandType.
 *
 * SECTIONS IN THIS FILE:
 *   9. Transaction property — preview
 *  10. CommandType.StoredProcedure — preview
 */

/*
 * =========================================================================
 * SECTION 9: Transaction PROPERTY — PREVIEW
 * =========================================================================
 *
 * Assign cmd.Transaction = activeSqlTransaction so multiple commands share
 * one atomic unit (commit/rollback together). BeginTransaction, isolation
 * levels, and pooling interactions are ch06 Transactions & Connection Pooling.
 *
 * COVERED IN DETAIL LATER → 06. Transactions & Connection Pooling
 * -------------------------------------------------------------------------
 */
public static class TransactionPreview
{
    public static void PreviewCommandTransaction(SqlConnection connection)
    {
        Console.WriteLine("--- Transaction property preview ---");
        Console.WriteLine("  SqlTransaction tx = connection.BeginTransaction();");
        Console.WriteLine("  cmd.Transaction = tx;  // same tx on every command in the unit");
        Console.WriteLine("  tx.Commit();  // or tx.Rollback() on failure");
        Console.WriteLine("  COVERED IN DETAIL LATER → 06. Transactions & Connection Pooling");
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 10: CommandType.StoredProcedure — PREVIEW
 * =========================================================================
 *
 * When CommandText is a procedure name, set CommandType.StoredProcedure.
 * Input/output/return parameters and result shapes are ch07 Stored Procedures.
 *
 * COVERED IN DETAIL LATER → 07. Stored Procedures & Output Parameters
 * -------------------------------------------------------------------------
 */
public static class StoredProcedurePreview
{
    public static void PreviewStoredProcedure(SqlConnection connection)
    {
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "dbo.usp_ProductCount";              // procedure name, not SQL text
        command.CommandType = CommandType.StoredProcedure;         // required when not ad-hoc SQL

        object? count = command.ExecuteScalar(); // proc returns single scalar via SELECT COUNT
        Console.WriteLine("--- StoredProcedure CommandType preview ---");
        Console.WriteLine($"  dbo.usp_ProductCount ? active products: {count}");
        Console.WriteLine("  COVERED IN DETAIL LATER → 07. Stored Procedures & Output Parameters");
        Console.WriteLine();
    }
}
