# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/01. Classes & Objects - Done`

---

#### Q1. (R) A loan portal caches `Customer` instances in memory between requests. After one user edits a profile, another user sees the same name and loan amount. Review:

```csharp
public class CustomerCache
{
    private readonly Dictionary<int, Customer> _cache = new();

    public Customer GetOrCreate(int id)
    {
        if (!_cache.ContainsKey(id))
            _cache[id] = new Customer(); // Id assigned by parameterless ctor
        return _cache[id];
    }

    public void UpdateLoan(int id, Customer updated)
    {
        var existing = GetOrCreate(id);
        existing.Name = updated.Name;
        existing.LoanAmount = updated.LoanAmount;
        existing.RateOfInterest = updated.RateOfInterest;
        existing.DurationOfLoan = updated.DurationOfLoan;
    }
}
```

```csharp
// Request A
var draft = cache.GetOrCreate(7);
draft.Name = "Meera Shah";
draft.LoanAmount = 250_000;

// Request B — same id, minutes later
var profile = cache.GetOrCreate(7);
Console.WriteLine(profile.Name); // prints Meera Shah
```

What is wrong with how objects are shared, and how would you fix it?

**Answer:** `Customer` is a reference type — `GetOrCreate` returns the same heap instance for a given id, and mutating fields through one variable changes the single shared object every caller sees. The cache conflates **identity** (one live object per id) with **session draft state** that should be isolated per request.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Reference semantics | `_cache[id]` stores one `Customer` reference; all callers mutate the same instance | Cross-request data bleed — user B sees user A's in-progress edits |
| Design | In-memory singleton cache of mutable domain objects without copy-on-read/write | Violates tenant/session isolation; hard to reason about in multi-user apps |
| Lifetime | `Customer` uses mutable public fields (chapter style) | Any holder of the reference can change state — no encapsulation boundary |
| Correctness | `GetOrCreate` assigns new `Customer()` but key is `id` while `Customer.Id` comes from static `CustomerCount` | Id/key mismatch risk if cache key ≠ `Customer.Id` |

**Fix (priority order):**

1. **Do not cache mutable domain entities** as shared writeable graphs — cache immutable DTOs/snapshots, or store ids and load fresh per request from a database.
2. If caching is required, return **copies** on read (`MemberwiseClone` only as a stopgap; prefer explicit DTO mapping) and treat cache entries as read-only.
3. Replace public fields with properties and encapsulate updates behind methods that validate invariants (see chapter **02. Properties and Indexers**).
4. Scope draft state to the **request** (scoped DI service), not a process-wide dictionary keyed by user id without version checks.
5. Use `TryGetValue` instead of `ContainsKey` + indexer for clarity and single lookup.

**Production takeaway:** Reference assignment copies the pointer, not the object — the chapter's `enrolled = student1` demo is intentional; in production, shared mutable caches cause the same surprise at scale. See **Program.cs** Section 5 — reference semantics.

---

#### Q2. (R) A student lookup API throws `NullReferenceException` in production when a roll number is missing. Review the service:

```csharp
public Student? FindByRoll(int rollNumber, List<Student> roster)
{
    return roster.FirstOrDefault(s => s.RollNumber == rollNumber);
}

public string BuildReportLine(int rollNumber, List<Student> roster)
{
    Student student = FindByRoll(rollNumber, roster);
    return $"{student.StudentName} — Roll {student.RollNumber}, Age {student.Age}";
}
```

