// To clear the interview, you must complete at least one bug fix and two tasks.
// A single task may involve multiple functions; sub-parts like 2.1 and 2.2 are considered one task.
// Please provide a verbal walkthrough of your thought process while writing the code.

// payment transaction https://www.onlinegdb.com/online_java_compiler#

/*<bug-task1>
 * We are developing a payment transaction monitoring system that tracks
 * accounts and their transactions. The system can compute each account's
 * current balance and basic statistics.
 *
 * Classes:
 *   TransactionType — enum: CREDIT, DEBIT
 *   Transaction     — a single transaction (id, accountId, type, amount, timestampSec)
 *   Account         — an account (accountId, ownerName)
 *   AccountManager  — manages accounts and transactions
 *
 * TASK 1: Read the code. The test is not passing due to a bug.
 *         Find and fix the bug in AccountManager.
 </bug-task1>
 */

/*<task2>
 * TASK 2: Implement getAverageTransactionAmountByAccount():
 *   - Return Map<String, Double>: accountId → average |amount| of transactions.
 *   - Both CREDIT and DEBIT considered; use absolute values.
 *   - Accounts with no transactions should NOT appear.
 </task2>
 */

/*<task3>
 * TASK 3: Implement getTransactionFees():
 *   - First 3 transactions per account are FREE.
 *   - From 4th onward: CREDIT costs $1, DEBIT costs $2.
 *   - Process in chronological order (by timestampSec).
 *   - Return Map<String, Integer>: accountId → total fees (ALL accounts).
 </task3>
 */

// transaction group by acccount id
// order by timestampsec
// skip 3
// Sum( if(credit add 1 if depb add 2))

/*<task4>
 * TASK 4: Implement getSuspiciousAccounts():
 *   - Consider only DEBIT transactions.
 *   - A DEBIT is "large" if amount >= 50.
 *   - An account is suspicious if it has 3+ large DEBITs within
 *     ANY 60-second window (inclusive of endpoints: t and t+60 are same window).
 *   - Process per-account in chronological order.
 *   - Return sorted List<String> of suspicious accountIds.
 </task4>
 */

