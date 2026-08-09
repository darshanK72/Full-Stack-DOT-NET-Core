/*
 * =============================================================================
 * 01. UNIT TESTING BASICS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Foundational unit testing — why we test, Arrange-Act-Assert, naming,
 *        unit vs integration, solution layout, dotnet test, and xUnit preview.
 *
 * WHY IT MATTERS:
 *   Manual clicking through an app does not scale. Every bug fix and refactor
 *   risks breaking something else. Automated tests run in seconds, document
 *   expected behavior, and let you change code with confidence. Teams treat
 *   tests as part of the product — not optional homework after coding.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Why automated tests matter (regression, design, living documentation)
 *   2.  Arrange-Act-Assert — the universal test structure
 *   3.  Test naming — MethodName_Scenario_ExpectedResult
 *   4.  Unit tests vs integration tests — scope, speed, and failure meaning
 *   5.  Typical test project structure in a .NET solution
 *   6.  Running tests with dotnet test (companion xUnit project)
 *   7.  Preview: xUnit [Fact] and Assert.* (full depth in ch.02–04)
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using UnitTestingBasics.Services;

namespace UnitTestingBasics;

/*
 * =========================================================================
 * SECTION 1: WHY AUTOMATED TESTS
 * =========================================================================
 *
 * Imagine GradeCalculator shipped without tests. A developer "simplifies"
 * the passing threshold from 60 to 65. The app still compiles. Manual QA
 * checks one happy path. Weeks later, support tickets report wrong grades.
 *
 * A unit test asserting IsPassing(59) == false and IsPassing(60) == true
 * would fail immediately on the bad change — before merge.
 *
 * | Benefit              | What it means                                      |
 * |----------------------|----------------------------------------------------|
 * | Regression safety    | Catch breaks when code changes                     |
 * | Design feedback      | Hard-to-test code often signals tight coupling     |
 * | Living documentation | Test names describe behavior better than comments  |
 * | Faster feedback      | dotnet test in seconds vs full manual retest       |
 *
 * Tests do not replace thinking — they encode decisions you already made
 * so the computer re-checks them every build.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 2: SYSTEM UNDER TEST — Services/GradeCalculator.cs
 * =========================================================================
 *
 * Open Services/GradeCalculator.cs for the SUT used throughout this chapter.
 * Production logic lives there; Program.cs and UnitTestingBasics.Tests/
 * exercise it without duplicating business rules.
 *
 * Splitting app code from test code is deliberate:
 *   UnitTestingBasics.csproj           ← runnable console + SUT library code
 *   UnitTestingBasics.Tests.csproj     ← references main project; no Main()
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 3: ARRANGE-ACT-ASSERT (AAA)
 * =========================================================================
 *
 * Every good test — manual or framework-based — has three phases:
 *
 *   Arrange — set up inputs, dependencies, and preconditions
 *   Act     — invoke the one behavior being verified
 *   Assert  — check the outcome (return value, exception, state change)
 *
 * Frameworks (xUnit, MSTest) provide Assert.Equal, Assert.True, etc.
 * ManualTest below mirrors those helpers so you see AAA in plain C# first.
 *
 * COVERED IN DETAIL LATER → 02. xUnit
 *   Assert.Throws overloads, custom messages, test discovery, dotnet test output
 * -------------------------------------------------------------------------
 */
internal static class ManualTest
{
    public static void AssertEqual<T>(T expected, T actual, string context)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException(
                $"Expected {expected}, but was {actual} ({context}).");
        }
    }

    public static void AssertTrue(bool condition, string context)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Expected true ({context}).");
        }
    }

    public static void AssertFalse(bool condition, string context)
    {
        if (condition)
        {
            throw new InvalidOperationException($"Expected false ({context}).");
        }
    }

    public static void AssertThrows<TException>(Action action, string context)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return; // expected exception type — test passes
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Expected {typeof(TException).Name} but got {ex.GetType().Name} ({context}).");
        }

        throw new InvalidOperationException(
            $"Expected {typeof(TException).Name} but no exception was thrown ({context}).");
    }
}

