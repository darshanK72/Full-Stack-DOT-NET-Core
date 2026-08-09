# EquipmentBooking — Solution & Concepts Guide

Interview problem: build and extend a sports-facility equipment reservation backend in C#.

| File | Purpose |
|------|---------|
| `EquipmentBooking.cs` | Problem statement + stub implementations + tests (Tasks 2–4 unimplemented) |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 1 — Bug Fix](#task-1--bug-fix-isavailable--makereservation)
5. [Task 2-1 — Reservations by Member](#task-2-1--getreservationsformember)
6. [Task 2-2 — Equipment Summary](#task-2-2--getequipmentsummary)
7. [Task 3 — Available Equipment with Turnaround](#task-3--getavailableequipment)
8. [Task 4 — Most Booked Equipment Ranking](#task-4--getmostbookedequipment)
9. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
10. [Interview Tips](#interview-tips)

---

## Problem Overview

`FacilityManager` is the central service that:

- Registers **equipment** (treadmills, rowing machines, etc.)
- Creates **reservations** for members over integer time windows (minutes from start of day)
- Answers **availability and analytics queries**

Each task builds on the previous one — from a simple status-filter bug to buffered availability and top-k ranking.

```
┌─────────────────┐     ┌──────────────────┐
│  AddEquipment() │────▶│  List<Equipment> │
└─────────────────┘     └──────────────────┘
                               │
┌─────────────────┐            │
│MakeReservation()│────────────┼──▶ List<Reservation>
└─────────────────┘            │
                               ▼
                    ┌──────────────────────┐
                    │ Query methods        │
                    │ (Tasks 1–4)          │
                    └──────────────────────┘
```

---

## Core Concepts

### 1. Enums (`ReservationStatus`)

Enums restrict values to a fixed set. They are type-safe alternatives to magic strings or integers.

```csharp
enum ReservationStatus { ACTIVE, CANCELLED }
```

**Why it matters:** Cancelled reservations must **not** block slots or appear in member/summary queries. Almost every method in this problem filters on `Status == ACTIVE`.

---

### 2. Interval Overlap Detection

Two time intervals `[startA, endA]` and `[startB, endB]` **overlap** when:

```csharp
startA < endB && endA > startB
```

This is the standard half-open interval test used in scheduling problems. Task 1 uses it in `IsAvailable`; Task 3 extends it with a 30-minute buffer on each side.

| Scenario | startA | endA | startB | endB | Overlap? |
|----------|--------|------|--------|------|----------|
| res1: 480–600, res2: 540–660 | 540 | 660 | 480 | 600 | Yes |
| res1: 480–600, res2: 660–720 | 660 | 720 | 480 | 600 | No (adjacent) |
| res1: 480–600, res2: 480–600 (different equipment) | — | — | — | — | No (different equipmentId) |

---

### 3. String Equality — `.Equals()` vs `==`

C# compares strings with `==` using **value equality**, but interview tests often use `new string("Alice Johnson".ToCharArray())` to force you to compare by **content**, not reference.

**Safe approach:** always use `memberName.Equals(res.MemberName)` (or `string.Equals(a, b)`).

```csharp
// WRONG for the test with new string(...)
if (res.MemberName == memberName)  // may fail on reference comparison in some contexts

// CORRECT
if (res.MemberName.Equals(memberName))
```

---

### 4. Dictionary Return Types with String Keys

`GetEquipmentSummary` returns `Dictionary<string, int>` with fixed keys:

| Key | Meaning |
|-----|---------|
| `"total_reservations"` | Count of **active** reservations for the equipment |
| `"total_minutes"` | Sum of `(EndTime - StartTime)` across active reservations |

Always populate **both** keys, even when values are 0.

---

### 5. Turnaround Buffer (Task 3)

The facility requires **30 minutes** between reservations on the same equipment. A requested window `[start, end]` is blocked if it falls within 30 minutes of any active reservation — before **or** after.

**Effective blocked zone** for reservation `[res.Start, res.End]`:

```
[res.Start - 30, res.End + 30]
```

Request is unavailable if it overlaps this expanded zone:

```csharp
startTime < res.EndTime + 30 && endTime > res.StartTime - 30
```

| Existing reservation | Request | Gap | Available? |
|---------------------|---------|-----|------------|
| 480–600 | 630–720 | 30 min after (600→630) | Yes (exactly 30) |
| 480–600 | 620–720 | 20 min after | No |
| 600–720 | 500–580 | 20 min before (580→600) | No |
| 600–720 | 500–570 | 30 min before (570→600) | Yes (exactly 30) |

---

### 6. Tie-Breaking Rules (Task 4)

| Field | Tie-break rule |
|-------|----------------|
| `GetMostBookedEquipment` ranking | Higher total minutes first; tie → **lower equipment ID** |

Equipment with **zero active minutes** is excluded entirely (not ranked as 0).

---

## Data Structures Used

### Domain Classes

```
Equipment
├── EquipmentId : int
└── Name        : string

Reservation
├── ReservationId : int
├── MemberName    : string
├── EquipmentId   : int
├── StartTime     : int   (minutes from day start)
├── EndTime       : int
├── Status        : ReservationStatus
└── GetDuration() : int   → EndTime - StartTime
```

---

## Task 1 — Bug Fix: `IsAvailable` / `MakeReservation`

### Requirement

- `MakeReservation` succeeds only when equipment is free for the requested window.
- Overlapping active reservations on the **same equipment** must block booking.
- **Cancelled** reservations must **not** block future bookings.

### The Bug

```csharp
public bool IsAvailable(int equipmentId, int startTime, int endTime)
{
    foreach (Reservation res in Reservations)
    {
        if (res.EquipmentId == equipmentId)   // BUG: ignores Status
        {
            if (startTime < res.EndTime && endTime > res.StartTime)
                return false;
        }
    }
    return true;
}
```

Cancelled reservations still participate in overlap checks, so `TestCancelledReservationFreesSlot` fails.

### Fix

Add an active-status filter:

```csharp
public bool IsAvailable(int equipmentId, int startTime, int endTime)
{
    foreach (Reservation res in Reservations)
    {
        if (res.EquipmentId == equipmentId
                && res.Status == ReservationStatus.ACTIVE)
        {
            if (startTime < res.EndTime && endTime > res.StartTime)
                return false;
        }
    }
    return true;
}
```

### Test Data Breakdown

| Test | What it verifies |
|------|------------------|
| `TestMakeReservation` | Overlap on same equipment → second booking rejected |
| `TestDifferentEquipmentNoConflict` | Same time window on different equipment → both succeed |
| `TestCancelledReservationFreesSlot` | After cancel, same slot is bookable again |

### Concept: Status as a Soft Delete

Cancellation sets `Status = CANCELLED` but keeps the object in the list. Queries must filter by `ACTIVE` — a common pattern in event-sourced or audit-friendly designs.

---

## Task 2-1 — `GetReservationsForMember(string memberName)`

### Requirement

Return all **active** reservations for the given member. Empty list if none. Cancelled reservations excluded.

### Solution

```csharp
public List<Reservation> GetReservationsForMember(string memberName)
{
    List<Reservation> result = new List<Reservation>();
    foreach (Reservation res in Reservations)
    {
        if (res.MemberName.Equals(memberName)
                && res.Status == ReservationStatus.ACTIVE)
        {
            result.Add(res);
        }
    }
    return result;
}
```

### LINQ equivalent

```csharp
public List<Reservation> GetReservationsForMember(string memberName)
{
    return Reservations
        .Where(r => r.MemberName.Equals(memberName)
                 && r.Status == ReservationStatus.ACTIVE)
        .ToList();
}
```

### Worked Example

```
Reservations:
  101 Alice, equip 1, ACTIVE
  102 Bob,   equip 1, ACTIVE
  103 Alice, equip 2, ACTIVE
  104 Carol, equip 2, ACTIVE

GetReservationsForMember("Alice Johnson") → [101, 103]

After CancelReservation(101):
GetReservationsForMember("Alice Johnson") → [103]
```

### Concepts Applied

1. **Filter by two conditions** — member name AND active status
2. **Value-based string match** — `.Equals()` handles the `new string(...)` test case
3. **Return type is list of domain objects** — tests use `.Contains(res1)` which relies on `Reservation.Equals` (compares `ReservationId`)

---

## Task 2-2 — `GetEquipmentSummary(int equipmentId)`

### Requirement

Return a map with:

- `"total_reservations"` — count of active reservations on that equipment
- `"total_minutes"` — sum of durations across active reservations

Both 0 when no active reservations exist. Cancelled reservations excluded.

### Solution

```csharp
public Dictionary<string, int> GetEquipmentSummary(int equipmentId)
{
    int totalReservations = 0;
    int totalMinutes = 0;

    foreach (Reservation res in Reservations)
    {
        if (res.EquipmentId == equipmentId
                && res.Status == ReservationStatus.ACTIVE)
        {
            totalReservations++;
            totalMinutes += res.GetDuration();
        }
    }

    Dictionary<string, int> result = new Dictionary<string, int>();
    result["total_reservations"] = totalReservations;
    result["total_minutes"] = totalMinutes;
    return result;
}
```

### LINQ equivalent

```csharp
public Dictionary<string, int> GetEquipmentSummary(int equipmentId)
{
    var active = Reservations
        .Where(r => r.EquipmentId == equipmentId
                 && r.Status == ReservationStatus.ACTIVE)
        .ToList();

    return new Dictionary<string, int>
    {
        ["total_reservations"] = active.Count,
        ["total_minutes"] = active.Sum(r => r.GetDuration())
    };
}
```

### Worked Example (Equipment 1)

| Reservation | Duration | Status |
|-------------|----------|--------|
| 101: 480–540 | 60 | ACTIVE |
| 102: 660–720 | 60 | ACTIVE |
| 103: 720–780 | 60 | ACTIVE |

**Result:** `{ total_reservations: 3, total_minutes: 180 }`

After cancelling 101: `{ total_reservations: 2, total_minutes: 120 }`

---

## Task 3 — `GetAvailableEquipment(int startTime, int endTime)`

### Requirement

Return a **sorted ascending** list of equipment IDs from `EquipmentList` that are fully available for `[startTime, endTime]`, including the 30-minute turnaround on either side of every active reservation.

Empty list if none qualify.

### Solution

```csharp
public List<int> GetAvailableEquipment(int startTime, int endTime)
{
    List<int> result = new List<int>();

    foreach (Equipment eq in EquipmentList)
    {
        if (IsAvailableWithTurnaround(eq.EquipmentId, startTime, endTime))
            result.Add(eq.EquipmentId);
    }

    result.Sort();
    return result;
}

private bool IsAvailableWithTurnaround(int equipmentId, int startTime, int endTime)
{
    foreach (Reservation res in Reservations)
    {
        if (res.EquipmentId != equipmentId
                || res.Status != ReservationStatus.ACTIVE)
            continue;

        int bufferStart = res.StartTime - 30;
        int bufferEnd = res.EndTime + 30;

        if (startTime < bufferEnd && endTime > bufferStart)
            return false;
    }
    return true;
}
```

You may also fold the turnaround logic into `IsAvailable` if the spec requires `MakeReservation` to enforce the same 30-minute rule — read the task wording carefully. The provided tests only call `GetAvailableEquipment` for turnaround checks.

### Worked Example (from tests)

```
Equipment inventory: [1, 2, 3, 4]
Active reservations:
  equip 1: 480–600
  equip 2: 300–420
  equip 3: 700–780
  equip 4: (none)

Request 630–720:
  equip 1: buffer [450,630] → request starts at 630 → OK (exact 30-min gap)
  equip 2: buffer [270,450] → OK
  equip 3: buffer [670,810] → overlaps 700–720 → blocked
  equip 4: no reservations → OK
  Result: [1, 2, 4]

Request 620–720:
  equip 1: buffer [450,630] → 620 < 630 → blocked (only 20-min gap)
  Result: [2, 4]
```

### Concepts Applied

1. **Buffer expansion** — extend each reservation by ±30 before overlap test
2. **Iterate equipment registry** — loop `EquipmentList`, not reservation groups
3. **Explicit sort** — `result.Sort()` for ascending ID order
4. **Cancelled reservations ignored** — same ACTIVE filter as Task 1

---

## Task 4 — `GetMostBookedEquipment(int n)`

### Requirement

Return top `n` equipment IDs ranked by total **active** booked minutes (descending). Tie → lower equipment ID first. Exclude equipment with no active reservations. If fewer than `n` qualify, return all qualifying equipment.

### Solution

```csharp
public List<int> GetMostBookedEquipment(int n)
{
    Dictionary<int, int> minutesByEquipment = new Dictionary<int, int>();

    foreach (Reservation res in Reservations)
    {
        if (res.Status != ReservationStatus.ACTIVE)
            continue;

        if (!minutesByEquipment.ContainsKey(res.EquipmentId))
            minutesByEquipment[res.EquipmentId] = 0;

        minutesByEquipment[res.EquipmentId] += res.GetDuration();
    }

    List<int> sorted = minutesByEquipment
        .OrderByDescending(kv => kv.Value)
        .ThenBy(kv => kv.Key)
        .Select(kv => kv.Key)
        .ToList();

    if (n >= sorted.Count)
        return sorted;

    return sorted.GetRange(0, n);
}
```

### LINQ one-liner style

```csharp
public List<int> GetMostBookedEquipment(int n)
{
    return Reservations
        .Where(r => r.Status == ReservationStatus.ACTIVE)
        .GroupBy(r => r.EquipmentId)
        .Select(g => new { Id = g.Key, Minutes = g.Sum(r => r.GetDuration()) })
        .OrderByDescending(x => x.Minutes)
        .ThenBy(x => x.Id)
        .Take(n)
        .Select(x => x.Id)
        .ToList();
}
```

### Worked Example

```
Active reservations:
  equip 1: 60 + 60 = 120 min
  equip 2: 90 min
  equip 3: cancelled → excluded

GetMostBookedEquipment(2) → [1, 2]

Tie case (equip 10 and 20 both 60 min):
GetMostBookedEquipment(2) → [10, 20]   (lower ID first)
```

### Concepts Applied

1. **GroupBy + Sum** — aggregate minutes per equipment
2. **Multi-key sort** — `OrderByDescending` then `ThenBy`
3. **Take(n)** — top-k pattern
4. **Exclude zero-activity equipment** — only equipment appearing in active reservations enter the ranking

---

## LINQ Cheat Sheet for This Problem

```
Filter:      .Where(r => r.Status == ReservationStatus.ACTIVE)
             .Where(r => r.EquipmentId == id)
             .Where(r => r.MemberName.Equals(name))
Group:       .GroupBy(r => r.EquipmentId)
Aggregate:   .Sum(r => r.GetDuration())
             .Count()
Sort:        .OrderByDescending(x => x.Minutes).ThenBy(x => x.Id)
Limit:       .Take(n)
Convert:     .ToList()
Manual sort: result.Sort()   // for List<int> of equipment IDs
```

### Equivalent Imperative Style (Task 4 without LINQ)

```csharp
var totals = new Dictionary<int, int>();
foreach (var r in Reservations)
{
    if (r.Status != ReservationStatus.ACTIVE) continue;
    if (!totals.ContainsKey(r.EquipmentId))
        totals[r.EquipmentId] = 0;
    totals[r.EquipmentId] += r.GetDuration();
}

var ids = new List<int>(totals.Keys);
ids.Sort((a, b) =>
{
    int cmp = totals[b].CompareTo(totals[a]);  // desc by minutes
    return cmp != 0 ? cmp : a.CompareTo(b);      // asc by id
});

return ids.Count <= n ? ids : ids.GetRange(0, n);
```

---

## Interview Tips

### Reading the Problem

1. **Underline "active"** — cancelled reservations are excluded in nearly every query
2. **Watch string comparisons** — use `.Equals()` for member name lookups
3. **Note buffer rules in Task 3** — 30 minutes applies **both before and after** existing reservations
4. **Check sort direction** — Task 3 ascending by ID; Task 4 descending by minutes, ascending by ID on tie

### Debugging Task 1

When a slot stays blocked after cancellation, ask: *"Am I still treating cancelled reservations as conflicts?"*

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Not filtering `ACTIVE` in `IsAvailable` | Add `&& res.Status == ReservationStatus.ACTIVE` |
| Using `==` for member name | Use `memberName.Equals(res.MemberName)` |
| Forgetting cancelled in summary/member queries | Filter `Status == ACTIVE` |
| Task 3: only checking direct overlap | Expand reservation by ±30 before overlap test |
| Task 3: not sorting result | Call `result.Sort()` before return |
| Task 4: including equipment with 0 minutes | Only aggregate from active reservations |
| Task 4: wrong tie-break | `.ThenBy(x => x.EquipmentId)` for lower ID |

### Complexity (for follow-up questions)

For `e` equipment, `r` reservations:

| Method | Time | Space |
|--------|------|-------|
| Task 1 `IsAvailable` | O(r) | O(1) |
| Task 2-1 | O(r) | O(r) |
| Task 2-2 | O(r) | O(1) |
| Task 3 | O(e × r) | O(e) |
| Task 4 | O(r log r) | O(r) |

All are fine for typical interview data sizes.

---

## Quick Reference — All Solutions

```csharp
// TASK 1 — only ACTIVE reservations block slots
if (res.EquipmentId == equipmentId && res.Status == ReservationStatus.ACTIVE)
{
    if (startTime < res.EndTime && endTime > res.StartTime)
        return false;
}

// TASK 2-1 — member lookup
if (res.MemberName.Equals(memberName) && res.Status == ReservationStatus.ACTIVE)
    result.Add(res);

// TASK 2-2 — equipment summary
result["total_reservations"] = activeCount;
result["total_minutes"] = totalMinutes;

// TASK 3 — 30-min turnaround buffer
if (startTime < res.EndTime + 30 && endTime > res.StartTime - 30)
    return false;  // per equipment

// TASK 4 — top-n by booked minutes
.OrderByDescending(x => x.Minutes).ThenBy(x => x.Id).Take(n)
```

Implement these in `FacilityManager` inside `EquipmentBooking.cs` and run:

```bash
dotnet run --project "05. EquipmentBooking"
```

All seven tests should report **PASS**.
