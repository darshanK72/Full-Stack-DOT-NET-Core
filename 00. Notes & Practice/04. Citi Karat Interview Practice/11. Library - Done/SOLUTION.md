# Library — Solution & Concepts Guide

Interview problem: build and extend a library book management backend in C#.

| File | Purpose |
|------|---------|
| `Library.cs` | Problem statement + stub implementations + tests (Tasks 2–4 unimplemented) |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 1 — Bug Fix](#task-1--bug-fix-getcurrentcheckoutcount)
5. [Task 2 — Overdue Books by Member](#task-2--getoverduebooksbymember)
6. [Task 3 — Calculate Late Fees](#task-3--calculatelatefees)
7. [Task 4 — Loan Limit Violators](#task-4--getloanlimitviolators)
8. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
9. [Interview Tips](#interview-tips)

---

## Problem Overview

`LibraryManager` is the central service that:

- Registers **members** with a tier (`STANDARD`, `PREMIUM`)
- Catalogs **books** by genre
- Tracks **loans** with status (`ACTIVE`, `OVERDUE`, `RETURNED`)
- Computes **checkout counts, overdue reports, late fees, and limit violations**

Each task builds on the previous one — from a simple status-filter bug to tiered pricing and concurrent loan limits.

```
┌─────────────────┐     ┌──────────────────┐
│  AddMember()    │────▶│ Dictionary<int,  │
│  AddBook()      │     │ Member> / Book>  │
└─────────────────┘     └──────────────────┘
                               │
┌─────────────────┐            │
│   AddLoan()     │────────────┼──▶ List<Loan>
└─────────────────┘            │
                               ▼
                    ┌──────────────────────┐
                    │ Analytics methods    │
                    │ (Tasks 1–4)          │
                    └──────────────────────┘
```

---

## Core Concepts

### 1. Enums (`MemberTier`, `Genre`, `LoanStatus`)

Enums restrict values to a fixed set. They are type-safe alternatives to magic strings or integers.

```csharp
enum MemberTier { STANDARD, PREMIUM }
enum Genre      { FICTION, HISTORY, NON_FICTION, SCIENCE }
enum LoanStatus { ACTIVE, OVERDUE, RETURNED }
```

**Why it matters:** Every task filters or aggregates by `LoanStatus`. Misreading which statuses count is the most common mistake in this problem.

| Status | Counts as "checked out"? | Incurs late fees? | Counts toward limit? |
|--------|--------------------------|-------------------|----------------------|
| ACTIVE | Yes | No | Yes |
| OVERDUE | Yes | Yes | Yes |
| RETURNED | No | No | No |

---

### 2. Dictionary vs List — When to Use Which

| Structure | Use case in this problem |
|-----------|--------------------------|
| `Dictionary<int, Member>` | O(1) lookup by `memberId`; read tier for fee calculation |
| `Dictionary<int, Book>` | O(1) lookup by `bookId`; validate loans on add |
| `List<Loan>` | Transaction log of all loans; filter/group/aggregate |
| `Dictionary<K, V>` (return type) | Map memberId to computed values (book lists, fees) |
| `List<int>` (return type) | Sorted violator IDs |

**Key insight:** Members and books live in dictionaries (master registries). Loans live in a list (event log). Analytics methods **query the list** and **key results by member**.

---

### 3. LINQ (Language Integrated Query)

LINQ provides declarative operations over collections:

| Method | What it does |
|--------|--------------|
| `Where` | Filter (like SQL `WHERE`) |
| `GroupBy` | Partition into buckets by a key |
| `Select` | Project/transform each element |
| `OrderBy` | Sort ascending |
| `Sum`, `Count` | Aggregations |
| `ToDictionary` | Materialize key-value pairs |
| `ToList` | Materialize a list |

---

### 4. Guard Clauses & Edge Cases

| Condition | Expected behavior |
|-----------|-------------------|
| Member with no OVERDUE loans (Tasks 2–3) | Excluded from result map |
| Member with only RETURNED loans | Excluded from overdue/fee maps |
| Member exactly at loan limit (Task 4) | **Not** a violator |
| RETURNED loans (Task 1, 4) | Do not count as checked out or toward limit |
| ACTIVE loans (Task 3) | No late fees |

Always filter by the **correct status set** before aggregating.

---

### 5. Tier-Based Business Rules

Two tasks use member tier:

| Tier | Late fee rate (Task 3) | Concurrent loan limit (Task 4) |
|------|------------------------|----------------------------------|
| STANDARD | $0.50 per `loanDay` | 3 |
| PREMIUM | $0.25 per `loanDay` | 5 |

Look up tier via `members[memberId].tier` when computing per-member values.

---

## Data Structures Used

### Domain Classes

```
Member
├── memberId : int
├── name     : string
└── tier     : MemberTier

Book
├── bookId : int
├── title  : string
└── genre  : Genre

Loan
├── loanId   : int
├── memberId : int
├── bookId   : int
├── loanDays : int
└── status   : LoanStatus
```

---

## Task 1 — Bug Fix: `GetCurrentCheckoutCount()`

### Requirement

Return the total number of books **currently checked out** — only `ACTIVE` and `OVERDUE` loans count. `RETURNED` loans must be excluded.

### The Bug

```csharp
// BUG: counts ALL loans, including RETURNED
public int GetCurrentCheckoutCount()
{
    return loans.Count;
}
```

The test adds 4 loans: 1 ACTIVE, 1 OVERDUE, 2 RETURNED. Expected count is **2**, but the bug returns **4**.

### Fix

```csharp
public int GetCurrentCheckoutCount()
{
    return loans.Count(l =>
        l.status == LoanStatus.ACTIVE || l.status == LoanStatus.OVERDUE);
}
```

### Test Data Breakdown

| Loan ID | Member | Status | Counts? |
|---------|--------|--------|---------|
| 1 | 1 | ACTIVE | Yes |
| 2 | 1 | RETURNED | No |
| 3 | 2 | OVERDUE | Yes |
| 4 | 2 | RETURNED | No |

**Expected:** `2`

### Concept: Status Filtering

When a count is wrong, ask: *"Am I counting the right subset?"* The pattern `status == X || status == Y` appears again in Task 4.

---

## Task 2 — `GetOverdueBooksByMember()`

### Requirement

Return a map of `memberId → sorted list of bookIds` for books that are currently **OVERDUE** for that member.

**Rules:**
- Only members with at least one `OVERDUE` loan appear
- Each member's bookId list sorted in **ascending** order
- `ACTIVE` and `RETURNED` loans are ignored

### Solution

```csharp
public Dictionary<int, List<int>> GetOverdueBooksByMember()
{
    return loans
        .Where(l => l.status == LoanStatus.OVERDUE)
        .GroupBy(l => l.memberId)
        .ToDictionary(
            g => g.Key,
            g => g.Select(l => l.bookId).OrderBy(id => id).ToList());
}
```

### Step-by-Step (Test Data)

```
OVERDUE loans:
  member 1 → bookIds [1, 3]  → sorted [1, 3]
  member 2 → bookIds [2]     → sorted [2]

member 3: only RETURNED → excluded
member 4: no loans      → excluded

Result: { 1: [1, 3], 2: [2] }
```

### Concepts Applied

1. **Filter first** — `Where(l => l.status == OVERDUE)` narrows to relevant loans
2. **GroupBy memberId** — buckets overdue loans per member
3. **OrderBy on bookIds** — satisfies ascending sort requirement
4. **ToDictionary** — materializes the map

### Equivalent Imperative Style

```csharp
var result = new Dictionary<int, List<int>>();

foreach (Loan loan in loans)
{
    if (loan.status != LoanStatus.OVERDUE) continue;

    if (!result.ContainsKey(loan.memberId))
        result[loan.memberId] = new List<int>();

    result[loan.memberId].Add(loan.bookId);
}

foreach (int memberId in result.Keys.ToList())
    result[memberId].Sort();

return result;
```

---

## Task 3 — `CalculateLateFees()`

### Requirement

Compute total late fees per member. Only `OVERDUE` loans incur fees.

| Tier | Rate |
|------|------|
| STANDARD | $0.50 per `loanDay` |
| PREMIUM | $0.25 per `loanDay` |

Return `memberId → total fee`. Only members with at least one `OVERDUE` loan appear.

### Solution

```csharp
public Dictionary<int, double> CalculateLateFees()
{
    return loans
        .Where(l => l.status == LoanStatus.OVERDUE)
        .GroupBy(l => l.memberId)
        .ToDictionary(
            g => g.Key,
            g =>
            {
                double rate = members[g.Key].tier == MemberTier.PREMIUM ? 0.25 : 0.50;
                return g.Sum(l => l.loanDays * rate);
            });
}
```

### Step-by-Step (Test Data)

```
member 1 (STANDARD, $0.50/day):
  loan 1: 4 days OVERDUE  → 4 × 0.50 = 2.00
  loan 2: 6 days OVERDUE  → 6 × 0.50 = 3.00
  loan 3: RETURNED        → ignored
  Total: 5.00

member 2 (PREMIUM, $0.25/day):
  loan 4: 8 days OVERDUE  → 8 × 0.25 = 2.00
  loan 5: ACTIVE          → ignored
  Total: 2.00

member 3: only ACTIVE → excluded
member 4: no loans    → excluded

Result: { 1: 5.0, 2: 2.0 }
```

### Concepts Applied

1. **Tier lookup** — `members[g.Key].tier` drives the rate
2. **Sum per group** — aggregate `loanDays × rate` across overdue loans
3. **Double precision** — test uses `Math.Abs(fees[1] - 5.0) < 0.0001`

### Common Mistake

Charging fees on `ACTIVE` loans. Only `OVERDUE` status triggers fees — `ACTIVE` means still within the loan period.

---

## Task 4 — `GetLoanLimitViolators()`

### Requirement

Members have a maximum number of **concurrent** loans (`ACTIVE` + `OVERDUE`). `RETURNED` loans do not count.

| Tier | Max concurrent loans |
|------|---------------------|
| STANDARD | 3 |
| PREMIUM | 5 |

A member is a **violator** if their current count **exceeds** (not equals) their limit. Return a **sorted** list of violator `memberId`s.

### Solution

```csharp
public List<int> GetLoanLimitViolators()
{
    var violators = new List<int>();

    foreach (Member member in members.Values)
    {
        int concurrent = loans.Count(l =>
            l.memberId == member.memberId &&
            (l.status == LoanStatus.ACTIVE || l.status == LoanStatus.OVERDUE));

        int limit = member.tier == MemberTier.PREMIUM ? 5 : 3;

        if (concurrent > limit)
            violators.Add(member.memberId);
    }

    violators.Sort();
    return violators;
}
```

### LINQ Alternative

```csharp
public List<int> GetLoanLimitViolators()
{
    return members.Values
        .Where(m =>
        {
            int concurrent = loans.Count(l =>
                l.memberId == m.memberId &&
                (l.status == LoanStatus.ACTIVE || l.status == LoanStatus.OVERDUE));
            int limit = m.tier == MemberTier.PREMIUM ? 5 : 3;
            return concurrent > limit;
        })
        .Select(m => m.memberId)
        .OrderBy(id => id)
        .ToList();
}
```

### Worked Example (Main Test)

| Member | Tier | ACTIVE/OVERDUE | Limit | Violator? |
|--------|------|----------------|-------|-----------|
| 1 | STANDARD | 4 | 3 | Yes (4 > 3) |
| 2 | PREMIUM | 3 | 5 | No |
| 3 | STANDARD | 3 | 3 | No (exactly at limit) |
| 4 | PREMIUM | 6 | 5 | Yes (6 > 5) |

**Result:** `[1, 4]`

### Edge Case (Second Test Block)

| Member | Tier | ACTIVE/OVERDUE | Notes |
|--------|------|----------------|-------|
| 10 | STANDARD | 2 | 3 RETURNED loans ignored |
| 20 | STANDARD | 3 | exactly at limit — NOT violator |
| 30 | PREMIUM | 6 | exceeds limit of 5 |

**Result:** `[30]`

### Concepts Applied

1. **Iterate members, not loans** — every registered member must be checked
2. **Concurrent = ACTIVE + OVERDUE** — same status filter as Task 1
3. **Strict inequality** — `>` not `>=`; at-limit members are OK
4. **Sorted output** — `Sort()` or `OrderBy`

---

## LINQ Cheat Sheet for This Problem

```
Filter by status:  .Where(l => l.status == LoanStatus.OVERDUE)
                   .Where(l => l.status == ACTIVE || l.status == OVERDUE)
Group:             .GroupBy(l => l.memberId)
Project:           .Select(l => l.bookId)
Sort:              .OrderBy(id => id)
Aggregate:         .Sum(l => l.loanDays * rate)
                   .Count(l => condition)
Convert:           .ToDictionary(g => g.Key, g => ...)
                   .ToList()
```

### Status Filter Reuse

Tasks 1, 3, and 4 all depend on understanding which statuses matter:

```
Task 1: ACTIVE + OVERDUE  (checkout count)
Task 2: OVERDUE only      (overdue books)
Task 3: OVERDUE only      (late fees)
Task 4: ACTIVE + OVERDUE  (concurrent limit)
```

---

## Interview Tips

### Reading the Problem

1. **Underline status conditions** — "OVERDUE only" vs "ACTIVE + OVERDUE" changes every task
2. **Note tier rules** — STANDARD vs PREMIUM affects fees and limits differently
3. **Check boundary semantics** — "exceeds" vs "at or above" for violators

### Debugging Task 1

When a count is wrong, ask: *"Am I counting the right subset?"* The bug counts all loans instead of filtering by status.

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Including RETURNED in checkout count (Task 1) | Filter `ACTIVE \|\| OVERDUE` |
| Including ACTIVE loans in overdue map (Task 2) | Filter `OVERDUE` only |
| Charging fees on ACTIVE loans (Task 3) | Filter `OVERDUE` only |
| Using `>=` for violators (Task 4) | Use `>` — at-limit is OK |
| Counting RETURNED toward limit (Task 4) | Only `ACTIVE + OVERDUE` |
| Forgetting to sort bookIds (Task 2) | `.OrderBy(id => id)` |
| Forgetting to sort violator list (Task 4) | `.Sort()` or `.OrderBy` |

### Complexity (for follow-up questions)

For `n` loans and `m` members:

| Method | Time | Space |
|--------|------|-------|
| Task 1 | O(n) | O(1) |
| Task 2 | O(n log n) worst case (sort per group) | O(n) |
| Task 3 | O(n) | O(m) |
| Task 4 | O(m × n) | O(v) violators |

All are fine for typical interview data sizes.

---

## Quick Reference — All Four Solutions

```csharp
// TASK 1 — exclude RETURNED from checkout count
return loans.Count(l =>
    l.status == LoanStatus.ACTIVE || l.status == LoanStatus.OVERDUE);

// TASK 2 — overdue books grouped by member, sorted
return loans
    .Where(l => l.status == LoanStatus.OVERDUE)
    .GroupBy(l => l.memberId)
    .ToDictionary(g => g.Key, g => g.Select(l => l.bookId).OrderBy(id => id).ToList());

// TASK 3 — tiered late fees on OVERDUE loans only
return loans
    .Where(l => l.status == LoanStatus.OVERDUE)
    .GroupBy(l => l.memberId)
    .ToDictionary(g => g.Key, g =>
    {
        double rate = members[g.Key].tier == MemberTier.PREMIUM ? 0.25 : 0.50;
        return g.Sum(l => l.loanDays * rate);
    });

// TASK 4 — members exceeding concurrent loan limit
foreach (Member member in members.Values)
{
    int concurrent = loans.Count(l =>
        l.memberId == member.memberId &&
        (l.status == LoanStatus.ACTIVE || l.status == LoanStatus.OVERDUE));
    int limit = member.tier == MemberTier.PREMIUM ? 5 : 3;
    if (concurrent > limit) violators.Add(member.memberId);
}
violators.Sort();
```

Apply these implementations in `LibraryManager` inside `Library.cs` to make all tests pass.
