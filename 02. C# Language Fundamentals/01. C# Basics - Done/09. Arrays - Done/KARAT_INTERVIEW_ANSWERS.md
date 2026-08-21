# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/09. Arrays - Done`

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

**Answer:** The loop uses `i <= scores.Length`, which reads one past the last valid index (`Length - 1`), causing `IndexOutOfRangeException` on every non-empty array; dividing by `scores.Length` when the array is empty also throws `DivideByZeroException`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Off-by-one: `i <= Length` instead of `i < Length` | `IndexOutOfRangeException` on last iteration |
| Runtime | No guard when `scores.Length == 0` | `DivideByZeroException` on empty input |
| Design | No contract for "no scores" vs error | Callers cannot distinguish missing data from crash |

**Fix (priority order):**

1. Change loop bound to `i < scores.Length` — matches **Program.cs** Section 5 (`for` bound pattern).
2. Handle zero-length explicitly: return `0`, `double.NaN`, or throw `ArgumentException` — document the API contract.
3. Optionally validate `scores` is not null before accessing `.Length`.
4. Add unit tests for `[]`, single element, and multi-element arrays.

```csharp
public static double AverageScores(int[] scores)
{
    if (scores is null || scores.Length == 0)
        return 0; // or throw / return double.NaN — pick one contract

    double total = 0;
    for (int i = 0; i < scores.Length; i++)
        total += scores[i];

    return total / scores.Length;
}
```

**Production takeaway:** Bounds errors compile fine — Karat tests whether you spot `<= Length` vs `< Length` and empty-array division. See foundation **Arrays** — IndexOutOfRangeException gotcha; **Program.cs** Section 4 — bounds guard pattern.

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

**Answer:** Rectangular arrays use comma indexing (`[row, col]`); jagged arrays use chained brackets (`[row][col]`). Swapping the syntax produces compile errors — `int[,]` does not support `[1][2]`, and `string[][]` does not support `[1, 0]`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `shelfStock[1][2]` on `int[,]` | CS0021 — wrong indexer arity for rectangular array |
| Compile | `departments[1, 0]` on `string[][]` | CS0022 — jagged rows are separate arrays, not one comma-indexed grid |
| Design | Mixing mental models between grid types | Wrong structure choice wastes memory or complicates iteration |

**Fix (priority order):**

1. Rectangular: `shelfStock[1, 2]` — one memory block, every row same width (**Program.cs** Section 7).
2. Jagged: `departments[1][0]` — outer index selects row array, inner index selects element (**Program.cs** Section 8).
3. Choose `int[,]` when the grid is truly rectangular (shelf bins, image pixels, matrices).
4. Choose `T[][]` when row lengths vary naturally (departments with different course counts); use `departments[d].Length` per row, not a single column count.

**Production takeaway:** Jagged vs multidimensional is a **data-shape** decision, not syntax preference — wrong choice forces awkward padding or nested loops. See **Program.cs** Sections 7–8 and Quick Reference — rectangular vs jagged.

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

**Answer:** Assigning `snapshot = skuCodes` copies only the **reference**, not the elements — `Array.Sort` mutates the same heap array the caller still holds. Reassigning `skuCodes` inside `UpdateSku` rebinds a **local** parameter; the caller's variable still points at the original array.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Reference assignment masquerading as copy | Audit "original" reordered after sort — data integrity bug |
| Correctness | `skuCodes = new string[] { ... }` in void method | Caller's array unchanged; silent no-op |
| Design | `Array.Sort` is in-place | Any shared reference sees mutation |

**Fix (priority order):**

1. Snapshot before sort: `(string[])skuCodes.Clone()` or `skuCodes.ToArray()` — shallow copy of the array object (**Program.cs** Section 9–10, `DemonstrateSearchAndSort`).
2. To mutate caller's slots: `skuCodes[index] = newCode` with bounds check — no reassignment of the parameter.
3. If the caller must receive a new array instance, return `string[]` instead of `void`.
4. Document whether APIs mutate in place (`Array.Sort`, `Array.Reverse`) vs return new buffers.

```csharp
public static string[] GetSortedSkusForDisplay(string[] skuCodes)
{
    string[] sorted = (string[])skuCodes.Clone();
    Array.Sort(sorted);
    return sorted;
}

public static void UpdateSku(string[] skuCodes, int index, string newCode)
{
    if (index < 0 || index >= skuCodes.Length)
        throw new ArgumentOutOfRangeException(nameof(index));
    skuCodes[index] = newCode;
}
```

