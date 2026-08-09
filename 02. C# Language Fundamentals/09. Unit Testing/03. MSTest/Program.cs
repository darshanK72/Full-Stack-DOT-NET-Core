/*
 * TOPIC: Microsoft MSTest — attributes, parameterized tests, Assert.*, lifecycle
 *        hooks, and dotnet test discovery for .NET unit tests.
 *
 * WHY IT MATTERS:
 *   MSTest ships with Visual Studio templates and appears in many enterprise
 *   solutions. [TestClass], [TestMethod], [DataTestMethod], [DataRow], and
 *   Assert.* are the vocabulary you read in MSTest test projects — and they map
 *   cleanly to xUnit (Chapter 02) once you know the attribute names.
 *
 * WHAT YOU WILL LEARN:
 *   1.  MSTest project layout — production code + MSTest.Tests companion project
 *   2.  System under test — Models/CustomerTier.cs, Services/DiscountCalculator.cs
 *   3.  [TestClass] and [TestMethod] — DiscountCalculatorCoreTests.cs
 *   4.  Test naming conventions — MethodUnderTest_ExpectedResult_WhenCondition
 *   5.  [TestInitialize] and [TestCleanup] — per-test setup and teardown
 *   6.  [DataTestMethod] and [DataRow] — DiscountCalculatorDataTests.cs
 *   7.  Assert.* — AreEqual, IsTrue, IsFalse, ThrowsException — AssertTests.cs
 *   8.  [ClassInitialize] and [ClassCleanup] — DiscountCalculatorLifecycleTests.cs
 *   9.  [AssemblyInitialize] — preview (once per test assembly)
 *   10. MSTest vs xUnit comparison and running dotnet test
 */

using System;
using MSTest.Models;
using MSTest.Services;

