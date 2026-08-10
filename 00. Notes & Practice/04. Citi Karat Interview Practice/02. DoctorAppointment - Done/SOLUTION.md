# DoctorAppointment — Solution & Concepts Guide

Interview problem: build and extend a clinic doctor roster and patient visit management backend in C#.

| File | Purpose |
|------|---------|
| `../DoctorAppointment.cs` | Original practice file (entry point is `Main2` — not runnable as-is) |
| `DoctorAppointment.cs` | Runnable project copy (`Main2` renamed to `Main`; Tasks 2–4 left as TODO) |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 1 — Bug Fix: `Size()`](#task-1--bug-fix-size)
5. [Task 2 — Add Patient Visit](#task-2--addpatientvisit)
6. [Task 3 — Total Visit Duration](#task-3--gettotalvisitduration)
7. [Task 4 — Doctor Summaries](#task-4--getdoctorsummaries)
8. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
9. [Interview Tips](#interview-tips)

---

## Problem Overview

`DoctorManager` is the central service that:

- Maintains a **roster of doctors** (add, remove, lookup by ID)
- Records **patient visits** against each doctor (Task 2)
- Computes **duration totals and activity summaries** per doctor (Tasks 3–4)

Each task builds on the previous one — from a simple off-by-one bug fix to grouping and aggregation.

```
┌─────────────────┐     ┌──────────────────┐
│  AddDoctor()    │────▶│  List<Doctor>    │
│  RemoveDoctor() │     │  (each Doctor    │
│  GetDoctorById()│     │   has visits)    │
└─────────────────┘     └──────────────────┘
                               │
┌─────────────────┐            │
│ AddPatientVisit()│───────────┘
└─────────────────┘
                               ▼
                    ┌──────────────────────┐
                    │ Analytics methods    │
                    │ (Tasks 3–4)          │
                    └──────────────────────┘
```

---

## Core Concepts

### 1. Enums (`VisitType`)

Enums restrict values to a fixed set. They are type-safe alternatives to magic strings or integers.

```csharp
enum VisitType { SCHEDULED, COMPLETED, CANCELLED }
```

**Why it matters:** Task 4 groups visits by `visitType` to find the most frequent type. Enum equality (`== VisitType.COMPLETED`) is used in comparisons and grouping.

---

### 2. Nullable Types (`VisitType?`)

Task 4 requires `mostFrequentVisitType` to be `null` when a doctor has no visits.

```csharp
public VisitType? mostFrequentVisitType;  // can hold a value OR null
```

A **nullable value type** (`T?`) wraps value types (like enums and `int`) so they can represent "no value."

---

### 3. List vs Dictionary — When to Use Which

| Structure | Use case in this problem |
|-----------|--------------------------|
| `List<Doctor>` | Ordered roster of doctors; linear lookup by ID via `GetDoctorById` |
| `List<PatientVisit>` (on each `Doctor`) | Visits stored **on the doctor object** — not a separate global list |
| `Dictionary<int, int>` / `Dictionary<int, DoctorSummary>` | Return type for analytics — map `doctorId` to computed values |

**Key insight:** Unlike ClinicManager (which keeps appointments in a separate list), this problem nests visits inside each `Doctor`. Task 2 adds to `doctor.visits`; Tasks 3–4 iterate over `doctors` and their `visits` lists.

---

### 4. LINQ (Language Integrated Query)

LINQ provides declarative operations over collections:

| Method | What it does |
|--------|--------------|
| `Where` | Filter (like SQL `WHERE`) |
| `GroupBy` | Partition into buckets by a key |
| `Select` | Project/transform each element |
| `OrderByDescending` | Sort descending |
| `Sum`, `Count` | Aggregations |
| `First` | Take the first element after sorting |
| `ToDictionary` | Materialize key-value pairs |

Add `using System.Linq;` when using LINQ in your solution.

---

### 5. Guard Clauses & Edge Cases

| Condition | Expected behavior |
|-----------|-------------------|
| Unknown `doctorId` in Task 2 | Ignore the visit (no-op) |
| Doctor with no visits (Task 3) | Map to `0` duration |
| Doctor with no visits (Task 4) | `{ 0, 0, null }` — still included in result |
| Tied visit types (Task 4) | Any one of the tied types may be returned |

---

### 6. Tie-Breaking Rules (Task 4)

Unlike ClinicManager Task 3, **Task 4 does not require alphabetical tie-breaking**. The spec says:

> If multiple visit types are tied for most frequent, any one of them may be returned.

The test data is constructed so each doctor has a **unique** winner — no ties in the provided tests.

---

## Data Structures Used

### Domain Classes

```
Doctor
├── doctorId   : int
├── doctorName : string
└── visits     : List<PatientVisit>

PatientVisit
├── patientId     : int
├── doctorId      : int
├── visitDuration : int   (minutes)
└── visitType     : VisitType

DoctorSummary (Task 4 output)
├── totalVisits            : int
├── totalDuration          : int
└── mostFrequentVisitType  : VisitType?
```

---

## Task 1 — Bug Fix: `Size()`

### Requirement

Return the count of doctors currently managed by `DoctorManager`.

### The Bug

```csharp
public int Size()
{
    return doctors.Count - 1;  // BUG: off by one
}
```

After adding 5 doctors, `doctors.Count` is 5, but `Size()` returns 4.

### Fix

```csharp
public int Size()
{
    return doctors.Count;
}
```

### Test Data Breakdown

| Action | doctors.Count | Expected Size() |
|--------|---------------|-----------------|
| Add 5 doctors | 5 | 5 |
| Remove doctor 2 | 4 | 4 |

### Concept: Off-by-One Errors

When a count is wrong by exactly 1, check for:

- Extra increment/decrement (`++`, `--`, `- 1`, `+ 1`)
- Wrong loop bounds (`<` vs `<=`)
- Index vs count confusion (last index is `Count - 1`, but count **is** `Count`)

---

## Task 2 — `AddPatientVisit(int doctorId, PatientVisit visit)`

### Requirement

Record a patient visit against the relevant doctor. If the doctor does not exist, ignore the visit.

### Solution

```csharp
public void AddPatientVisit(int doctorId, PatientVisit visit)
{
    Doctor doctor = GetDoctorById(doctorId);
    if (doctor != null)
    {
        doctor.visits.Add(visit);
    }
}
```

### Step-by-Step

```
Add doctors 1, 2, 3
Add visits to doctor 1: v1, v2, v3  →  Dr.1.visits.Count = 3
Add visits to doctor 2: v4, v5       →  Dr.2.visits.Count = 2
Add visit to doctor 99: v6           →  ignored (doctor not found)
Dr.3 has no visits                   →  Dr.3.visits.Count = 0
```

### Concepts Applied

1. **Lookup before mutate** — reuse `GetDoctorById` instead of duplicating search logic
2. **Silent ignore on missing entity** — no exception; simply do nothing
3. **Nested collection** — visits live on the `Doctor` object, not a global list

---

## Task 3 — `GetTotalVisitDuration()`

### Requirement

Return a map of each doctor ID to the total visit duration (minutes) across **all** their visits. Doctors with no visits map to `0`. **Every registered doctor** must appear in the result.

### Solution (LINQ)

```csharp
using System.Linq;

public Dictionary<int, int> GetTotalVisitDuration()
{
    return doctors.ToDictionary(
        d => d.doctorId,
        d => d.visits.Sum(v => v.visitDuration));
}
```

### Solution (Imperative)

```csharp
public Dictionary<int, int> GetTotalVisitDuration()
{
    var result = new Dictionary<int, int>();

    foreach (Doctor doctor in doctors)
    {
        int total = 0;
        foreach (PatientVisit visit in doctor.visits)
        {
            total += visit.visitDuration;
        }
        result[doctor.doctorId] = total;
    }

    return result;
}
```

### Worked Example

| Doctor | Visits (minutes) | Total |
|--------|------------------|-------|
| 1 | 30 + 45 + 15 | 90 |
| 2 | 60 + 90 | 150 |
| 3 | 20 | 20 |
| 4 | (none) | 0 |
| 5 | (none) | 0 |

**Note:** All visit types count toward duration — there is no status filter (unlike ClinicManager Task 2).

### Why Iterate Over `doctors`?

If you only grouped visits, doctors with **zero** visits might be missing. The spec requires every registered doctor to appear with `0`.

---

## Task 4 — `GetDoctorSummaries()`

### Requirement

For **every registered doctor**, return a `DoctorSummary`:

| Field | Rule |
|-------|------|
| `totalVisits` | Count of all visits (any type) |
| `totalDuration` | Sum of `visitDuration` |
| `mostFrequentVisitType` | Most frequent type; tie → any is OK; no visits → `null` |

### Solution (LINQ)

```csharp
using System.Linq;

public Dictionary<int, DoctorSummary> GetDoctorSummaries()
{
    var result = new Dictionary<int, DoctorSummary>();

    foreach (Doctor doctor in doctors)
    {
        if (doctor.visits.Count == 0)
        {
            result[doctor.doctorId] = new DoctorSummary(0, 0, null);
            continue;
        }

        int totalVisits = doctor.visits.Count;
        int totalDuration = doctor.visits.Sum(v => v.visitDuration);

        VisitType mostFrequent = doctor.visits
            .GroupBy(v => v.visitType)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;

        result[doctor.doctorId] = new DoctorSummary(
            totalVisits, totalDuration, mostFrequent);
    }

    return result;
}
```

### Worked Example (Doctor 1)

```
Visits:
  COMPLETED  × 2
  SCHEDULED  × 1
  CANCELLED  × 1

totalVisits   = 4
totalDuration = 30 + 45 + 20 + 10 = 105
mostFrequent  = COMPLETED (count 2)
```

### Worked Example (Doctor 2)

```
Visits:
  SCHEDULED  × 2
  COMPLETED  × 1

totalVisits   = 3
totalDuration = 60 + 90 + 30 = 180
mostFrequent  = SCHEDULED (count 2)
```

### Worked Example (Doctors 4 & 5 — no visits)

```
DoctorSummary(0, 0, null)
```

### Concepts Applied

1. **Master entity iteration** — loop `doctors`, not just visit groups
2. **GroupBy + OrderByDescending + First** — find the mode (most frequent value)
3. **Nullable return** — `null` for doctors with no data

---

## LINQ Cheat Sheet for This Problem

```
Sum:         .Sum(v => v.visitDuration)
Count:       doctor.visits.Count   or   .Count() on a group
Group:       .GroupBy(v => v.visitType)
Sort:        .OrderByDescending(g => g.Count())
First:       .First()                    // after sort = "pick winner"
Convert:     .ToDictionary(d => d.doctorId, d => ...)
```

### Equivalent Imperative Style (Task 3 without LINQ)

```csharp
var result = new Dictionary<int, int>();
foreach (Doctor doctor in doctors)
{
    int total = 0;
    foreach (PatientVisit visit in doctor.visits)
        total += visit.visitDuration;
    result[doctor.doctorId] = total;
}
return result;
```

### Equivalent Imperative Style (Task 4 mode without LINQ)

```csharp
var counts = new Dictionary<VisitType, int>();
foreach (PatientVisit visit in doctor.visits)
{
    if (!counts.ContainsKey(visit.visitType))
        counts[visit.visitType] = 0;
    counts[visit.visitType]++;
}

VisitType mostFrequent = VisitType.SCHEDULED;
int maxCount = 0;
foreach (var kvp in counts)
{
    if (kvp.Value > maxCount)
    {
        maxCount = kvp.Value;
        mostFrequent = kvp.Key;
    }
}
```

---

## Interview Tips

### Reading the Problem

1. **Note where data lives** — visits are on each `Doctor`, not a separate global list
2. **Check filter conditions** — Task 3 sums **all** visits; no status filter
3. **Check who must appear in output** — all registered doctors in Tasks 3 & 4

### Debugging Task 1

When a count is wrong by exactly 1, look for `- 1` or `+ 1` near `Count`.

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Throwing on unknown doctor in Task 2 | Silently ignore — spec says "can be ignored" |
| Missing doctors with 0 visits in Task 3/4 | Loop `doctors`, not visit groups |
| Filtering by visit type in Task 3 | Sum **all** durations regardless of type |
| Forgetting `null` for empty doctors in Task 4 | Return `new DoctorSummary(0, 0, null)` |
| Using a global visit list | Add to `doctor.visits` on the matched `Doctor` |

### Complexity (for follow-up questions)

For `d` doctors and `v` total visits:

| Method | Time | Space |
|--------|------|-------|
| Task 1 | O(1) | O(1) |
| Task 2 | O(d) lookup | O(1) per add |
| Task 3 | O(d + v) | O(d) |
| Task 4 | O(d + v) | O(d) |

All are fine for typical interview data sizes.

---

## Quick Reference — All Four Solutions

```csharp
// TASK 1 — fix off-by-one
public int Size() => doctors.Count;

// TASK 2 — add visit to doctor (ignore if missing)
public void AddPatientVisit(int doctorId, PatientVisit visit)
{
    Doctor doctor = GetDoctorById(doctorId);
    if (doctor != null)
        doctor.visits.Add(visit);
}

// TASK 3 — total duration per doctor (iterate doctors!)
return doctors.ToDictionary(
    d => d.doctorId,
    d => d.visits.Sum(v => v.visitDuration));

// TASK 4 — per-doctor summary (iterate doctors!)
foreach (Doctor doctor in doctors)
{
    if (doctor.visits.Count == 0)
        result[doctor.doctorId] = new DoctorSummary(0, 0, null);
    else
        // count, sum, GroupBy visitType → most frequent
}
```

Apply these changes in `DoctorAppointment.cs` to pass all four tests, then run:

```bash
dotnet run --project "04. DoctorAppointment/DoctorAppointment.csproj"
```

Expected output when fully implemented: **4 passed, 0 failed**.