**Production takeaway:** Arrays are reference types — assignment shares storage until you `Clone`, `Copy`, or `Resize`. Karat stacks this with in-place `Array` helpers. See foundation **Arrays** — arrays as reference types.

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

**Answer:** The `foreach` iteration variable is read-only — assigning to `score` does not compile (CS1654). Even if it compiled, mutating the iteration variable would not write back to the source array; slot updates require index access.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Assignment to `foreach` iteration variable | CS1654 — cannot modify foreach variable |
| Logic | Expectation that `score = …` updates the array | Silent failure even in languages that allow it — wrong mental model |
| Design | In-place mutation needs index or `for` | Failing scores never normalized |

**Fix (priority order):**

1. Use `for` with index when mutating slots (**Program.cs** Section 6 — "use for when you must mutate slots by index").
2. Guard index if length may be zero.
3. Alternative: `for (int i = 0; i < scores.Length; i++)` with `scores[i] = passingMark` when below threshold.

```csharp
public static void NormalizeScores(int[] scores, int passingMark)
{
    for (int i = 0; i < scores.Length; i++)
    {
        if (scores[i] < passingMark)
            scores[i] = passingMark;
    }
}
```

**Production takeaway:** `foreach` on arrays is for **read-only** traversal — production code that "fixes up" collections in place should use indexed loops or LINQ projecting to a new array. See **Program.cs** Section 6 foreach rules; Quick Reference — foreach iteration variable assignment.

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

**Answer:** Arrays expose `.Length` (property); `List<T>` exposes `.Count`. Using `scores.Count` on `int[]` fails at compile time (CS1061). Even after fixing that, parallel arrays/lists must have matching lengths before indexed pairing.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `scores.Count` on `int[]` | CS1061 — arrays have `Length`, not `Count` |
| Runtime | Mismatched `subjects.Count` vs `scores.Length` | `IndexOutOfRangeException` or wrong pairings in report |
| Design | Mixed abstractions (`List<T>` + `T[]`) without length contract | Fragile after partial migration |

**Fix (priority order):**

1. Use `scores.Length` for arrays; `subjects.Count` for lists — or normalize both to same type.
2. Guard: `if (subjects.Count != scores.Length) throw new ArgumentException(...)` before the loop.
3. Prefer a single model (both `List<T>` or both arrays) when rows are paired by index — matches **Program.cs** Section 5 `BuildScoreReport` parallel-array pattern.
4. For empty collections, loop runs zero times — safe; still validate pairing when one side is non-empty.

```csharp
if (subjects.Count != scores.Length)
    throw new ArgumentException("subjects and scores must have the same length.");

int lastIndex = scores.Length - 1;
```

**Production takeaway:** `Length` vs `Count` is a common migration footgun — Karat tests whether you know which type owns which member. See **Program.cs** Section 14 List preview vs Section 3 `Length` property.

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

**Answer:** Zero-length outer arrays make `departments[0]` throw immediately; `AverageDepartmentSize` divides by zero when the outer length is 0; `HasAnyCourses` returns true whenever any department row exists, even if every inner array is empty — violating the stated contract.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `departments[0][0]` when outer `Length == 0` | `IndexOutOfRangeException` on first access |
| Runtime | `totalCourses / departments.Length` when outer length 0 | `DivideByZeroException` |
| Correctness | `HasAnyCourses` checks outer length only | Returns `true` for `[ [], [], [] ]` — false positive |
| Null | Uninitialized jagged row (`departments[i]` null) | `NullReferenceException` in `dept.Length` |

**Fix (priority order):**

1. Null-check parameter; treat null/empty outer as "no courses."
2. `GetFirstCourseCode`: scan for first non-null inner array with `Length > 0`, or return nullable / throw meaningful `InvalidOperationException`.
3. `AverageDepartmentSize`: if `departments.Length == 0`, return `0` or `double.NaN` — document contract.
4. `HasAnyCourses`: `departments.Any(d => d is { Length: > 0 })` or explicit nested loop — outer length alone is insufficient.

```csharp
public static bool HasAnyCourses(string[][] departments)
{
    if (departments is null || departments.Length == 0)
        return false;

    foreach (string[] dept in departments)
    {
        if (dept is { Length: > 0 })
            return true;
    }
    return false;
}
```

**Production takeaway:** Jagged arrays have **two** length dimensions — outer and per-row inner — and zero-length is valid at either level. Production APIs must define behavior for empty outer, empty inners, and null rows. See **Program.cs** Section 8 — `departments.Length` vs `departments[d].Length`; Section 4 bounds guard.

---
