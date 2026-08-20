# TollBoothMock — Solution & Concepts Guide

Interview problem: analyze toll-booth logs on a divided highway — find data anomalies and compute highway statistics in C#.

| File | Purpose |
|------|---------|
| `../TollBoothMock.cs` | Original practice file (test runner class is named `Main` — CS0542 if copied as-is; `return plates` causes CS0029 in C#) |
| `TollBoothMock.cs` | Runnable project copy (`Main` renamed to `TollBoothMockStub`; Task 1 given minimal C# compile fix; Tasks 2–4 left as TODO) |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 1 — Bug Fix: GetUniquePlates](#task-1--bug-fix-getuniqueplates)
5. [Task 2 — FindMismatchedEntries](#task-2--findmismatchedentries)
6. [Task 3 — GetCarsOnHighway](#task-3--getcarsonhighway)
7. [Task 4 — GetMainroadPassCount](#task-4--getmainroadpasscount)
8. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
9. [Interview Tips](#interview-tips)

---

## Problem Overview

`HighwayTracker` stores toll-booth log entries and answers analytics questions about license plates on the highway.

Log types:

| Type | Meaning |
|------|---------|
| `ENTRY` | Car enters the highway at a toll booth |
| `EXIT` | Car leaves the highway at a toll booth |
| `M` | Main-road sensor — plate recorded at full speed (no stop) |

```
       Exit Booth           Entry Booth
           |                    |
   --------X--------------------X--------
        /                          \
       /            M               \
   ---X-------------X---------------X---
       \                            /
        \                          /
   ------X------------------------X------
           |                    |
       Entry Booth           Exit Booth
```

Each log: `[type, plate, timestamp]`

Example trip for `ABC123`:

```
ENTRY  10:00 → M 10:05 → M 10:15 → EXIT 10:30
```

```
┌─────────────────┐     ┌──────────────────┐
│    AddLog()     │────▶│ List<TollBoothLog>│
└─────────────────┘     └──────────────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │ Analytics methods    │
                    │ (Tasks 1–4)          │
                    └──────────────────────┘
```

---

## Core Concepts

### 1. Set vs List — Uniqueness

A **Set** (`HashSet<T>` in C#) holds **at most one** copy of each value. A **List** allows duplicates.

Task 1 asks for unique license plates. Using `List<string>` and adding every log's plate can store duplicates if the same car appears in many logs.

| Structure | Duplicates? | Use here |
|-----------|-------------|----------|
| `List<string>` | Yes | Wrong for "unique plates" |
| `HashSet<string>` | No | Correct return type |

**C# note:** The root file returns `plates` (a `List<string>`) where `HashSet<string>` is expected — that is **CS0029** (won't compile). The project copy wraps with `new HashSet<string>(plates)` so the project builds; the cleaner fix is to use `HashSet` from the start.

---

### 2. ENTRY / EXIT Pairing

Think of each plate's logs as a small state machine:

```
(no ENTRY yet) ──ENTRY──▶ on highway ──EXIT──▶ (trip complete)
```

| Pattern | Meaning |
|---------|---------|
| ENTRY only (no EXIT) | Car still on highway → `NO_EXIT` |
| EXIT only (no ENTRY) | Data anomaly → `NO_ENTRY` |
| ENTRY + EXIT | Normal trip |
| Neither | Plate only appears in `M` logs (possible but not in sample data) |

Tasks 2 and 3 both reason about **whether a plate has ENTRY without matching EXIT**.

---

### 3. Counting vs Filtering Log Types

Task 4 counts logs where `type == "M"`. The XML comment mentions "between entry and exit," but the **method comment and tests** count **all** `M` logs per plate (including cars still on the highway).

Always trust the **tests** when comments disagree.

---

### 4. Dictionary as Result Map

Several methods return `Dictionary<string, T>` — plate → value:

```csharp
Dictionary<string, string>  // plate → "NO_EXIT" | "NO_ENTRY"
Dictionary<string, int>     // plate → M-sensor pass count
```

Only include plates that match the rule (Task 2: mismatches only; Task 4: plates with at least one `M` log, per tests).

---

## Data Structures Used

### Domain Classes

```
TollBoothLog
├── type       : string   ("ENTRY", "EXIT", "M")
├── plate      : string
└── timestamp  : string

HighwayTracker
└── logs       : List<TollBoothLog>
```

### Sample Dataset (`CreateSampleTracker`)

| Plate | Logs | Status |
|-------|------|--------|
| ABC123 | ENTRY, M, M, EXIT | Normal trip (2 M passes) |
| DEF456 | ENTRY, M | On highway — `NO_EXIT` |
| GHI789 | EXIT only | Anomaly — `NO_ENTRY` |
| JKL012 | ENTRY, EXIT | Normal trip (0 M passes) |

**Expected results:**

| Method | Expected |
|--------|----------|
| GetUniquePlates | `{ABC123, DEF456, GHI789, JKL012}` — size 4 |
| FindMismatchedEntries | `DEF456 → NO_EXIT`, `GHI789 → NO_ENTRY` |
| GetCarsOnHighway | `1` (DEF456) |
| GetMainroadPassCount | `ABC123 → 2`, `DEF456 → 1`, `JKL012 → 0` (or absent) |

---

## Task 1 — Bug Fix: GetUniquePlates

### The Bug

The method accumulates plates in a `List<string>` and returns it where a `HashSet<string>` is required.

Problems:

1. **Compile error (C#):** `List<string>` cannot be returned as `HashSet<string>` (CS0029).
2. **Design error:** A list does not express "unique set" intent; duplicates would inflate counts if you ever used `List.Count` before deduping.

### Fix (recommended)

Use `HashSet<string>` directly:

```csharp
public HashSet<string> GetUniquePlates()
{
    HashSet<string> plates = new HashSet<string>();
    foreach (TollBoothLog log in logs)
    {
        plates.Add(log.plate);
    }
    return plates;
}
```

### LINQ one-liner

```csharp
public HashSet<string> GetUniquePlates()
{
    return logs.Select(log => log.plate).ToHashSet();
}
```

### Minimal C# port fix (project copy only)

If you must keep the `List` loop temporarily:

```csharp
return new HashSet<string>(plates);
```

This dedupes at return time but still uses the wrong intermediate structure.

---

## Task 2 — FindMismatchedEntries

### Requirements

Return a map: plate → `"NO_EXIT"` or `"NO_ENTRY"` for plates that:

- Entered but never exited (`NO_EXIT`)
- Exited but never entered (`NO_ENTRY`)

Plates with both ENTRY and EXIT are **not** mismatches — omit them.

### Approach

1. Scan all logs; track which plates have seen ENTRY and which have seen EXIT (use two `HashSet`s or one `Dictionary<string, (bool hasEntry, bool hasExit)>`).
2. For each plate with ENTRY and no EXIT → add `NO_EXIT`.
3. For each plate with EXIT and no ENTRY → add `NO_ENTRY`.

### Solution (imperative)

```csharp
public Dictionary<string, string> FindMismatchedEntries()
{
    HashSet<string> entered = new HashSet<string>();
    HashSet<string> exited = new HashSet<string>();

    foreach (TollBoothLog log in logs)
    {
        if (log.type == "ENTRY") entered.Add(log.plate);
        else if (log.type == "EXIT") exited.Add(log.plate);
    }

    Dictionary<string, string> result = new Dictionary<string, string>();
    foreach (string plate in entered)
    {
        if (!exited.Contains(plate))
            result[plate] = "NO_EXIT";
    }
    foreach (string plate in exited)
    {
        if (!entered.Contains(plate))
            result[plate] = "NO_ENTRY";
    }
    return result;
}
```

### Worked Example

| Plate | entered? | exited? | Result |
|-------|----------|---------|--------|
| ABC123 | Yes | Yes | (omit) |
| DEF456 | Yes | No | `NO_EXIT` |
| GHI789 | No | Yes | `NO_ENTRY` |
| JKL012 | Yes | Yes | (omit) |

**Result:** 2 entries.

### LINQ alternative

```csharp
var entered = logs.Where(l => l.type == "ENTRY").Select(l => l.plate).ToHashSet();
var exited  = logs.Where(l => l.type == "EXIT").Select(l => l.plate).ToHashSet();

var result = new Dictionary<string, string>();
foreach (string p in entered.Except(exited))
    result[p] = "NO_EXIT";
foreach (string p in exited.Except(entered))
    result[p] = "NO_ENTRY";
return result;
```

---

## Task 3 — GetCarsOnHighway

### Requirements

Count cars **currently on the highway**: entered but not yet exited.

This is the count of plates with at least one ENTRY and **no** EXIT — not the same as Task 2's map (which also flags EXIT-without-ENTRY anomalies).

### Approach

Reuse the ENTRY/EXIT sets from Task 2:

```csharp
public int GetCarsOnHighway()
{
    HashSet<string> entered = new HashSet<string>();
    HashSet<string> exited = new HashSet<string>();

    foreach (TollBoothLog log in logs)
    {
        if (log.type == "ENTRY") entered.Add(log.plate);
        else if (log.type == "EXIT") exited.Add(log.plate);
    }

    int count = 0;
    foreach (string plate in entered)
    {
        if (!exited.Contains(plate))
            count++;
    }
    return count;
}
```

### LINQ one-liner

```csharp
var entered = logs.Where(l => l.type == "ENTRY").Select(l => l.plate).ToHashSet();
var exited  = logs.Where(l => l.type == "EXIT").Select(l => l.plate).ToHashSet();
return entered.Except(exited).Count();
```

### Worked Example

| Plate | On highway? |
|-------|-------------|
| ABC123 | No (exited) |
| DEF456 | **Yes** |
| GHI789 | No (never entered) |
| JKL012 | No (exited) |

**Result:** `1`

---

## Task 4 — GetMainroadPassCount

### Requirements

Return a map: plate → count of `M` (main-road) logs for that plate.

Per the tests:

- ABC123: 2 M logs → `2`
- DEF456: 1 M log → `1` (still on highway; M logs count)
- JKL012: 0 M logs → `0` or key absent (test uses `GetValueOrDefault`)

`M` logs for plates that only EXIT (`GHI789`) are not asserted — omit or include as 0.

### Approach

Single pass: for each log with `type == "M"`, increment count for `log.plate`.

```csharp
public Dictionary<string, int> GetMainroadPassCount()
{
    Dictionary<string, int> counts = new Dictionary<string, int>();
    foreach (TollBoothLog log in logs)
    {
        if (log.type == "M")
        {
            if (!counts.ContainsKey(log.plate))
                counts[log.plate] = 0;
            counts[log.plate]++;
        }
    }
    return counts;
}
```

### LINQ alternative

```csharp
return logs
    .Where(l => l.type == "M")
    .GroupBy(l => l.plate)
    .ToDictionary(g => g.Key, g => g.Count());
```

### Variant: M logs strictly between ENTRY and EXIT

If an interviewer asks for "between entry and exit" (per XML comment), parse timestamps and only count `M` logs where `entryTime < mTime < exitTime`. The provided tests do **not** require this — stick to simple counting unless spec changes.

```csharp
// Advanced (not required for these tests):
// 1. Group logs by plate
// 2. Find first ENTRY and first EXIT timestamps
// 3. Count M logs with timestamp in (entry, exit)
```

---

## LINQ Cheat Sheet for This Problem

```
Filter:      .Where(l => l.type == "ENTRY")
             .Where(l => l.type == "M")
Project:     .Select(l => l.plate)
Distinct:    .ToHashSet()
Set ops:     entered.Except(exited)
Group:       .GroupBy(l => l.plate)
Count:       .Count()  or  g.Count() on groups
Convert:     .ToDictionary(g => g.Key, g => g.Count())
```

### Equivalent Imperative Style (Task 4 without LINQ)

```csharp
var counts = new Dictionary<string, int>();
foreach (var log in logs)
{
    if (log.type != "M") continue;
    counts[log.plate] = counts.GetValueOrDefault(log.plate, 0) + 1;
}
return counts;
```

---

## Interview Tips

### Reading the Problem

1. **Draw the highway diagram** — ENTRY/EXIT/M semantics become obvious.
2. **Underline log types** — string compare `"ENTRY"`, `"EXIT"`, `"M"` (case-sensitive in tests).
3. **Check output shape** — Set vs Map vs int; which plates to include/exclude.
4. **Resolve comment conflicts** — Task 3 XML mentions `getAverageTripTime` but code/tests use `GetCarsOnHighway`; Task 4 XML says "between entry and exit" but tests count all `M` logs.

### Debugging Task 1

Ask: *"Am I using the right collection for uniqueness?"* Sets dedupe; lists don't.

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Returning `List` as `HashSet` (CS0029) | Use `HashSet` or `new HashSet<string>(list)` |
| Counting EXIT-only cars as "on highway" | Only ENTRY without EXIT |
| Including normal trips in mismatch map | Omit plates with both ENTRY and EXIT |
| Counting ENTRY/EXIT as M passes | Filter `log.type == "M"` only |
| CS0542: class `Main` + method `Main()` | Rename harness to `TollBoothMockStub` in project copy |

### Complexity (for follow-up questions)

For `n` logs and `p` unique plates:

| Method | Time | Space |
|--------|------|-------|
| GetUniquePlates | O(n) | O(p) |
| FindMismatchedEntries | O(n) | O(p) |
| GetCarsOnHighway | O(n) | O(p) |
| GetMainroadPassCount | O(n) | O(p) |

Single-pass solutions are optimal for unsorted logs.

### Extension Questions Interviewers Ask

- **Average trip time:** For plates with ENTRY and EXIT, parse timestamps and average `(exit - entry)`.
- **Duplicate ENTRY without EXIT:** Should second ENTRY increment "on highway" count? (Usually track per-plate state.)
- **Out-of-order logs:** Sort by timestamp before pairing ENTRY/EXIT.

---

## Quick Reference — All Four Solutions

```csharp
// TASK 1 — unique plates
HashSet<string> plates = new HashSet<string>();
foreach (TollBoothLog log in logs) plates.Add(log.plate);
return plates;

// TASK 2 — mismatches
// entered.Except(exited) → NO_EXIT; exited.Except(entered) → NO_ENTRY

// TASK 3 — cars on highway
return entered.Except(exited).Count();

// TASK 4 — M-sensor counts
logs.Where(l => l.type == "M").GroupBy(l => l.plate)
    .ToDictionary(g => g.Key, g => g.Count());
```

Implement Tasks 2–4 in `TollBoothMock.cs`, then run:

```bash
dotnet run --project "16. TollBoothMock/TollBoothMock.csproj"
```

Expected output when complete:

```
Running testGetUniquePlates
Running testFindMismatchedEntries
Running testGetCarsOnHighway
Running testGetMainroadPassCount
All Toll Booth tests passed!
```
