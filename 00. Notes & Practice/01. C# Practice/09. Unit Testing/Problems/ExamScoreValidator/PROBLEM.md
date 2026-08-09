---
module: 09. Unit Testing
difficulty: Medium
chapters: 01 Unit Testing Basics
domain: AcademicAssessment
---

# Exam Score Validator

Build a **.NET 8 solution from scratch** with production grading logic and an **xUnit companion test project**. Implement the SUT first, then complete the test TODOs so `dotnet test` passes.

## Business context

A university portal converts raw exam scores (0–100) into letter grades, pass/fail flags, and GPA points for transcript export. Automated tests must catch regressions when grading thresholds change.

## Solution layout

```
ExamScoreValidator/
  ExamScoreValidator.csproj          ← production + demo Main
  Program.cs                         ← CourseGradeService SUT + Program
  ExamScoreValidator.Tests/
    ExamScoreValidator.Tests.csproj  ← references parent; xUnit packages
    CourseGradeServiceTests.cs       ← complete TODO test methods
```

## Definitions (production — implement TODOs in `Program.cs`)

**Class `CourseGradeService`**

- `string GetLetterGrade(int score)` — A ≥ 90, B ≥ 80, C ≥ 70, D ≥ 60, else F; throw `ArgumentOutOfRangeException` if score ∉ [0, 100]
- `bool IsPassing(int score)` — true when score ≥ 60; same range validation
- `decimal GetGradePoints(int score)` — A=4.0, B=3.0, C=2.0, D=1.0, F=0.0; same range validation

## Tests (implement TODOs in `CourseGradeServiceTests.cs`)

Use **Arrange-Act-Assert** and naming pattern `MethodName_Scenario_ExpectedResult`.

| Test method | Requirement |
|-------------|-------------|
| `GetLetterGrade_Score95_ReturnsA` | Happy path A |
| `GetLetterGrade_Score72_ReturnsC` | Mid-tier letter |
| `IsPassing_Score60_ReturnsTrue` | Boundary pass |
| `IsPassing_Score59_ReturnsFalse` | Boundary fail |
| `GetLetterGrade_ScoreOver100_ThrowsArgumentOutOfRange` | `Assert.Throws<ArgumentOutOfRangeException>` |
| `GetLetterGrade_NegativeScore_ThrowsArgumentOutOfRange` | Same exception type |

## Demo Main

Print letter grade, passing flag, and grade points for scores 95, 59, and 72.

## Constraints

- net8, explicit usings, nullable enabled
- No database or file I/O in SUT
- Tests must not call `Console` — test SUT methods only

## Non-goals

MSTest, mocking, integration tests against a real database

## Evaluation

[EVALUATION.md](EVALUATION.md)
