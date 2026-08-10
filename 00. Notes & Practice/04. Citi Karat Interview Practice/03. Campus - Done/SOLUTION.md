# Campus — Solution & Concepts Guide

Interview problem: build and extend a corporate campus physical access management system in C#.

| File | Purpose |
|------|---------|
| `Campus.cs` (parent folder) | Original problem statement + tests (Tasks 2–4 stubs) |
| `Campus.cs` (this folder) | Runnable project copy — implement tasks here |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 1 — Bug Fix: `IsAuthorized`](#task-1--bug-fix-isauthorized)
5. [Task 2 — Log Access & After-Hours Users](#task-2--log-access--after-hours-users)
6. [Task 3 — Hourly Occupancy Report](#task-3--hourly-occupancy-report)
7. [Task 4 — Total Minutes on Campus](#task-4--total-minutes-on-campus)
8. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
9. [Interview Tips](#interview-tips)

---

## Problem Overview

`AccessManager` is the central service that:

- Registers **users** with an access level (`VISITOR`, `STAFF`, `ADMIN`)
- Checks whether a user is **authorized** for a required access level
- Records **badge-in/badge-out events** per user
- Reports **after-hours access**, **hourly building occupancy**, and **total time on campus**

Each task builds on the previous one — from a simple authorization bug fix to interval overlap logic.

```
┌─────────────────┐     ┌──────────────────┐
│  AddUser()      │────▶│  List<User>      │
└─────────────────┘     └──────────────────┘
                               │
┌─────────────────┐            │
│  LogAccess()    │────────────┼──▶ Dictionary<int, List<AccessEvent>>
└─────────────────┘            │
                               ▼
                    ┌──────────────────────┐
                    │ Analytics methods    │
                    │ (Tasks 1–4)          │
                    └──────────────────────┘
```

---

## Core Concepts

### 1. Enums with Implicit Ranking (`AccessLevel`)

Access levels form a hierarchy encoded as integer values:

```csharp
enum AccessLevel
{
    VISITOR = 1,
    STAFF   = 2,
    ADMIN   = 3
}
```

The `AccessLevelExtensions.Rank()` helper exposes the numeric rank. **Authorization** means the user's rank is **≥** the required rank — not an exact match.

| User level | Authorized for |
|------------|----------------|
| ADMIN (3) | VISITOR, STAFF, ADMIN |
| STAFF (2) | VISITOR, STAFF |
| VISITOR (1) | VISITOR only |

---

### 2. Time as Minutes Since Midnight

All times are **integers** representing minutes from 00:00:

| Clock time | Minutes |
|------------|---------|
| 08:00 | 480 |
| 20:00 | 1200 |
| 02:10 | 130 |

Campus hours: **480–1200** (08:00–20:00, inclusive at boundaries).

---

### 3. Interval Overlap Logic

Several tasks depend on whether a time range `[entry, exit)` overlaps another range.

**After-hours event:** `entryTime < 480` **OR** `exitTime > 1200`

**Present during hour h:** hour h covers `[h*60, (h+1)*60)`. An event `[entry, exit)` overlaps hour h when:

```
entry < (h+1)*60  AND  exit > h*60
```

**Boundary rule:** A session ending exactly on an hour boundary does **not** count in the next hour. Example: event `[120, 180)` is present in hour 2 only (minutes 120–179), not hour 3.

---

### 4. Dictionary vs List — When to Use Which

| Structure | Use case in this problem |
|-----------|--------------------------|
| `List<User>` | All registered users; linear scan for lookup by `userId` |
| `Dictionary<int, List<AccessEvent>>` | O(1) lookup of events by `userId`; append on `LogAccess` |
| `Dictionary<int, int>` (return type) | Map hour → distinct user count |

**Key insight:** Users live in a list (small interview datasets). Events are keyed by user in a dictionary for fast per-user aggregation.

---

### 5. Guard Clauses & Edge Cases

| Condition | Expected behavior |
|-----------|-------------------|
| Unknown `userId` in `LogAccess` | Return `false`, ignore event |
| Unknown `userId` in `GetTotalMinutesOnCampus` | Return `0` |
| User with no events | Not in after-hours list; total minutes = 0 |
| Hour with no users present | Omitted from occupancy dictionary |
| Event exactly at 480 or 1200 | **Within** hours (not after-hours) |

---

## Data Structures Used

### Domain Classes

```
User
├── userId       : int
├── name         : string
└── accessLevel  : AccessLevel

AccessEvent
├── id         : int
├── entryTime  : int   (minutes since midnight)
└── exitTime   : int   (minutes since midnight)

AccessManager
├── users      : List<User>
└── userEvents : Dictionary<int, List<AccessEvent>>
```

---

## Task 1 — Bug Fix: `IsAuthorized`

### Requirement

Return `true` when the user's access level rank is **the same or higher** than the required level. Return `false` if the user is not found.

### The Bug

```csharp
// BUG: exact match — ADMIN fails for VISITOR/STAFF requirements
return user.accessLevel == requiredLevel;
```

An ADMIN user should pass checks for VISITOR and STAFF areas, but exact equality returns `false`.

### Fix

```csharp
public bool IsAuthorized(int userId, AccessLevel requiredLevel)
{
    foreach (User user in users)
    {
        if (user.userId == userId)
        {
            return user.accessLevel.Rank() >= requiredLevel.Rank();
        }
    }
    return false;
}
```

Alternative without extension method:

```csharp
return (int)user.accessLevel >= (int)requiredLevel;
```

### Test Breakdown

| Check | User | Required | Rank comparison | Expected |
|-------|------|----------|-----------------|----------|
| 1 | ADMIN (3) | VISITOR (1) | 3 ≥ 1 | `true` |
| 2 | ADMIN (3) | STAFF (2) | 3 ≥ 2 | `true` |

### Concept: Rank vs Equality

This is a classic **privilege hierarchy** pattern. Think of it like Unix file permissions or role-based access: higher roles inherit lower permissions.

---

## Task 2 — Log Access & After-Hours Users

### Requirements

**2.1 `LogAccess(userId, evt)`**
- If user exists: append event to that user's list, return `true`
- If user does not exist: ignore event, return `false`

**2.2 `GetAfterHoursUsers()`**
- Return sorted ascending list of user IDs with **at least one** after-hours event
- After-hours: `entryTime < 480` OR `exitTime > 1200`
- Exclude users with no after-hours events (including users with no events)

### Solution

```csharp
public bool LogAccess(int userId, AccessEvent evt)
{
    if (!UserExists(userId))
        return false;

    if (!userEvents.ContainsKey(userId))
        userEvents[userId] = new List<AccessEvent>();

    userEvents[userId].Add(evt);
    return true;
}

private bool UserExists(int userId)
{
    foreach (User user in users)
    {
        if (user.userId == userId)
            return true;
    }
    return false;
}

private bool IsAfterHours(AccessEvent evt)
{
    return evt.entryTime < 480 || evt.exitTime > 1200;
}

public List<int> GetAfterHoursUsers()
{
    var result = new List<int>();

    foreach (var kvp in userEvents)
    {
        foreach (AccessEvent evt in kvp.Value)
        {
            if (IsAfterHours(evt))
            {
                result.Add(kvp.Key);
                break; // one after-hours event is enough
            }
        }
    }

    result.Sort();
    return result;
}
```

### Step-by-Step (Test Case)

```
User 10 added (STAFF)
LogAccess(10, event id=1, entry=400, exit=500)

Is after-hours?
  entry 400 < 480  → YES (started before campus opens)

GetAfterHoursUsers() → [10]
```

### Concepts Applied

1. **Validate before mutate** — check user exists before storing event
2. **Lazy dictionary init** — create event list on first log for a user
3. **Early break** — once one after-hours event found, no need to scan remaining events
4. **Sort before return** — spec requires ascending order

---

## Task 3 — Hourly Occupancy Report

### Requirement

Return a dictionary mapping each hour `h` (0–23) to the count of **distinct users** present during that hour. Only include hours with at least one user.

A user is present during hour `h` if any event overlaps `[h*60, (h+1)*60)`.

### Solution

```csharp
public Dictionary<int, int> GetHourlyOccupancy()
{
    var hourUsers = new Dictionary<int, HashSet<int>>();

    foreach (var kvp in userEvents)
    {
        int userId = kvp.Key;
        foreach (AccessEvent evt in kvp.Value)
        {
            int startHour = evt.entryTime / 60;
            int endHour = (evt.exitTime - 1) / 60; // exit is exclusive

            for (int h = startHour; h <= endHour; h++)
            {
                if (!hourUsers.ContainsKey(h))
                    hourUsers[h] = new HashSet<int>();
                hourUsers[h].Add(userId);
            }
        }
    }

    var result = new Dictionary<int, int>();
    foreach (var kvp in hourUsers)
        result[kvp.Key] = kvp.Value.Count;

    return result;
}
```

### Alternative: Overlap Check Per Hour

```csharp
for (int h = 0; h < 24; h++)
{
    int hourStart = h * 60;
    int hourEnd = (h + 1) * 60;

    if (evt.entryTime < hourEnd && evt.exitTime > hourStart)
    {
        // user present in hour h
    }
}
```

Both approaches are equivalent. The loop-from-startHour-to-endHour version is more efficient when sessions span few hours.

### Worked Example (Full Spec)

```
User 20: session [130, 260)
User 21: session [200, 230)

User 20 spans hours:
  startHour = 130/60 = 2
  endHour   = (260-1)/60 = 4   → hours 2, 3, 4

User 21 spans hours:
  startHour = 200/60 = 3
  endHour   = (230-1)/60 = 3   → hour 3 only

Result: { 2: 1, 3: 2, 4: 1 }
```

### Why `(exitTime - 1) / 60`?

Exit time is treated as **exclusive** — a session ending at minute 180 is **not** present in hour 3 (which starts at 180). Subtracting 1 before integer division maps boundary exits to the previous hour.

| Event | exitTime | `(exit-1)/60` | Last hour present |
|-------|----------|---------------|-------------------|
| [120, 180) | 180 | 179/60 = 2 | hour 2 only ✓ |
| [130, 260) | 260 | 259/60 = 4 | hours 2, 3, 4 ✓ |

### Test Breakdown

```
User 20: [130, 260) → present in hours 2, 3, 4
Assert: occ.ContainsKey(2)  → true
```

### Concepts Applied

1. **HashSet for distinct counts** — same user in multiple events still counts once per hour
2. **Exclusive end boundary** — critical for hour-boundary correctness
3. **Sparse result** — omit empty hours from output dictionary

---

## Task 4 — Total Minutes on Campus

### Requirement

Return the sum of `(exitTime - entryTime)` across all events for a given user. Return `0` if the user does not exist or has no events.

*(Task 4 was added manually — not yet seen in live Karat interviews.)*

### Solution

```csharp
public int GetTotalMinutesOnCampus(int userId)
{
    if (!userEvents.ContainsKey(userId))
        return 0;

    int total = 0;
    foreach (AccessEvent evt in userEvents[userId])
        total += evt.exitTime - evt.entryTime;

    return total;
}
```

### Test Breakdown

```
User 1: one event [100, 150)
Duration = 150 - 100 = 50
Expected: 50
```

### Edge Cases

| Scenario | Result |
|----------|--------|
| User never added | `0` (no dictionary entry) |
| User added but no events logged | `0` |
| Multiple events | Sum of all durations |

---

## LINQ Cheat Sheet for This Problem

```
Find user:       users.FirstOrDefault(u => u.userId == id)
Check exists:    users.Any(u => u.userId == id)
Filter events:   userEvents.Where(kvp => kvp.Value.Any(IsAfterHours))
Distinct IDs:    .Select(kvp => kvp.Key).OrderBy(id => id).ToList()
Sum durations:   userEvents[userId].Sum(e => e.exitTime - e.entryTime)
```

### Equivalent Imperative Style (Task 3 without LINQ)

```csharp
var hourUsers = new Dictionary<int, HashSet<int>>();

foreach (var kvp in userEvents)
{
    int userId = kvp.Key;
    foreach (AccessEvent evt in kvp.Value)
    {
        for (int h = 0; h < 24; h++)
        {
            int hourStart = h * 60;
            int hourEnd = (h + 1) * 60;
            if (evt.entryTime < hourEnd && evt.exitTime > hourStart)
            {
                if (!hourUsers.ContainsKey(h))
                    hourUsers[h] = new HashSet<int>();
                hourUsers[h].Add(userId);
            }
        }
    }
}
```

Imperative loops are often clearer for interval problems in timed interviews.

---

## Interview Tips

### Reading the Problem

1. **Underline comparison operators** — `>=` for authorization vs `==` (the Task 1 trap)
2. **Note boundary rules** — "exactly at 480" is within hours; "ends after 1200" is after-hours
3. **Check exclusive vs inclusive** — exit time does not count in the next hour bucket

### Debugging Task 1

When authorization fails unexpectedly, ask: *"Am I checking rank/hierarchy or exact equality?"*

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Exact enum match in Task 1 | Compare `(int)user.accessLevel >= (int)requiredLevel` |
| Forgetting to return `false` for unknown user in `LogAccess` | Guard with user-exists check first |
| Using `exitTime / 60` for last hour | Use `(exitTime - 1) / 60` for exclusive exit |
| Counting users per event instead of distinct | Use `HashSet<int>` per hour |
| Including hour 0 when no one present | Only add hours with ≥ 1 user to result |
| After-hours off-by-one at 480/1200 | `< 480` and `> 1200`, not `<=` / `>=` |

### Complexity (for follow-up questions)

For `u` users, `e` total events, average session spanning `h` hours:

| Method | Time | Space |
|--------|------|-------|
| `IsAuthorized` | O(u) | O(1) |
| `LogAccess` | O(u) | O(1) amortized append |
| `GetAfterHoursUsers` | O(u × e) | O(u) |
| `GetHourlyOccupancy` | O(e × h) | O(24 × u) worst case |
| `GetTotalMinutesOnCampus` | O(e) | O(1) |

For interview data sizes, linear scans are acceptable. Mention that a `Dictionary<int, User>` would make lookups O(1) if asked about scale.

---

## Quick Reference — All Four Solutions

```csharp
// TASK 1 — fix authorization (rank comparison)
return user.accessLevel.Rank() >= requiredLevel.Rank();

// TASK 2 — log access
if (!UserExists(userId)) return false;
if (!userEvents.ContainsKey(userId)) userEvents[userId] = new List<AccessEvent>();
userEvents[userId].Add(evt);
return true;

// TASK 2 — after-hours users
// Collect userIds with any event where entryTime < 480 || exitTime > 1200, then Sort()

// TASK 3 — hourly occupancy
// For each event, iterate hours from entryTime/60 to (exitTime-1)/60
// Track distinct userIds per hour in HashSet, convert to counts

// TASK 4 — total minutes
if (!userEvents.ContainsKey(userId)) return 0;
return userEvents[userId].Sum(e => e.exitTime - e.entryTime);
```

Implement each task in `Campus.cs` within the marked `// <bug/taskN>` and `// <taskN>` sections, then run:

```bash
dotnet run --project "03. Campus/Campus.csproj"
```

Expected after all fixes: **4 Passed, 0 Failed**.
