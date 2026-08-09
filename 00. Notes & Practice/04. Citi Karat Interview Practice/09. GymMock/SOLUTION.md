# GymMock — Solution & Concepts Guide

Interview problem: build and extend a gym membership and workout tracking backend in C#.

| File | Purpose |
|------|---------|
| `GymMock.cs` | Problem statement + stub implementations + tests (Tasks 2–4 unimplemented) |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 1 — Bug Fix](#task-1--bug-fix-getmembershipstatistics)
5. [Task 2.1 — Add Workout](#task-21--addworkout)
6. [Task 2.2 — Average Workout Durations](#task-22--getaverageworkoutdurations)
7. [Task 3 — Due Payments](#task-3--getduepayments)
8. [Task 4 — Gym Buddies](#task-4--getgymbuddies)
9. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
10. [Interview Tips](#interview-tips)

---

## Problem Overview

`Membership` is the central service that:

- Registers **members** with a tier (`BRONZE`, `SILVER`, `GOLD`)
- Stores **workout sessions** per member (integer time slots in minutes)
- Computes **membership statistics, billing, and social overlap**

Each task builds on the previous one — from a two-part bug fix to tiered pricing and interval overlap analysis.

```
┌─────────────────┐     ┌──────────────────┐
│  AddMember()    │────▶│  List<Member>    │
└─────────────────┘     └──────────────────┘
                               │
┌─────────────────┐            │
│  AddWorkout()   │────────────┼──▶ Dictionary<int, List<Workout>>
└─────────────────┘            │
                               ▼
                    ┌──────────────────────┐
                    │ Analytics methods    │
                    │ (Tasks 1–4)          │
                    └──────────────────────┘
```

---

## Core Concepts

### 1. Enums (`MembershipStatus`)

Enums restrict values to a fixed set. They are type-safe alternatives to magic strings or integers.

```csharp
enum MembershipStatus { BRONZE, SILVER, GOLD }
```

| Tier | Meaning in this problem |
|------|-------------------------|
| BRONZE | Default / free tier — 1 free workout before hourly charges |
| SILVER | Paid tier — 3 free workouts |
| GOLD | Paid tier — 5 free workouts, lowest hourly rate |

**Why it matters:** Task 1 counts "paid" members; Tasks 3–4 branch on tier for pricing rules.

---

### 2. Nullable Types (`double?`)

Task 2.2 requires `null` when a member has no workouts.

```csharp
Dictionary<int, double?>  // value is average minutes OR null
```

A **nullable value type** (`T?`) wraps value types so they can represent "no value."

---

### 3. Dictionary vs List — When to Use Which

| Structure | Use case in this problem |
|-----------|--------------------------|
| `List<Member>` | All registered members; iterate for "include everyone" results |
| `Dictionary<int, List<Workout>>` | O(1) lookup of workouts by `memberId` |
| `Dictionary<K, V>` (return type) | Map memberId → computed value (average, payment, buddies) |

**Key insight:** When the spec says **all members** must appear in the result, loop `members`, not `workoutMap.Keys`.

---

### 4. Integer Division vs Floating Point

Task 1's second bug is a classic C# trap:

```csharp
// WRONG: both operands are int → integer division → 4/4 = 1, but 1/4 = 0
double rate = (totalPaidMembers / totalMembers) * 100.0;

// CORRECT: cast before dividing
double rate = totalMembers == 0 ? 0.0 : (double)totalPaidMembers / totalMembers * 100.0;
```

Cast **before** the division, not after.

---

### 5. Ceiling to Hours

Task 3 bills by the hour, rounded **up**:

| Duration (min) | Hours billed |
|----------------|--------------|
| 1–60 | 1 |
| 61–120 | 2 |
| 80 | 2 |

```csharp
// Option A: Math.Ceiling
int hours = (int)Math.Ceiling(durationMinutes / 60.0);

// Option B: integer arithmetic (no floating point)
int hours = (durationMinutes + 59) / 60;
```

Both are equivalent for positive durations.

---

### 6. Interval Overlap Detection

Two time intervals `[startA, endA]` and `[startB, endB]` **overlap** when:

```csharp
startA < endB && endA > startB
```

**Shared duration** between two intervals:

```csharp
int overlapStart = Math.Max(startA, startB);
int overlapEnd   = Math.Min(endA, endB);
int shared = overlapStart < overlapEnd ? overlapEnd - overlapStart : 0;
```

Task 4 sums shared duration across **all workout pairs** between two members, then ranks buddies by that total.

---

## Data Structures Used

### Domain Classes

```
Workout
├── id        : int   (ordering key for billing)
├── startTime : int   (minutes)
└── endTime   : int   (minutes)

Member
├── memberId          : int
├── name              : string
└── membershipStatus  : MembershipStatus

MembershipStatistics (Task 1 output)
├── totalMembers     : int
├── totalPaidMembers : int
└── conversionRate   : double   (percentage 0–100)
```

---

## Task 1 — Bug Fix: `GetMembershipStatistics()`

### Requirement

Return membership statistics:

- `totalMembers` — count of all members
- `totalPaidMembers` — count of members on a **paid** tier (SILVER or GOLD)
- `conversionRate` — `(totalPaidMembers / totalMembers) * 100.0` (0.0 if total is 0)

### Bug 1 — Missing SILVER

```csharp
// BUG: only GOLD counted; SILVER is also paid
if (m.membershipStatus == MembershipStatus.GOLD)
```

### Fix

```csharp
if (m.membershipStatus == MembershipStatus.SILVER
    || m.membershipStatus == MembershipStatus.GOLD)
{
    totalPaidMembers++;
}
```

### Bug 2 — Integer Division

```csharp
// BUG: 4/4 = 1, then 1 * 100 = 100 — works here by luck
//      but 1/4 = 0 → conversionRate = 0
double conversionRate = totalMembers == 0 ? 0.0 : (totalPaidMembers / totalMembers) * 100.0;
```

### Fix

```csharp
double conversionRate = totalMembers == 0
    ? 0.0
    : (double)totalPaidMembers / totalMembers * 100.0;
```

### Test Data Breakdown

| Member | Status | Paid? |
|--------|--------|-------|
| 1 | SILVER | Yes |
| 2 | GOLD | Yes |
| 3 | SILVER | Yes |
| 4 | SILVER | Yes |

**Expected:** `totalMembers=4`, `totalPaidMembers=4`, `conversionRate=100.0`

---

## Task 2.1 — `AddWorkout()`

### Requirement

- Add a workout for an existing member → return `true`
- Unknown `memberId` → ignore workout, return `false`

### Solution

```csharp
public bool AddWorkout(int memberId, Workout workout)
{
    bool memberExists = false;
    foreach (Member m in members)
    {
        if (m.memberId == memberId)
        {
            memberExists = true;
            break;
        }
    }

    if (!memberExists)
        return false;

    if (!workoutMap.ContainsKey(memberId))
        workoutMap[memberId] = new List<Workout>();

    workoutMap[memberId].Add(workout);
    return true;
}
```

### Concepts Applied

1. **Validate before mutate** — check membership before touching `workoutMap`
2. **Lazy list creation** — create the workout list on first add for a member
3. **Boolean return** — signals success/failure to the caller

---

## Task 2.2 — `GetAverageWorkoutDurations()`

### Requirement

Return `Dictionary<int, double?>` mapping **every member** to their average workout duration in minutes. No workouts → `null`.

### Solution

Add `using System.Linq;` at the top, then:

```csharp
public Dictionary<int, double?> GetAverageWorkoutDurations()
{
    var result = new Dictionary<int, double?>();

    foreach (Member m in members)
    {
        if (!workoutMap.ContainsKey(m.memberId) || workoutMap[m.memberId].Count == 0)
        {
            result[m.memberId] = null;
            continue;
        }

        List<Workout> workouts = workoutMap[m.memberId];
        double total = 0;
        foreach (Workout w in workouts)
            total += w.GetDuration();

        result[m.memberId] = total / workouts.Count;
    }

    return result;
}
```

LINQ equivalent:

```csharp
result[m.memberId] = workoutMap[m.memberId].Average(w => (double)w.GetDuration());
```

### Worked Example

Member 12 with workouts `(10→20)` and `(30→50)`:

```
Durations: 10 min, 20 min
Average: (10 + 20) / 2 = 15.0
```

Member with no workouts → `{ memberId: null }`

---

## Task 3 — `GetDuePayments()`

### Requirement

Tiered pricing after free workouts:

| Tier | Free workouts | Hourly rate (after free quota) |
|------|---------------|-------------------------------|
| BRONZE | 1 | $10/hour |
| SILVER | 3 | $8/hour |
| GOLD | 5 | $6/hour |

- Process workouts in **ID order** (not insertion order)
- Bill duration rounded **up** to whole hours
- Return **all members** (payment = 0 if nothing owed)

### Solution

```csharp
public Dictionary<int, int> GetDuePayments()
{
    var result = new Dictionary<int, int>();

    foreach (Member m in members)
    {
        int freeQuota = GetFreeWorkoutCount(m.membershipStatus);
        int hourlyRate = GetHourlyRate(m.membershipStatus);
        int totalPayment = 0;

        if (workoutMap.ContainsKey(m.memberId))
        {
            List<Workout> sorted = new List<Workout>(workoutMap[m.memberId]);
            sorted.Sort((a, b) => a.GetId().CompareTo(b.GetId()));

            for (int i = 0; i < sorted.Count; i++)
            {
                if (i >= freeQuota)
                {
                    int hours = (sorted[i].GetDuration() + 59) / 60;
                    totalPayment += hourlyRate * hours;
                }
            }
        }

        result[m.memberId] = totalPayment;
    }

    return result;
}

private int GetFreeWorkoutCount(MembershipStatus status)
{
    switch (status)
    {
        case MembershipStatus.BRONZE: return 1;
        case MembershipStatus.SILVER: return 3;
        case MembershipStatus.GOLD:   return 5;
        default: return 0;
    }
}

private int GetHourlyRate(MembershipStatus status)
{
    switch (status)
    {
        case MembershipStatus.BRONZE: return 10;
        case MembershipStatus.SILVER: return 8;
        case MembershipStatus.GOLD:   return 6;
        default: return 0;
    }
}
```

### Worked Example — BRONZE member with 3 workouts

| ID | Duration | Position | Free? | Hours | Cost |
|----|----------|----------|-------|-------|------|
| 1 | 45 min | 1st | Yes (free quota = 1) | — | $0 |
| 2 | 80 min | 2nd | No | 2 | $20 |
| 3 | 30 min | 3rd | No | 1 | $10 |

**Total payment: $30**

### Worked Example — SILVER member with 4 workouts

First 3 free, 4th at $8/hour:

| ID | Duration | Billable? | Hours | Cost |
|----|----------|-----------|-------|------|
| 10 | 60 min | No (1st free) | — | $0 |
| 20 | 90 min | No (2nd free) | — | $0 |
| 30 | 45 min | No (3rd free) | — | $0 |
| 40 | 80 min | Yes | 2 | $16 |

**Total payment: $16**

### Concepts Applied

1. **Sort before indexed logic** — free-quota depends on position after sorting by ID
2. **Ceiling division** — `(duration + 59) / 60`
3. **Include zero-payment members** — loop all `members`, default payment = 0

---

## Task 4 — `GetGymBuddies()`

### Requirement

Two members are **buddies** if any of their workout time slots overlap.

Return `Dictionary<int, List<int>>`:

- Every member appears as a key
- Value = buddy member IDs sorted by **total shared duration descending**
- Members with no overlapping workouts get an empty list

### Solution

```csharp
public Dictionary<int, List<int>> GetGymBuddies()
{
    var result = new Dictionary<int, List<int>>();

    foreach (Member m in members)
    {
        var buddyShared = new List<KeyValuePair<int, int>>();

        foreach (Member other in members)
        {
            if (m.memberId == other.memberId)
                continue;

            int shared = ComputeSharedDuration(m.memberId, other.memberId);
            if (shared > 0)
                buddyShared.Add(new KeyValuePair<int, int>(other.memberId, shared));
        }

        buddyShared.Sort((a, b) =>
        {
            int byDuration = b.Value.CompareTo(a.Value);
            if (byDuration != 0) return byDuration;
            return a.Key.CompareTo(b.Key); // tie-break: smaller memberId first
        });

        var buddyIds = new List<int>();
        foreach (var kv in buddyShared)
            buddyIds.Add(kv.Key);

        result[m.memberId] = buddyIds;
    }

    return result;
}

private int ComputeSharedDuration(int memberIdA, int memberIdB)
{
    if (!workoutMap.ContainsKey(memberIdA) || !workoutMap.ContainsKey(memberIdB))
        return 0;

    int total = 0;
    foreach (Workout wa in workoutMap[memberIdA])
    {
        foreach (Workout wb in workoutMap[memberIdB])
        {
            int start = Math.Max(wa.GetStartTime(), wb.GetStartTime());
            int end   = Math.Min(wa.GetEndTime(), wb.GetEndTime());
            if (start < end)
                total += end - start;
        }
    }
    return total;
}
```

### Worked Example

```
Member 1: workout (0–60)
Member 2: workout (30–90)   → overlap 30–60 = 30 min shared
Member 3: workout (100–120) → no overlap with member 1

Member 1 buddies: [2]        (shared 30)
Member 2 buddies: [1]        (shared 30)
Member 3 buddies: []       (no overlaps)
```

### Multi-Workout Overlap

If member A has two sessions and member B has one that overlaps both, **sum** both overlap lengths:

```
A: (0–60) and (90–120)
B: (30–100)

Overlap 1: max(0,30) to min(60,100) = 30–60 → 30 min
Overlap 2: max(90,30) to min(120,100) = 90–100 → 10 min
Total shared: 40 min
```

### Concepts Applied

1. **Pairwise comparison** — O(m² × w²) for members × workouts; fine for interview sizes
2. **Overlap primitive** — reusable helper for scheduling problems
3. **Custom sort** — duration desc, then memberId asc for stable tie-breaks
4. **All members in output** — even isolated members get `[]`

---

## LINQ Cheat Sheet for This Problem

Add `using System.Linq;` when using these:

```
Filter:      .Where(m => m.memberId == id)
Sort:        .OrderBy(w => w.GetId())
             .OrderByDescending(kv => kv.Value)
Aggregate:   .Average(w => (double)w.GetDuration())
             .Sum(...)
Convert:     .ToList()
             .ToDictionary(...)
```

### Imperative vs LINQ (Task 2.2)

Both are acceptable in interviews. Imperative loops show you understand the math; LINQ is concise for averages and sorting.

---

## Interview Tips

### Reading the Problem

1. **Underline "all members"** — loop `members`, not just keys in `workoutMap`
2. **Note ordering rules** — Task 3 sorts by workout **ID**, not start time
3. **Watch for two bugs in one method** — Task 1 has both a filter bug and a division bug

### Debugging Task 1

When a percentage is 0 but counts look right, check for **integer division**. When a count is too low, check **missing enum cases** in a filter.

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Only counting GOLD as paid | Include SILVER and GOLD |
| Integer division for conversionRate | Cast to `(double)` before dividing |
| Skipping members with no workouts in Task 2.2 | Set value to `null`, still include key |
| Billing before sorting workouts in Task 3 | Sort by `GetId()` first |
| Using floor instead of ceiling for hours | Use `(duration + 59) / 60` |
| Missing members in Task 3/4 output | Loop all `members` |
| Counting overlap as binary (yes/no) only | Sum overlap minutes for buddy ranking |

### Complexity (for follow-up questions)

For `m` members and `w` workouts per member on average:

| Method | Time | Space |
|--------|------|-------|
| Task 1 | O(m) | O(1) |
| Task 2.1 | O(m) | O(1) amortized |
| Task 2.2 | O(m × w) | O(m) |
| Task 3 | O(m × w log w) | O(m) |
| Task 4 | O(m² × w²) | O(m²) |

All are fine for typical interview data sizes.

---

## Quick Reference — All Solutions

```csharp
// TASK 1 — count SILVER + GOLD; cast for percentage
if (m.membershipStatus == MembershipStatus.SILVER
    || m.membershipStatus == MembershipStatus.GOLD)
    totalPaidMembers++;

double conversionRate = totalMembers == 0
    ? 0.0
    : (double)totalPaidMembers / totalMembers * 100.0;

// TASK 2.1 — validate member, then append to workoutMap
if (!memberExists) return false;
workoutMap[memberId].Add(workout);
return true;

// TASK 2.2 — all members; null if no workouts
foreach (Member m in members) { ... average or null ... }

// TASK 3 — sort by ID, skip free quota, ceiling hours × rate
sorted.Sort((a, b) => a.GetId().CompareTo(b.GetId()));
if (i >= freeQuota) totalPayment += hourlyRate * ((duration + 59) / 60);

// TASK 4 — pairwise overlap sum, sort buddies by shared duration desc
int shared = Math.Max(startA, startB) < Math.Min(endA, endB) ? ... : 0;
```

Implement these inside the `Membership` class in `GymMock.cs` to pass all tests.
