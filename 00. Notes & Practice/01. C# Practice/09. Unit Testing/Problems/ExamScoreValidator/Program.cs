/*
 * PROBLEM: Exam Score Validator
 *
 * A university portal converts raw exam scores into letter grades, pass/fail
 * flags, and GPA points. Tests must catch regressions when thresholds change.
 *
 * This exercise covers:
 *   ch01 — Arrange-Act-Assert test structure
 *   ch01 — MethodName_Scenario_ExpectedResult naming
 *   ch01 — unit tests vs integration (SUT has no I/O)
 *   ch01 — xUnit [Fact], Assert.Equal/True/False/Throws
 */

using System;

namespace AcademicAssessment
{
    /*
     * Pure in-memory grading logic — the system under test (SUT).
     * No Console, database, or HTTP calls in this class.
     */
    public class CourseGradeService
    {
        /*
         * Maps score 0–100 to letter A/B/C/D/F.
         * Throws ArgumentOutOfRangeException when score is outside [0, 100].
         */
        public string GetLetterGrade(int score)
        {
            // TODO: validate range; return letter per PROBLEM.md thresholds
            throw new NotImplementedException();
        }

        /*
         * Returns true when score >= 60.
         * Throws ArgumentOutOfRangeException when score is outside [0, 100].
         */
        public bool IsPassing(int score)
        {
            // TODO: validate range; return pass/fail at boundary 60
            throw new NotImplementedException();
        }

        /*
         * Returns GPA points: A=4.0, B=3.0, C=2.0, D=1.0, F=0.0.
         * Throws ArgumentOutOfRangeException when score is outside [0, 100].
         */
        public decimal GetGradePoints(int score)
        {
            // TODO: map letter grade to points
            throw new NotImplementedException();
        }
    }

    /*
     * Entry point. Owns all Console I/O — not covered by unit tests.
     */
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: create CourseGradeService; print letter, passing, points for 95, 59, 72
            throw new NotImplementedException();
        }
    }
}