namespace MSTest;

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * SECTION 1: MSTEST PROJECT LAYOUT
         *
         * MSTest NuGet packages (referenced in MSTest.Tests/MSTest.Tests.csproj):
         *
         *   Microsoft.NET.Test.Sdk  — common host for dotnet test
         *   MSTest.TestFramework    — [TestClass], [TestMethod], Assert.*
         *   MSTest.TestAdapter      — discovery bridge for Test Explorer / CLI
         *
         * This folder structure:
         *
         *   03. MSTest/
         *     MSTest.csproj                    — production code (SUT)
         *     Program.cs                       — reading entry (you are here)
         *     Models/CustomerTier.cs
         *     Services/DiscountCalculator.cs
         *     MSTest.Tests/
         *       MSTest.Tests.csproj            — test project (ProjectReference → ..)
         *       DiscountCalculatorCoreTests.cs
         *       DiscountCalculatorDataTests.cs
         *       DiscountCalculatorAssertTests.cs
         *       DiscountCalculatorLifecycleTests.cs
         *
         * Tests live in a separate project so production builds never ship test code.
         * Chapter 01. Unit Testing Basics assumed: Arrange-Act-Assert, red/green, SUT.
         * -------------------------------------------------------------------------
         */

        var calculator = new DiscountCalculator();

        Console.WriteLine("=== DiscountCalculator demo (production code) ===");
        Console.WriteLine();

        decimal tierTotal = calculator.ApplyTierDiscount(120.00m, CustomerTier.Silver);
        Console.WriteLine($"Silver tier on $120.00 → ${tierTotal:F2}");

        decimal couponTotal = calculator.ApplyCoupon(50.00m, "WELCOME20");
        Console.WriteLine($"WELCOME20 on $50.00    → ${couponTotal:F2}");

        bool freeShipping = calculator.IsEligibleForFreeShipping(80.00m, CustomerTier.Standard);
        Console.WriteLine($"Free shipping at $80 Standard tier → {freeShipping}");

        Console.WriteLine();
        Console.WriteLine("=== Companion tests — open MSTest.Tests/ while reading ===");
        Console.WriteLine("Run: dotnet test \"03. MSTest/MSTest.Tests/MSTest.Tests.csproj\"");
        Console.WriteLine();


        /*
         * SECTION 3: [TestClass] AND [TestMethod]
         *
         * [TestClass] marks a container class. Only public instance methods tagged
         * [TestMethod] (or [DataTestMethod]) inside it are executed as tests.
         *
         *   [TestClass]
         *   public class DiscountCalculatorCoreTests
         *   {
         *       [TestMethod]
         *       public void ApplyPercentDiscount_ReducesTotal_WhenRateIsTenPercent()
         *       {
         *           // Arrange → Act → Assert
         *       }
         *   }
         *
         * File: MSTest.Tests/DiscountCalculatorCoreTests.cs
         *
         * Each [TestMethod] must be independent — no reliance on execution order.
         * If the method completes without a failed Assert, the test passes.
         * An unhandled exception fails the test immediately.
         * -------------------------------------------------------------------------
         */

        Console.WriteLine("[TestClass] + [TestMethod] → DiscountCalculatorCoreTests.cs");
        Console.WriteLine("  • single-scenario percent discount");
        Console.WriteLine("  • standard tier on $100 order");
        Console.WriteLine();


        /*
         * SECTION 4: TEST NAMING CONVENTIONS
         *
         * Readable names show up in Test Explorer, CI logs, and code review:
         *
         *   MethodUnderTest_ExpectedResult_WhenCondition
         *
         * Examples in this chapter:
         *
         *   ApplyPercentDiscount_ReducesTotal_WhenRateIsTenPercent
         *   ApplyCoupon_ThrowsInvalidOperation_WhenOrderBelowMinimum
         *   IsEligibleForFreeShipping_IsTrue_WhenGoldTierRegardlessOfAmount
         *
         * Avoid vague names (Test1, CheckDiscount). The name should describe the
         * behavior under test so a failure message tells you what broke without
         * opening the method body.
         * -------------------------------------------------------------------------
         */

        Console.WriteLine("Naming pattern: MethodUnderTest_ExpectedResult_WhenCondition");
        Console.WriteLine();


        /*
         * SECTION 5: [TestInitialize] AND [TestCleanup]
         *
         * Per-test lifecycle hooks on the same [TestClass]:
         *
         *   [TestInitialize]  — runs BEFORE each [TestMethod] / [DataTestMethod]
         *   [TestCleanup]     — runs AFTER each test, even when Assert fails
         *
         * File: DiscountCalculatorCoreTests.cs — fresh DiscountCalculator and a
         * diagnostic list reset every test; [TestCleanup] clears mutable state.
         *
         * xUnit equivalent: constructor runs before each [Fact]; implement
         * IDisposable.Dispose for post-test cleanup (Chapter 02).
         * -------------------------------------------------------------------------
         */

        Console.WriteLine("[TestInitialize] / [TestCleanup] → DiscountCalculatorCoreTests.cs");
        Console.WriteLine();


        /*
         * SECTION 6: [DataTestMethod] AND [DataRow]
         *
         * [DataTestMethod] marks a parameterized test. Attach one or more [DataRow]
         * attributes; the runner invokes the method once per row, passing row values
         * as method arguments in order.
         *
         *   [DataTestMethod]
         *   [DataRow(100.00, 0.10, 90.00)]
         *   [DataRow(50.00,  0.20, 40.00)]
         *   public void ApplyPercentDiscount_ReturnsExpectedTotal(
         *       double orderTotal, double discountRate, double expectedTotal)
         *
         * File: DiscountCalculatorDataTests.cs
         *
         * Pitfall: [DataRow] does not accept decimal literals — use double and cast
         * to decimal inside the test body when verifying money calculations.
         *
         * Alternative for complex rows: [DynamicData] from a static property/method.
         * xUnit equivalent: [Theory] + [InlineData] — Chapter 02.
         * -------------------------------------------------------------------------
         */

        Console.WriteLine("[DataTestMethod] + [DataRow] → DiscountCalculatorDataTests.cs");
        Console.WriteLine("  • percent discount rows (double → decimal cast)");
        Console.WriteLine("  • tier discount rows (enum + expected total)");
        Console.WriteLine("  • free-shipping boolean rows");
        Console.WriteLine();


        /*
         * SECTION 7: Assert CLASS
         *
         * Namespace: Microsoft.VisualStudio.TestTools.UnitTesting
         *
         * File: DiscountCalculatorAssertTests.cs
         *
         *   Assert.AreEqual(expected, actual)
         *       Value equality. Optional delta overload for double/float tolerance.
         *
         *   Assert.IsTrue(condition) / Assert.IsFalse(condition)
         *       Boolean outcomes — eligibility flags, feature toggles.
         *
         *   Assert.ThrowsException<T>(() => action)
         *       Executes delegate; passes only if exception type T is thrown.
         *       Returns the caught exception for Message inspection.
         *
         *   StringAssert.Contains(substring, actualString)
         *       Substring check on exception messages or formatted output.
         *
         * Other members worth knowing:
         *   Assert.IsNull / IsNotNull, AreSame / AreNotSame, AreNotEqual, Fail(...)
         *   CollectionAssert.AreEqual
         *
         * Failed Assert.* throws AssertFailedException with expected vs actual.
         * -------------------------------------------------------------------------
         */

        Console.WriteLine("Assert.* → DiscountCalculatorAssertTests.cs");
        Console.WriteLine("  • AreEqual, IsTrue, IsFalse");
        Console.WriteLine("  • ThrowsException + StringAssert.Contains");
        Console.WriteLine();


        /*
         * SECTION 8: [ClassInitialize], [ClassCleanup], AND [AssemblyInitialize]
         *
         * Class-level (static, once per [TestClass]):
         *
         *   [ClassInitialize]  — public static void Init(TestContext context)
         *   [ClassCleanup]     — public static void Cleanup()
         *
         * File: DiscountCalculatorLifecycleTests.cs — loads a shared tier-rate table
         * once before any test in the class; [ClassCleanup] releases static state.
         *
         * Assembly-level (preview — heavier setup):
         *
         *   [AssemblyInitialize] on a separate class — runs once before ANY test
         *   in the assembly (e.g. global config, copying deployment files).
         *   Requires [assembly: Parallelize] awareness and careful static state.
         *
         *   [AssemblyCleanup] — runs once after all tests in the assembly finish.
         *
         * COVERED IN DETAIL LATER → advanced test infrastructure topics if needed.
         * For most unit tests, [TestInitialize] + fresh SUT is enough.
         * -------------------------------------------------------------------------
         */

        Console.WriteLine("[ClassInitialize] / [ClassCleanup] → DiscountCalculatorLifecycleTests.cs");
        Console.WriteLine("[AssemblyInitialize] — preview only; once-per-assembly global setup");
        Console.WriteLine();


        /*
         * SECTION 9: MSTEST VS xUNIT
         *
         * Both run under dotnet test. Attribute names differ; AAA structure is the same.
         * Chapter 02. xUnit owns full depth for [Fact], [Theory], fixtures, parallelism.
         * -------------------------------------------------------------------------
         */

        Console.WriteLine("=== MSTest vs xUnit (see table below in QUICK REFERENCE) ===");
        Console.WriteLine();


        /*
         * SECTION 10: RUNNING TESTS
         *
         * From 09. Unit Testing/ or repository root:
         *
         *   dotnet test Testing.sln
         *   dotnet test "03. MSTest/MSTest.Tests/MSTest.Tests.csproj"
         *   dotnet test --filter "FullyQualifiedName~ApplyCoupon"
         *   dotnet test --filter "TestCategory=Integration"   (when using [TestCategory])
         *
         * Visual Studio Test Explorer discovers tests when MSTest.TestAdapter is
         * referenced. Green = all Assert calls passed; red = failed Assert or exception.
         * -------------------------------------------------------------------------
         */

        Console.WriteLine("Run: dotnet test \"09. Unit Testing/Testing.sln\"");
    }
}

