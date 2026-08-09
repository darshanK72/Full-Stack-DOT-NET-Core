# ObstacleMock — Solution & Concepts Guide

Interview problem: build and extend an obstacle-course racing analytics backend in C#.

| File | Purpose |
|------|---------|
| `ObstacleMock.cs` | Problem statement + stub implementations + tests (Tasks 2–3 unimplemented; Task 1 has a bug) |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 0 — Run Basics (Provided)](#task-0--run-basics-provided)
5. [Task 1 — Bug Fix: PersonalBest](#task-1--bug-fix-personalbest)
6. [Task 2 — BestOfBests](#task-2--bestofbests)
7. [Task 3 — ChanceOfPersonalBest](#task-3--chanceofpersonalbest)
8. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
9. [Interview Tips](#interview-tips)

---

## Problem Overview

`RunCollection` stores all attempts (`Run` objects) on a single `Course` and computes racing statistics:

- **Personal best** — fastest *complete* run time
- **Best of bests** — theoretical perfect run (fastest per-obstacle time summed)
- **Chance of personal best** — Monte Carlo simulation for an in-progress run

```
┌─────────────┐     ┌──────────────────┐
│   Course    │────▶│  ObstacleCount   │
│  (title)    │     │  (fixed layout)  │
└─────────────┘     └──────────────────┘
         │
         ▼
┌─────────────┐     ┌──────────────────┐
│    Run      │────▶│  ObstacleTimes   │
│  Complete?  │     │  (List<int>)     │
└─────────────┘     └──────────────────┘
         │
         ▼
┌─────────────────────────────────────┐
│         RunCollection               │
│  PersonalBest() / BestOfBests() /   │
│  ChanceOfPersonalBest()             │
└─────────────────────────────────────┘
```

---

## Core Concepts

### 1. Complete vs Incomplete Runs

A run becomes **complete** when `ObstacleTimes.Count == Course.ObstacleCount`. Until then, it is **incomplete**.

| Run type | Counts toward PersonalBest? | Contributes to BestOfBests? |
|----------|----------------------------|----------------------------|
| Complete | Yes | Yes (all obstacle indices) |
| Incomplete | **No** | Yes (only indices that exist) |

**Key insight:** Task 1 filters on `Complete`; Task 2 uses partial data from incomplete runs.

---

### 2. Per-Obstacle Indexing

Obstacle times are stored in order. Index `0` is the first obstacle, index `1` the second, etc.

```
Run 1:  [3, 4, 5, 6]   ← indices 0–3
Run 4:  [5, 5, 3]      ← indices 0–2 only (incomplete)
```

For **BestOfBests** at index `i`, consider every run that has *at least* `i + 1` obstacles recorded.

For **ChanceOfPersonalBest**, historical times for remaining obstacle `i` come from all runs with `ObstacleTimes.Count > i`.

---

### 3. Monte Carlo Simulation (Task 3)

Instead of computing exact probability by enumerating combinations, the problem asks for **10,000 random trials**:

1. Start with the in-progress run's current total time.
2. For each remaining obstacle index, pick a random historical time for that index.
3. If final total ≤ `PersonalBest()`, count as success.
4. Return `successes / 10000.0`.

Tests use a **range** (e.g. 0.48–0.52) because randomness introduces variance. Use `Random` without a fixed seed (default behavior).

---

### 4. LINQ `Min` and Filtering

Task 1 is a classic "forgot to filter" bug:

```csharp
Runs.Min(r => r.GetRunTime())           // includes incomplete runs
Runs.Where(r => r.Complete).Min(...)    // correct
```

Always ask: **does every element in the collection qualify for this aggregation?**

---

## Data Structures Used

### Domain Classes

```
Course
├── Title          : string
└── ObstacleCount  : int

Run
├── Course         : Course
├── Complete       : bool
└── ObstacleTimes  : List<int>

RunCollection
├── Course         : Course
└── Runs           : List<Run>
```

### Example Dataset (used in Tasks 1 & 2)

```
Obstacles:    O1  O2  O3  O4
Run 1:         3   4   5   6   → total 18 (complete)
Run 2:         4   4   4   5   → total 17 (complete) ← personal best
Run 3:         4   5   4   6   → total 19 (complete)
Run 4:         5   5   3       → total 13 (incomplete — must NOT win PersonalBest)
```

Per-obstacle minimums: O1=3, O2=4, O3=3, O4=5 → **BestOfBests = 15**

---

## Task 0 — Run Basics (Provided)

The `Run` class is already implemented. Understand these behaviors before fixing Task 1:

| Behavior | Rule |
|----------|------|
| `AddObstacleTime(t)` | Appends time; sets `Complete = true` when count reaches `ObstacleCount` |
| After complete | Further `AddObstacleTime` throws `InvalidOperationException` |
| `GetRunTime()` | Sum of all recorded obstacle times (partial or full) |

The test verifies a 2-obstacle course: times 3 + 5 = 8, completion after the second add.

---

## Task 1 — Bug Fix: `PersonalBest()`

### Requirement

Return the **fastest complete run time** in the collection. If there are no runs, return `int.MaxValue`.

### The Bug

```csharp
// BUG: Min includes incomplete runs — Run 4 totals 13 and wins incorrectly
return Runs.Count > 0 ? Runs.Min(r => r.GetRunTime()) : int.MaxValue;
```

Run 4 is incomplete (`[5, 5, 3]`, total 13) but still has a lower sum than the true personal best of **17** (Run 2).

### Fix

```csharp
public int PersonalBest()
{
    if (Runs.Count == 0)
        return int.MaxValue;

    return Runs
        .Where(r => r.Complete)
        .Min(r => r.GetRunTime());
}
```

Imperative equivalent:

```csharp
int best = int.MaxValue;
foreach (Run r in Runs)
{
    if (r.Complete && r.GetRunTime() < best)
        best = r.GetRunTime();
}
return best;
```

### Test Data Breakdown

| Run | Times | Total | Complete? | Eligible for PB? |
|-----|-------|-------|-----------|------------------|
| 1 | 3,4,5,6 | 18 | Yes | Yes |
| 2 | 4,4,4,5 | **17** | Yes | **Yes — winner** |
| 3 | 4,5,4,6 | 19 | Yes | Yes |
| 4 | 5,5,3 | 13 | No | **No** |

**Expected:** `PersonalBest() == 17`

---

## Task 2 — `BestOfBests()`

### Requirement

Sum the **fastest time at each obstacle index** across all runs (including incomplete). Represents the theoretical perfect run if the racer nailed every obstacle at their historical best.

### Solution

```csharp
public int BestOfBests()
{
    int total = 0;

    for (int i = 0; i < Course.ObstacleCount; i++)
    {
        int bestAtIndex = int.MaxValue;

        foreach (Run run in Runs)
        {
            if (run.ObstacleTimes.Count > i)
            {
                int time = run.ObstacleTimes[i];
                if (time < bestAtIndex)
                    bestAtIndex = time;
            }
        }

        total += bestAtIndex;
    }

    return total;
}
```

LINQ version:

```csharp
public int BestOfBests()
{
    int total = 0;

    for (int i = 0; i < Course.ObstacleCount; i++)
    {
        int best = Runs
            .Where(r => r.ObstacleTimes.Count > i)
            .Min(r => r.ObstacleTimes[i]);

        total += best;
    }

    return total;
}
```

### Worked Example

| Index | Times across runs | Min |
|-------|-------------------|-----|
| O1 (0) | 3, 4, 4, 5 | **3** |
| O2 (1) | 4, 4, 5, 5 | **4** |
| O3 (2) | 5, 4, 4, 3 | **3** |
| O4 (3) | 6, 5, 6 *(Run 4 has no 4th obstacle)* | **5** |

**Sum:** 3 + 4 + 3 + 5 = **15**

### Concepts Applied

1. **Column-wise min** — think of runs as rows in a ragged 2D table
2. **Ragged arrays** — incomplete runs simply have fewer columns; skip missing indices with `Count > i`
3. **Different filter than Task 1** — incomplete runs *do* contribute their recorded obstacles

---

## Task 3 — `ChanceOfPersonalBest(Run inProgressRun)`

### Requirement

Given an in-progress run:

1. Run **10,000 trials**.
2. Each trial: for every **remaining** obstacle, randomly pick a historical time at that index from all runs that recorded it.
3. Return the fraction of trials where `currentTime + sum(remaining) <= PersonalBest()`.

### Solution

```csharp
public double ChanceOfPersonalBest(Run inProgressRun)
{
    const int trials = 10000;
    int personalBest = PersonalBest();
    int successes = 0;
    Random rng = new Random();

    int startIndex = inProgressRun.ObstacleTimes.Count;
    int baseTime = inProgressRun.GetRunTime();

    for (int t = 0; t < trials; t++)
    {
        int simulated = baseTime;

        for (int i = startIndex; i < Course.ObstacleCount; i++)
        {
            List<int> historical = new List<int>();
            foreach (Run run in Runs)
            {
                if (run.ObstacleTimes.Count > i)
                    historical.Add(run.ObstacleTimes[i]);
            }

            int pick = historical[rng.Next(historical.Count)];
            simulated += pick;
        }

        if (simulated <= personalBest)
            successes++;
    }

    return (double)successes / trials;
}
```

### Worked Example 1 — Simple 50/50

**Setup:** 3-obstacle course, two complete runs `{3,3,2}` and `{3,3,3}`. Personal best = **8**.

**In-progress run:** `{3, 3}` — one obstacle remaining (index 2).

Historical times at index 2: **2, 3**

| Picked time | Final total | ≤ 8? |
|-------------|-------------|------|
| 2 | 3+3+2 = 8 | Yes |
| 3 | 3+3+3 = 9 | No |

**Expected chance ≈ 0.50** (test accepts 0.48–0.52)

---

### Worked Example 2 — ~83.3% (5/6)

**Setup:** 4-obstacle course:

| Run | Times | Total | Complete? |
|-----|-------|-------|-----------|
| 1 | 3,3,2,3 | 11 | Yes |
| 2 | 3,3,3,2 | 11 | Yes |
| 3 | 5,5,2 | 12 | No |

Personal best = **11** (complete runs only).

**In-progress run:** `{3, 3}` — two obstacles remaining (indices 2 and 3).

Historical times:

| Index | Values | P(value) |
|-------|--------|----------|
| 2 | 2, 3, 2 | P(2)=2/3, P(3)=1/3 |
| 3 | 3, 2 | P(3)=1/2, P(2)=1/2 |

Simulated total = 6 + t₂ + t₃:

| t₂ | t₃ | Total | ≤ 11? |
|----|-----|-------|-------|
| 2 | 3 | 11 | Yes |
| 2 | 2 | 10 | Yes |
| 3 | 3 | 12 | **No** |
| 3 | 2 | 11 | Yes |

Failures only when t₂=3 **and** t₃=3 → P = (1/3)(1/2) = **1/6**

**Success probability = 1 − 1/6 = 5/6 ≈ 0.833** (test accepts 0.813–0.853)

### Concepts Applied

1. **Depends on fixed Task 1** — `PersonalBest()` must exclude incomplete runs
2. **Independent random picks** — each remaining obstacle sampled separately
3. **Historical pool per index** — only runs with enough obstacles contribute
4. **Monte Carlo tolerance** — 10k trials ≈ ±2% for probabilities near 0.5; tests use ranges

---

## LINQ Cheat Sheet for This Problem

```
Filter:      .Where(r => r.Complete)
             .Where(r => r.ObstacleTimes.Count > i)
Aggregate:   .Min(r => r.GetRunTime())
             .Min(r => r.ObstacleTimes[i])
Index loop:  for (int i = 0; i < Course.ObstacleCount; i++)
```

### When to Use LINQ vs Loops

| Task | LINQ | Loop |
|------|------|------|
| Task 1 | `.Where().Min()` — one-liner | Clear for interview whiteboard |
| Task 2 | `.Where().Min()` per index | Nested loop mirrors "column min" mental model |
| Task 3 | Less natural (random sampling) | **Prefer loops** — explicit trial/index logic |

---

## Interview Tips

### Reading the Problem

1. **Underline "complete" vs "all runs"** — different tasks use different filters
2. **Ragged data** — incomplete runs are partial rows, not invalid rows
3. **Task 3 builds on Task 1** — fix the bug before implementing simulation

### Debugging Task 1

When `Min` or `Max` returns an unexpectedly low value, check whether **every element should participate**. Incomplete runs often have fewer obstacles but can still have low partial sums.

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Including incomplete runs in `PersonalBest` | Filter `r.Complete` |
| Excluding incomplete runs from `BestOfBests` | Include any run with `Count > i` |
| Using `BestOfBests` as the threshold in Task 3 | Compare against `PersonalBest()` |
| Strict `<` instead of `<=` | Spec says `<= personalBest()` |
| Empty historical list for an index | Shouldn't happen in tests; guard or throw |
| Wrong remaining obstacle range | Start at `inProgressRun.ObstacleTimes.Count` |
| Fixed Random seed | Use default `new Random()` unless tests require seed |

### Complexity (for follow-up questions)

For `n` runs, `o` obstacles, `t` trials:

| Method | Time | Space |
|--------|------|-------|
| `PersonalBest` | O(n) | O(1) |
| `BestOfBests` | O(n × o) | O(1) |
| `ChanceOfPersonalBest` | O(t × o × n) | O(n) per trial for historical lists |

All are fine for typical interview sizes.

---

## Quick Reference — All Solutions

```csharp
// TASK 1 — only complete runs
return Runs.Where(r => r.Complete).Min(r => r.GetRunTime());

// TASK 2 — min per obstacle index, sum
for (int i = 0; i < Course.ObstacleCount; i++)
    total += Runs.Where(r => r.ObstacleTimes.Count > i)
                 .Min(r => r.ObstacleTimes[i]);

// TASK 3 — 10k Monte Carlo trials
int baseTime = inProgressRun.GetRunTime();
for (int t = 0; t < 10000; t++)
{
    int simulated = baseTime;
    for (int i = inProgressRun.ObstacleTimes.Count; i < Course.ObstacleCount; i++)
    {
        // collect historical[i] from all runs with Count > i
        simulated += historical[rng.Next(historical.Count)];
    }
    if (simulated <= PersonalBest()) successes++;
}
return (double)successes / 10000.0;
```

Implement these inside the `RunCollection` class in `ObstacleMock.cs` to pass all tests.

---

## Running the Project

```bash
cd "13. ObstacleMock"
dotnet run
```

Expected output before solving:

```
  PASS: TASK 0 — Run basics
  FAIL: TASK 1 — Personal best
  FAIL: TASK 2 — Best of bests
  FAIL: TASK 3 — Chance of personal best
```

After all fixes:

```
  PASS: TASK 0 — Run basics
  PASS: TASK 1 — Personal best
  PASS: TASK 2 — Best of bests
  PASS: TASK 3 — Chance of personal best
```
