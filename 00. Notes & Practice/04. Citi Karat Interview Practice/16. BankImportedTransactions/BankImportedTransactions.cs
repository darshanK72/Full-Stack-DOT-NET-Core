/*
 * <bug-task1>
 * We are building a program to manage a bank's imported transactions. The system
 * tracks transactions from multiple sources (partner institutions, customer
 * statement uploads, legacy migrations), each belonging to an account. The
 * program allows staff to import transactions, query per-account summaries, and
 * reconcile activity across sources.
 *
 * Definitions:
 * - A "transaction" is an object that represents a single financial event. It has
 *   properties for the transaction ID, account ID, amount (always positive),
 *   transaction type, timestamp (day number since a reference point),
 *   description, and source.
 * - An "import system" is a class used for managing all imported transactions for
 *   the bank.
 * - A "credit" is a transaction type that increases the account balance. DEPOSIT
 *   and INTEREST are credits.
 * - A "debit" is a transaction type that decreases the account balance.
 *   WITHDRAWAL, FEE, and TRANSFER are debits.
 * - netChange represents the net movement in an account's balance across all its
 *   transactions. It is positive if credits exceed debits, and negative if debits
 *   exceed credits.
 *
 * To begin with, we present you with two tasks:
 * 1-1) Read through and understand the code below. Please take as much time as
 *      necessary, and feel free to run the code.
 * 1-2) The test for ImportSystem is not passing due to a bug in the code. Make
 *      the necessary changes to ImportSystem to fix the bug.
 * </bug-task1>
 */

