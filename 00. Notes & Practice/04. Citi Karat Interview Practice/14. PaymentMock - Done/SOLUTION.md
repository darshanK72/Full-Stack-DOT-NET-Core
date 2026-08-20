# PaymentMock — Solution & Concepts Guide

Interview problem: build and extend a payment transaction monitoring backend in C#.

| File | Purpose |
|------|---------|
| `PaymentMock.cs` | Problem statement + stub implementations + tests (Tasks 2–4 unimplemented; Task 1 has a bug) |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

> **Note:** The project copy renames the test harness class from `Main` to `PaymentMockStub` to avoid CS0542 (`Main` class vs `Main()` entry point). The root `PaymentMock.cs` is unchanged.

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 1 — Bug Fix: GetBalances](#task-1--bug-fix-getbalances)
5. [Task 2 — Average Transaction Amount by Account](#task-2--getaveragetransactionamountbyaccount)
6. [Task 3 — Transaction Fees](#task-3--gettransactionfees)
7. [Task 4 — Suspicious Accounts (Sliding Window)](#task-4--getsuspiciousaccounts)
8. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
9. [Interview Tips](#interview-tips)

---

## Problem Overview

`AccountManager` is the central service that:

- Registers **accounts** (by `accountId`)
- Records **transactions** (CREDIT or DEBIT with amount and timestamp)
- Computes **balances**, **averages**, **fees**, and **fraud flags**

Each task builds on the previous one — from a simple balance bug to a sliding-window fraud detector.

```
┌─────────────────┐     ┌──────────────────┐
│  AddAccount()   │────▶│  List<Account>   │
└─────────────────┘     └──────────────────┘
                               │
┌─────────────────┐            │
│ AddTransaction()│────────────┼──▶ List<Transaction>
└─────────────────┘            │
                               ▼
                    ┌──────────────────────┐
                    │ Analytics methods    │
                    │ (Tasks 1–4)          │
                    └──────────────────────┘
```

---

## Core Concepts

### 1. Enums (`TransactionType`)

Enums restrict values to a fixed set — safer than magic strings.

```csharp
enum TransactionType { CREDIT, DEBIT }
```

| Type | Effect on balance |
|------|-------------------|
| CREDIT | Adds to balance (money in) |
| DEBIT | Subtracts from balance (money out) |

---

### 2. Absolute Value for Averages (Task 2)

Task 2 asks for the average **absolute** amount — direction (CREDIT vs DEBIT) does not matter, only magnitude.

```csharp
Math.Abs(t.GetAmount())   // DEBIT 50 and CREDIT 50 both contribute 50
```

**Why it matters:** A mix of +100 CREDIT and -40 DEBIT averages to `(100 + 40) / 2 = 70`, not `(100 - 40) / 2 = 30`.

---

### 3. Chronological Ordering (Tasks 3 & 4)

Both fee calculation and fraud detection require processing transactions **in time order** per account.

```csharp
transactions
    .Where(t => t.GetAccountId() == accountId)
    .OrderBy(t => t.GetTimestampSec())
```

**Key insight:** The raw `transactions` list may not be sorted. Always sort by `timestampSec` before counting "4th transaction" or scanning time windows.

---

### 4. Sliding Window (Task 4)

A **60-second window** starting at time `t` includes all events with timestamp in `[t, t + 60]` (inclusive on both ends).

```
Timeline:  0    10   20   30   40   50   60   70
Debits:         D1   D2        D3
Window from 10: [10, 70] → includes D1, D2, D3 if all >= 50
```

If 3+ large debits fall in any such window, the account is **suspicious**.

---

### 5. Dictionary vs List — When to Use Which

| Structure | Use case in this problem |
|-----------|--------------------------|
| `List<Account>` | Master registry of accounts |
| `List<Transaction>` | Append-only transaction log |
| `Dictionary<string, int>` | Balance or fee lookup by `accountId` |
| `Dictionary<string, double>` | Average amount per account |
| `List<string>` | Sorted output of suspicious account IDs |

---

## Data Structures Used

### Domain Classes

```
TransactionType — enum: CREDIT, DEBIT

Transaction
├── id            : string
├── accountId     : string
├── type          : TransactionType
├── amount        : int
└── timestampSec  : int

Account
├── accountId : string
└── ownerName : string

AccountManager
├── accounts     : List<Account>
└── transactions : List<Transaction>
```

### Method Summary

| Method | Returns | Key rule |
|--------|---------|----------|
| `GetBalances()` | `Dictionary<string, int>` | CREDIT adds, DEBIT subtracts |
| `GetAverageTransactionAmountByAccount()` | `Dictionary<string, double>` | Avg of \|amount\|; skip accounts with no txns |
| `GetTransactionFees()` | `Dictionary<string, int>` | First 3 free; then $1 CREDIT / $2 DEBIT; **all accounts** |
| `GetSuspiciousAccounts()` | `List<string>` | 3+ large debits (≥50) in any 60s window; sorted |

---

## Task 1 — Bug Fix: `GetBalances()`

### Requirement

Return each account's current balance: sum of CREDIT amounts minus sum of DEBIT amounts. Accounts with no transactions start at 0.

### The Bug

```csharp
foreach (Transaction t in transactions)
{
    if (t.GetTransactionType() == TransactionType.CREDIT)
    {
        // ... add amount
    }
    // BUG: DEBIT subtraction is missing
}
```

Only CREDIT transactions update the balance. DEBITs are silently ignored.

### Fix

```csharp
public Dictionary<string, int> GetBalances()
{
    Dictionary<string, int> balances = new Dictionary<string, int>();
    foreach (Account a in accounts)
        balances[a.accountId] = 0;

    foreach (Transaction t in transactions)
    {
        string acct = t.GetAccountId();
        if (!balances.ContainsKey(acct))
            balances[acct] = 0;

        if (t.GetTransactionType() == TransactionType.CREDIT)
            balances[acct] += t.GetAmount();
        else
            balances[acct] -= t.GetAmount();
    }
    return balances;
}
```

### Test Data Breakdown

| Txn | Type | Amount | Running balance |
|-----|------|--------|-----------------|
| T1 | CREDIT | 100 | 100 |
| T2 | DEBIT | 40 | **60** |

**Buggy result:** 100 (DEBIT ignored)  
**Expected:** 60

### Concept: Handle Both Branches

When a method processes an enum, ask: **"Did I handle every enum value?"** A missing `else` for DEBIT is a classic interview trap.

---

## Task 2 — `GetAverageTransactionAmountByAccount()`

### Requirement

Return `accountId → average |amount|` across all transactions for that account.

**Rules:**
- Use absolute values for both CREDIT and DEBIT
- Accounts with **no** transactions must **not** appear in the map

### Solution

```csharp
public Dictionary<string, double> GetAverageTransactionAmountByAccount()
{
    Dictionary<string, List<int>> amountsByAccount = new Dictionary<string, List<int>>();

    foreach (Transaction t in transactions)
    {
        string acct = t.GetAccountId();
        if (!amountsByAccount.ContainsKey(acct))
            amountsByAccount[acct] = new List<int>();
        amountsByAccount[acct].Add(Math.Abs(t.GetAmount()));
    }

    Dictionary<string, double> result = new Dictionary<string, double>();
    foreach (KeyValuePair<string, List<int>> kvp in amountsByAccount)
    {
        List<int> amounts = kvp.Value;
        double sum = 0;
        foreach (int a in amounts) sum += a;
        result[kvp.Key] = sum / amounts.Count;
    }
    return result;
}
```

LINQ equivalent:

```csharp
return transactions
    .GroupBy(t => t.GetAccountId())
    .ToDictionary(
        g => g.Key,
        g => g.Average(t => (double)Math.Abs(t.GetAmount())));
```

### Step-by-Step Example

```
Account A1:
  DEBIT  50  → |50| = 50
  CREDIT 30  → |30| = 30
  DEBIT  70  → |70| = 70

Average = (50 + 30 + 70) / 3 = 50.0
Result: { "A1": 50.0 }
```

Account B2 with zero transactions → **not in map**.

---

## Task 3 — `GetTransactionFees()`

### Requirement

Compute per-account transaction fees:

| Transaction # (chronological) | Fee |
|--------------------------------|-----|
| 1st, 2nd, 3rd | $0 (free) |
| 4th and later — CREDIT | $1 each |
| 4th and later — DEBIT | $2 each |

Return fees for **every registered account** (including accounts with $0 fees).

### Solution

```csharp
public Dictionary<string, int> GetTransactionFees()
{
    Dictionary<string, int> fees = new Dictionary<string, int>();
    foreach (Account a in accounts)
        fees[a.accountId] = 0;

    Dictionary<string, List<Transaction>> byAccount = new Dictionary<string, List<Transaction>>();
    foreach (Transaction t in transactions)
    {
        string acct = t.GetAccountId();
        if (!byAccount.ContainsKey(acct))
            byAccount[acct] = new List<Transaction>();
        byAccount[acct].Add(t);
    }

    foreach (KeyValuePair<string, List<Transaction>> kvp in byAccount)
    {
        List<Transaction> sorted = kvp.Value;
        sorted.Sort((a, b) => a.GetTimestampSec().CompareTo(b.GetTimestampSec()));

        for (int i = 0; i < sorted.Count; i++)
        {
            if (i < 3) continue;

            Transaction t = sorted[i];
            if (t.GetTransactionType() == TransactionType.CREDIT)
                fees[kvp.Key] += 1;
            else
                fees[kvp.Key] += 2;
        }
    }

    return fees;
}
```

### Step-by-Step Example

```
Account A1 (sorted by timestamp):
  #  Time  Type    Fee
  1   10   CREDIT   0   (free tier)
  2   20   DEBIT    0   (free tier)
  3   30   CREDIT   0   (free tier)
  4   40   DEBIT    2   (4th → DEBIT fee)
  5   50   CREDIT   1   (5th → CREDIT fee)

Total fees for A1 = 3
```

---

## Task 4 — `GetSuspiciousAccounts()`

### Requirement

Flag accounts with suspicious debit activity:

1. Consider only **DEBIT** transactions where `amount >= 50` ("large")
2. An account is suspicious if it has **3 or more** large debits within **any** 60-second window
3. Window is **inclusive**: timestamps `t` and `t + 60` are in the same window
4. Process per-account in chronological order
5. Return a **sorted** list of suspicious `accountId` strings

### Solution

```csharp
public List<string> GetSuspiciousAccounts()
{
    HashSet<string> suspicious = new HashSet<string>();

    Dictionary<string, List<Transaction>> largeDebitsByAccount = new Dictionary<string, List<Transaction>>();

    foreach (Transaction t in transactions)
    {
        if (t.GetTransactionType() != TransactionType.DEBIT) continue;
        if (t.GetAmount() < 50) continue;

        string acct = t.GetAccountId();
        if (!largeDebitsByAccount.ContainsKey(acct))
            largeDebitsByAccount[acct] = new List<Transaction>();
        largeDebitsByAccount[acct].Add(t);
    }

    foreach (KeyValuePair<string, List<Transaction>> kvp in largeDebitsByAccount)
    {
        List<Transaction> debits = kvp.Value;
        debits.Sort((a, b) => a.GetTimestampSec().CompareTo(b.GetTimestampSec()));

        bool flagged = false;
        for (int i = 0; i < debits.Count && !flagged; i++)
        {
            int windowStart = debits[i].GetTimestampSec();
            int count = 0;
            for (int j = i; j < debits.Count; j++)
            {
                if (debits[j].GetTimestampSec() <= windowStart + 60)
                    count++;
                else
                    break;
            }
            if (count >= 3)
                flagged = true;
        }

        if (flagged)
            suspicious.Add(kvp.Key);
    }

    List<string> result = new List<string>(suspicious);
    result.Sort();
    return result;
}
```

### Sliding Window Walkthrough

```
Account X — large debits only (amount >= 50):
  D1 @ t=10
  D2 @ t=25
  D3 @ t=40
  D4 @ t=100

Window starting at D1 (t=10): range [10, 70]
  D1(10) ✓  D2(25) ✓  D3(40) ✓  D4(100) ✗
  Count = 3 → SUSPICIOUS
```

---

## LINQ Cheat Sheet for This Problem

| Goal | LINQ |
|------|------|
| Filter debits | `.Where(t => t.GetTransactionType() == TransactionType.DEBIT)` |
| Filter large debits | `.Where(t => t.GetAmount() >= 50)` |
| Group by account | `.GroupBy(t => t.GetAccountId())` |
| Sort by time | `.OrderBy(t => t.GetTimestampSec())` |
| Average | `.Average(t => (double)Math.Abs(t.GetAmount()))` |

---

## Interview Tips

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Using signed amounts in Task 2 average | Use `Math.Abs` |
| Sorting globally instead of per-account | Group by `accountId` first |
| Charging fee on 3rd transaction | Free tier = first **3** (indices 0–2) |
| Including CREDIT in fraud detection | Task 4: DEBIT only, amount >= 50 |
| Non-inclusive window | Use `<= windowStart + 60` |
| Unsorted suspicious list | Call `result.Sort()` before return |
| Missing accounts in fee map | Initialize from `accounts` list |

---

## Quick Reference — All Solutions

```csharp
// TASK 1 — CREDIT adds, DEBIT subtracts
else balances[acct] -= t.GetAmount();

// TASK 2 — average absolute amount per account
GroupBy account → Average(Math.Abs(amount))

// TASK 3 — first 3 free, then $1 CREDIT / $2 DEBIT (chronological)
Initialize all accounts to 0; sort by timestamp; charge from index 3

// TASK 4 — 3+ large debits (>= 50) in any 60s inclusive window
Filter, sort per account, sliding window count >= 3, return sorted list
```

---

## Running the Project

```bash
cd "14. PaymentMock"
dotnet run
```

Expected output before solving: 1 pass (bug test may fail), 3 fails. After all fixes: 4 passed, 0 failed.
