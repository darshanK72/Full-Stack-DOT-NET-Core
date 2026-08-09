---
module: 01. C# Language Fundamentals
difficulty: Hard
chapters: 04 Operators, 06 Methods, 07 Control Flow, 09 Loops, 10 Exception Handling
domain: Banking
---

# Payment Transaction Monitor

Build a **.NET 8 console application from scratch**.

## Business context

A fintech compliance tool ingests account transactions and flags suspicious activity and computes fee totals.

## Definitions

**Enum `TxnType`:** `CREDIT`, `DEBIT`

**Class `Transaction`:** `AccountId`, `Type`, `Amount` (decimal), `TimestampSec` (long, Unix seconds)

**Class `AccountManager`**

- `AddTransaction(txn)` — append to global list

## Features

### 1. Net balance per account

`GetNetBalance(int accountId)` — sum CREDIT amounts minus DEBIT amounts for that account. No transactions → `0`.

### 2. Fee calculation

First **3 transactions per account** are free. From 4th onward: CREDIT fee `1.00`, DEBIT fee `2.00`. Process in **timestamp ascending** order per account.

`GetTotalFees()` → sum of all fees across accounts.

Example: one account, 4 debits → one fee of 2.00 (only 4th charged).

### 3. Suspicious window

`GetSuspiciousAccountIds()` — return sorted list of account ids that have **≥2 DEBIT** transactions within any **60-second sliding window** (inclusive). If none, empty list.

Example: debits at t=100 and t=150 same account → flag.

## Demo Main

Seed transactions covering fee tier and suspicious case; print balance, fees, suspicious ids.

## Constraints

- net8, explicit usings, decimal money
- Sort by TimestampSec where required

## Non-goals

Persistence, threading, real Unix APIs

## Evaluation

[EVALUATION.md](EVALUATION.md)

---

## Extended Scenarios

Implement these after the three core report methods are working.

### EX1 — Account transaction summary

Add a method that accepts an account Id and returns a summary string built with
StringBuilder showing total credits, total debits, net balance, and the number
of transactions charged a fee.

This exercises StringBuilder (ch08), decimal accumulation (ch02, ch04), and
combining the results of multiple passes over a filtered collection (ch06).

### EX2 — Largest single debit

Add a method that returns the single largest DEBIT transaction amount for a given
account.  Return zero when the account has no debit transactions.

This exercises filtering by enum value (ch02, ch06) and a running-max pattern
over decimal values (ch04).

### EX3 — Fee-free account list

Add a method that returns a sorted list of account Ids that have never been charged
a fee — accounts with three or fewer total transactions.

This exercises grouping by account (ch09), counting within each group (ch06), and
the same timestamp-ordering logic used in GetTotalFees (ch06).

---

## Implementation Guide

### Classes and their responsibilities

| Class | Responsibility |
|-------|----------------|
| `TxnType` | Enum distinguishing incoming from outgoing money |
| `Transaction` | Data model for one financial event |
| `AccountManager` | Ingests transactions and runs three compliance reports |

### Method contracts

| Method | What it does | Key edge case |
|--------|-------------|---------------|
| `AddTransaction(txn)` | Appends a transaction to the global log | No validation at intake |
| `GetNetBalance(accountId)` | CREDIT total minus DEBIT total for the account | Returns 0 when account has no transactions |
| `GetTotalFees()` | Sum of all per-account fees using the tiered rule | First 3 transactions per account are always free |
| `GetSuspiciousAccountIds()` | Sorted list of accounts with rapid debit bursts | Window boundary is inclusive (≤ 60 seconds) |

### Business rules to enforce

- `TimestampSec` is a `long`, not `int`, because Unix timestamps will overflow a 32-bit integer in January 2038 (ch02)
- Fee calculation must process each account's transactions in ascending timestamp order — arrival order in the list does not count (ch06 sort before processing)
- The suspicious window check is inclusive: two debits exactly 60 seconds apart are inside the window and trigger a flag
- `GetSuspiciousAccountIds` returns a sorted list — the output order must be deterministic regardless of the order transactions were added (ch06)
- Grouping transactions by account is a pattern you will use in both `GetTotalFees` and `GetSuspiciousAccountIds` — consider factoring it into a private helper (ch07)

### Concepts by chapter

| Chapter | Where it applies |
|---------|-----------------|
| ch02 | `long` for Unix timestamp; `decimal` for all money values |
| ch04 | `+=` and `-=` for running balance; `long` subtraction for time-window check |
| ch06 | Grouping by account; sorting per-account lists; nested loops for sliding window |
| ch07 | Void AddTransaction; analytics methods return typed values |
| ch09 | `Dictionary<int, List<Transaction>>` for per-account grouping; `List<int>` for suspicious ids |

**Chapter cross-reference:**
- `long` for timestamp (overflow reason) → ch02
- `decimal` for money, `1.00m` literal → ch02, ch04
- Dictionary grouping → ch09
- Nested loops, `goto` / bool flag for early exit → ch06
- `List.Sort((a,b) => ...)` for timestamp ordering → ch06
- `ch04`: `long` subtraction and comparison for 60s window