/*
 * =========================================================================
 * SECTION 4: TEST NAMING — MethodName_Scenario_ExpectedResult
 * =========================================================================
 *
 * Names should read like specifications — not "Test1" or "CheckGrade".
 *
 *   MethodName_Scenario_ExpectedResult
 *
 * Examples used in this chapter:
 *
 *   GetLetterGrade_Score95_ReturnsA
 *   IsPassing_Score59_ReturnsFalse
 *   GetLetterGrade_ScoreOver100_ThrowsArgumentOutOfRange
 *
 * Variations teams use:
 *
 *   MethodName_ExpectedResult_WhenScenario   (xUnit style)
 *   Should_ExpectedResult_When_Scenario      (BDD style)
 *
 * Pick one pattern per project and stay consistent. A failing test name
 * should tell you what broke without opening the method body.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 5: UNIT TESTS VS INTEGRATION TESTS
 * =========================================================================
 *
 * | Aspect           | Unit test                         | Integration test              |
 * |------------------|-----------------------------------|-------------------------------|
 * | Scope            | One class/method, fast            | Multiple components together  |
 * | Dependencies     | Real or none (pure logic)         | Real DB, HTTP, file system    |
 * | Speed            | Milliseconds                      | Seconds to minutes            |
 * | When it fails    | Logic bug in one unit             | Wiring, config, environment   |
 * | Count            | Many (testing pyramid base)       | Fewer, slower, higher in pyramid |
 *
 * UNIT example (this chapter):
 *   GradeCalculator.GetLetterGrade(95) → "A"
 *   No database, no network, no disk — only in-memory objects.
 *
 * INTEGRATION example (hypothetical — not implemented here):
 *   POST /api/grades with JSON → API controller → GradeCalculator
 *   → save to SQL Server → read back → assert HTTP 200 and stored grade.
 *   Failure might be connection string, EF mapping, or serialization —
 *   not just calculator math.
 *
 * Rule of thumb: many fast unit tests, fewer integration tests, even fewer
 * end-to-end UI tests.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 6: TEST PROJECT STRUCTURE
 * =========================================================================
 *
 * Typical .NET solution layout for this module:
 *
 *   09. Unit Testing/
 *     Testing.sln
 *     01. Unit Testing Basics/
 *       UnitTestingBasics.csproj              ← app under test (OutputType Exe here)
 *       Program.cs                            ← reading entry + manual AAA demo
 *       Services/
 *         GradeCalculator.cs                  ← SUT
 *       UnitTestingBasics.Tests/              ← *.Tests suffix convention
 *         UnitTestingBasics.Tests.csproj
 *         GradeCalculatorTests.cs
 *     02. xUnit/                              ← full xUnit chapter (later)
 *     03. MSTest/
 *     04. Mocking and Test Doubles/
 *
 * Key .csproj pieces in UnitTestingBasics.Tests.csproj:
 *
 *   <PackageReference Include="Microsoft.NET.Test.Sdk" … />
 *   <PackageReference Include="xunit" … />
 *   <PackageReference Include="xunit.runner.visualstudio" … />
 *   <ProjectReference Include="..\UnitTestingBasics.csproj" />
 *
 * Conventions:
 *   • Test class: {ClassUnderTest}Tests  (GradeCalculatorTests)
 *   • Test file often mirrors source file name
 *   • Solution groups both projects for dotnet build / dotnet test
 *
 * Larger apps often use src/MyApp/ and tests/MyApp.Tests/ — same idea.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 7: RUNNING dotnet test
 * =========================================================================
 *
 * The companion project UnitTestingBasics.Tests/ contains xUnit tests.
 * From the 09. Unit Testing folder:
 *
 *   dotnet test Testing.sln
 *   dotnet test "01. Unit Testing Basics/UnitTestingBasics.Tests"
 *
 * dotnet test builds the test project and all ProjectReference targets,
 * discovers [Fact] methods, runs them, and prints passed/failed/skipped.
 *
 * Useful flags:
 *   dotnet test --filter "FullyQualifiedName~GetLetterGrade"
 *   dotnet test --logger "console;verbosity=detailed"
 *
 * This chapter's Program.cs (dotnet run) runs manual AAA tests in Main;
 * dotnet test runs the xUnit project separately — both target GradeCalculator.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 8: PREVIEW — xUnit, MSTest, AND MOCKING
 * =========================================================================
 *
 * COVERED IN DETAIL LATER → 02. xUnit
 *   [Fact] and [Theory], [InlineData], shared fixtures, parallel runs
 *
 * COVERED IN DETAIL LATER → 03. MSTest
 *   [TestClass], [TestMethod], [DataRow], Visual Studio Test Explorer
 *
 * COVERED IN DETAIL LATER → 04. Mocking and Test Doubles
 *   Moq/NSubstitute, IEmailService fake, Verify() call counts, isolating SUT
 *
 * This chapter uses ManualTest helpers so AAA and naming are visible in plain
 * C#. Open UnitTestingBasics.Tests/GradeCalculatorTests.cs for framework Assert.*
 * -------------------------------------------------------------------------
 */

