---
module: 01. C# Language Fundamentals
difficulty: Hard
chapters: 04 Operators, 06 Methods, 07 Control Flow, 09 Arrays, 09 Loops, 10 Exception Handling
domain: HealthcareScheduling
---

# Clinic Appointment Manager

Build a **.NET 8 console application from scratch** implementing backend logic for a small clinic. This mirrors interview-style **manager class** problems: register entities, record events, compute analytics.

## Business context

`ClinicManager` tracks doctors and appointments. Reception books appointments; reporting needs statistics for staffing decisions.

## Definitions

**Enums**

- `AppointmentStatus`: `SCHEDULED`, `COMPLETED`, `CANCELLED`, `NO_SHOW`
- `AppointmentType`: `CONSULTATION`, `FOLLOWUP`, `EMERGENCY`

**Classes**

- `Doctor`: `DoctorId` (int), `Name`
- `Appointment`: `AppointmentId`, `DoctorId`, `PatientId`, `Type`, `Status`, `DurationMinutes` (int > 0)

**Manager:** `ClinicManager`

- `AddDoctor(doctor)` — ignore duplicate DoctorId (return false)
- `AddAppointment(appointment)` — only if doctor exists; return false if unknown doctor
- Data stored in in-memory collections you choose

## Analytics methods (implement all)

### 1. `GetCompletedCount()`

Count appointments with status `COMPLETED`.

### 2. `GetAverageDurationByType()`

Return a dictionary mapping each `AppointmentType` to average `DurationMinutes` **for COMPLETED only**. Types with no completed appointments **still appear** with average `0`.

### 3. `GetDoctorWorkloadRanking(int topK)`

Consider **COMPLETED** appointments only. Sum `DurationMinutes` per doctor. Return list of doctor ids sorted by:

1. Total minutes **descending**
2. Tie → lower `DoctorId` first

Return at most `topK` entries. If `topK <= 0`, return empty list.

## Example

Doctors 1 and 2. Completed: (doc1, 30min), (doc1, 40min), (doc2, 20min).

Ranking top 2 → `[1, 2]` (70 vs 20 minutes).

## CLI (minimal)

Menu to seed doctors/appointments and print each analytics method — or a `Main` that runs a **demo script** with hard-coded data then prints results (acceptable for this exercise).

## Constraints

- net8, explicit usings
- No LINQ required (loops and dictionaries allowed)
- Validate duration > 0 when adding appointment
- Use exceptions or bool returns consistently — document choice in README comment

## Non-goals

Database, UI beyond console demo, async

## Evaluation

[EVALUATION.md](EVALUATION.md)

---

## Extended Scenarios

Implement these after the core analytics are working.

### EX1 — Cancellation rate per doctor

Add a method that returns the cancellation rate for each doctor — the proportion of
their total appointments that were CANCELLED or NO_SHOW.

This exercises counting with two conditions (ch04) and decimal division (ch02, ch04)
over grouped data (ch09).

### EX2 — Appointment type distribution

Add a method that returns, for a given doctor, a count of each AppointmentType across
all statuses.  Use the enum to ensure all three types appear in the result even with a zero count.

This exercises iterating all enum values (ch02), Dictionary initialisation (ch09),
and per-type accumulation (ch06).

### EX3 — Patient visit frequency

Add a method that returns the patient Id who visited the clinic most often (across all
doctors and all statuses).  On a tie return the lower patient Id.

This exercises Dictionary-based frequency counting (ch09), the running-max pattern (ch06),
and tie-breaking with integer comparison (ch04).

---

## Implementation Guide

### Classes and their responsibilities

| Class | Responsibility |
|-------|----------------|
| `AppointmentStatus` | Enum representing the lifecycle of an appointment |
| `AppointmentType` | Enum classifying the clinical purpose of an appointment |
| `Doctor` | Data model for a registered clinician |
| `Appointment` | Data model for one booked slot; DurationMinutes must be positive |
| `ClinicManager` | Manages rosters and appointments; provides all analytics |

### Method contracts

| Method | What it does | Returns false when |
|--------|-------------|-------------------|
| `AddDoctor(doctor)` | Registers a doctor | DoctorId already registered |
| `AddAppointment(appt)` | Logs an appointment | DoctorId not registered or DurationMinutes ≤ 0 |
| `GetCompletedCount()` | Counts COMPLETED appointments | — |
| `GetAverageDurationByType()` | Average minutes per type for COMPLETED only | — |
| `GetDoctorWorkloadRanking(topK)` | Ranked list of doctor Ids by COMPLETED minutes | returns empty list when topK ≤ 0 |

### Business rules to enforce

- `GetAverageDurationByType` must include all three AppointmentType values in its result, even types with no completed appointments — those map to 0.0; use `Enum.GetValues` to iterate all values (ch02)
- `GetDoctorWorkloadRanking` considers COMPLETED appointments only; SCHEDULED, CANCELLED, and NO_SHOW are excluded from the minutes total
- Tie-breaking in `GetDoctorWorkloadRanking`: equal total minutes → lower DoctorId ranks first; this is a two-key sort (ch06)
- `topK ≤ 0` must return an empty list immediately without processing any data

### Concepts by chapter

| Chapter | Where it applies |
|---------|-----------------|
| ch02 | Enums for status and type; `Enum.GetValues` to iterate all enum members; int cast to double for averaging |
| ch04 | Comparison operators for sorting; integer cast before division |
| ch06 | Loops for counting, accumulating, and sorting; two-key sort comparison |
| ch07 | Bool return from Add methods; analytics methods return typed results |
| ch09 | `Dictionary<AppointmentType, double>` for averages; `List<int>` for ranking |
