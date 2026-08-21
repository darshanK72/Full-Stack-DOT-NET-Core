# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/09. Unit Testing/01. Unit Testing Basics`

---

#### Q1. (R) A teammate adds coverage for `GradeCalculator` after reading this chapter's AAA section. The test passes in CI but does not protect behavior. Review:

```csharp
[Fact]
public void GetLetterGrade_ValidInput_DoesNotThrow()
{
    GradeCalculator calculator = new GradeCalculator();

    calculator.GetLetterGrade(85);
}
```

What is wrong, and how would you rewrite it using the chapter's naming and AAA pattern?

**Answer:** The test has no assertion — it only checks that the call does not throw, so any return value (including `"F"` for 85) still passes. Unit tests must assert observable outcomes, not merely "completed without exception."

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Missing Assert phase | False green — wrong grades ship undetected |
| AAA | Act without Assert | Violates Section 3 pattern; test documents nothing |
| Naming | Name claims "DoesNotThrow" but never verifies grade | Misleading failure signal when renamed later |

**Fix (priority order):**

1. Assert the **behavior** — expected letter for a known input — using `Assert.Equal`.
2. Rename to `GetLetterGrade_Score85_ReturnsB` (MethodName_Scenario_ExpectedResult from Section 4).
3. Keep Arrange–Act–Assert comments or blank lines so review catches missing asserts.

```csharp
[Fact]
public void GetLetterGrade_Score85_ReturnsB()
{
    // Arrange
    GradeCalculator calculator = new GradeCalculator();

    // Act
    string grade = calculator.GetLetterGrade(85);

    // Assert
    Assert.Equal("B", grade);
}
```

**Production takeaway:** "No throw" is not a specification unless throwing is the only failure mode you care about — Karat uses this to catch tests that inflate coverage metrics without encoding decisions. See **GradeCalculatorTests.cs** and **Program.cs** Section 9 for correct AAA examples.

---

#### Q2. (R) A PR adds "boundary" tests for `IsPassing`. Review:

```csharp
[Fact]
public void IsPassing_BoundaryTests()
{
    var calc = new GradeCalculator();

    Assert.False(calc.IsPassing(59));
    bool atThreshold = calc.IsPassing(60);
    Assert.True(atThreshold);
    calc.IsPassing(100);
}
```

Identify AAA violations and any assertion gaps. What would you change before approving the PR?

**Answer:** The method mixes multiple scenarios in one test, interleaves acts and asserts, and ends with an Act (`IsPassing(100)`) that has no assertion — so a regression at the maximum valid score would not fail this test.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| AAA | Multiple Act/Assert cycles in one `[Fact]` | Unclear which scenario failed; harder to maintain |
| Correctness | `IsPassing(100)` never asserted | Upper boundary untested despite being invoked |
| Naming | Generic `BoundaryTests` | Failing CI line does not identify 59 vs 60 vs 100 |
| Design | One test owns three behaviors | Violates "one logical behavior per test" from Section 3 |

**Fix (priority order):**

1. Split into focused tests — mirror **GradeCalculatorTests.cs**: `IsPassing_Score59_ReturnsFalse`, `IsPassing_Score60_ReturnsTrue`.
2. Add `IsPassing_Score100_ReturnsTrue` (or fold max boundary into a dedicated test).
3. Each test: one Arrange, one Act, one Assert (or one exception assert).
4. Optionally use `[Theory]` + `[InlineData]` for table-driven boundaries (preview in Section 8 — full **xUnit** chapter).

**Production takeaway:** Bundled boundary tests look thorough but hide gaps — Karat tests whether you read AAA structure, not just boundary vocabulary. See **Program.cs** Sections 3–4.

---

#### Q3. (R) After a refactor, `GradeCalculator` keeps the same public API but replaces nested `if` checks with a private `GradeBand[]` lookup. This test now fails and blocks merge:

```csharp
[Fact]
public void GetLetterGrade_InternalBandCount_IsFive()
{
    var field = typeof(GradeCalculator).GetField(
        "_bands",
        BindingFlags.NonPublic | BindingFlags.Instance);

    var bands = (Array)field!.GetValue(new GradeCalculator())!;

    Assert.Equal(5, bands.Length);
}
```