The domain model uses public fields (as in this chapter's `Student` class). What breaks, and what would you change?

**Answer:** `FindByRoll` correctly returns `null` when no match exists, but `BuildReportLine` assigns that result to a non-nullable `Student` and dereferences fields — producing `NullReferenceException` instead of a controlled "not found" response.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Null reference | Missing guard after `FirstOrDefault` | Runtime crash on unknown roll number |
| Nullable flow | Return type `Student?` but consumer treats as always present | Compiler warnings ignored; NRE in prod |
| API contract | No distinction between "invalid input" and "missing entity" | Callers cannot return 404/problem details |
| Domain model | Public fields allow `StudentName` to remain unset/null despite ctor defaults | Weaker invariants when objects constructed outside parameterized ctor paths |

**Fix (priority order):**

1. Guard before dereference: `if (student is null) return "Unknown roll"…` or throw `KeyNotFoundException` / return `Result<string>` — match API layer (404 + `ProblemDetails`).
2. Annotate honestly: `Student? student = FindByRoll(...)` and enable nullable reference types project-wide (`<Nullable>enable</Nullable>`).
3. Prefer **factory/constructor paths** that establish required fields (`StudentName`, `RollNumber`) so valid instances cannot be half-initialized.
4. Move reporting to a method that accepts `Student` only after null check, or use null-conditional: `student?.StudentName ?? "(unknown)"` for display-only paths.
5. Long term: replace public fields with properties and validation (chapter **07. Encapsulation**).

**Production takeaway:** Nullable reference types express intent — `Student?` means "may be absent"; production services must branch before field access. See **Program.cs** Section 6 — null references and `?.` / `??`.

---

#### Q3. (R) After `Student` gained only a parameterized constructor (`Student(string studentName, int rollNumber)`), a teammate adds a factory method. `dotnet build` fails. Review:

```csharp
public static Student CreateFromImport(ImportRow row)
{
    return new Student
    {
        StudentName = row.Name,
        RollNumber = row.Roll,
        Percentage = row.Score,
        Address = row.Address ?? string.Empty
    };
}
```

What conflicted with the class design, and how do you fix it without weakening invariants?

**Answer:** Object initializer syntax requires a **parameterless constructor** (or an accessible ctor chain). Once `Student(string, int)` was added, the compiler stopped synthesizing a default ctor — so `new Student { … }` does not compile (CS7036 / no accessible parameterless constructor).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Object initializer without parameterless ctor | Build blocked after ctor change |
| Invariants | Initializer sets `RollNumber`/`StudentName` **after** construction — bypasses ctor validation | Duplicate initialization paths; null names possible if ctor rules added later |
| Design | Two construction stories (ctor vs initializer) for the same type | Team confusion about which fields are required at birth |
| Data integrity | `Percentage`/`Address` set outside ctor while identity fields expected in ctor | Import rows can create inconsistent students |

**Fix (priority order):**

1. **Preferred:** Call the parameterized ctor, then set remaining fields via `AssignDetails` or a dedicated import method:

```csharp
public static Student CreateFromImport(ImportRow row)
{
    var student = new Student(row.Name, row.Roll);
    student.AssignDetails(row.DateOfBirth, row.Age, row.Score, row.Address ?? string.Empty);
    return student;
}
```

2. If object initializers are required, add an explicit parameterless ctor **only** with clear rules (often `private` + static factory) — avoid public parameterless ctors that leave identity unset.
3. Centralize validation in one place (ctor or static factory), not split across initializer + methods.
4. Add unit tests that import rows missing required columns fail fast at construction time.

**Production takeaway:** `new T()` and `new T { … }` are not interchangeable — initializers still run a ctor first. See **Program.cs** Section 3 — adding any ctor removes the compiler-generated default.

---

#### Q4. (D) Your team models loans with the chapter's `Customer` class — public fields plus `CalculateTotalInterest()` on the instance. A new developer moves all interest math into a static `LoanCalculator` and leaves `Customer` as a data bag. Review both approaches. Which would you standardize on for a production lending module, and why?

**Answer:** Prefer a **rich domain model** where `Customer` (or a renamed `LoanAccount`) owns `CalculateTotalInterest()` and enforces loan rules, supplemented by application services for orchestration — not an **anemic** `Customer` with public fields and all behavior in static helpers.

**Rich domain (chapter style, evolved):**

- Behavior lives with data: `CalculateTotalInterest()` reads `LoanAmount`, `RateOfInterest`, `DurationOfLoan` from the instance — matches **Program.cs** Section 3.
- Easier to test one object: construct `Customer`, set fields, assert interest without static glue.
- Natural path to encapsulation: replace public fields with properties, add validation ("rate must be > 0") inside the type.

**Anemic model (static `LoanCalculator`):**

- Acceptable for **pure functions** over DTOs (reporting, batch ETL) or when entities are persistence shapes only (some CRUD APIs).
- Risk: every caller must remember to invoke the calculator; invariants scatter across services; duplicate formulas drift.

**Production standard:**

- **Core lending domain:** rich entities/value objects + domain services for multi-entity rules (e.g., cross-account limits).
- **API/integration layer:** map entities to DTOs; do not expose public mutable fields.
- **Static calculators:** only for stateless policy tables or shared math with no instance context.

**Production takeaway:** Karat tests whether you recognize anemic vs rich trade-offs — tutorials use public fields for clarity; production moves behavior inward and encapsulates state. See **Program.cs** — `Customer.CalculateTotalInterest()` vs field-only `Student` with `AssignDetails`.

---

#### Q5. (M) A scheduling feature stores each student's date of birth and a "next review date." A bug report says review dates never update on the student record. Review:

```csharp
public void ScheduleReview(Student student, DateTime reviewDate)
{
    DateTime scheduled = student.DateOfBirth;
    scheduled = reviewDate; // developer intended to persist review on student
}

public void Demo()
{
    var s = new Student("Darshan K.", 101);
    s.AssignDetails(new DateTime(2000, 12, 7), 15, 78.52, "Malegaon");
    ScheduleReview(s, new DateTime(2026, 9, 1));
    Console.WriteLine(s.DateOfBirth); // still 2000-12-07
}
```

Explain the behavior using **reference vs value** semantics. What would you change?

**Answer:** `Student` is a **reference type** — the parameter `student` points at the heap object and could be mutated through it. `DateTime` is a **value type** — `scheduled = student.DateOfBirth` copies the date value into a local; reassigning `scheduled` only changes the local copy, not `student.DateOfBirth`. The developer confused assigning a new value to a local struct with updating instance state.

**Mechanism:**

| Type | Assignment | Effect in snippet |
|---|---|---|
| `Student` (class) | Passed by reference | Mutations like `student.RollNumber = x` would persist |
| `DateTime` (struct) | Copied by value | `scheduled = reviewDate` does not write back to `student` |

**Fix:**

1. Add a field/property on `Student` (e.g., `NextReviewDate`) and assign directly: `student.NextReviewDate = reviewDate;`.
2. Or, if overloading `DateOfBirth` was intentional, assign to the instance field: `student.DateOfBirth = reviewDate;` (usually wrong semantically — separate fields are clearer).
3. For value-type updates that must stick, always mutate through the owning object, not a detached local copy — same lesson as `birthDateCopy = birthDateCopy.AddYears(1)` not changing `student1.DateOfBirth` in **Program.cs** Section 11.

**Production takeaway:** Reference variables alias one object; value types copy on assignment — production bugs often mix the two when developers expect struct locals to mirror writes. See **Program.cs** Sections 5 and 11 — reference semantics vs struct copy independence.

---

#### Q6. (R) An enrollment module aliases student records for audit trails. Roll numbers change unexpectedly in downstream reports. Review:

```csharp
public class EnrollmentService
{
    public void RegisterAuditCopy(Student liveEnrollment, List<Student> auditTrail)
    {
        Student auditEntry = liveEnrollment; // snapshot for compliance
        auditTrail.Add(auditEntry);
    }

    public void CorrectRollNumber(Student liveEnrollment, int correctedRoll)
    {
        liveEnrollment.RollNumber = correctedRoll;
    }
}
```

```csharp
var student = new Student("Priya Nair", 102);
service.RegisterAuditCopy(student, auditTrail);
service.CorrectRollNumber(student, 1102);
// auditTrail[0].RollNumber is now 1102 — not the original 102
```

What went wrong with object identity, and how would you fix the audit trail?

**Answer:** `Student auditEntry = liveEnrollment` copies the **reference**, not a snapshot — `auditTrail` and `liveEnrollment` denote the same instance. `ReferenceEquals(auditEntry, liveEnrollment)` is true, so correcting the live record mutates the "audit" entry too. Compliance expects **value snapshots** or immutable records, not shared aliases.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Reference sharing | Audit list stores pointers to live objects | Historical reports rewrite when live data changes |
| Identity vs equality | No distinction between "same student over time" and "point-in-time copy" | Audit trail legally/operationally invalid |
| Design | Mutable `Student` with public fields | Any holder can mutate shared state unintentionally |
| Correctness | Comment says "snapshot" but code aliases | Reviewers miss bug without `ReferenceEquals` mental model |

**Fix (priority order):**

1. Store **immutable audit DTOs** or value snapshots at registration time:

```csharp
auditTrail.Add(new StudentAuditRecord(
    liveEnrollment.StudentName,
    liveEnrollment.RollNumber,
    capturedAt: DateTime.UtcNow));
```

2. If full `Student` copies are required, implement explicit `Clone()` / mapping to a new `Student` instance — never add the same reference twice.
3. Prefer append-only audit logs (events) keyed by enrollment id, not mutable object graphs in a `List<Student>`.
4. For live corrections, mutate only the authoritative record; audits remain frozen records.
5. Use `ReferenceEquals` in tests to assert audit entries are **not** the same instance as live enrollment.

**Production takeaway:** `ReferenceEquals` and `==` on classes compare identity by default — "copy" in business language usually means new instance or immutable record, not `=`. See **Program.cs** Section 5 — `enrolled = student1` shares one object; Section 5b — separate `new Student(...)` for independent instances.
