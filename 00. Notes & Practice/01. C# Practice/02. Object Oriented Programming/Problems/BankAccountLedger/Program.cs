/*
 * PROBLEM: Bank Account Ledger
 *
 * Tellers deposit and withdraw on checking accounts while a dashboard listens
 * for large balance movements. Account numbers must compare correctly when
 * used as dictionary keys.
 *
 * This exercise covers:
 *   ch02 — read-only properties; override ToString, Equals, GetHashCode
 *   ch07 — private balance field; balance mutable only through methods
 *   ch08 — BalanceChanged event; EventHandler<T>; custom EventArgs
 *   ch09 — bank account encapsulation example
 */

using System;
using System.Collections.Generic;

namespace RetailBanking
{
    /*
     * Carries old balance, new balance, and delta after a successful deposit or withdraw.
     */
    class BalanceChangedEventArgs : EventArgs
    {
        public decimal OldBalance { get; }
        public decimal NewBalance { get; }
        public decimal Delta { get; }

        public BalanceChangedEventArgs(decimal oldBalance, decimal newBalance, decimal delta)
        {
            OldBalance = oldBalance;
            NewBalance = newBalance;
            Delta = delta;
        }
    }

    /*
     * A single checking account with encapsulated balance and change notifications.
     *
     * Balance has no public setter — callers must use Deposit and Withdraw.
     * Equals/GetHashCode use AccountNumber only (case-insensitive ordinal).
     */
    class BankAccount
    {
        private decimal _balance;

        public string AccountNumber { get; }
        public string OwnerName { get; }
        public decimal Balance => _balance;

        public event EventHandler<BalanceChangedEventArgs>? BalanceChanged;

        public BankAccount(string accountNumber, string ownerName, decimal openingBalance = 0m)
        {
            // TODO: validate accountNumber and ownerName; set fields; assign _balance
            throw new NotImplementedException();
        }

        public void Deposit(decimal amount)
        {
            // TODO: reject amount <= 0; update balance; raise BalanceChanged
            throw new NotImplementedException();
        }

        public void Withdraw(decimal amount)
        {
            // TODO: reject invalid amount; reject insufficient funds; update; raise event
            throw new NotImplementedException();
        }

        public override bool Equals(object? obj)
        {
            // TODO: compare AccountNumber case-insensitively
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            // TODO: hash AccountNumber case-insensitively — must match Equals
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            // TODO: "{AccountNumber} ({OwnerName}): {Balance:C}"
            throw new NotImplementedException();
        }
    }

    /*
     * Holds all open accounts keyed by account number.
     */
    class Ledger
    {
        private readonly Dictionary<string, BankAccount> _accounts = new Dictionary<string, BankAccount>();

        public bool OpenAccount(BankAccount account)
        {
            // TODO: return false if account number already exists
            throw new NotImplementedException();
        }

        public bool TryGetAccount(string accountNumber, out BankAccount? account)
        {
            // TODO: lookup using same equality semantics as OpenAccount
            throw new NotImplementedException();
        }

        public decimal GetTotalBalance()
        {
            // TODO: sum all account balances
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: create ledger and accounts; subscribe handler when |Delta| >= 1000
            // TODO: perform deposits/withdrawals that trigger and do not trigger handler
            // TODO: unsubscribe; show no further prints on later movements
            throw new NotImplementedException();
        }

        static void OnLargeMovement(object? sender, BalanceChangedEventArgs e)
        {
            // TODO: print a line describing the large movement
            throw new NotImplementedException();
        }
    }
}
