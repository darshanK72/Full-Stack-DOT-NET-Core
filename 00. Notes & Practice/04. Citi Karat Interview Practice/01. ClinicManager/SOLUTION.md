# ClinicManager — Solution & Concepts Guide

Interview problem: build and extend a clinic appointment management backend in C#.

| File | Purpose |
|------|---------|
| `ClinicManager.Problem.cs` | Original problem statement + tests (Tasks 2–4 skipped) |
| `ClinicManager.Answer.cs` | Complete solution with all tests enabled |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 1 — Bug Fix](#task-1--bug-fix-getappointmentstatistics)
5. [Task 2 — Average Duration by Type](#task-2--getaverageappointmentdurationbytype)
6. [Task 3 — Doctor Appointment Summary](#task-3--getdoctorappointmentsummary)
7. [Task 4 — Doctor Workload Ranking](#task-4--getdoctorworkloadranking)
8. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
9. [Interview Tips](#interview-tips)

---

## Problem Overview

`ClinicManager` is the central service that:

- Registers **doctors** (by `doctorId`)
- Books **appointments** (only for known doctors)
- Computes **statistics and rankings** over appointment data

Each task builds on the previous one — from a simple bug fix to multi-level grouping and sorting.

```
┌─────────────────┐     ┌──────────────────┐
│  AddDoctor()    │────▶│  Dictionary<int, │
│                 │     │  Doctor>         │
└─────────────────┘     └──────────────────┘
                               │
┌─────────────────┐            │
│ AddAppointment()│────────────┼──▶ List<Appointment>
└─────────────────┘            │
                               ▼
                    ┌──────────────────────┐
                    │ Analytics methods    │
                    │ (Tasks 1–4)          │
                    └──────────────────────┘
```

---

## Core Concepts

### 1. Enums (`AppointmentStatus`, `AppointmentType`)

Enums restrict values to a fixed set. They are type-safe alternatives to magic strings or integers.

```csharp
enum AppointmentStatus { SCHEDULED, COMPLETED, CANCELLED, NO_SHOW }
enum AppointmentType    { CONSULTATION, FOLLOWUP, EMERGENCY }
```

**Why it matters:** Filtering (`status == COMPLETED`) and grouping (`GroupBy(a => a.appointmentType)`) rely on enum equality. Enum names sort alphabetically when you call `.ToString()` — used for tie-breaking in Task 3.

| Enum name order (alphabetical) | Value |
|-------------------------------|-------|
| CONSULTATION | C comes first |
| EMERGENCY | E comes second |
| FOLLOWUP | F comes last |

---

### 2. Nullable Types (`AppointmentType?`)

Task 3 requires `busiestAppointmentType` to be `null` when a doctor has no appointments.

```csharp
public AppointmentType? busiestAppointmentType;  // can hold a value OR null
```

A **nullable value type** (`T?`) wraps value types (like enums and `int`) so they can represent "no value."

---

### 3. Dictionary vs List — When to Use Which

| Structure | Use case in this problem |
|-----------|--------------------------|
| `Dictionary<int, Doctor>` | O(1) lookup by `doctorId`; ensures one entry per doctor |
| `List<Appointment>` | Ordered collection of all appointments; iterate/filter/group |
| `Dictionary<K, V>` (return type) | Map a key (doctorId or AppointmentType) to a computed value |

**Key insight:** Doctors live in a dictionary (master registry). Appointments live in a list (transaction log). Analytics methods **query the list** and **key results by doctor or type**.

---

### 4. LINQ (Language Integrated Query)

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
| `Take(k)` | Limit to top k results |
| `ToDictionary` | Materialize key-value pairs |
| `ToList` | Materialize a list |

LINQ can be written as **method syntax** (used in this solution) or **query syntax** (`from x in ... where ... select ...`).

---

### 5. Guard Clauses & Edge Cases

Several tasks have explicit edge-case rules:

| Condition | Expected behavior |
|-----------|-------------------|
| `k <= 0` (Task 4) | Return empty list |
| Doctor with no appointments (Task 2) | Return empty map |
| Unknown `doctorId` (Task 2) | Return empty map (no matching appointments) |
| Doctor with no appointments (Task 3) | `{ 0, 0, null }` — still included in result |
| `total == 0` (Task 1) | `noShowRate = 0.0` (avoid division by zero) |

Always handle edge cases **before** the main logic (`if (k <= 0) return ...`).

---

### 6. Tie-Breaking Rules (Critical for Tasks 3 & 4)

Different tasks use **different** tie-break strategies:

| Task | Field | Tie-break rule |
|------|-------|----------------|
| Task 3 | `busiestAppointmentType` | **Alphabetical** on enum name (`ToString()`) |
| Task 4 | `mostFrequentPatientId` | **Smaller** `patientId` |
| Task 4 | Doctor ranking | **Smaller** `doctorId` when `totalMinutes` tied |

Read tie-break rules carefully — they are a common interview trap.

---

## Data Structures Used

### Domain Classes

```
Doctor
├── doctorId : int
└── name     : string

Appointment
├── appointmentId    : int
├── doctorId         : int
├── patientId        : int
├── durationMinutes  : int
├── status           : AppointmentStatus
└── appointmentType  : AppointmentType

AppointmentStats (Task 1 output)
├── totalAppointments     : int
├── completedAppointments : int
└── noShowRate            : double

DoctorAppointmentSummary (Task 3 output)
├── totalAppointments      : int
├── totalMinutes           : int
└── busiestAppointmentType : AppointmentType?
```

---

## Task 1 — Bug Fix: `GetAppointmentStatistics()`

### Requirement

Return global appointment statistics:

- `totalAppointments` — count of **all** appointments
- `completedAppointments` — count where `status == COMPLETED`
- `noShowRate` — `noShowCount / totalAppointments` (0.0 if total is 0)

### The Bug

```csharp
// BUG: increments for EVERY appointment, ignoring status
foreach (Appointment a in appointments)
{
    completed++;
}
```

This treats all 5 test appointments as completed, but only 2 have `status == COMPLETED`.

### Fix

```csharp
foreach (Appointment a in appointments)
{
    if (a.status == AppointmentStatus.COMPLETED)
        completed++;
}
```

### Test Data Breakdown

| ID | Status | Counts toward |
|----|--------|---------------|
| 1 | COMPLETED | total, completed |
| 2 | COMPLETED | total, completed |
| 3 | NO_SHOW | total, noShow |
| 4 | CANCELLED | total only |
| 5 | SCHEDULED | total only |

**Expected:** `total=5`, `completed=2`, `noShowRate=1/5=0.2`

### Concept: Integer Division vs Floating Point

```csharp
double noShowRate = total > 0 ? (double)noShows / total : 0.0;
```

Casting `(double)noShows` before division ensures floating-point division (`0.2`), not integer division (`0`).

---

## Task 2 — `GetAverageAppointmentDurationByType(int doctorId)`

### Requirement

For a given doctor, return a map of `AppointmentType → average duration (minutes)`.

**Rules:**
- Only `COMPLETED` appointments count
- Only types with at least one completed appointment appear in the map
- No completed appointments → empty map

### Solution

```csharp
public Dictionary<AppointmentType, double> GetAverageAppointmentDurationByType(int doctorId)
{
    return appointments
        .Where(a => a.doctorId == doctorId && a.status == AppointmentStatus.COMPLETED)
        .GroupBy(a => a.appointmentType)
        .ToDictionary(g => g.Key, g => g.Average(a => (double)a.durationMinutes));
}
```

### Step-by-Step (Doctor 1)

```
Appointments for doctor 1:
  CONSULTATION, 30 min, COMPLETED  ✓
  CONSULTATION, 41 min, COMPLETED  ✓
  FOLLOWUP,     20 min, COMPLETED  ✓
  CONSULTATION, 100 min, CANCELLED ✗ (filtered out)

After GroupBy:
  CONSULTATION → [30, 41]  → average = 35.5
  FOLLOWUP     → [20]      → average = 20.0

Result: { CONSULTATION: 35.5, FOLLOWUP: 20.0 }
```

### Concepts Applied

1. **Filter before aggregate** — `Where` narrows the dataset first
2. **GroupBy** — splits data into buckets by `appointmentType`
3. **Average** — LINQ computes mean; cast to `double` for precision
4. **ToDictionary** — converts `IGrouping` into `Dictionary<AppointmentType, double>`

---

## Task 3 — `GetDoctorAppointmentSummary()`

### Requirement

For **every registered doctor**, return:

| Field | Rule |
|-------|------|
| `totalAppointments` | Count of all appointments (any status) |
| `totalMinutes` | Sum of `durationMinutes` |
| `busiestAppointmentType` | Most frequent type; tie → alphabetical; no appointments → `null` |

### Solution

```csharp
public Dictionary<int, DoctorAppointmentSummary> GetDoctorAppointmentSummary()
{
    var result = new Dictionary<int, DoctorAppointmentSummary>();

    foreach (Doctor doctor in doctors.Values)
    {
        List<Appointment> doctorAppts = appointments
            .Where(a => a.doctorId == doctor.doctorId)
            .ToList();

        if (doctorAppts.Count == 0)
        {
            result[doctor.doctorId] = new DoctorAppointmentSummary(0, 0, null);
            continue;
        }

        int totalAppts = doctorAppts.Count;
        int totalMins = doctorAppts.Sum(a => a.durationMinutes);

        AppointmentType busiest = doctorAppts
            .GroupBy(a => a.appointmentType)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key.ToString())
            .First()
            .Key;

        result[doctor.doctorId] = new DoctorAppointmentSummary(totalAppts, totalMins, busiest);
    }

    return result;
}
```

### Why Iterate Over `doctors`, Not `appointments`?

If you only grouped appointments, doctors with **zero** appointments would be missing from the result. The spec requires every registered doctor to appear with `{ 0, 0, null }`.

### Tie-Break Example (Doctor 2)

```
FOLLOWUP  × 1
EMERGENCY × 1   ← tie on count

Alphabetical: "EMERGENCY" < "FOLLOWUP"  →  EMERGENCY wins
```

### Tie-Break Example (Doctor 4)

```
CONSULTATION × 1  ← wins (C < F)
FOLLOWUP     × 1
```

### Concepts Applied

1. **Master entity iteration** — loop the registry (`doctors`), not derived data
2. **Multi-key sorting** — `OrderByDescending` then `ThenBy` for tie-breaks
3. **Nullable return** — `null` for doctors with no data

---

## Task 4 — `GetDoctorWorkloadRanking(int k)`

### Requirement

Return top `k` doctors ranked by workload. Each entry: `{ doctorId, totalMinutes, mostFrequentPatientId }`.

**Rules:**
- Only doctors with at least one appointment
- `totalMinutes` = sum of all `durationMinutes` (any status)
- `mostFrequentPatientId` = patient with most appointments; tie → **smaller patientId**
- Sort by `totalMinutes` descending; tie → **smaller doctorId**
- `k <= 0` → empty list

### Solution

```csharp
public List<int[]> GetDoctorWorkloadRanking(int k)
{
    if (k <= 0)
        return new List<int[]>();

    return appointments
        .GroupBy(a => a.doctorId)
        .Select(g => new
        {
            DoctorId = g.Key,
            TotalMinutes = g.Sum(a => a.durationMinutes),
            MostFrequentPatientId = g
                .GroupBy(a => a.patientId)
                .OrderByDescending(pg => pg.Count())
                .ThenBy(pg => pg.Key)
                .First()
                .Key
        })
        .OrderByDescending(x => x.TotalMinutes)
        .ThenBy(x => x.DoctorId)
        .Take(k)
        .Select(x => new int[] { x.DoctorId, x.TotalMinutes, x.MostFrequentPatientId })
        .ToList();
}
```

### Worked Example (Test Case 2, k=3)

| Doctor | Appointments | Total Minutes | Top Patient |
|--------|-------------|---------------|-------------|
| 1 | 3 | 90 | 100 (×2) |
| 2 | 3 | 90 | 102 (×2) |
| 3 | 2 | 90 | 104 (104 vs 105 tied → 104) |
| 4 | 1 | 30 | 106 |

Sort by minutes desc → doctors 1, 2, 3 all have 90 → tiebreak by doctorId asc → **1, 2, 3**. Doctor 4 excluded by `Take(3)`.

**Result:**
```
[1, 90, 100]
[2, 90, 102]
[3, 90, 104]
```

### Concepts Applied

1. **Nested GroupBy** — group by doctor, then by patient within each doctor
2. **Anonymous types** — `new { DoctorId, TotalMinutes, ... }` for intermediate projection
3. **Take(k)** — top-k pattern (like SQL `LIMIT`)
4. **Guard clause** — early return for invalid `k`

---

## LINQ Cheat Sheet for This Problem

```
Filter:      .Where(a => a.doctorId == id && a.status == COMPLETED)
Group:       .GroupBy(a => a.appointmentType)
Aggregate:   .Sum(a => a.durationMinutes)
             .Average(a => (double)a.durationMinutes)
             .Count()                    // on a group
Sort:        .OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
First:       .First()                    // after sort = "pick winner"
Limit:       .Take(k)
Convert:     .ToDictionary(g => g.Key, g => g.Average(...))
             .ToList()
```

### Equivalent Imperative Style (Task 2 without LINQ)

```csharp
var sums = new Dictionary<AppointmentType, int>();
var counts = new Dictionary<AppointmentType, int>();

foreach (var a in appointments)
{
    if (a.doctorId != doctorId || a.status != AppointmentStatus.COMPLETED)
        continue;

    if (!sums.ContainsKey(a.appointmentType))
    {
        sums[a.appointmentType] = 0;
        counts[a.appointmentType] = 0;
    }
    sums[a.appointmentType] += a.durationMinutes;
    counts[a.appointmentType]++;
}

var result = new Dictionary<AppointmentType, double>();
foreach (var type in sums.Keys)
    result[type] = (double)sums[type] / counts[type];

return result;
```

LINQ is shorter and less error-prone; imperative style shows you understand what's happening under the hood — useful in interviews.

---

## Interview Tips

### Reading the Problem

1. **Underline filter conditions** — "only COMPLETED" vs "all statuses" changes every task
2. **Note tie-break rules** — they differ between tasks (alphabetical vs numeric min)
3. **Check who must appear in output** — all doctors (Task 3) vs only doctors with appointments (Task 4)

### Debugging Task 1

When a count is wrong, ask: *"Am I counting the right subset?"* The bug was counting all items instead of filtering by status.

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Forgetting to filter by `COMPLETED` in Task 2 | Add `&& a.status == COMPLETED` in `Where` |
| Missing doctors with 0 appointments in Task 3 | Loop `doctors.Values`, not appointment groups |
| Wrong tie-break in Task 3 | Use `.ThenBy(g => g.Key.ToString())` for alphabetical |
| Wrong tie-break in Task 4 | `.ThenBy(pg => pg.Key)` for smaller patientId |
| Integer division for noShowRate | Cast to `(double)` before dividing |
| Not handling `k <= 0` | Guard clause at the start of Task 4 |

### Complexity (for follow-up questions)

For `n` appointments and `d` doctors:

| Method | Time | Space |
|--------|------|-------|
| Task 1 | O(n) | O(1) |
| Task 2 | O(n) | O(t) types |
| Task 3 | O(d × n) | O(d) |
| Task 4 | O(n log d) | O(d) |

All are fine for typical interview data sizes.

---

## Quick Reference — All Four Solutions

```csharp
// TASK 1 — fix completed count
if (a.status == AppointmentStatus.COMPLETED) completed++;

// TASK 2 — average duration by type
return appointments
    .Where(a => a.doctorId == doctorId && a.status == AppointmentStatus.COMPLETED)
    .GroupBy(a => a.appointmentType)
    .ToDictionary(g => g.Key, g => g.Average(a => (double)a.durationMinutes));

// TASK 3 — per-doctor summary (iterate doctors!)
foreach (Doctor doctor in doctors.Values) { ... }

// TASK 4 — top-k workload ranking
if (k <= 0) return new List<int[]>();
return appointments.GroupBy(a => a.doctorId).Select(...).OrderByDescending(...).Take(k).ToList();
```

See `ClinicManager.Answer.cs` for the full runnable implementation with all tests.