public class Program
{
    /*
     * =========================================================================
     * SECTION 9: DEMONSTRATION — manual AAA run in Main
     * =========================================================================
     *
     * Main orchestrates the same scenarios as GradeCalculatorTests.cs but
     * prints PASS/FAIL to the console using ManualTest helpers from Section 3.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Manual AAA demo — GradeCalculator ===");
        Console.WriteLine();

        int passed = 0;
        int failed = 0;
        List<string> failures = new List<string>();

        RunManualTest(
            "GetLetterGrade_Score95_ReturnsA",
            () =>
            {
                GradeCalculator calculator = new GradeCalculator(); // Arrange
                string grade = calculator.GetLetterGrade(95);       // Act
                ManualTest.AssertEqual("A", grade, "letter grade for 95"); // Assert
            },
            ref passed,
            ref failed,
            failures);

        RunManualTest(
            "GetLetterGrade_Score72_ReturnsC",
            () =>
            {
                GradeCalculator calculator = new GradeCalculator();
                string grade = calculator.GetLetterGrade(72);
                ManualTest.AssertEqual("C", grade, "letter grade for 72");
            },
            ref passed,
            ref failed,
            failures);

        RunManualTest(
            "IsPassing_Score60_ReturnsTrue",
            () =>
            {
                GradeCalculator calculator = new GradeCalculator();
                bool passing = calculator.IsPassing(60);
                ManualTest.AssertTrue(passing, "score 60 should pass");
            },
            ref passed,
            ref failed,
            failures);

        RunManualTest(
            "IsPassing_Score59_ReturnsFalse",
            () =>
            {
                GradeCalculator calculator = new GradeCalculator();
                bool passing = calculator.IsPassing(59);
                ManualTest.AssertFalse(passing, "score 59 should not pass");
            },
            ref passed,
            ref failed,
            failures);

        RunManualTest(
            "GetLetterGrade_ScoreOver100_ThrowsArgumentOutOfRange",
            () =>
            {
                GradeCalculator calculator = new GradeCalculator();
                ManualTest.AssertThrows<ArgumentOutOfRangeException>(
                    () => calculator.GetLetterGrade(101),
                    "scores above 100 must throw");
            },
            ref passed,
            ref failed,
            failures);

        Console.WriteLine();
        Console.WriteLine($"Results: {passed} passed, {failed} failed");

        if (failures.Count > 0)
        {
            Console.WriteLine("Failures:");
            foreach (string message in failures)
            {
                Console.WriteLine($"  - {message}");
            }

            Environment.ExitCode = 1;
            return;
        }

        Console.WriteLine("All manual tests passed.");
        Console.WriteLine();
        Console.WriteLine("Unit tests: GradeCalculator in isolation (Section 5).");
        Console.WriteLine("Framework tests: dotnet test Testing.sln (Section 7).");
        Console.WriteLine("xUnit Assert.*: UnitTestingBasics.Tests/GradeCalculatorTests.cs");
    }

    private static void RunManualTest(
        string testName,
        Action testBody,
        ref int passed,
        ref int failed,
        List<string> failures)
    {
        try
        {
            testBody();
            passed++;
            Console.WriteLine($"  PASS  {testName}");
        }
        catch (Exception ex)
        {
            failed++;
            string message = $"{testName}: {ex.Message}";
            failures.Add(message);
            Console.WriteLine($"  FAIL  {message}");
        }
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — UNIT TESTING BASICS
 * =============================================================================
 *
 * AAA pattern
 *   Arrange — setup inputs and dependencies
 *   Act     — call the method under test once
 *   Assert  — verify outcome (value, exception, side effect)
 *
 * Naming
 *   MethodName_Scenario_ExpectedResult
 *   Example: IsPassing_Score59_ReturnsFalse
 *
 * Unit vs integration
 *   Unit        — one class, no I/O, fast, many tests
 *   Integration — real DB/API/files, slower, fewer tests
 *
 * Solution layout
 *   MyApp/MyApp.csproj
 *   MyApp.Tests/MyApp.Tests.csproj  → ProjectReference to MyApp
 *
 * CLI
 *   dotnet test                          run all tests in solution/project
 *   dotnet test --filter "Name~Grade"    run matching tests
 *   dotnet test --logger "console;verbosity=detailed"
 *   dotnet run --project UnitTestingBasics.csproj   manual tests in Main
 *
 * Create test project (reference)
 *   dotnet new xunit -n MyApp.Tests -o MyApp.Tests
 *   dotnet sln add MyApp.Tests/MyApp.Tests.csproj
 *   dotnet add MyApp.Tests reference ../MyApp/MyApp.csproj
 *
 * xUnit preview (companion test project)
 *   [Fact]                              marks a test method
 *   Assert.Equal(expected, actual)
 *   Assert.True(condition) / Assert.False(condition)
 *   Assert.Throws<T>(() => action())
 *
 * Later chapters
 *   02. xUnit — [Fact], [Theory], fixtures
 *   03. MSTest — [TestMethod], [TestClass]
 *   04. Mocking and Test Doubles — Moq, stubs, fakes
 *
 * =============================================================================
 */
