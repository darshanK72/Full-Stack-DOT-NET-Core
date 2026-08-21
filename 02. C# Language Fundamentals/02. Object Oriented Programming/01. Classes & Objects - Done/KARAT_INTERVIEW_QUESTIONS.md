# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/01. Classes & Objects - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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

---

#### Q4. (D) Your team models loans with the chapter's `Customer` class — public fields plus `CalculateTotalInterest()` on the instance. A new developer moves all interest math into a static `LoanCalculator` and leaves `Customer` as a data bag. Review both approaches. Which would you standardize on for a production lending module, and why?

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
