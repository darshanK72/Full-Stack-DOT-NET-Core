---
module: 06. Multithreading & Async Programming
difficulty: Medium
chapters: 06 Synchronization and Locks
domain: RetailBanking
---

# Community Bank Ledger

Build a **.NET 8 console application from scratch** protecting shared account balances with `lock`, `Interlocked`, `ReaderWriterLockSlim`, and deadlock-safe lock ordering.

## Business context

Multiple teller threads deposit and withdraw on shared accounts while an audit thread reads balances. A rate table is read often and updated rarely. Demonstrate unsafe vs safe counters and avoid classic lock-order deadlocks.

## Definitions

**Class `BankAccount`**

- Private `decimal _balance`; dedicated readonly `_balanceGate` object
- `decimal Balance { get; }` — read under lock
- `void Deposit(decimal amount)` — reject ≤ 0; add under lock
- `void Withdraw(decimal amount)` — reject ≤ 0 or insufficient funds (`InvalidOperationException`); subtract under lock
- Never lock on `this` or public types

**Static class `TransactionCounter`**

- `static void RunUnsafeRace(int workers, int incrementsEach)` — shared int++ without sync; print expected vs actual
- `static int RunInterlocked(int workers, int incrementsEach)` — `Interlocked.Increment` on shared int; return final count

**Class `ExchangeRateTable`**

- `ReaderWriterLockSlim _lock`
- `decimal GetRate(string currency)` — read lock; default 1.0m if missing
- `void UpdateRate(string currency, decimal rate)` — write lock; reject rate ≤ 0
- `void Dispose()` — dispose lock slim

**Class `TransferService`**

- `static void Transfer(BankAccount from, BankAccount to, decimal amount)` — **deadlock-safe**: compare account hash codes (or consistent id) to order two locks before updating both balances
- `static void TransferUnsafe(BankAccount from, BankAccount to, decimal amount)` — lock from then to (document deadlock risk in comment only — implement for demo)

## Demo Main

1. Race demo: unsafe vs Interlocked (10 workers × 1000 increments).
2. Two accounts; parallel transfers using safe `Transfer`.
3. Rate table: concurrent reads + one writer update.
4. Optional: comment-only explanation if `TransferUnsafe` deadlocks under swap directions.

## Constraints

- net8, explicit usings, `decimal` for money
- Short critical sections — no I/O inside lock
- Dispose `ReaderWriterLockSlim` when done

## Non-goals

Mutex cross-process, real persistence

## Evaluation

[EVALUATION.md](EVALUATION.md)
