/*
 * PROBLEM: Community Bank Ledger
 *
 * Multiple teller threads update shared bank balances under lock,
 * increment transaction counters with Interlocked, read exchange rates
 * via ReaderWriterLockSlim, and transfer funds without deadlocking.
 *
 * This exercise covers:
 *   ch06 — lock / Monitor exclusive access on dedicated sync root
 *   ch06 — race condition vs Interlocked.Increment
 *   ch06 — ReaderWriterLockSlim many readers / one writer
 *   ch06 — deadlock prevention via consistent lock ordering
 *   ch06 — volatile preview for stop flags (optional in counter demo)
 */

using System;
using System.Threading;

namespace RetailBanking
{
    /*
     * Single account balance. Never lock on `this`.
     */
    class BankAccount
    {
        private readonly object _balanceGate = new object();
        private decimal _balance;

        public BankAccount(decimal openingBalance)
        {
            _balance = openingBalance;
        }

        public decimal Balance
        {
            get
            {
                // TODO: read _balance under lock
                throw new NotImplementedException();
            }
        }

        public void Deposit(decimal amount)
        {
            // TODO: reject amount <= 0; add under lock
            throw new NotImplementedException();
        }

        public void Withdraw(decimal amount)
        {
            // TODO: reject invalid amount or insufficient funds; subtract under lock
            throw new NotImplementedException();
        }
    }

    static class TransactionCounter
    {
        public static void RunUnsafeRace(int workers, int incrementsEach)
        {
            // TODO: spawn threads incrementing shared int without sync; print mismatch
            throw new NotImplementedException();
        }

        public static int RunInterlocked(int workers, int incrementsEach)
        {
            // TODO: Interlocked.Increment; return final count
            throw new NotImplementedException();
        }
    }

    /*
     * Exchange rates: frequent reads, rare updates.
     */
    class ExchangeRateTable : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly System.Collections.Generic.Dictionary<string, decimal> _rates =
            new System.Collections.Generic.Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        public decimal GetRate(string currency)
        {
            // TODO: enter read lock; return rate or 1.0m
            throw new NotImplementedException();
        }

        public void UpdateRate(string currency, decimal rate)
        {
            // TODO: enter write lock; reject rate <= 0
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _lock.Dispose();
        }
    }

    static class TransferService
    {
        public static void Transfer(BankAccount from, BankAccount to, decimal amount)
        {
            // TODO: order locks consistently (e.g. by RuntimeHelpers.GetHashCode) then update both
            throw new NotImplementedException();
        }

        public static void TransferUnsafe(BankAccount from, BankAccount to, decimal amount)
        {
            // TODO: lock from then to — deadlock risk when directions swap
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: race demo; safe transfers; rate table read/write demo
            throw new NotImplementedException();
        }
    }
}
