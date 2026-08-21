using System.Data;
using Microsoft.Data.SqlClient;
using TransactionsAndConnectionPooling.Utils;

namespace TransactionsAndConnectionPooling.Services;

/*
 * FILE ROLE: Nested using pattern for SqlConnection and SqlTransaction with explicit Commit.
 *
 * SECTIONS IN THIS FILE:
 *   8. using pattern for connections and transactions
 */

/*
 * SECTION 8: using PATTERN FOR CONNECTIONS AND TRANSACTIONS
 *
 * Nested using ensures Dispose order (transaction first, then connection):
 *
 *   using (var connection = new SqlConnection(cs))
 *   {
 *       connection.Open();
 *       using (var transaction = connection.BeginTransaction())
 *       {
 *           // commands with cmd.Transaction = transaction
 *           transaction.Commit(); // required before Dispose - else Rollback on Dispose
 *       }
 *   } // connection.Close() returns socket to pool - Section 9
 *
 * try/catch inside the block: catch -> transaction.Rollback() -> rethrow or handle.
 * Relying on Dispose alone also rolls back, but explicit Rollback logs intent clearly.
 */
internal static class TransactionUsingPatternDemo
{
    public static string RunExplicitUsingCommit()
    {
        using (SqlConnection connection = new SqlConnection(ConnectionStrings.AdoNetTutorial))
        {
            connection.Open();
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                using SqlCommand command = new SqlCommand(
                    "UPDATE dbo.Accounts SET Balance = Balance WHERE AccountId = 1;", // no-op sanity update
                    connection,
                    transaction);
                command.ExecuteNonQuery();
                transaction.Commit(); // must Commit before transaction.Dispose()
            }
        }

        return "using(connection) + using(transaction) + Commit - pattern completed.";
    }
}
