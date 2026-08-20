# Hotel — Solution & Concepts Guide

Interview problem: build and extend a hotel room booking management backend in C#.

| File | Purpose |
|------|---------|
| `../Hotel.cs` | Original practice file (root copy — not edited) |
| `Hotel.cs` | Runnable project copy (Tasks 2–4 left as TODO) |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 1 — Bug Fix](#task-1--bug-fix-gettotalrevenue)
5. [Task 2 — Average Stay Duration by Room Type](#task-2--getaveragestaydurationbyroomtype)
6. [Task 3 — Guest Booking Summary](#task-3--getguestbookingsummary)
7. [Task 4 — Top Spenders](#task-4--gettopspenders)
8. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
9. [Interview Tips](#interview-tips)

---

## Problem Overview

`HotelManager` is the central service that:

- Registers **guests** (by `guestId`)
- Registers **rooms** (by `roomId`, with type and nightly price)
- Records **bookings** (only for known guests and rooms)
- Computes **revenue, stay analytics, and spending rankings**

Each task builds on the previous one — from a simple status-filter bug fix to multi-level grouping and top-k ranking.

```
┌─────────────────┐     ┌──────────────────┐
│  AddGuest()     │────▶│  Dictionary<int, │
│                 │     │  Guest>          │
└─────────────────┘     └──────────────────┘
┌─────────────────┐     ┌──────────────────┐
│  AddRoom()      │────▶│  Dictionary<int, │
│                 │     │  Room>           │
└─────────────────┘     └──────────────────┘
                               │
┌─────────────────┐            │
│  AddBooking()   │────────────┼──▶ List<Booking>
└─────────────────┘            │
                               ▼
                    ┌──────────────────────┐
                    │ Analytics methods    │
                    │ (Tasks 1–4)          │
                    └──────────────────────┘
```

---

## Core Concepts

### 1. Enums (`RoomType`, `BookingStatus`)

Enums restrict values to a fixed set. They are type-safe alternatives to magic strings or integers.

```csharp
enum RoomType      { DELUXE, STANDARD, SUITE }
enum BookingStatus { CONFIRMED, CHECKED_IN, CHECKED_OUT, CANCELLED }
```

**Why it matters:** Filtering (`status == CHECKED_OUT`) and grouping (`GroupBy` on room type) rely on enum equality. Enum names sort alphabetically when you call `.ToString()` — used for tie-breaking in Task 3.

| Enum name order (alphabetical) | Value |
|-------------------------------|-------|
| DELUXE | D comes first |
| STANDARD | S comes second |
| SUITE | U comes last |

---

### 2. Nullable Types (`RoomType?`)

Task 3 requires `favoriteRoomType` to be `null` when a guest has no bookings.

```csharp
public RoomType? favoriteRoomType;  // can hold a value OR null
```

A **nullable value type** (`T?`) wraps value types (like enums and `int`) so they can represent "no value."

---

### 3. Dictionary vs List — When to Use Which

| Structure | Use case in this problem |
|-----------|--------------------------|
| `Dictionary<int, Guest>` | O(1) lookup by `guestId`; ensures one entry per guest |
| `Dictionary<int, Room>` | O(1) lookup by `roomId`; provides `roomType` and `pricePerNight` |
| `List<Booking>` | Ordered collection of all bookings; iterate/filter/group |
| `Dictionary<K, V>` (return type) | Map a key (guestId or RoomType) to a computed value |

**Key insight:** Guests and rooms live in dictionaries (master registries). Bookings live in a list (transaction log). Analytics methods **query the list** and often **join through `rooms`** to get room type or price.

---

### 4. Joining Bookings to Rooms

Unlike ClinicManager (where appointment data is self-contained), hotel bookings reference rooms by `roomId`:

```csharp
rooms[b.roomId].roomType       // get room type for a booking
rooms[b.roomId].pricePerNight  // get price for revenue/spend calculations
```

When grouping by room type, use:

```csharp
.GroupBy(b => rooms[b.roomId].roomType)
```

---

### 5. LINQ (Language Integrated Query)

LINQ provides declarative operations over collections:

| Method | What it does |
|--------|--------------|
| `Where` | Filter (like SQL `WHERE`) |
| `GroupBy` | Partition into buckets by a key |
| `Select` | Project/transform each element |
| `OrderBy` / `OrderByDescending` | Sort ascending / descending |
| `ThenBy` / `ThenByDescending` | Secondary sort (tiebreaker) |
| `Sum`, `Average`, `Count` | Aggregations |
| `First` | Take the first element after sorting |
| `Take(n)` | Limit to top n results |
| `ToDictionary` | Materialize key-value pairs |
| `ToList` | Materialize a list |

---

### 6. Guard Clauses & Edge Cases

| Condition | Expected behavior |
|-----------|-------------------|
| Unknown `guestId` (Task 2) | Return empty map |
| Guest with no CHECKED_OUT bookings (Task 2) | Return empty map |
| Guest with no bookings (Task 3) | `{ 0, 0, null }` — still included in result |
| Guest with no CHECKED_OUT bookings (Task 4) | Spend = 0; still included when `n` is large enough |
| `n` larger than guest count (Task 4) | Return all guests, sorted |

Always handle edge cases **before** the main logic where appropriate.

---

### 7. Tie-Breaking Rules (Critical for Tasks 3 & 4)

| Task | Field | Tie-break rule |
|------|-------|----------------|
| Task 3 | `favoriteRoomType` | **Alphabetical** on enum name (`ToString()`) |
| Task 4 | Guest ranking | **Smaller** `guestId` when spend is tied |

Read tie-break rules carefully — they are a common interview trap.

---

## Data Structures Used

### Domain Classes

```
Guest
├── guestId : int
└── name    : string

Room
├── roomId        : int
├── roomType      : RoomType
└── pricePerNight : int

Booking
├── bookingId : int
├── guestId   : int
├── roomId    : int
├── status    : BookingStatus
└── nights    : int

GuestBookingSummary (Task 3 output)
├── totalBookings    : int
├── totalNights      : int
└── favoriteRoomType : RoomType?
```

---

## Task 1 — Bug Fix: `GetTotalRevenue()`

### Requirement

Return total revenue across all bookings **except cancelled ones**.

Revenue per booking = `room.pricePerNight × booking.nights`.

### The Bug

The comment says "non-cancelled bookings" but the code sums **every** booking:

```csharp
public int GetTotalRevenue() {
    int total = 0;
    foreach (Booking b in bookings) {
        total += rooms[b.roomId].pricePerNight * b.nights;  // BUG: no status check
    }
    return total;
}
```

This includes the CANCELLED booking ($500), producing 1400 instead of 900.

### Fix

```csharp
public int GetTotalRevenue() {
    int total = 0;
    foreach (Booking b in bookings) {
        if (b.status == BookingStatus.CANCELLED)
            continue;
        total += rooms[b.roomId].pricePerNight * b.nights;
    }
    return total;
}
```

### Test Data Breakdown

| Booking | Guest | Room | Status | Nights | Price/night | Revenue | Included? |
|---------|-------|------|--------|--------|-------------|---------|-----------|
| 1 | 1 | STANDARD | CONFIRMED | 3 | $100 | $300 | ✓ |
| 2 | 1 | DELUXE | CHECKED_OUT | 2 | $200 | $400 | ✓ |
| 3 | 2 | STANDARD | CANCELLED | 5 | $100 | $500 | ✗ |
| 4 | 2 | DELUXE | CHECKED_IN | 1 | $200 | $200 | ✓ |

**Expected:** $300 + $400 + $200 = **900**

### Concept: Comment vs Code Mismatch

When a test fails on a "bug fix" task, compare the **comment/spec** to the **actual filter logic**. Here the comment correctly describes the rule; the loop body does not enforce it.

---

## Task 2 — `GetAverageStayDurationByRoomType(int guestId)`

### Requirement

For a given guest, return a map of `RoomType → average nights`.

**Rules:**
- Only `CHECKED_OUT` bookings count
- Only room types with at least one CHECKED_OUT booking appear in the map
- Unknown guestId or no CHECKED_OUT bookings → empty map

### Solution

```csharp
public Dictionary<RoomType, double> GetAverageStayDurationByRoomType(int guestId)
{
    if (!guests.ContainsKey(guestId))
        return new Dictionary<RoomType, double>();

    return bookings
        .Where(b => b.guestId == guestId && b.status == BookingStatus.CHECKED_OUT)
        .GroupBy(b => rooms[b.roomId].roomType)
        .ToDictionary(g => g.Key, g => g.Average(b => (double)b.nights));
}
```

### Step-by-Step (Guest 1)

```
CHECKED_OUT bookings for guest 1:
  room 1 (STANDARD) → 3 nights  ✓
  room 1 (STANDARD) → 5 nights  ✓
  room 2 (DELUXE)   → 2 nights  ✓
  room 3 (SUITE)    → CONFIRMED  ✗ (filtered out)

After GroupBy:
  STANDARD → [3, 5]  → average = 4.0
  DELUXE   → [2]     → average = 2.0

Result: { STANDARD: 4.0, DELUXE: 2.0 }
```

### Concepts Applied

1. **Guard for unknown guest** — check `guests.ContainsKey` first
2. **Filter before aggregate** — `Where` narrows to CHECKED_OUT only
3. **Join via dictionary** — `rooms[b.roomId].roomType` resolves room type
4. **Average** — cast to `double` for precision

---

## Task 3 — `GetGuestBookingSummary()`

### Requirement

For **every registered guest**, return:

| Field | Rule |
|-------|------|
| `totalBookings` | Count of all bookings (any status) |
| `totalNights` | Sum of `nights` (any status) |
| `favoriteRoomType` | Most frequent room type; tie → alphabetical; no bookings → `null` |

### Solution

```csharp
public Dictionary<int, GuestBookingSummary> GetGuestBookingSummary()
{
    var result = new Dictionary<int, GuestBookingSummary>();

    foreach (Guest guest in guests.Values)
    {
        List<Booking> guestBookings = bookings
            .Where(b => b.guestId == guest.guestId)
            .ToList();

        if (guestBookings.Count == 0)
        {
            result[guest.guestId] = new GuestBookingSummary(0, 0, null);
            continue;
        }

        int totalBookings = guestBookings.Count;
        int totalNights = guestBookings.Sum(b => b.nights);

        RoomType favorite = guestBookings
            .GroupBy(b => rooms[b.roomId].roomType)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key.ToString())
            .First()
            .Key;

        result[guest.guestId] = new GuestBookingSummary(totalBookings, totalNights, favorite);
    }

    return result;
}
```

### Why Iterate Over `guests`, Not `bookings`?

If you only grouped bookings, guests with **zero** bookings would be missing from the result. The spec requires every registered guest to appear with `{ 0, 0, null }`.

### Tie-Break Example (Guest 2)

```
DELUXE × 1
SUITE  × 1   ← tie on count

Alphabetical: "DELUXE" < "SUITE"  →  DELUXE wins
```

### Worked Example (Guest 1)

| Booking | Room Type | Status | Nights |
|---------|-----------|--------|--------|
| 1 | STANDARD | CHECKED_OUT | 3 |
| 2 | STANDARD | CONFIRMED | 5 |
| 3 | DELUXE | CHECKED_OUT | 2 |
| 4 | SUITE | CANCELLED | 1 |

- `totalBookings` = 4 (all statuses)
- `totalNights` = 3 + 5 + 2 + 1 = 11
- Room type counts: STANDARD × 2, DELUXE × 1, SUITE × 1 → **STANDARD** wins

---

## Task 4 — `GetTopSpenders(int n)`

### Requirement

Return top `n` guest IDs ranked by total spend.

**Rules:**
- Only `CHECKED_OUT` bookings count toward spend
- Spend per booking = `room.pricePerNight × booking.nights`
- Sort by spend descending; tie → **smaller guestId**
- Guests with no CHECKED_OUT bookings have spend = 0 and are included

### Solution

```csharp
public List<int> GetTopSpenders(int n)
{
    return guests.Values
        .Select(g => new
        {
            GuestId = g.guestId,
            Spend = bookings
                .Where(b => b.guestId == g.guestId && b.status == BookingStatus.CHECKED_OUT)
                .Sum(b => rooms[b.roomId].pricePerNight * b.nights)
        })
        .OrderByDescending(x => x.Spend)
        .ThenBy(x => x.GuestId)
        .Take(n)
        .Select(x => x.GuestId)
        .ToList();
}
```

### Worked Example (Main Test, n=4)

| Guest | CHECKED_OUT spend | Included in top 4? |
|-------|-------------------|--------------------|
| 1 | $300 + $300 = $600 | ✓ (rank 2) |
| 2 | $900 | ✓ (rank 1) |
| 3 | $0 (CHECKED_IN only) | ✓ (rank 3) |
| 4 | $0 (no bookings) | ✓ (rank 4) |

**Result:** `[2, 1, 3, 4]`

### Tie-Break Example (hm2)

Both guests spend $500 → lower guestId first → `[10, 20]`

### Concepts Applied

1. **Iterate all guests** — zero-spend guests must appear when `n` is large enough
2. **Filter by status for spend** — only CHECKED_OUT counts (unlike Task 1 revenue)
3. **Top-n pattern** — `OrderByDescending` → `ThenBy` → `Take(n)`

---

## LINQ Cheat Sheet for This Problem

```
Filter:      .Where(b => b.guestId == id && b.status == CHECKED_OUT)
Join:        rooms[b.roomId].roomType / .pricePerNight
Group:       .GroupBy(b => rooms[b.roomId].roomType)
Aggregate:   .Sum(b => b.nights)
             .Sum(b => rooms[b.roomId].pricePerNight * b.nights)
             .Average(b => (double)b.nights)
             .Count()                    // on a group
Sort:        .OrderByDescending(g => g.Count()).ThenBy(g => g.Key.ToString())
             .OrderByDescending(x => x.Spend).ThenBy(x => x.GuestId)
First:       .First()                    // after sort = "pick winner"
Limit:       .Take(n)
Convert:     .ToDictionary(g => g.Key, g => g.Average(...))
             .ToList()
```

### Equivalent Imperative Style (Task 2 without LINQ)

```csharp
if (!guests.ContainsKey(guestId))
    return new Dictionary<RoomType, double>();

var sums = new Dictionary<RoomType, int>();
var counts = new Dictionary<RoomType, int>();

foreach (Booking b in bookings)
{
    if (b.guestId != guestId || b.status != BookingStatus.CHECKED_OUT)
        continue;

    RoomType type = rooms[b.roomId].roomType;
    if (!sums.ContainsKey(type))
    {
        sums[type] = 0;
        counts[type] = 0;
    }
    sums[type] += b.nights;
    counts[type]++;
}

var result = new Dictionary<RoomType, double>();
foreach (RoomType type in sums.Keys)
    result[type] = (double)sums[type] / counts[type];

return result;
```

---

## Interview Tips

### Reading the Problem

1. **Underline filter conditions** — "only CHECKED_OUT" vs "all statuses" changes every task
2. **Note tie-break rules** — alphabetical (Task 3) vs smaller guestId (Task 4)
3. **Check who must appear in output** — all guests (Tasks 3 & 4) vs filtered subsets (Task 2)

### Debugging Task 1

When revenue is too high, ask: *"Am I including bookings I should exclude?"* The comment says non-cancelled; the code includes cancelled.

### Task 1 vs Task 4 — Status Filtering Difference

| Method | Status rule |
|--------|---------------|
| `GetTotalRevenue()` | Exclude **CANCELLED** only; CONFIRMED and CHECKED_IN count |
| `GetTopSpenders()` | Include **CHECKED_OUT** only |

Do not assume the same filter applies across tasks.

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Forgetting to exclude CANCELLED in Task 1 | Add `if (b.status == CANCELLED) continue;` |
| Forgetting CHECKED_OUT filter in Task 2/4 | Add `&& b.status == CHECKED_OUT` in `Where` |
| Missing guests with 0 bookings in Task 3 | Loop `guests.Values`, not booking groups |
| Wrong tie-break in Task 3 | Use `.ThenBy(g => g.Key.ToString())` for alphabetical |
| Wrong tie-break in Task 4 | `.ThenBy(x => x.GuestId)` for smaller id |
| Forgetting room lookup | Use `rooms[b.roomId].roomType` / `.pricePerNight` |
| Not handling unknown guestId in Task 2 | Return empty map early |

### Complexity (for follow-up questions)

For `n` bookings and `g` guests:

| Method | Time | Space |
|--------|------|-------|
| Task 1 | O(n) | O(1) |
| Task 2 | O(n) | O(t) room types |
| Task 3 | O(g × n) | O(g) |
| Task 4 | O(g × n + g log g) | O(g) |

All are fine for typical interview data sizes.

---

## Quick Reference — All Four Solutions

```csharp
// TASK 1 — exclude cancelled bookings
if (b.status == BookingStatus.CANCELLED) continue;

// TASK 2 — average nights by room type for a guest
if (!guests.ContainsKey(guestId)) return new Dictionary<RoomType, double>();
return bookings
    .Where(b => b.guestId == guestId && b.status == BookingStatus.CHECKED_OUT)
    .GroupBy(b => rooms[b.roomId].roomType)
    .ToDictionary(g => g.Key, g => g.Average(b => (double)b.nights));

// TASK 3 — per-guest summary (iterate guests!)
foreach (Guest guest in guests.Values) { ... }

// TASK 4 — top-n spenders
return guests.Values
    .Select(g => new { GuestId = g.guestId, Spend = ... })
    .OrderByDescending(x => x.Spend).ThenBy(x => x.GuestId)
    .Take(n).Select(x => x.GuestId).ToList();
```

Implement these in `Hotel.cs` and run `dotnet run` from the `10. Hotel` folder to verify all tests pass.
