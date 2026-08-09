using System;
using AcademicAssessment;
using Xunit;

namespace ExamScoreValidator.Tests;

/*
 * xUnit companion tests for CourseGradeService.
 * Complete each TODO using Arrange-Act-Assert.
 * Naming: MethodName_Scenario_ExpectedResult
 */
public class CourseGradeServiceTests
{
    [Fact]
    public void GetLetterGrade_Score95_ReturnsA()
    {
        // TODO: Arrange CourseGradeService, Act GetLetterGrade(95), Assert "A"
        throw new NotImplementedException();
    }

    [Fact]
    public void GetLetterGrade_Score72_ReturnsC()
    {
        // TODO: Assert "C" for score 72
        throw new NotImplementedException();
    }

    [Fact]
    public void IsPassing_Score60_ReturnsTrue()
    {
        // TODO: Assert.True for score 60
        throw new NotImplementedException();
    }

    [Fact]
    public void IsPassing_Score59_ReturnsFalse()
    {
        // TODO: Assert.False for score 59
        throw new NotImplementedException();
    }
}

public class CourseGradeServiceValidationTests
{
    [Fact]
    public void GetLetterGrade_ScoreOver100_ThrowsArgumentOutOfRange()
    {
        // TODO: Assert.Throws<ArgumentOutOfRangeException>(() => service.GetLetterGrade(101))
        throw new NotImplementedException();
    }

    [Fact]
    public void GetLetterGrade_NegativeScore_ThrowsArgumentOutOfRange()
    {
        // TODO: Assert.Throws for score -1
        throw new NotImplementedException();
    }
}
