using System;
using System.Data;
using Microsoft.Data.SqlClient;
using TransactionsAndConnectionPooling.Utils;

namespace TransactionsAndConnectionPooling.Services;

/*
 * FILE ROLE: SqlTransaction transfer demo — BeginTransaction, Commit, Rollback, and command enlistment.
 *
 * SECTIONS IN THIS FILE:
 *   5. SqlTransaction — BeginTransaction, Commit, Rollback
 *   6. Assign SqlCommand.Transaction — every command shares one transaction
 */

/*
 * SECTION 5: SqlTransaction - BeginTransaction, Commit, Rollback
 *
 * A transaction groups one or more commands into a single atomic unit:
 *   connection.BeginTransaction()  -> SqlTransaction object
 *   transaction.Commit()           -> persist all changes
 *   transaction.Rollback()       -> undo all changes since BeginTransaction
 *
 * If you Dispose a SqlTransaction without Commit, the provider calls Rollback.
 *
 * Transfer pattern (debit + credit must both succeed):
 *   1. BeginTransaction (optionally pass IsolationLevel - Section 7)
 *   2. Debit source account
 *   3. Credit destination account
 *   4. Commit - or Rollback on any failure
 */
internal static class AccountTransferService
{
    public static (bool Committed, string Message) TransferWithSqlTransaction(
        int fromAccountId,
        int toAccountId,
        decimal amount,
        IsolationLevel isolationLevel)
    {
        using SqlConnection connection = new SqlConnection(ConnectionStrings.AdoNetTutorial);
        connection.Open();
        using SqlTransaction transaction = connection.BeginTransaction(isolationLevel); // default overload = ReadCommitted

        try
        {
            if (amount <= 0m)
            {
                throw new InvalidOperationException("Transfer amount must be positive.");
            }

            decimal fromBalance = ReadBalance(connection, transaction, fromAccountId);
            if (fromBalance < amount)
            {
                throw new InvalidOperationException(
                    $"Insufficient funds: account {fromAccountId} has {fromBalance:C}, need {amount:C}.");
            }

            ExecuteBalanceUpdate(connection, transaction, fromAccountId, -amount); // debit
            ExecuteBalanceUpdate(connection, transaction, toAccountId, amount);    // credit

            transaction.Commit(); // all commands succeed - make changes permanent
            return (true, $"SQL COMMIT at {isolationLevel} - transfer of {amount:C} persisted.");
        }
        catch (Exception ex)
        {
            transaction.Rollback(); // explicit rollback - undo debit/credit in this transaction
            return (false, "SQL ROLLBACK - " + ex.Message);
        }
    }

    /*
     * SECTION 6: ASSIGN SqlCommand.Transaction - every command in the unit shares one transaction
     *
     * Without cmd.Transaction = transaction, each ExecuteNonQuery runs in auto-commit
     * mode (its own implicit transaction). Assign the same SqlTransaction instance to
     * every SqlCommand participating in the unit of work.
     */
    private static decimal ReadBalance(SqlConnection connection, SqlTransaction transaction, int accountId)
    {
        using SqlCommand command = new SqlCommand(
            "SELECT Balance FROM dbo.Accounts WHERE AccountId = @AccountId;",
            connection,
            transaction); // same connection + same transaction object
        command.Parameters.Add(new SqlParameter("@AccountId", SqlDbType.Int) { Value = accountId });
        object? scalar = command.ExecuteScalar();
        return scalar is decimal balance ? balance : 0m;
    }

    private static void ExecuteBalanceUpdate(
        SqlConnection connection,
        SqlTransaction transaction,
        int accountId,
        decimal delta)
    {
        using SqlCommand command = new SqlCommand(
            "UPDATE dbo.Accounts SET Balance = Balance + @Delta WHERE AccountId = @AccountId;",
            connection,
            transaction); // cmd.Transaction property wires command into the open transaction
        command.Parameters.Add(new SqlParameter("@Delta", SqlDbType.Decimal) { Value = delta });
        command.Parameters.Add(new SqlParameter("@AccountId", SqlDbType.Int) { Value = accountId });
        int rows = command.ExecuteNonQuery();
        if (rows != 1)
        {
            throw new InvalidOperationException($"Expected to update account {accountId}, affected {rows} row(s).");
        }
    }
}
