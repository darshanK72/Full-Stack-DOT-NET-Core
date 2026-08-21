/*
 * TOPIC: ADO.NET transactions and connection pooling - grouping multiple SQL
 *        statements into an atomic unit (commit or rollback) and reusing open
 *        connections efficiently via the provider pool.
 *
 * WHY IT MATTERS:
 *   Transferring money between accounts requires debit AND credit to succeed
 *   together. If credit fails after debit, money vanishes. Transactions guarantee
 *   all-or-nothing. Connection pooling makes Open/Close cheap so web apps can
 *   borrow and return connections per request without paying TCP + login every time.
 *
 * WHAT YOU WILL LEARN:
 *   1.  SqlTransaction - BeginTransaction, Commit, Rollback
 *   2.  Assigning SqlCommand.Transaction so commands share one transaction
 *   3.  Isolation levels (ReadCommitted default; overview of others)
 *   4.  using pattern for connections and transactions (Dispose -> auto-rollback)
 *   5.  Connection pooling - how it works; Min/Max Pool Size in the string
 *   6.  SqlConnection.ClearPool / ClearAllPools (preview)
 *   7.  Why Open/Close is cheap when pooling is enabled
 *   8.  SqlBulkCopy - preview of bulk insert API
 *   9.  Async transactions - preview (full coverage in chapter 08)
 *
 * CHAPTER MAP:
 *   1.  Connection strings              → Utils/ConnectionStrings.cs
 *   2.  Account row type                 → Models/Account.cs
 *   3.  In-memory ledger fallback        → Services/InMemoryLedger.cs
 *   4.  Database bootstrap               → Services/AdoNetTutorialBootstrap.cs
 *   5.  SqlTransaction transfer          → Services/AccountTransferService.cs
 *   6.  SqlCommand.Transaction enlistment  → Services/AccountTransferService.cs
 *   7.  Isolation levels                 → Services/IsolationLevelReference.cs
 *   8.  using pattern                    → Services/TransactionUsingPatternDemo.cs
 *   9.  Connection pooling               → Services/ConnectionPoolingDemo.cs
 *  10.  ClearPool preview                 → Services/ConnectionPoolingDemo.cs
 *  11.  SqlBulkCopy preview               → Services/SqlBulkCopyPreview.cs
 *  12.  Async transactions preview        → Services/AsyncTransactionPreview.cs
 *  13.  Demonstration                     → Program.cs Main
 */

using System;
using System.Collections.Generic;
using System.Data;
using TransactionsAndConnectionPooling.Models;
using TransactionsAndConnectionPooling.Services;

namespace TransactionsAndConnectionPooling;