What category of testing mistake is this, and how should passing thresholds be verified instead?

**Answer:** This is **implementation testing** — it couples the suite to private fields and internal structure, so harmless refactors break CI even when public behavior is unchanged. Tests should target **observable behavior** through the public API (`GetLetterGrade`, `IsPassing`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Reflection on `_bands` | Breaks on rename, reorder, or algorithm change |
| Maintainability | Tests private layout, not contract | Blocks refactors that improve code without changing behavior |
| Correctness | Band count ≠ correct mapping | Could pass with wrong thresholds inside five bands |

**Fix (priority order):**

1. Delete the reflection test; do not widen visibility (`internal` + `InternalsVisibleTo`) unless there is a strong seam reason.
2. Assert **outputs at boundaries** — e.g. 90→`"A"`, 89→`"B"`, 59→not passing, 60→passing — as in **GradeCalculator.cs** and manual tests in **Program.cs** Section 9.
3. For invalid input, keep `Assert.Throws<ArgumentOutOfRangeException>` on public methods (see **GradeCalculatorValidationTests**).
4. If logic grows complex, extract a testable public abstraction (e.g. `IGradeScale`) rather than testing privates.

**Production takeaway:** Karat distinguishes "tests broke" from "behavior broke" — implementation-coupled tests create merge friction without catching user-visible bugs. See Section 5: unit tests exercise one class through its public surface.

---

#### Q4. (R) A developer wants tests to "document business rules" and adds:

```csharp
private const int PassingThreshold = 60;

[Fact]
public void IsPassing_ReimplementsThresholdLogic()
{
    int score = 72;
    bool expected = score >= PassingThreshold;

    bool actual = new GradeCalculator().IsPassing(score);

    Assert.Equal(expected, actual);
}
```

Why does this test provide little value and risk false confidence? Show a better test for the same rule.

**Answer:** The test **duplicates production logic** in the test body (`score >= PassingThreshold`), so both sides can be wrong the same way and the assertion still passes. It verifies the copy matches the copy, not that `GradeCalculator` matches the business rule.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Expected value computed with same rule as SUT | Bug in threshold (e.g. `>= 65`) may pass if test constant matches |
| Magic numbers | `72` is arbitrary mid-range | Does not pin the actual decision boundary at 60 |
| Documentation | Test name hides which rule is encoded | Reviewers cannot see boundary intent |

**Fix (priority order):**

1. Replace with **fixed expected outcomes** at meaningful inputs — not derived from the same formula.
2. Test **both sides of the boundary** (59 false, 60 true) as in this chapter's **GradeCalculatorTests.cs**.
3. If the threshold is a business constant, define it once in production code and test boundaries against known cases; do not recompute `expected` from that constant in the test unless testing a pure helper you own separately.

```csharp
[Fact]
public void IsPassing_Score59_ReturnsFalse()
{
    GradeCalculator calculator = new GradeCalculator();
    bool passing = calculator.IsPassing(59);
    Assert.False(passing);
}

[Fact]
public void IsPassing_Score60_ReturnsTrue()
{
    GradeCalculator calculator = new GradeCalculator();
    bool passing = calculator.IsPassing(60);
    Assert.True(passing);
}
```

**Production takeaway:** Tests should be independent oracles — Karat uses duplicated logic in tests to check whether you understand false confidence vs boundary specification.

---

#### Q5. (R) QA reports that a grading bug shipped despite green tests. The suite includes:

```csharp
[Fact]
public void GetLetterGrade_Score72_ReturnsC()
{
    Assert.Equal("C", new GradeCalculator().GetLetterGrade(72));
}

[Fact]
public void GetLetterGrade_Score95_ReturnsA()
{
    Assert.Equal("A", new GradeCalculator().GetLetterGrade(95));
}

[Fact]
public void GetLetterGrade_InvalidScore_ThrowsWithExactMessage()
{
    var ex = Assert.Throws<ArgumentOutOfRangeException>(
        () => new GradeCalculator().GetLetterGrade(101));

    Assert.Equal("Score must be between 0 and 100.", ex.Message);
}
```

Which tests are brittle, which gaps remain for `GetLetterGrade`, and what would you add or change?

**Answer:** The exception test is **brittle** against full `Message` strings; the happy-path tests use **unexplained magic numbers** and leave **boundary gaps** (e.g. 89 vs 90, 69 vs 70, 0, 100, negative scores) that would catch off-by-one threshold bugs.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Brittleness | Exact `ex.Message` match | Fails on wording/localization/refactor though throw type and param still correct |
| Coverage gap | Only 72 and 95 tested | Wrong B/C or A/B cutoff ships undetected |
| Magic numbers | 72, 95 without comment | Readers cannot tell boundary vs mid-range intent |
| Naming | Inline one-liners skip AAA structure | Harder review for missing cases |

**Fix (priority order):**

1. Assert **exception type** and optionally `paramName` / inner details — not full message text unless message is a stable contract.

```csharp
var ex = Assert.Throws<ArgumentOutOfRangeException>(
    () => new GradeCalculator().GetLetterGrade(101));
Assert.Equal("score", ex.ParamName);
```

2. Add boundary pairs: 90/89 (`A`/`B`), 80/79, 70/69, 60/59 (`D`/`F`) — one act/assert each or `[Theory]` rows.
3. Add 0 and 100 edge cases; keep negative and >100 in **GradeCalculatorValidationTests** style.
4. Prefer names like `GetLetterGrade_Score90_ReturnsA`; use `[InlineData(90, "A")]` with column names in comments if using Theory later.

**Production takeaway:** Green tests with sparse boundaries and stringly exception asserts are a common post-release surprise — Karat tests prioritization of **behavior at edges** over happy-path literals. See **GradeCalculator.cs** thresholds and **Program.cs** Section 9 manual boundary cases.

---

#### Q6. (D) Two approaches are proposed for the failing grade at 59 / passing at 60 boundary used in this chapter's `Program.cs` and `GradeCalculatorTests.cs`:

**A — one test per boundary side (chapter style):**

```csharp
[Fact] public void IsPassing_Score59_ReturnsFalse() { … Assert.False(calc.IsPassing(59)); }
[Fact] public void IsPassing_Score60_ReturnsTrue()  { … Assert.True(calc.IsPassing(60)); }
```

**B — single parameterized test:**

```csharp
[Theory]
[InlineData(59, false)]
[InlineData(60, true)]
public void IsPassing_AtPassingThreshold_ReturnsExpected(int score, bool expected)
{
    Assert.Equal(expected, new GradeCalculator().IsPassing(score));
}
```

Which approach better supports maintainability and failure diagnosis in a large team, and what naming or data choices still matter?

**Answer:** For two critical boundaries, **Approach A** (chapter style) gives the clearest failure names in CI and matches this folder's teaching convention; **Approach B** scales better when many `(score, expected)` rows exist, but only if each row represents an independent specification — not a duplicate of production logic.

**Approach A — prefer when:**

- Boundaries are few and business-critical (59/60 passing rule in **GradeCalculator.cs**).
- Test Explorer / `dotnet test` output should read like specs: `IsPassing_Score59_ReturnsFalse` fails without opening the method.
- New contributors learn AAA from explicit Arrange/Act/Assert blocks in **GradeCalculatorTests.cs**.

**Approach B — prefer when:**

- Dozens of grade rows would duplicate boilerplate; `[Theory]` + `[InlineData]` or `[MemberData]` reduces noise (full pattern in chapter **02. xUnit**).
- Data is clearly labeled — e.g. `[InlineData(59, false)] // one below passing threshold` — so magic numbers stay documented.
- Team agrees on one theory name: `IsPassing_AtPassingThreshold_ReturnsExpected` is acceptable; avoid vague `BoundaryTests`.

**Shared rules (both approaches):**

- Test **behavior** (`IsPassing` result), not how `score >= 60` is implemented internally.
- Always include **both sides** of the boundary; never only the passing case.
- One logical assert target per test method (A naturally; B requires each row to be a distinct case).

**Production takeaway:** The trade-off is **failure signal clarity** vs **DRY at scale** — Karat expects you to justify choice for team size and data volume, not declare one pattern universally superior. This chapter intentionally uses Approach A in **Program.cs** Section 9 and **GradeCalculatorTests.cs** before introducing `[Theory]` in the next chapter.