/*
 * <task2>
 * As transactions are imported, the system needs to record each one and update
 * the account's running balance based on transaction direction (credits add,
 * debits subtract). The system also needs to report activity summaries broken
 * down by transaction type.
 *
 * Definitions (in addition to those from Q1):
 * - balances is a mapping on ImportSystem from each accountId to its current
 *   running balance. New accounts start at $0 before their first transaction.
 *
 * Add three things to the system:
 *
 * 2-1) Extend the ImportSystem class so it maintains a balances attribute — a
 *    mapping from accountId to running balance, initially empty.
 *
 * 2-2) recordTransaction(transactionId, accountId, amount, transactionType,
 *    timestamp, description, source): create a new Transaction, append it to
 *    the system, and update the account's balance based on the transaction's
 *    direction. Credits (DEPOSIT, INTEREST) add the amount; debits (WITHDRAWAL,
 *    FEE, TRANSFER) subtract it. If the account is new (not yet in balances),
 *    initialize it to $0 before applying the change.
 *
 * 2-3) getTypeSummary(): return a mapping from transaction type name (a string,
 *    uppercase, matching the enum name exactly — for example "DEPOSIT") to an
 *    inner mapping with keys "count" (integer) and "total_amount" (double or
 *    equivalent). Aggregate across all accounts. Only include types that have
 *    at least one transaction; omit types that have none.
 *
 * The testRecordTransaction and testGetTypeSummary methods are provided.
 * </task2>
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetQuestions
{
    enum TransactionType
    {
        DEPOSIT,
        WITHDRAWAL,
        TRANSFER,
        FEE,
        INTEREST,
    }

    class Transaction
    {
        public readonly string transactionId;
        public readonly int accountId;
        public readonly double amount;
        public readonly TransactionType transactionType;
        public readonly int timestamp;
        public readonly string description;
        public readonly string source;

        public Transaction(string transactionId, int accountId, double amount,
            TransactionType transactionType, int timestamp,
            string description, string source)
        {
            this.transactionId = transactionId;
            this.accountId = accountId;
            this.amount = amount;
            this.transactionType = transactionType;
            this.timestamp = timestamp;
            this.description = description;
            this.source = source;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Transaction t) return false;
            return transactionId == t.transactionId
                && accountId == t.accountId
                && Math.Abs(amount - t.amount) < 1e-9
                && transactionType == t.transactionType
                && timestamp == t.timestamp
                && description == t.description
                && source == t.source;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(transactionId, accountId, amount, transactionType,
                timestamp, description, source);
        }

        public override string ToString()
        {
            return "Transaction ID: " + transactionId
                + ", Account: " + accountId
                + ", Amount: " + amount
                + ", Type: " + transactionType
                + ", Timestamp: " + timestamp
                + ", Description: " + description
                + ", Source: " + source;
        }
    }

    class ImportSystem
    {
        public List<Transaction> transactions = new List<Transaction>();

        public void AddTransaction(Transaction transaction)
        {
            transactions.Add(transaction);
        }

        public Dictionary<string, object> GetAccountSummary(int accountId)
        {
            double totalCredits = 0.0;
            double totalDebits = 0.0;
            int transactionCount = 0;

            foreach (Transaction t in transactions)
            {
                if (t.accountId != accountId) continue;
                transactionCount++;
                if (t.transactionType == TransactionType.DEPOSIT
                    || t.transactionType == TransactionType.INTEREST)
                {
                    totalCredits += t.amount;
                }
                else if (t.transactionType == TransactionType.WITHDRAWAL
                    || t.transactionType == TransactionType.FEE
                    || t.transactionType == TransactionType.TRANSFER)
                {
                    totalDebits += t.amount;
                }
            }

            double netChange = totalCredits + totalDebits;

            Dictionary<string, object> result = new Dictionary<string, object>();
            result["total_credits"] = totalCredits;
            result["total_debits"] = totalDebits;
            result["net_change"] = netChange;
            result["transaction_count"] = transactionCount;
            return result;
        }

        public Dictionary<int, double> balances = new Dictionary<int, double>();

        public void RecordTransaction(string transactionId, int accountId, double amount,
            TransactionType transactionType, int timestamp, string description, string source)
        {
            // TODO: implement
        }

        public Dictionary<string, Dictionary<string, object>> GetTypeSummary()
        {
            // TODO: implement
            return new Dictionary<string, Dictionary<string, object>>();
        }
    }

    public class BankImportedTransactionsStub
    {
        static int passed = 0, failed = 0, skipped = 0;

        public static void Main(string[] args)
        {
            RunTest("testTransaction", TestTransaction);

            // <bug-task1>
            RunTest("testAccountSummary", TestAccountSummary);
            // </bug-task1>

            // <task2>
            SkipTest("testRecordTransaction");
            SkipTest("testGetTypeSummary");
            // </task2>

            Console.WriteLine("\nResults: " + passed + " passed, " + failed + " failed, " + skipped + " skipped");
        }

        static void TestTransaction()
        {
            Transaction t = new Transaction(
                "t001", 101, 500.00, TransactionType.DEPOSIT, 1, "Salary", "SRC_A");
            AssertEquals("t001", t.transactionId);
            AssertEquals(101, t.accountId);
            AssertEquals(500.00, t.amount, 0.001);
            AssertEquals(TransactionType.DEPOSIT, t.transactionType);
            AssertEquals(1, t.timestamp);
            AssertEquals("Salary", t.description);
            AssertEquals("SRC_A", t.source);
        }

        // <bug-task1>
        static void TestAccountSummary()
        {
            ImportSystem system = new ImportSystem();

            system.AddTransaction(new Transaction("t001", 101, 500.00, TransactionType.DEPOSIT,    1, "Salary",        "SRC_A"));
            system.AddTransaction(new Transaction("t002", 101, 300.00, TransactionType.DEPOSIT,    2, "Refund",        "SRC_A"));
            system.AddTransaction(new Transaction("t003", 101, 150.00, TransactionType.WITHDRAWAL, 3, "ATM",           "SRC_A"));
            system.AddTransaction(new Transaction("t004", 101,  50.00, TransactionType.FEE,        4, "Service fee",   "SRC_A"));
            system.AddTransaction(new Transaction("t005", 101, 200.00, TransactionType.TRANSFER,   5, "Wire out",      "SRC_A"));
            system.AddTransaction(new Transaction("t006", 101,  20.00, TransactionType.INTEREST,   6, "Interest paid", "SRC_A"));

            system.AddTransaction(new Transaction("t007", 202, 100.00, TransactionType.DEPOSIT,    7, "Deposit",       "SRC_A"));
            system.AddTransaction(new Transaction("t008", 202,  30.00, TransactionType.WITHDRAWAL, 8, "Withdrawal",    "SRC_A"));

            Dictionary<string, object> summary101 = system.GetAccountSummary(101);
            AssertEquals(820.00, (double)summary101["total_credits"], 0.001);
            AssertEquals(400.00, (double)summary101["total_debits"], 0.001);
            AssertEquals(420.00, (double)summary101["net_change"], 0.001);
            AssertEquals(6, (int)summary101["transaction_count"]);

            Dictionary<string, object> summary202 = system.GetAccountSummary(202);
            AssertEquals(100.00, (double)summary202["total_credits"], 0.001);
            AssertEquals(30.00, (double)summary202["total_debits"], 0.001);
            AssertEquals(70.00, (double)summary202["net_change"], 0.001);
            AssertEquals(2, (int)summary202["transaction_count"]);
        }
        // </bug-task1>

        // <task2>
        static void TestRecordTransaction()
        {
            ImportSystem system = new ImportSystem();

            system.RecordTransaction("t001", 101, 500.00, TransactionType.DEPOSIT, 1, "Salary", "SRC_A");
            AssertEquals(1, system.transactions.Count);
            AssertEquals(500.00, system.balances[101], 0.001);

            system.RecordTransaction("t002", 101, 150.00, TransactionType.WITHDRAWAL, 2, "ATM", "SRC_A");
            AssertEquals(2, system.transactions.Count);
            AssertEquals(350.00, system.balances[101], 0.001);

            system.RecordTransaction("t003", 101, 50.00, TransactionType.FEE, 3, "Service fee", "SRC_A");
            AssertEquals(3, system.transactions.Count);
            AssertEquals(300.00, system.balances[101], 0.001);

            system.RecordTransaction("t004", 101, 20.00, TransactionType.INTEREST, 4, "Interest paid", "SRC_A");
            AssertEquals(4, system.transactions.Count);
            AssertEquals(320.00, system.balances[101], 0.001);

            system.RecordTransaction("t005", 202, 200.00, TransactionType.DEPOSIT, 5, "Deposit", "SRC_A");
            AssertEquals(5, system.transactions.Count);
            AssertEquals(200.00, system.balances[202], 0.001);
            AssertEquals(320.00, system.balances[101], 0.001);

            system.RecordTransaction("t006", 202, 50.00, TransactionType.TRANSFER, 6, "Wire", "SRC_A");
            AssertEquals(6, system.transactions.Count);
            AssertEquals(150.00, system.balances[202], 0.001);
        }

        static void TestGetTypeSummary()
        {
            ImportSystem system = new ImportSystem();

            system.RecordTransaction("t001", 101, 500.00, TransactionType.DEPOSIT,    1, "Salary",       "SRC_A");
            system.RecordTransaction("t002", 101, 300.00, TransactionType.DEPOSIT,    2, "Refund",       "SRC_A");
            system.RecordTransaction("t003", 202, 800.00, TransactionType.DEPOSIT,    3, "Big deposit",  "SRC_A");
            system.RecordTransaction("t004", 101, 150.00, TransactionType.WITHDRAWAL, 4, "ATM",          "SRC_A");
            system.RecordTransaction("t005", 202,  50.00, TransactionType.WITHDRAWAL, 5, "ATM",          "SRC_A");
            system.RecordTransaction("t006", 101,  50.00, TransactionType.FEE,        6, "Service fee",  "SRC_A");
            system.RecordTransaction("t007", 202, 200.00, TransactionType.TRANSFER,   7, "Wire",         "SRC_A");
            system.RecordTransaction("t008", 101,  20.00, TransactionType.INTEREST,   8, "Interest",     "SRC_A");

            Dictionary<string, Dictionary<string, object>> expected = new Dictionary<string, Dictionary<string, object>>();
            expected["DEPOSIT"] = new Dictionary<string, object> { ["count"] = 3, ["total_amount"] = 1600.00 };
            expected["WITHDRAWAL"] = new Dictionary<string, object> { ["count"] = 2, ["total_amount"] = 200.00 };
            expected["FEE"] = new Dictionary<string, object> { ["count"] = 1, ["total_amount"] = 50.00 };
            expected["TRANSFER"] = new Dictionary<string, object> { ["count"] = 1, ["total_amount"] = 200.00 };
            expected["INTEREST"] = new Dictionary<string, object> { ["count"] = 1, ["total_amount"] = 20.00 };
            AssertTypeSummaryEquals(expected, system.GetTypeSummary());

            ImportSystem system2 = new ImportSystem();
            system2.RecordTransaction("t100", 300, 100.00, TransactionType.DEPOSIT, 1, "First",  "SRC_A");
            system2.RecordTransaction("t101", 300, 200.00, TransactionType.DEPOSIT, 2, "Second", "SRC_A");

            Dictionary<string, Dictionary<string, object>> expected2 = new Dictionary<string, Dictionary<string, object>>();
            expected2["DEPOSIT"] = new Dictionary<string, object> { ["count"] = 2, ["total_amount"] = 300.00 };
            AssertTypeSummaryEquals(expected2, system2.GetTypeSummary());
        }
        // </task2>

        static void AssertTypeSummaryEquals(
            Dictionary<string, Dictionary<string, object>> expected,
            Dictionary<string, Dictionary<string, object>> actual)
        {
            if (expected.Count != actual.Count)
                throw new Exception("Type summary count mismatch: expected " + expected.Count + " but got " + actual.Count);
            foreach (string key in expected.Keys)
            {
                if (!actual.ContainsKey(key))
                    throw new Exception("Missing type key: " + key);
                AssertEquals((int)expected[key]["count"], (int)actual[key]["count"]);
                AssertEquals((double)expected[key]["total_amount"], (double)actual[key]["total_amount"], 0.001);
            }
        }

        static void AssertEquals(int expected, int actual)
        {
            if (expected != actual) throw new Exception("Expected " + expected + " but got " + actual);
        }

        static void AssertEquals(double expected, double actual, double delta)
        {
            if (Math.Abs(expected - actual) > delta)
                throw new Exception("Expected " + expected + " but got " + actual + " (delta " + delta + ")");
        }

        static void AssertEquals<T>(T expected, T actual)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new Exception("Expected " + expected + " but got " + actual);
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
                Console.WriteLine("      Error Detail: " + e.Message);
            }
        }

        static void SkipTest(string name)
        {
            skipped++;
            Console.WriteLine("SKIPPED (Disabled): " + name);
        }
    }
}
