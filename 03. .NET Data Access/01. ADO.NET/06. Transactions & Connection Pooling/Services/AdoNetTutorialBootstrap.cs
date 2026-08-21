using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TransactionsAndConnectionPooling.Models;
using TransactionsAndConnectionPooling.Utils;

namespace TransactionsAndConnectionPooling.Services;

/*
 * FILE ROLE: Creates AdoNetTutorial database, Accounts table, and seed data for SQL transaction demos.
 *
 * SECTIONS IN THIS FILE:
 *   4. Database bootstrap — create AdoNetTutorial and Accounts table
 */

/*
 * SECTION 4: DATABASE BOOTSTRAP - create AdoNetTutorial and Accounts table
 *
 * Runs against master to CREATE DATABASE, then against AdoNetTutorial for DDL/DML.
 * Returns false when LocalDB is unreachable so Main can switch to InMemoryLedger.
 */
internal static class AdoNetTutorialBootstrap
{
    public static bool TryEnsureDatabaseReady(out string failureReason)
    {
        failureReason = string.Empty;

        try
        {
            using (SqlConnection masterConnection = new SqlConnection(ConnectionStrings.Master))
            {
                masterConnection.Open();
                using SqlCommand createDb = new SqlCommand(
                    """
                    IF DB_ID(N'AdoNetTutorial') IS NULL
                        CREATE DATABASE AdoNetTutorial;
                    """,
                    masterConnection);
                createDb.ExecuteNonQuery();
            }

            using (SqlConnection connection = new SqlConnection(ConnectionStrings.AdoNetTutorial))
            {
                connection.Open();
                using SqlCommand createTable = new SqlCommand(
                    """
                    IF OBJECT_ID(N'dbo.Accounts', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.Accounts
                        (
                            AccountId   INT            NOT NULL PRIMARY KEY,
                            AccountName NVARCHAR(100)  NOT NULL,
                            Balance     DECIMAL(18, 2) NOT NULL
                        );
                    END;
                    """,
                    connection);
                createTable.ExecuteNonQuery();

                using SqlCommand mergeSeed = new SqlCommand(
                    """
                    MERGE dbo.Accounts AS target
                    USING (VALUES
                        (1, N'Checking', 1000.00),
                        (2, N'Savings',   500.00)
                    ) AS source (AccountId, AccountName, Balance)
                    ON target.AccountId = source.AccountId
                    WHEN MATCHED THEN
                        UPDATE SET AccountName = source.AccountName, Balance = source.Balance
                    WHEN NOT MATCHED THEN
                        INSERT (AccountId, AccountName, Balance)
                        VALUES (source.AccountId, source.AccountName, source.Balance);
                    """,
                    connection);
                mergeSeed.ExecuteNonQuery();
            }

            return true;
        }
        catch (SqlException ex)
        {
            failureReason = ex.Message;
            return false;
        }
    }

    public static void ResetAccountsToSeed()
    {
        using SqlConnection connection = new SqlConnection(ConnectionStrings.AdoNetTutorial);
        connection.Open();
        using SqlCommand reset = new SqlCommand(
            """
            UPDATE dbo.Accounts SET Balance = 1000.00 WHERE AccountId = 1;
            UPDATE dbo.Accounts SET Balance =  500.00 WHERE AccountId = 2;
            """,
            connection);
        reset.ExecuteNonQuery();
    }

    public static List<Account> ReadAllAccounts()
    {
        List<Account> accounts = new List<Account>();
        using SqlConnection connection = new SqlConnection(ConnectionStrings.AdoNetTutorial);
        connection.Open();
        using SqlCommand command = new SqlCommand(
            "SELECT AccountId, AccountName, Balance FROM dbo.Accounts ORDER BY AccountId;",
            connection);
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            accounts.Add(new Account
            {
                AccountId = reader.GetInt32(0),
                AccountName = reader.GetString(1),
                Balance = reader.GetDecimal(2),
            });
        }

        return accounts;
    }
}