// filter account where type is debit and amount >= 50
// order by timestamp
// foreach transaction, we find if any transaction exists if prev and current differ is less thatn or equal to 60 sec, add that into output, return sorted list of accounts ids

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetQuestions
{
    public class Payment
    {
        enum TransactionType { CREDIT, DEBIT }

        class Transaction
        {
            private string id, accountId;
            private TransactionType type;
            private int amount, timestampSec;

            public Transaction(string id, string accountId, TransactionType type, int amount, int timestampSec)
            {
                this.id = id; this.accountId = accountId; this.type = type;
                this.amount = amount; this.timestampSec = timestampSec;
            }
            public string GetAccountId() { return accountId; }
            public TransactionType GetTransactionType() { return type; }
            public int GetAmount() { return amount; }
            public int GetTimestampSec() { return timestampSec; }
        }

        class Account
        {
            public string accountId, ownerName;
            public Account(string accountId, string ownerName)
            {
                this.accountId = accountId; this.ownerName = ownerName;
            }
        }

        class AccountManager
        {
            public List<Account> accounts = new List<Account>();
            public List<Transaction> transactions = new List<Transaction>();

            public void AddAccount(Account a) { accounts.Add(a); }
            public void AddTransaction(Transaction t) { transactions.Add(t); }

            /**
             * BUG 1: Balance Calculation
             * Logic Errors:
             * 1. It only processes CREDIT transactions.
             * 2. It completely ignores DEBIT transactions, failing to subtract from the balance.
             */
            public Dictionary<string, int> GetBalances()
            {
                Dictionary<string, int> balances = new Dictionary<string, int>();
                foreach (Account a in accounts) balances[a.accountId] = 0;

                foreach (Transaction t in transactions)
                {
                    if (t.GetTransactionType() == TransactionType.CREDIT)
                    {
                        int current = balances.ContainsKey(t.GetAccountId()) ? balances[t.GetAccountId()] : 0;
                        balances[t.GetAccountId()] = current + t.GetAmount();
                    }
                    // BUG: Logic for DEBIT subtraction is missing here
                    else if (t.GetTransactionType() == TransactionType.DEBIT)
                    {
                        int current = balances.ContainsKey(t.GetAccountId()) ? balances[t.GetAccountId()] : 0;
                        balances[t.GetAccountId()] = current - t.GetAmount();
                    }
                }
                return balances;
            }

            /**
             * TASK 1: Average Transaction Amount
             * Currently unimplemented - returns empty map.
             */
            public Dictionary<string, double> GetAverageTransactionAmountByAccount()
            {
                return transactions
                    .GroupBy(trs => trs.GetAccountId())
                    .ToDictionary(
                        g => g.Key,
                        g => g.Average(trs => trs.GetAmount())
                    );
            }

            /**
             * TASK 2: Transaction Fees
             * Currently unimplemented - returns empty map.
             */
            public Dictionary<string, int> GetTransactionFees()
            {
                Dictionary<string, int> output = new Dictionary<string, int>();
                foreach (Account acc in accounts)
                {
                    output[acc.accountId] = 0;
                }

                foreach (var grp in transactions.GroupBy(trs => trs.GetAccountId()))
                {
                    int total = grp.OrderBy(trs => trs.GetTimestampSec())
                    .Skip(3)
                    .Sum(trs => (trs.GetTransactionType() == TransactionType.CREDIT) ? 1 : 2);
                    output[grp.Key] = total;
                }
                return output;
            }

            /**
             * TASK 3: Suspicious Accounts (3+ large debits in 60s)
             * Currently unimplemented - returns empty list.
             */
            public List<string> GetSuspiciousAccounts()
            {
                List<string> output = new List<string>();
                var suppAccsGrps = transactions
                    .Where(trs => trs.GetTransactionType() == TransactionType.DEBIT && trs.GetAmount() >= 50)
                    .GroupBy(trs => trs.GetAccountId());

                foreach (var grp in suppAccsGrps)
                {
                    var trans = grp.OrderBy(trs => trs.GetTimestampSec()).ToList();
                    for (int i = 0; i < trans.Count; i++)
                    {
                        int count = 0;
                        int startWindow = trans[i].GetTimestampSec();
                        for (int j = i; j < trans.Count; j++)
                        {
                            if (trans[j].GetTimestampSec() <= startWindow + 60)
                            {
                                count++;
                            }
                        }
                        if (count >= 3)
                        {
                            output.Add(grp.Key);
                        }
                    }
                }

                return output.OrderBy(o => o).ToList();
            }
        }

        static int passed = 0, failed = 0;

        public static void Main(string[] args)
        {
            Console.WriteLine("=== TRANSACTION MONITORING TEST SUITE ===\n");

            // <bug-task1>
            RunTest("BUG 1: Balance Accuracy (Debit Check)", () =>
            {
                AccountManager mgr = new AccountManager();
                mgr.AddAccount(new Account("A1", "Alice"));
                mgr.AddTransaction(new Transaction("T1", "A1", TransactionType.CREDIT, 100, 10));
                mgr.AddTransaction(new Transaction("T2", "A1", TransactionType.DEBIT, 40, 20));
                // Should be 60. But Bug 1 will result in 100.
                Check(mgr.GetBalances()["A1"] == 60, "Expected balance 60, but got: " + mgr.GetBalances()["A1"]);
            });
            // </bug-task1>

            // <task2>
            RunTest("TASK 1: Average Abs Amounts", () =>
            {
                AccountManager mgr = new AccountManager();
                mgr.AddAccount(new Account("A1", "Alice"));
                mgr.AddTransaction(new Transaction("T1", "A1", TransactionType.DEBIT, 50, 10));
                Check(mgr.GetAverageTransactionAmountByAccount().ContainsKey("A1"), "Task 1: Account missing from averages");
            });
            // </task2>

            // <task3>
            RunTest("TASK 2: Tiered Fees", () =>
            {
                var mgr = new AccountManager();
                mgr.AddAccount(new Account("A1", "Alice"));

                var fees = mgr.GetTransactionFees();

                Check(
                    fees.TryGetValue("A1", out var fee),
                    "Task 2: Account missing from fees report"
                );

                Check(
                    fee == 0,
                    $"Task 2: Expected zero fees for an account with no transactions, but found {fee}"
                );
            });
            // </task3>

            // <task4>
            RunTest("TASK 3: Suspicious Activity", () =>
            {
                // Should flag: 3 large debits within 60s (inclusive endpoints)
                AccountManager mgr1 = new AccountManager();
                mgr1.AddAccount(new Account("A1", "Alice"));
                mgr1.AddTransaction(new Transaction("T1", "A1", TransactionType.DEBIT, 50, 0));
                mgr1.AddTransaction(new Transaction("T2", "A1", TransactionType.DEBIT, 60, 30));
                mgr1.AddTransaction(new Transaction("T3", "A1", TransactionType.DEBIT, 75, 60)); // t+60 inclusive
                var r1 = mgr1.GetSuspiciousAccounts();
                Check(r1.Contains("A1"), "Task 3: A1 should be flagged (3 debits in 0–60s)");

                // Should NOT flag: window exceeded by 1 second
                AccountManager mgr2 = new AccountManager();
                mgr2.AddAccount(new Account("A2", "Bob"));
                mgr2.AddTransaction(new Transaction("T1", "A2", TransactionType.DEBIT, 50, 0));
                mgr2.AddTransaction(new Transaction("T2", "A2", TransactionType.DEBIT, 50, 30));
                mgr2.AddTransaction(new Transaction("T3", "A2", TransactionType.DEBIT, 50, 61)); // just outside
                var r2 = mgr2.GetSuspiciousAccounts();
                Check(!r2.Contains("A2"), "Task 3: A2 should NOT be flagged (61s > 60s window)");

                // Should NOT flag: debits below $50
                AccountManager mgr3 = new AccountManager();
                mgr3.AddAccount(new Account("A3", "Carol"));
                mgr3.AddTransaction(new Transaction("T1", "A3", TransactionType.DEBIT, 49, 0));
                mgr3.AddTransaction(new Transaction("T2", "A3", TransactionType.DEBIT, 49, 10));
                mgr3.AddTransaction(new Transaction("T3", "A3", TransactionType.DEBIT, 49, 20));
                Check(!mgr3.GetSuspiciousAccounts().Contains("A3"),
                    "Task 3: Debits < 50 should not trigger flag");

                // Result should be sorted
                AccountManager mgr4 = new AccountManager();
                mgr4.AddAccount(new Account("B1", "Dave"));
                mgr4.AddAccount(new Account("A1", "Eve"));
                foreach (var id in new[] { "B1", "A1" })
                    for (int t = 0; t <= 40; t += 20)
                        mgr4.AddTransaction(new Transaction($"T{id}{t}", id,
                            TransactionType.DEBIT, 50, t));
                var r4 = mgr4.GetSuspiciousAccounts();
                Check(r4.SequenceEqual(r4.OrderBy(x => x).ToList()),
                "Task 3: Result should be sorted alphabetically");
            });
            // </task4>

            Console.WriteLine("\nResults: " + passed + " passed, " + failed + " failed");
        }

        static void Check(bool condition, string msg)
        {
            if (!condition) throw new Exception(msg);
        }

        static void RunTest(string name, Action test)
        {
            try
            {
                test();
                passed++;
                Console.WriteLine("PASS: " + name);
            }
            catch (Exception e)
            {
                failed++;
                Console.WriteLine("FAIL: " + name);
                Console.WriteLine("      Error: " + e.Message);
                Console.WriteLine("-------------------------------------------");
            }
        }
    }
}
