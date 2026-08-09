using System;

namespace UnitTestingBasics.Services;

/*
 * =========================================================================
 * SECTION 2: SYSTEM UNDER TEST (SUT) — GradeCalculator
 * =========================================================================
 *
 * The system under test (SUT) is the production code you exercise in tests.
 * Here it is a small domain service with no database, HTTP, or file I/O —
 * pure in-memory logic that is easy to unit test in isolation.
 *
 * Keeping SUT in Services/ (separate from Program.cs) mirrors real solutions:
 *   • App/library project  → business logic
 *   • *.Tests project      → references the app and calls SUT methods
 *
 * Tests in this chapter target GradeCalculator only — not Main, not console I/O.
 * -------------------------------------------------------------------------
 */
public class GradeCalculator
{
    // Maps numeric score (0–100) to a letter grade; throws if out of range
    public string GetLetterGrade(int score)
    {
        if (score < 0 || score > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(score),
                score,
                "Score must be between 0 and 100.");
        }

        if (score >= 90)
        {
            return "A";
        }

        if (score >= 80)
        {
            return "B";
        }

        if (score >= 70)
        {
            return "C";
        }

        if (score >= 60)
        {
            return "D";
        }

        return "F";
    }

    // Passing threshold is 60; same range validation as GetLetterGrade
    public bool IsPassing(int score)
    {
        if (score < 0 || score > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(score),
                score,
                "Score must be between 0 and 100.");
        }

        return score >= 60;
    }
}
