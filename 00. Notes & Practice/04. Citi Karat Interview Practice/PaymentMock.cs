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
using System;
using System.Collections.Generic;

namespace DotNetQuestions
{
    public class Main
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
                }
                return balances;
            }

            /**
             * TASK 1: Average Transaction Amount
             * Currently unimplemented - returns empty map.
             */
            public Dictionary<string, double> GetAverageTransactionAmountByAccount()
            {
                return new Dictionary<string, double>();
            }

            /**
             * TASK 2: Transaction Fees
             * Currently unimplemented - returns empty map.
             */
            public Dictionary<string, int> GetTransactionFees()
            {
                return new Dictionary<string, int>();
            }

            /**
             * TASK 3: Suspicious Accounts (3+ large debits in 60s)
             * Currently unimplemented - returns empty list.
             */
            public List<string> GetSuspiciousAccounts()
            {
                return new List<string>();
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
                AccountManager mgr = new AccountManager();
                mgr.AddAccount(new Account("A1", "Alice"));
                Check(mgr.GetTransactionFees().ContainsKey("A1"), "Task 2: Account missing from fees report");
            });
            // </task3>

            // <task4>
            RunTest("TASK 3: Suspicious Activity", () =>
            {
                AccountManager mgr = new AccountManager();
                Check(mgr.GetSuspiciousAccounts() != null, "Task 3: Method returned null");
                // Logic would fail here as it returns an empty list
                Check(!(mgr.GetSuspiciousAccounts().Count == 0) == false, "Task 3 not implemented");
                throw new Exception("Suspicious logic not implemented");
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
