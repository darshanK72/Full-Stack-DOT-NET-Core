# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/09. Unit Testing/01. Unit Testing Basics`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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
