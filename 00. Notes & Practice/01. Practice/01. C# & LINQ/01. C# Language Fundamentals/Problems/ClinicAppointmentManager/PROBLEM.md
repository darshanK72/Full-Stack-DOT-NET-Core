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
