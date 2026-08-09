using System;
using UnitTestingBasics.Services;
using Xunit;

namespace UnitTestingBasics.Tests;

/*
 * =========================================================================
 * SECTION 9: xUnit [Fact] AND Assert.Equal / Assert.True / Assert.False
 * =========================================================================
 *
 * The companion test project runs with dotnet test. Each [Fact] method is
 * one independent test discovered and executed by the xUnit runner.
 *
 *   [Fact]  — marks a parameterless test method (no return value)
 *   Assert.Equal(expected, actual)  — values must match (uses IEquatable<T> or Equals)
 *   Assert.True(condition)          — condition must be true
 *   Assert.False(condition)         — condition must be false
 *
 * Test class naming convention: {ClassUnderTest}Tests → GradeCalculatorTests
 * Test method naming: MethodName_Scenario_ExpectedResult (same as manual tests)
 *
 * COVERED IN DETAIL LATER → 02. xUnit
 *   [Theory], [InlineData], fixtures, ITestOutputHelper, parallel execution
 *
 * COVERED IN DETAIL LATER → 03. MSTest
 *   [TestClass], [TestMethod], [DataRow] — same AAA pattern, different attributes
 * -------------------------------------------------------------------------
 */
public class GradeCalculatorTests
{
    [Fact]
    public void GetLetterGrade_Score95_ReturnsA()
    {
        // Arrange
        GradeCalculator calculator = new GradeCalculator();

        // Act
        string grade = calculator.GetLetterGrade(95);

        // Assert
        Assert.Equal("A", grade);
    }

    [Fact]
    public void GetLetterGrade_Score72_ReturnsC()
    {
        GradeCalculator calculator = new GradeCalculator();
        string grade = calculator.GetLetterGrade(72);
        Assert.Equal("C", grade);
    }

    [Fact]
    public void IsPassing_Score60_ReturnsTrue()
    {
        GradeCalculator calculator = new GradeCalculator();
        bool passing = calculator.IsPassing(60);
        Assert.True(passing);
    }

    [Fact]
    public void IsPassing_Score59_ReturnsFalse()
    {
        GradeCalculator calculator = new GradeCalculator();
        bool passing = calculator.IsPassing(59);
        Assert.False(passing);
    }
}

/*
 * =========================================================================
 * SECTION 10: Assert.Throws<T> — EXPECTING AN EXCEPTION
 * =========================================================================
 *
 * When invalid input must throw, assert the exception type (and optionally
 * message) instead of a return value:
 *
 *   Assert.Throws<ArgumentOutOfRangeException>(() => calculator.GetLetterGrade(101));
 *
 * xUnit executes the lambda; if TException is not thrown, the test fails.
 *
 * COVERED IN DETAIL LATER → 04. Mocking and Test Doubles
 *   Replace real dependencies (IEmailService, HttpClient) with mocks/stubs
 *   so SUT tests stay fast and deterministic.
 * -------------------------------------------------------------------------
 */
public class GradeCalculatorValidationTests
{
    [Fact]
    public void GetLetterGrade_ScoreOver100_ThrowsArgumentOutOfRange()
    {
        GradeCalculator calculator = new GradeCalculator();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.GetLetterGrade(101));
    }

    [Fact]
    public void GetLetterGrade_NegativeScore_ThrowsArgumentOutOfRange()
    {
        GradeCalculator calculator = new GradeCalculator();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.GetLetterGrade(-1));
    }
}