public class Program
{
    /*
     * SECTION 13: DEMONSTRATION - Main orchestrates commit, rollback, pooling, and previews
     *
     * Flow:
     *   1. Try LocalDB AdoNetTutorial - fallback to InMemoryLedger on failure
     *   2. Successful transfer (COMMIT)
     *   3. Failed transfer (ROLLBACK - insufficient funds)
     *   4. Isolation level reference + explicit ReadCommitted transfer
     *   5. using-pattern demo
     *   6. Connection pooling timing + ClearPool preview
     *   7. SqlBulkCopy and async forward references
     */
    public static void Main(string[] args)
    {
        bool useSql = AdoNetTutorialBootstrap.TryEnsureDatabaseReady(out string bootstrapFailure);
        InMemoryLedger inMemoryLedger = new InMemoryLedger();

        string storageMode = useSql
            ? "LocalDB AdoNetTutorial (SQL transactions)"
            : "In-memory ledger (LocalDB unavailable: " + bootstrapFailure + ")";

        decimal transferAmount = 200.00m;
        decimal excessiveAmount = 2000.00m;

        string balancesBefore = FormatBalances(useSql, inMemoryLedger);
        (bool commitOk, string commitMessage) = RunTransfer(useSql, inMemoryLedger, 1, 2, transferAmount);
        string balancesAfterCommit = FormatBalances(useSql, inMemoryLedger);

        if (useSql)
        {
            AdoNetTutorialBootstrap.ResetAccountsToSeed(); // reset for rollback demo
        }
        else
        {
            inMemoryLedger.ResetToSeed();
        }

        (bool rollbackOk, string rollbackMessage) = RunTransfer(useSql, inMemoryLedger, 1, 2, excessiveAmount);
        string balancesAfterRollback = FormatBalances(useSql, inMemoryLedger);

        string isolationText = IsolationLevelReference.DescribeLevels();
        (bool explicitOk, string explicitMessage) = useSql
            ? AccountTransferService.TransferWithSqlTransaction(1, 2, 50.00m, IsolationLevel.ReadCommitted)
            : inMemoryLedger.Transfer(1, 2, 50.00m);

        string usingPatternResult = useSql
            ? TransactionUsingPatternDemo.RunExplicitUsingCommit()
            : "using-pattern applies to SqlConnection/SqlTransaction - skipped in in-memory mode.";

        (long poolMs, string poolExplanation) = useSql
            ? ConnectionPoolingDemo.MeasurePooledOpenClose(20)
            : (0L, "Pooling demo requires LocalDB - skipped in in-memory mode.");

        string clearPoolResult = useSql
            ? ConnectionPoolingDemo.PreviewClearPool()
            : "ClearPool preview requires LocalDB - skipped in in-memory mode.";

        string bulkCopyPreview = SqlBulkCopyPreview.DescribeUsage();
        string asyncPreview = AsyncTransactionPreview.ForwardReference();

        Console.WriteLine("=== 06. Transactions & Connection Pooling ===");
        Console.WriteLine("Storage: " + storageMode);
        Console.WriteLine();
        Console.WriteLine("--- Balances before transfer ---");
        Console.WriteLine(balancesBefore);
        Console.WriteLine("--- COMMIT demo ($200 Checking -> Savings) ---");
        Console.WriteLine(commitMessage + " Success=" + commitOk);
        Console.WriteLine(balancesAfterCommit);
        Console.WriteLine("--- ROLLBACK demo ($2000 - insufficient funds) ---");
        Console.WriteLine(rollbackMessage + " Success=" + rollbackOk);
        Console.WriteLine(balancesAfterRollback);
        Console.WriteLine("--- Isolation levels ---");
        Console.WriteLine(isolationText);
        Console.WriteLine("Explicit ReadCommitted transfer: " + explicitMessage + " Success=" + explicitOk);
        Console.WriteLine("--- using pattern ---");
        Console.WriteLine(usingPatternResult);
        Console.WriteLine("--- Connection pooling ---");
        Console.WriteLine(poolExplanation + " (" + poolMs + " ms)");
        Console.WriteLine(clearPoolResult);
        Console.WriteLine("--- Previews ---");
        Console.WriteLine(bulkCopyPreview);
        Console.WriteLine(asyncPreview);
    }

    private static (bool Committed, string Message) RunTransfer(
        bool useSql,
        InMemoryLedger inMemoryLedger,
        int fromAccountId,
        int toAccountId,
        decimal amount)
    {
        if (useSql)
        {
            return AccountTransferService.TransferWithSqlTransaction(
                fromAccountId,
                toAccountId,
                amount,
                IsolationLevel.ReadCommitted);
        }

        return inMemoryLedger.Transfer(fromAccountId, toAccountId, amount);
    }

    private static string FormatBalances(bool useSql, InMemoryLedger inMemoryLedger)
    {
        if (useSql)
        {
            List<Account> accounts = AdoNetTutorialBootstrap.ReadAllAccounts();
            return FormatAccountList(accounts);
        }

        return FormatAccountList(inMemoryLedger.GetAccounts());
    }

    private static string FormatAccountList(IReadOnlyList<Account> accounts)
    {
        List<string> lines = new List<string>();
        foreach (Account account in accounts)
        {
            lines.Add($"  {account.AccountId} {account.AccountName}: {account.Balance:C}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}

/*
 * QUICK REFERENCE
 *
 * Begin:     using var tx = connection.BeginTransaction(IsolationLevel.ReadCommitted);
 * Enlist:    cmd = new SqlCommand(sql, connection, tx);  // or cmd.Transaction = tx;
 * Success:   tx.Commit();
 * Failure:   tx.Rollback();  // or let tx.Dispose() roll back if not committed
 * Pool:      Close/Dispose returns connection; same connection string reuses pool
 * Pool size: Min Pool Size=n; Max Pool Size=n in connection string
 * Clear:     SqlConnection.ClearPool(conn); SqlConnection.ClearAllPools();
 * Bulk:      SqlBulkCopy - preview; enlist optional SqlTransaction
 * Async:     chapter 08 - OpenAsync, ExecuteNonQueryAsync inside transactions
 */
