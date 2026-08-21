# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/09. Arrays - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A batch job averages exam scores for reporting. QA reports intermittent `IndexOutOfRangeException` in production when a student has no scores yet. Review the helper:

```csharp
public static double AverageScores(int[] scores)
{
    double total = 0;
    for (int i = 0; i <= scores.Length; i++)
    {
        total += scores[i];
    }
    return total / scores.Length;
}
```

What is wrong, and how would you fix it for both empty and populated arrays?

---

#### Q2. (R) A developer models a textbook shelf grid with a 2D array, then copies a jagged-array traversal pattern from another service. Review:

```csharp
int[,] shelfStock = new int[,]
{
    { 12, 8, 15 },
    { 5, 20, 10 },
    { 9, 11, 7 }
};

int centerBin = shelfStock[1][2];  // row 1, column 2

string[][] departments = CreateDepartmentCourses();
int scienceCourses = departments[1, 0].Length;  // first science course
```

What breaks at compile time or runtime, and when would you choose `int[,]` vs `string[][]`?

---

#### Q3. (R) A pricing service must keep an immutable snapshot of SKU codes before sorting for audit, but the audit log shows the "original" list reordered. Review:

```csharp
public static string[] GetSortedSkusForDisplay(string[] skuCodes)
{
    string[] snapshot = skuCodes;           // preserve original order
    Array.Sort(snapshot);
    return snapshot;
}

public static void UpdateSku(string[] skuCodes, int index, string newCode)
{
    skuCodes = new string[] { "TBK-999" };  // replace caller's array
}
```

What went wrong with "snapshot" and `UpdateSku`, and how do you fix both?

---

#### Q4. (R) A pass-rate calculator tries to normalize scores in place during iteration:

```csharp
public static void NormalizeScores(int[] scores, int passingMark)
{
    foreach (int score in scores)
    {
        if (score < passingMark)
        {
            score = passingMark;   // bump failing scores to minimum pass
        }
    }
}
```

What fails (compile and/or runtime behavior), and what pattern should replace it?

---

#### Q5. (R) After migrating parallel arrays to `List<T>`, a report builder fails to compile. Review:

```csharp
public static string BuildScoreReport(List<string> subjects, int[] scores)
{
    StringBuilder report = new StringBuilder();
    for (int i = 0; i < subjects.Count; i++)
    {
        report.Append(subjects[i]);
        report.Append('=');
        report.Append(scores[i]);
    }

    int totalSlots = scores.Count;
    int lastIndex = scores.Count - 1;
    return report.ToString();
}
```

What errors appear, and what guard would you add before pairing `subjects` and `scores` by index?

---

#### Q6. (M) A campus API returns course offerings per department. When every department is closed for the term, the outer array exists but inner arrays are empty. Review:

```csharp
public static int GetFirstCourseCode(string[][] departments)
{
    return departments[0][0].GetHashCode();  // quick non-null check
}

public static double AverageDepartmentSize(string[][] departments)
{
    int totalCourses = 0;
    foreach (string[] dept in departments)
    {
        totalCourses += dept.Length;
    }
    return (double)totalCourses / departments.Length;
}

public static bool HasAnyCourses(string[][] departments)
{
    return departments.Length > 0;  // API contract: non-empty when courses exist
}
```

What breaks when `departments` is `new string[0][]`, when the outer array has rows but every inner array is empty, and how would you harden these helpers?

---
