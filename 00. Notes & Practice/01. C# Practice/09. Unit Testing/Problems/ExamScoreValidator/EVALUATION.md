# Exam Score Validator — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| GetLetterGrade thresholds + validation | 25 |
| IsPassing boundary at 60 | 15 |
| GetGradePoints mapping | 15 |
| Six xUnit tests with AAA structure | 30 |
| Test naming convention | 10 |
| Demo Main (no test logic in Main) | 5 |

## AI Review Prompt

Evaluate ExamScoreValidator against PROBLEM.md. Confirm SUT is pure logic, tests use xUnit [Fact] and Assert.Throws, and `dotnet test` passes. Score /100, note AAA/naming strengths, verdict.

---

## Model Answer Checklist

- [ ] Score 101 throws ArgumentOutOfRangeException
- [ ] Score 60 is passing; 59 is not
- [ ] Tests independent — no shared mutable state
- [ ] `dotnet test ExamScoreValidator.Tests` all green
