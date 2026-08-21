# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/12. Generation Operations`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A training-portal report paginates sessions with "show sessions 3 through 5." Review the paging helper. What is wrong with the `Range` call, and what does the caller actually get?

```csharp
public static IEnumerable<int> GetSessionPage(int firstSession, int lastSession)
{
    // Product spec: inclusive window firstSession..lastSession
    return Enumerable.Range(firstSession, lastSession);
}

// Caller:
foreach (int session in GetSessionPage(3, 5))
{
    Console.WriteLine($"Session {session}");
}
// Expected: 3, 4, 5
```

---

#### Q2. (R) Seat padding mirrors the tutorial's `Concat` + `Repeat` pattern. When a session sells out, the nightly job throws before writing the report. Review:

```csharp
const int seatsPerSession = 4;

string[] confirmed = GetConfirmedAttendees(sessionId); // length may equal seatsPerSession

IEnumerable<string> fullSeatRow = confirmed
    .Concat(Enumerable.Repeat("Open", seatsPerSession - confirmed.Length));

Console.WriteLine(string.Join(" | ", fullSeatRow));
```

What breaks when every seat is confirmed, and how do you fix it without abandoning lazy `IEnumerable<string>` composition?

---

#### Q3. (R) A developer pre-builds per-session rosters with `Repeat` before loading enrollments. After loading session 1, session 2 lists the same students. Review:

```csharp
const int sessionCount = 3;

List<Enrollment> sharedRoster = new List<Enrollment>();
List<List<Enrollment>> rosters = Enumerable.Repeat(sharedRoster, sessionCount).ToList();

for (int i = 0; i < rosters.Count; i++)
{
    rosters[i].AddRange(GetEnrollmentsForSession(i + 1));
}

// QA: rosters[0] and rosters[1] always have identical Count
```

What is wrong with this generation pattern for reference types, and what should replace it?

---

#### Q4. (R) A weekly score report uses `DefaultIfEmpty` so `Average` never throws and empty advanced tracks still produce a CSV row. QA reports inflated headcount and misleading averages. Review both call sites:

```csharp
IEnumerable<Enrollment> advanced = enrollments
    .Where(e => e.Level == TrainingLevel.Advanced);

Enrollment sentinel = new Enrollment("—", "No advanced enrollments", TrainingLevel.Advanced);

IEnumerable<Enrollment> exportRows = advanced.DefaultIfEmpty(sentinel);

double averageScore = advanced
    .Select(e => e.AssessmentScore)
    .DefaultIfEmpty(0)
    .Average();

// Export: foreach (var row in exportRows) WriteCsvRow(row);
// Dashboard: displays averageScore and exportRows.Count() as "advanced enrollment count"
```

Diagnose the sentinel confusion and the count/average mismatch. What would you change and in what order?

---

#### Q5. (R) A repository refactor returns `null` when a course has no enrollments instead of `Enumerable.Empty<Enrollment>()`. Review the report service after deploy:

```csharp
public IEnumerable<Enrollment> GetEnrollmentsForCourse(string courseCode)
{
    if (!_catalog.ContainsKey(courseCode))
        return null;

    var rows = _catalog[courseCode];
    return rows.Count == 0 ? null : rows;
}

// ReportService — no null checks (old API always returned Empty):
var names = GetEnrollmentsForCourse("RET-000").Select(e => e.DisplayName);
int headcount = GetEnrollmentsForCourse("RET-000").Count();
bool hasAny = GetEnrollmentsForCourse("RET-000").Any();
```

What breaks in production for retired courses, and how does `Enumerable.Empty<T>()` fix the contract?

---

#### Q6. (M) A metrics helper treats `Enumerable.Empty<Enrollment>()` as a cacheable singleton and a teammate tries to mutate it before returning. Review:

```csharp
IEnumerable<Enrollment> emptyA = Enumerable.Empty<Enrollment>();
IEnumerable<Enrollment> emptyB = Enumerable.Empty<Enrollment>();

if (ReferenceEquals(emptyA, emptyB))
{
    _metrics.Increment("empty-enrollment-singleton");
}

public IEnumerable<Enrollment> GetOrSeed(string courseCode)
{
    if (!_catalog.TryGetValue(courseCode, out var list) || list.Count == 0)
    {
        var mutable = (List<Enrollment>)Enumerable.Empty<Enrollment>();
        mutable.Add(new Enrollment("SEED", "Placeholder", TrainingLevel.Beginner));
        return mutable;
    }

    return list;
}
```

What is correct about `Empty<T>()`'s singleton behavior, and what fails at runtime in `GetOrSeed`?
