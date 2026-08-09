# GamingPlatformTest — Solution & Concepts Guide

Interview problem: build and extend an online gaming platform backend that tracks players, match history, and statistics in C#.

| File | Purpose |
|------|---------|
| `GamingPlatformTest.cs` | Problem statement + stub implementations + inline test suite |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 1 — Bug Fix](#task-1--bug-fix-getplayerstatistics)
5. [Task 2.1 — Add Match Result](#task-21--addmatchresult)
6. [Task 2.2 — Average Score By Outcome](#task-22--getaveragescorebyoutcome)
7. [Task 3 — Head To Head](#task-3--getheadtohead)
8. [Task 4 — Recent Form](#task-4--getrecentform)
9. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
10. [Interview Tips](#interview-tips)

---

## Problem Overview

`GameManager` is the central service that:

- Registers **players** (by `playerId`)
- Stores **match results** (each record is one player's perspective of a match)
- Computes **statistics, head-to-head summaries, and form rankings**

Each task builds on the previous one — from a simple counting bug to timestamp-ordered analytics and top-k style ranking.

```
┌─────────────────┐     ┌──────────────────┐
│   AddPlayer()   │────▶│ Dictionary<int,  │
│                 │     │ Player>          │
└─────────────────┘     └──────────────────┘
                               │
┌─────────────────┐            │
│ AddMatchResult()│────────────┼──▶ List<MatchResult>
└─────────────────┘            │
                               ▼
                    ┌──────────────────────┐
                    │ Analytics methods    │
                    │ (Tasks 1–4)          │
                    └──────────────────────┘
```

---

## Core Concepts

### 1. Enums (`Outcome`)

Enums restrict match outcomes to a fixed set:

```csharp
enum Outcome { WIN, LOSS, DRAW }
```

**Why it matters:** Every analytics method filters or groups by `outcome`. Task 4 maps outcomes to points (WIN=3, DRAW=1, LOSS=0).

---

### 2. Player-Perspective Records (Critical!)

Each physical match produces **two** `MatchResult` rows — one per player:

```
Match at t=1000: Player 1 beats Player 2
  → (playerId=1, opponentId=2, outcome=WIN,  ...)
  → (playerId=2, opponentId=1, outcome=LOSS, ...)
```

| Method | Which records to use |
|--------|---------------------|
| `GetPlayerStatistics` | All rows where `playerId == target` |
| `GetAverageScoreByOutcome` | All rows where `playerId == target` |
| `GetHeadToHead(p1, p2)` | Only rows where `playerId == p1 AND opponentId == p2` |
| `GetRecentForm` | All rows where `playerId == target`, ordered by `timestamp` |

**Common trap:** Counting both players' records in head-to-head doubles every match. Always filter from **player1's perspective** only.

---

### 3. Nullable Types (`Outcome?`, `long?`)

`HeadToHead` uses nullable fields when two players have never met:

```csharp
public Outcome? lastResult;        // null if no matches
public long? lastMatchTimestamp;   // null if no matches
```

---

### 4. Dictionary vs List

| Structure | Use case in this problem |
|-----------|--------------------------|
| `Dictionary<int, Player>` | O(1) lookup by `playerId`; validate known players |
| `List<MatchResult>` | Append-only match log; filter/group/order for analytics |
| `Dictionary<Outcome, double>` | Return type for per-outcome averages |

---

### 5. Timestamp Ordering

`MatchResult` implements `IComparable<MatchResult>` by `timestamp`. For "most recent" or "last n" queries:

```csharp
.OrderBy(m => m.timestamp)           // ascending (oldest first)
.Skip(count - n)                     // take last n after ordering
.Last()                              // most recent match
```

Task 4 requires **last n matches ordered by timestamp ascending**, then sum points from that slice.

---

### 6. Guard Clauses & Edge Cases

| Condition | Expected behavior |
|-----------|-------------------|
| Unknown `playerId` in `AddMatchResult` | Ignore — do not store |
| Player with no results (Task 2.2) | Return empty map |
| Players never faced each other (Task 3) | All counts 0, nulls for last fields |
| `n <= 0` (Task 4) | Return empty list |
| Player with fewer than `n` results (Task 4) | Exclude from output |

---

## Data Structures Used

### Domain Classes

```
Player
├── playerId : int
└── username : string

MatchResult
├── playerId   : int
├── opponentId : int
├── outcome    : Outcome
├── score      : int
└── timestamp  : long

PlayerStats (Task 1 output)
├── totalMatches : int
├── wins         : int
└── winRate      : double

HeadToHead (Task 3 output)
├── winsPlayer1        : int
├── winsPlayer2        : int
├── draws              : int
├── totalMatches       : int
├── lastResult         : Outcome?
└── lastMatchTimestamp : long?
```

---

## Task 1 — Bug Fix: `GetPlayerStatistics()`

### Requirement

For a given player, return:

- `totalMatches` — count of **all** match results for that player
- `wins` — count where `outcome == WIN`
- `winRate` — `wins / totalMatches` (0.0 if total is 0)

### The Bug

```csharp
int totalMatches = playerMatches
        .Where(m => m.outcome == Outcome.WIN)   // BUG: only counts wins
        .Count();
int wins = playerMatches
        .Where(m => m.outcome == Outcome.WIN)
        .Count();
```

Both `totalMatches` and `wins` count wins only, so a player with 4 matches (2 wins, 1 loss, 1 draw) reports `totalMatches=2` instead of `4`.

### Fix

```csharp
public PlayerStats GetPlayerStatistics(int playerId)
{
    List<MatchResult> playerMatches = matchResults
            .Where(m => m.playerId == playerId)
            .ToList();

    int totalMatches = playerMatches.Count;
    int wins = playerMatches
            .Where(m => m.outcome == Outcome.WIN)
            .Count();
    double winRate = totalMatches > 0 ? (double)wins / totalMatches : 0.0;

    return new PlayerStats(totalMatches, wins, winRate);
}
```

### Test Data Breakdown (Player 1)

| Outcome | Score | Timestamp |
|---------|-------|-----------|
| WIN | 80 | 1000 |
| LOSS | 50 | 2000 |
| DRAW | 60 | 3000 |
| WIN | 90 | 4000 |

**Expected:** `totalMatches=4`, `wins=2`, `winRate=0.5`

Player 2 has 2 draws → `totalMatches=2`, `wins=0`, `winRate=0.0`

### Concept: Cast Before Division

```csharp
(double)wins / totalMatches   // → 0.5
wins / totalMatches           // integer division if both int → 0
```

---

## Task 2.1 — `AddMatchResult()`

### Requirement

Store a match result. If `playerId` is not a registered player, **ignore** the result.

### Solution

```csharp
public void AddMatchResult(MatchResult result)
{
    if (!players.ContainsKey(result.playerId))
        return;

    matchResults.Add(result);
}
```

### Test Verification

- Two valid results stored (players 1 and 2)
- Result for unknown player 99 is ignored
- `matchResults.Count == 2`

### Concept: Validate Before Mutating

Same pattern as `ClinicManager.AddAppointment` — check the foreign key (`playerId`) exists before appending to the log.

---

## Task 2.2 — `GetAverageScoreByOutcome(int playerId)`

### Requirement

Return a map of `Outcome → average score` for the given player.

**Rules:**
- Only include outcomes the player has at least one result for
- If the player has no match results, return an empty map

### Solution

```csharp
public Dictionary<Outcome, double> GetAverageScoreByOutcome(int playerId)
{
    return matchResults
        .Where(m => m.playerId == playerId)
        .GroupBy(m => m.outcome)
        .ToDictionary(g => g.Key, g => g.Average(m => (double)m.score));
}
```

### Step-by-Step (Player 1)

```
Match results for player 1:
  WIN,  score=80  @ 1000
  WIN,  score=90  @ 2000
  DRAW, score=70  @ 3000

After GroupBy:
  WIN  → [80, 90]  → average = 85.0
  DRAW → [70]      → average = 70.0
  (no LOSS records → LOSS key absent)

Result: { WIN: 85.0, DRAW: 70.0 }
```

Player 2's LOSS average: (50 + 60) / 2 = 55.0

Player 3 with no results → empty map (LINQ produces no groups)

### Concepts Applied

1. **Filter by player** — `Where(m => m.playerId == playerId)`
2. **GroupBy outcome** — one bucket per enum value
3. **Average with double cast** — `(double)m.score` for precision
4. **ToDictionary** — materialize the map

---

## Task 3 — `GetHeadToHead(int playerId1, int playerId2)`

### Requirement

Summarize all matches **from player1's perspective** against player2:

| Field | Meaning |
|-------|---------|
| `winsPlayer1` | Times player1's outcome was WIN |
| `winsPlayer2` | Times player1's outcome was LOSS |
| `draws` | Times player1's outcome was DRAW |
| `totalMatches` | Count of player1-vs-player2 records |
| `lastResult` | Outcome of the most recent match (player1's view) |
| `lastMatchTimestamp` | Timestamp of that most recent match |

If they never played: all numeric fields 0, `lastResult` and `lastMatchTimestamp` null.

### Solution

```csharp
public HeadToHead GetHeadToHead(int playerId1, int playerId2)
{
    List<MatchResult> h2h = matchResults
        .Where(m => m.playerId == playerId1 && m.opponentId == playerId2)
        .OrderBy(m => m.timestamp)
        .ToList();

    if (h2h.Count == 0)
        return new HeadToHead(0, 0, 0, 0, null, null);

    int winsPlayer1 = h2h.Count(m => m.outcome == Outcome.WIN);
    int winsPlayer2 = h2h.Count(m => m.outcome == Outcome.LOSS);
    int draws = h2h.Count(m => m.outcome == Outcome.DRAW);
    MatchResult last = h2h[h2h.Count - 1];

    return new HeadToHead(
        winsPlayer1, winsPlayer2, draws, h2h.Count,
        last.outcome, last.timestamp);
}
```

### Worked Example: `GetHeadToHead(1, 2)`

Records where `playerId=1, opponentId=2`:

| Timestamp | Outcome | Maps to |
|-----------|---------|---------|
| 1000 | WIN | winsPlayer1++ |
| 2000 | LOSS | winsPlayer2++ |
| 3000 | DRAW | draws++ |
| 4000 | WIN | winsPlayer1++, lastResult=WIN |

**Result:** wins1=2, wins2=1, draws=1, total=4, lastResult=WIN, timestamp=4000

### Reverse Perspective: `GetHeadToHead(2, 1)`

Filter `playerId=2, opponentId=1` — completely different slice of data:

| Timestamp | Outcome (player 2's view) |
|-----------|---------------------------|
| 1000 | LOSS |
| 2000 | WIN |
| 3000 | DRAW |
| 4000 | LOSS |

**Result:** wins1=1, wins2=2, lastResult=LOSS at 4000

### Concepts Applied

1. **Directional filtering** — head-to-head is asymmetric; swapping arguments changes the result
2. **Map LOSS → opponent win** — from player1's perspective, their LOSS counts as a win for player2
3. **Order by timestamp** — last element = most recent match

---

## Task 4 — `GetRecentForm(int n)`

### Requirement

Return `[playerId, formPoints]` for each player with **at least n** match results.

**Rules:**
- Take only the **last n** results per player, ordered by timestamp ascending
- Points: WIN=3, DRAW=1, LOSS=0
- Sort by `formPoints` descending; tiebreak by `playerId` ascending
- `n <= 0` → empty list

### Solution

```csharp
public List<int[]> GetRecentForm(int n)
{
    if (n <= 0)
        return new List<int[]>();

    return players.Keys
        .Select(playerId =>
        {
            List<MatchResult> matches = matchResults
                .Where(m => m.playerId == playerId)
                .OrderBy(m => m.timestamp)
                .ToList();
            return new { playerId, matches };
        })
        .Where(x => x.matches.Count >= n)
        .Select(x =>
        {
            List<MatchResult> lastN = x.matches
                .Skip(x.matches.Count - n)
                .ToList();

            int formPoints = lastN.Sum(m =>
                m.outcome == Outcome.WIN ? 3 :
                m.outcome == Outcome.DRAW ? 1 : 0);

            return new int[] { x.playerId, formPoints };
        })
        .OrderByDescending(arr => arr[1])
        .ThenBy(arr => arr[0])
        .ToList();
}
```

### Worked Example — Case 1 (`n=2`)

**Player 1** (3 matches: W@1000, W@2000, W@3000):
- Last 2 → W@2000, W@3000 → 3 + 3 = **6 points**

**Player 2** (3 matches: L@1000, L@2000, W@3000):
- Last 2 → L@2000, W@3000 → 0 + 3 = **3 points**

**Player 3** (1 match) → excluded (< 2 results)

**Result:** `[[1, 6], [2, 3]]`

### Worked Example — Case 2 (`n=3`, tiebreaker)

Both players' last 3 = W, D, L → 3 + 1 + 0 = **4 points each**

Tiebreak by `playerId` ascending → player 1 before player 2.

`n=0` and `n=-1` → empty list (guard clause).

### Concepts Applied

1. **Skip(count - n)** — classic "last n elements" after ascending sort
2. **Conditional sum** — ternary or switch for point mapping
3. **Multi-key sort** — `OrderByDescending` then `ThenBy`
4. **Eligibility filter** — `Count >= n` before computing form

### Alternative: TakeLast (if available)

```csharp
List<MatchResult> lastN = matches.TakeLast(n).ToList();
```

`TakeLast(n)` is available in .NET 6+ and reads more clearly than `Skip(count - n)`.

---

## LINQ Cheat Sheet for This Problem

```
Filter:      .Where(m => m.playerId == id && m.opponentId == opp)
Group:       .GroupBy(m => m.outcome)
Aggregate:   .Count(), .Average(m => (double)m.score), .Sum(...)
Sort:        .OrderBy(m => m.timestamp)
             .OrderByDescending(arr => arr[1]).ThenBy(arr => arr[0])
Slice:       .Skip(count - n)          // last n after ascending sort
             .TakeLast(n)              // .NET 6+
Convert:     .ToDictionary(g => g.Key, g => g.Average(...))
             .ToList()
Guard:       if (n <= 0) return new List<int[]>();
             if (!players.ContainsKey(id)) return;
```

### Imperative Style (Task 2.2 without LINQ)

```csharp
var sums = new Dictionary<Outcome, int>();
var counts = new Dictionary<Outcome, int>();

foreach (MatchResult m in matchResults)
{
    if (m.playerId != playerId) continue;

    if (!sums.ContainsKey(m.outcome))
    {
        sums[m.outcome] = 0;
        counts[m.outcome] = 0;
    }
    sums[m.outcome] += m.score;
    counts[m.outcome]++;
}

var result = new Dictionary<Outcome, double>();
foreach (Outcome o in sums.Keys)
    result[o] = (double)sums[o] / counts[o];

return result;
```

---

## Interview Tips

### Reading the Problem

1. **Note whose perspective** — player stats use `playerId`; head-to-head uses `(playerId1, opponentId2)` only
2. **Two records per match** — never double-count in head-to-head
3. **"Last n" means timestamp order** — sort ascending, then take the tail
4. **Tie-break direction** — Task 4 uses smaller `playerId` wins (ascending)

### Debugging Task 1

When counts look wrong, ask: *"Am I counting all items or filtering too aggressively?"* The bug filtered `totalMatches` to wins only.

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Double-counting head-to-head | Filter only `playerId == p1 && opponentId == p2` |
| Including both players' rows in H2H | One perspective only |
| Wrong "last n" order | Sort ascending first, then `Skip(count - n)` |
| Forgetting unknown player check in AddMatchResult | `players.ContainsKey(result.playerId)` |
| Not handling `n <= 0` | Guard clause at start of Task 4 |
| Integer division in winRate | Cast `(double)wins` before dividing |

### Complexity (for follow-up questions)

For `p` players and `m` match results:

| Method | Time | Space |
|--------|------|-------|
| Task 1 | O(m) | O(k) player matches |
| Task 2.1 | O(1) amortized append | O(1) |
| Task 2.2 | O(m) | O(3) outcomes max |
| Task 3 | O(m) | O(k) h2h matches |
| Task 4 | O(p × m log m) | O(p) |

All acceptable for typical interview sizes.

---

## Quick Reference — All Solutions

```csharp
// TASK 1 — fix totalMatches count
int totalMatches = playerMatches.Count;

// TASK 2.1 — store if player exists
if (!players.ContainsKey(result.playerId)) return;
matchResults.Add(result);

// TASK 2.2 — average score by outcome
return matchResults
    .Where(m => m.playerId == playerId)
    .GroupBy(m => m.outcome)
    .ToDictionary(g => g.Key, g => g.Average(m => (double)m.score));

// TASK 3 — head-to-head from player1's perspective
List<MatchResult> h2h = matchResults
    .Where(m => m.playerId == playerId1 && m.opponentId == playerId2)
    .OrderBy(m => m.timestamp).ToList();

// TASK 4 — recent form ranking
if (n <= 0) return new List<int[]>();
// ... filter players with >= n matches, take last n, sum points, sort
```

Run the project to verify: Task 1 should pass after the bug fix; Tasks 2–4 pass once implemented.

```bash
dotnet run --project "08. GamingPlatformTest"
```

Expected when fully solved: **7 passed, 0 failed** (BUG 1-2, Task 2.1, Task 2.2, Task 3, Task 4 Case 1, Task 4 Case 2).