/*
 * QUICK REFERENCE — MSTest
 *
 * Packages (test project):
 *   Microsoft.NET.Test.Sdk, MSTest.TestAdapter, MSTest.TestFramework
 *
 * Attributes:
 *   [TestClass]           group tests; required container
 *   [TestMethod]          single test case (no parameters)
 *   [DataTestMethod]      parameterized test — use with [DataRow]
 *   [DataRow(a, b, c)]    one input row (repeat on same method)
 *   [TestInitialize]      before each test (instance method)
 *   [TestCleanup]         after each test (instance method)
 *   [ClassInitialize]     once before all tests in class (static + TestContext)
 *   [ClassCleanup]        once after all tests in class (static)
 *   [AssemblyInitialize]  once per test assembly (static, separate class)
 *   [Ignore("reason")]    skip test
 *
 * Assert (common):
 *   Assert.AreEqual(expected, actual)
 *   Assert.IsTrue(condition) / Assert.IsFalse(condition)
 *   Assert.ThrowsException<T>(() => code)
 *   Assert.IsNull(x) / Assert.IsNotNull(x)
 *   StringAssert.Contains(substring, text)
 *
 * MSTest vs xUnit:
 * +---------------------------+---------------------------+---------------------------+
 * | Concept                   | MSTest                    | xUnit                     |
 * +---------------------------+---------------------------+---------------------------+
 * | Mark test class           | [TestClass]               | (none — plain public class)|
 * | Single test case          | [TestMethod]              | [Fact]                    |
 * | Parameterized test        | [DataTestMethod]+[DataRow]| [Theory]+[InlineData]     |
 * | Skip test                 | [Ignore("reason")]        | [Fact(Skip = "reason")]   |
 * | Before each test          | [TestInitialize]          | constructor               |
 * | After each test           | [TestCleanup]             | IDisposable.Dispose       |
 * | Shared class setup        | [ClassInitialize]         | IClassFixture<T>          |
 * | Equality assert           | Assert.AreEqual(exp, act) | Assert.Equal(exp, act)    |
 * | Boolean assert            | Assert.IsTrue(cond)       | Assert.True(cond)         |
 * | Exception assert          | Assert.ThrowsException<T> | Assert.Throws<T>          |
 * | Test output in reports    | Console / Debug           | ITestOutputHelper           |
 * | Default parallel run      | Sequential per assembly*  | Parallel by default       |
 * +---------------------------+---------------------------+---------------------------+
 * * MSTest V3+ improves parallelism; see assembly Parallelize attributes for tuning.
 *
 * Test naming:
 *   MethodUnderTest_ExpectedResult_WhenCondition
 *
 * Commands:
 *   dotnet test Testing.sln
 *   dotnet test --filter "FullyQualifiedName~DiscountCalculator"
 */
