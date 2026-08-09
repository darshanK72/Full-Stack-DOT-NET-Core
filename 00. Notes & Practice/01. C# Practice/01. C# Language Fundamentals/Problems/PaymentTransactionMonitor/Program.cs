/*
 * PROBLEM: Payment Transaction Monitor
 *
 * A fintech compliance tool ingests account transactions and runs three
 * checks: it computes net account balances, calculates total platform fees
 * using a tiered rule, and flags accounts with rapid successive debit activity.
 *
 * This is a manager-class style problem.  Main runs a scripted demo with known
 * data so you can verify each method against expected output.
 *
 * This exercise covers:
 *   ch02 — long for Unix timestamps (int overflows in 2038); decimal for money
 *   ch04 — decimal += and -= for running balance; long subtraction for time windows
 *   ch06 — nested loops for the sliding-window check; sorting per-account lists
 *   ch07 — each analytics method returns a typed result; AddTransaction is void
 *   ch09 — Dictionary for grouping transactions per account; List for suspicious ids
 *   ch10 — no exceptions are thrown here; contrast with ch10 patterns from LibraryLoanTracker
 */

using System;
using System.Collections.Generic;

namespace Banking
{
    /*
     * Classifies each transaction as incoming (CREDIT) or outgoing (DEBIT).
     * Net balance = sum of CREDITs minus sum of DEBITs for an account.
     */
    enum TxnType
    {
        CREDIT, DEBIT
    }

    /*
     * Represents one financial event on an account.
     *
     * TimestampSec is a Unix epoch value in whole seconds.  It is stored as long
     * rather than int because a 32-bit integer overflows in January 2038.
     */
    class Transaction
    {
        public int     AccountId    { get; set; }
        public TxnType Type         { get; set; }
        public decimal Amount       { get; set; }
        public long    TimestampSec { get; set; }
    }

    /*
     * Ingests transactions and produces compliance reports.
     * No Console calls are permitted inside this class.
     */
    class AccountManager
    {
        private List<Transaction> _transactions = new List<Transaction>();

        /*
         * Appends a transaction to the global log.
         * No validation is applied at intake — all rules are enforced during reporting.
         */
        public void AddTransaction(Transaction txn)
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Returns the net balance for the specified account.
         *
         * Net balance = total CREDIT amounts minus total DEBIT amounts for that account.
         * Returns zero when the account has no transactions in the log.
         */
        public decimal GetNetBalance(int accountId)
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Returns the total fees charged across all accounts.
         *
         * Fee rule (applied per account, processing transactions in ascending
         * timestamp order):
         *   — the first three transactions per account are always free,
         *   — from the fourth transaction onward: CREDIT incurs a 1.00 fee,
         *     DEBIT incurs a 2.00 fee.
         *
         * Fees from all accounts are summed into one total.
         */
        public decimal GetTotalFees()
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Returns a sorted list of account Ids that are flagged as suspicious.
         *
         * An account is suspicious when it has two or more DEBIT transactions
         * within any sixty-second window (the window boundary is inclusive, meaning
         * two debits exactly sixty seconds apart are counted as within the window).
         *
         * The returned list is sorted in ascending order of account Id.
         * Returns an empty list when no accounts are flagged.
         */
        public List<int> GetSuspiciousAccountIds()
        {
            // TODO: implement
            throw new NotImplementedException();
        }
    }

    /*
     * Entry point — scripted demo.
     *
     * Seeds two accounts designed to exercise each rule, then prints
     * each report alongside the expected output for verification.
     */
    class Program
    {
        static void Main(string[] args)
        {
            AccountManager manager = new AccountManager();

            /*
             * Account 1 — exercises the fee tier:
             *   Txn 1 (DEBIT  100, t=1000)  — free (1st)
             *   Txn 2 (DEBIT   50, t=2000)  — free (2nd)
             *   Txn 3 (CREDIT 200, t=3000)  — free (3rd)
             *   Txn 4 (DEBIT   75, t=4000)  — charged: DEBIT fee = 2.00
             *
             * Account 2 — exercises the suspicious-window check:
             *   Txn 5 (DEBIT 300, t=100)
             *   Txn 6 (DEBIT 150, t=150)  — 150 - 100 = 50 seconds apart -> flagged
             */

            /*
             * Expected output when all methods are implemented:
             *
             *   Account 1 balance : -25.00     (200 - 100 - 50 - 75)
             *   Account 2 balance : -450.00    (0 - 300 - 150)
             *   Total fees        : 2.00        (only 4th txn on account 1 is charged)
             *   Suspicious accounts: 2
             */

            // TODO: call AddTransaction with the six transactions above, then print each report
            throw new NotImplementedException();
        }
    }
}
