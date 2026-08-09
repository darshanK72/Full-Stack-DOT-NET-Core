# BankImportedTransactions — Solution & Concepts Guide

Interview problem: build and extend a bank imported-transactions backend in C#.

| File | Purpose |
|------|---------|
| `BankImportedTransactions.cs` | Original problem statement + tests (Task 2 skipped) |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 1 — Bug Fix](#task-1--bug-fix-getaccountsummary)
5. [Task 2 — RecordTransaction & GetTypeSummary](#task-2--recordtransaction--gettypesummary)
6. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
7. [Interview Tips](#interview-tips)

---

## Problem Overview

`ImportSystem` is the central service that:

- Stores **transactions** imported from multiple sources (partner institutions, customer uploads, legacy migrations)
- Computes **per-account summaries** (credits, debits, net change)
- Maintains **running balances** per account as transactions are recorded
- Reports **activity summaries** broken down by transaction type

Each task builds on the previous one — from a simple bug fix to balance tracking and aggregation.

```
┌─────────────────────┐     ┌──────────────────────┐
│  AddTransaction()   │────▶│  List<Transaction>   │
│  RecordTransaction()│     │  (transaction log)   │
└─────────────────────┘     └──────────────────────┘
                                      │
                                      ▼
                            ┌──────────────────────┐
                            │ GetAccountSummary()  │
                            │ GetTypeSummary()     │
                            └──────────────────────┘
                                      │
                            ┌──────────────────────┐
                            │ Dictionary<int,      │
                            │ double> balances     │
                            └──────────────────────┘
```

---

## Core Concepts

### 1. Enums (`TransactionType`)

Enums restrict values to a fixed set. They are type-safe alternatives to magic strings.

```csharp
enum TransactionType
{
    DEPOSIT,
    WITHDRAWAL,
    TRANSFER,
    FEE,
    INTEREST,
}
```

**Why it matters:** Credit vs debit classification relies on enum equality. `ToString()` produces the uppercase name used as keys in `GetTypeSummary()` (e.g. `"DEPOSIT"`).

---

### 2. Credits vs Debits

| Category | Types | Effect on balance |
|----------|-------|-------------------|
| **Credit** | `DEPOSIT`, `INTEREST` | Adds to balance (+amount) |
| **Debit** | `WITHDRAWAL`, `FEE`, `TRANSFER` | Subtracts from balance (−amount) |

Amounts are **always positive** in the `Transaction` object. Direction is determined entirely by `transactionType`.

**Key insight:** The same classification logic appears in both `GetAccountSummary()` (Task 1) and `RecordTransaction()` (Task 2). Extracting a helper like `IsCredit(TransactionType)` is optional but reduces duplication.

---

### 3. Dictionary vs List — When to Use Which

| Structure | Use case in this problem |
|-----------|--------------------------|
| `List<Transaction>` | Ordered log of all imported transactions; iterate/filter/group |
| `Dictionary<int, double> balances` | O(1) lookup and update of running balance by `accountId` |
| `Dictionary<string, object>` | Flexible return type for summary maps with mixed value types |
| `Dictionary<string, Dictionary<string, object>>` | Nested map for type-level aggregation in `GetTypeSummary()` |

**Key insight:** Transactions live in a list (append-only log). Balances live in a dictionary (mutable state keyed by account). Summary methods **query the list** and **return computed dictionaries**.

---

### 4. `Dictionary<string, object>` Return Types

Interview problems often use `Dictionary<string, object>` instead of typed classes for summary results. This means:

- Values must be cast when read: `(double)summary["total_credits"]`
- Keys are snake_case strings: `"total_credits"`, `"net_change"`, `"count"`, `"total_amount"`
- Be precise about key names and value types — tests assert exact matches

---

### 5. Guard Clauses & Edge Cases

| Condition | Expected behavior |
|-----------|-------------------|
| Account with no transactions in summary | All totals = 0, count = 0 |
| New account in `RecordTransaction` | Initialize balance to `$0` before applying change |
| Transaction type with zero records in `GetTypeSummary` | Omit that type from the result |
| Floating-point comparisons in tests | Use delta tolerance (e.g. `0.001`) |

---

## Data Structures Used

### Domain Classes

```
Transaction
├── transactionId    : string
├── accountId        : int
├── amount           : double  (always positive)
├── transactionType  : TransactionType
├── timestamp        : int     (day number since reference point)
├── description      : string
└── source           : string

ImportSystem
├── transactions : List<Transaction>
└── balances     : Dictionary<int, double>   (Task 2)
```

### Summary Output Shapes

```
GetAccountSummary(accountId) → Dictionary<string, object>
├── "total_credits"      : double
├── "total_debits"       : double
├── "net_change"         : double   (credits − debits)
└── "transaction_count"  : int

GetTypeSummary() → Dictionary<string, Dictionary<string, object>>
└── "DEPOSIT" → { "count": int, "total_amount": double }
    "WITHDRAWAL" → { ... }
    ... (only types with ≥ 1 transaction)
```

---

## Task 1 — Bug Fix: `GetAccountSummary()`

### Requirement

For a given `accountId`, return a summary of that account's transactions:

| Key | Rule |
|-----|------|
| `total_credits` | Sum of amounts where type is `DEPOSIT` or `INTEREST` |
| `total_debits` | Sum of amounts where type is `WITHDRAWAL`, `FEE`, or `TRANSFER` |
| `net_change` | `totalCredits − totalDebits` (positive if credits exceed debits) |
| `transaction_count` | Count of transactions for that account only |

### The Bug

```csharp
// BUG: adds credits and debits instead of subtracting debits
double netChange = totalCredits + totalDebits;
```

This produces **1220.00** for account 101 instead of **420.00**, because it treats debits as additive rather than subtractive.

### Fix

```csharp
double netChange = totalCredits - totalDebits;
```

### Test Data Breakdown — Account 101

| ID | Type | Amount | Category |
|----|------|--------|----------|
| t001 | DEPOSIT | 500.00 | credit |
| t002 | DEPOSIT | 300.00 | credit |
| t003 | WITHDRAWAL | 150.00 | debit |
| t004 | FEE | 50.00 | debit |
| t005 | TRANSFER | 200.00 | debit |
| t006 | INTEREST | 20.00 | credit |

**Calculation:**
- `total_credits` = 500 + 300 + 20 = **820.00**
- `total_debits` = 150 + 50 + 200 = **400.00**
- `net_change` = 820 − 400 = **420.00**
- `transaction_count` = **6**

### Test Data Breakdown — Account 202

| ID | Type | Amount | Category |
|----|------|--------|----------|
| t007 | DEPOSIT | 100.00 | credit |
| t008 | WITHDRAWAL | 30.00 | debit |

**Expected:** credits=100, debits=30, net_change=**70**, count=2

### Concept: Sign Convention

In banking, debits reduce balance but amounts are stored as positive numbers. The **sign is encoded in the type**, not the amount field. A common mistake is flipping the sign on the amount itself rather than subtracting debits from credits.

---

## Task 2 — `RecordTransaction()` & `GetTypeSummary()`

### Task 2-1: `balances` Dictionary

Already declared in the stub:

```csharp
public Dictionary<int, double> balances = new Dictionary<int, double>();
```

New accounts implicitly start at `$0` — you initialize on first access, not at construction time.

---

### Task 2-2: `RecordTransaction()`

#### Requirement

1. Create a new `Transaction` and append it to `transactions`
2. Update the account's running balance:
   - Credits (`DEPOSIT`, `INTEREST`) → add amount
   - Debits (`WITHDRAWAL`, `FEE`, `TRANSFER`) → subtract amount
3. If account is new, initialize balance to `$0` before applying the change

#### Solution

```csharp
public void RecordTransaction(string transactionId, int accountId, double amount,
    TransactionType transactionType, int timestamp, string description, string source)
{
    Transaction transaction = new Transaction(
        transactionId, accountId, amount, transactionType,
        timestamp, description, source);
    transactions.Add(transaction);

    if (!balances.ContainsKey(accountId))
        balances[accountId] = 0.0;

    if (transactionType == TransactionType.DEPOSIT
        || transactionType == TransactionType.INTEREST)
    {
        balances[accountId] += amount;
    }
    else if (transactionType == TransactionType.WITHDRAWAL
        || transactionType == TransactionType.FEE
        || transactionType == TransactionType.TRANSFER)
    {
        balances[accountId] -= amount;
    }
}
```

#### Step-by-Step (Test Flow)

```
Account 101:
  +500 DEPOSIT     → balance = 500
  −150 WITHDRAWAL  → balance = 350
  −50  FEE         → balance = 300
  +20  INTEREST    → balance = 320

Account 202:
  +200 DEPOSIT     → balance = 200  (new account, starts at 0)
  −50  TRANSFER    → balance = 150
```

#### Concepts Applied

1. **Check-then-act on dictionary** — `ContainsKey` before read/update for new accounts
2. **Reuse credit/debit classification** — same if/else pattern as Task 1
3. **Single responsibility per call** — create, store, and update balance in one method

---

### Task 2-3: `GetTypeSummary()`

#### Requirement

Return a map from transaction type name (uppercase enum string) to an inner map:

| Inner key | Value |
|-----------|-------|
| `"count"` | Number of transactions of that type (all accounts) |
| `"total_amount"` | Sum of amounts for that type |

Only include types with at least one transaction.

#### Solution (Imperative)

```csharp
public Dictionary<string, Dictionary<string, object>> GetTypeSummary()
{
    Dictionary<string, Dictionary<string, object>> result =
        new Dictionary<string, Dictionary<string, object>>();

    foreach (Transaction t in transactions)
    {
        string typeName = t.transactionType.ToString();

        if (!result.ContainsKey(typeName))
        {
            result[typeName] = new Dictionary<string, object>
            {
                ["count"] = 0,
                ["total_amount"] = 0.0
            };
        }

        result[typeName]["count"] = (int)result[typeName]["count"] + 1;
        result[typeName]["total_amount"] = (double)result[typeName]["total_amount"] + t.amount;
    }

    return result;
}
```

#### Solution (LINQ)

```csharp
public Dictionary<string, Dictionary<string, object>> GetTypeSummary()
{
    return transactions
        .GroupBy(t => t.transactionType.ToString())
        .ToDictionary(
            g => g.Key,
            g => new Dictionary<string, object>
            {
                ["count"] = g.Count(),
                ["total_amount"] = g.Sum(t => t.amount)
            });
}
```

#### Worked Example (Full Test Case)

| ID | Type | Amount |
|----|------|--------|
| t001 | DEPOSIT | 500 |
| t002 | DEPOSIT | 300 |
| t003 | DEPOSIT | 800 |
| t004 | WITHDRAWAL | 150 |
| t005 | WITHDRAWAL | 50 |
| t006 | FEE | 50 |
| t007 | TRANSFER | 200 |
| t008 | INTEREST | 20 |

**Result:**

```
DEPOSIT    → count=3, total_amount=1600.00
WITHDRAWAL → count=2, total_amount=200.00
FEE        → count=1, total_amount=50.00
TRANSFER   → count=1, total_amount=200.00
INTEREST   → count=1, total_amount=20.00
```

Types with zero transactions are omitted automatically.

#### Concepts Applied

1. **GroupBy** — partition transactions by type
2. **Nested dictionaries** — outer key is type name, inner holds aggregated stats
3. **Enum.ToString()** — produces uppercase name matching test expectations

---

## LINQ Cheat Sheet for This Problem

```
Filter:      .Where(t => t.accountId == accountId)
Group:       .GroupBy(t => t.transactionType.ToString())
Aggregate:   .Sum(t => t.amount)
             .Count()                         // on a group
Convert:     .ToDictionary(g => g.Key, g => ...)
```

### Equivalent Imperative Style (GetTypeSummary without LINQ)

```csharp
var counts = new Dictionary<string, int>();
var totals = new Dictionary<string, double>();

foreach (var t in transactions)
{
    string key = t.transactionType.ToString();
    if (!counts.ContainsKey(key))
    {
        counts[key] = 0;
        totals[key] = 0.0;
    }
    counts[key]++;
    totals[key] += t.amount;
}

var result = new Dictionary<string, Dictionary<string, object>>();
foreach (string key in counts.Keys)
{
    result[key] = new Dictionary<string, object>
    {
        ["count"] = counts[key],
        ["total_amount"] = totals[key]
    };
}
return result;
```

LINQ is shorter; imperative style shows you understand aggregation under the hood — useful in interviews.

---

## Interview Tips

### Reading the Problem

1. **Separate amount sign from type** — amounts are always positive; direction comes from `TransactionType`
2. **Note dictionary key formats** — snake_case strings (`"total_credits"`) vs enum names (`"DEPOSIT"`)
3. **Check aggregation scope** — `GetAccountSummary` filters by account; `GetTypeSummary` aggregates across all accounts

### Debugging Task 1

When a computed total is wrong, trace the formula:
- Are credits and debits classified correctly?
- Is the final formula addition when it should be subtraction?

The bug was a **sign error**: `+` instead of `−` in `netChange`.

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| `netChange = credits + debits` | `netChange = credits - debits` |
| Forgetting to filter by `accountId` in summary | Skip transactions where `t.accountId != accountId` |
| Using negative amounts for debits | Amount is always positive; subtract based on type |
| Missing new-account init in `RecordTransaction` | `if (!balances.ContainsKey(accountId)) balances[accountId] = 0.0` |
| Wrong key casing in `GetTypeSummary` | Use `transactionType.ToString()` — produces `"DEPOSIT"`, not `"Deposit"` |
| Including types with zero transactions | Only add to result when processing actual transactions |

### Enabling Task 2 Tests

In `BankImportedTransactionsStub.Main()`, replace the skipped tests:

```csharp
// Change from:
SkipTest("testRecordTransaction");
SkipTest("testGetTypeSummary");

// To:
RunTest("testRecordTransaction", TestRecordTransaction);
RunTest("testGetTypeSummary", TestGetTypeSummary);
```

### Complexity (for follow-up questions)

For `n` transactions and `a` accounts:

| Method | Time | Space |
|--------|------|-------|
| `GetAccountSummary` | O(n) | O(1) |
| `RecordTransaction` | O(1) amortized | O(1) |
| `GetTypeSummary` | O(n) | O(t) types |

All are fine for typical interview data sizes.

---

## Quick Reference — Both Solutions

```csharp
// TASK 1 — fix net change sign
double netChange = totalCredits - totalDebits;

// TASK 2-2 — record transaction and update balance
if (!balances.ContainsKey(accountId))
    balances[accountId] = 0.0;

if (transactionType == TransactionType.DEPOSIT
    || transactionType == TransactionType.INTEREST)
    balances[accountId] += amount;
else if (transactionType == TransactionType.WITHDRAWAL
    || transactionType == TransactionType.FEE
    || transactionType == TransactionType.TRANSFER)
    balances[accountId] -= amount;

// TASK 2-3 — type summary (LINQ)
return transactions
    .GroupBy(t => t.transactionType.ToString())
    .ToDictionary(
        g => g.Key,
        g => new Dictionary<string, object>
        {
            ["count"] = g.Count(),
            ["total_amount"] = g.Sum(t => t.amount)
        });
```

Implement Task 1 first and confirm `testAccountSummary` passes before moving to Task 2.
