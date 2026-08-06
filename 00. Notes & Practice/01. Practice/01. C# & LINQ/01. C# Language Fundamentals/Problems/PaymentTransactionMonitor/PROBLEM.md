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
