---
module: 02. Object Oriented Programming
difficulty: Medium
chapters: 02 Properties, 07 Encapsulation, 08 Events, 09 Bank Account Example
domain: Banking
---

# Bank Account Ledger

Build a **.NET 8 console application from scratch** for a simple retail bank ledger with encapsulation and balance-change notifications.

## Business context

Tellers deposit and withdraw on checking accounts. A dashboard listens for large movements. Account numbers must compare correctly when used as dictionary keys.

## Definitions

**Class `BankAccount`**

- Private field backing balance; public read-only `Balance` property (no public setter)
- `AccountNumber` (string, non-empty) and `OwnerName` — init via constructor
- `Deposit(decimal amount)` — reject `amount <= 0` with `ArgumentOutOfRangeException`
- `Withdraw(decimal amount)` — reject invalid amount; throw `InvalidOperationException` if insufficient funds
- Event `BalanceChanged` typed `EventHandler<BalanceChangedEventArgs>`
- Raise event after successful deposit/withdraw with old balance, new balance, and `Delta`
- Override `Equals`/`GetHashCode` based on `AccountNumber` only (case-insensitive ordinal)
- Override `ToString()` → `"{AccountNumber} ({OwnerName}): {Balance:C}"`

**Class `BalanceChangedEventArgs : EventArgs`**

- `OldBalance`, `NewBalance`, `Delta` (decimal properties, get-only)

**Class `Ledger`**

- `OpenAccount(BankAccount account)` — false if account number already exists (use your equality semantics)
- `TryGetAccount(string accountNumber, out BankAccount? account)`
- `GetTotalBalance()` — sum all account balances

## Event subscription demo

In `Main`, subscribe a handler that prints when `|Delta| >= 1000`. Perform deposits/withdrawals triggering and not triggering the handler. Unsubscribe and show no further prints.

## Constraints

- net8, explicit usings, decimal money
- Balance only mutable through deposit/withdraw methods

## Non-goals

Interest, overdraft products, threading

## Evaluation

[EVALUATION.md](EVALUATION.md)
