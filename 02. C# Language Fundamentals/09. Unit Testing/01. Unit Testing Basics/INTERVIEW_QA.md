# Unit Testing Basics — Interview Q&A

---

## Q1. What is the Arrange-Act-Assert (AAA) pattern and why is it the standard structure for unit tests?

**Concepts**
- AAA pattern structure
- test readability
- separation of concerns in tests
- Arrange phase setup
- Act phase invocation
- Assert phase verification

**Answer**

The Arrange-Act-Assert pattern divides every unit test into three clearly separated phases. In the Arrange phase you set up everything needed for the test: create the system under test, initialize input values, and configure any dependencies. In the Act phase you invoke exactly one operation on the system under test — typically a single method call. In the Assert phase you verify that the outcome matches your expectation.

This structure matters for several reasons. It makes tests easier to read because anyone scanning the file can immediately identify what is being set up, what is being executed, and what is being checked. It enforces discipline by discouraging tests that call multiple operations and check multiple unrelated outcomes. It also makes failures easier to diagnose: if the assertion fails, you already know the act succeeded enough to reach that line, and the isolation of the arrange block tells you exactly what state was involved.

```csharp
[Fact]
public void GetLetterGrade_Score95_ReturnsA()
{
    // Arrange
    var calculator = new GradeCalculator();
    int score = 95;

    // Act
    string result = calculator.GetLetterGrade(score);

    // Assert
    Assert.Equal("A", result);
}
```

Deviating from AAA — such as asserting partway through a test or calling Act twice — makes failures ambiguous and violates the single-responsibility principle of tests.

---

## Q2. What are the FIRST principles of unit testing?

**Concepts**
- FIRST acronym
- fast test execution
- test isolation
- repeatability
- self-validating assertion
- timely authorship

**Answer**

FIRST is a set of five properties that distinguish high-quality unit tests from fragile or unreliable ones.

Fast means tests run in milliseconds, not seconds. A suite of thousands of tests should complete quickly enough to run on every save or commit without disrupting the developer's flow. Slow tests get skipped, which defeats their purpose.

Isolated means each test is completely independent of every other test. Tests must not share mutable state through static fields, databases, or files. Any test should pass or fail regardless of what order it runs in or what other tests have executed.

Repeatable means a test produces the same result every time, on every machine, at any time of day. Tests that rely on the current date, random numbers, or network availability are not repeatable.

Self-validating means the test itself declares pass or fail through assertions — no human needs to read output logs and decide whether the result is correct. If `Assert.Equal("A", result)` passes, the test passes.

Timely means tests are written at the same time as the production code, ideally before it (TDD). Tests written long after the code tend to be shaped around existing bugs rather than intended behavior.

Applied to `GradeCalculator`, each test is sub-millisecond (fast), creates its own instance (isolated), uses deterministic integer inputs (repeatable), uses `Assert.Equal` (self-validating), and was written alongside the implementation (timely).

---

## Q3. What is the test pyramid, and why do unit tests occupy the base?

**Concepts**
- test pyramid layers
- unit tests vs integration tests vs E2E tests
- test execution speed
- maintenance cost
- feedback loop

**Answer**

The test pyramid is a visual model proposed by Mike Cohn that describes the ideal distribution of automated tests across three layers. Unit tests form the base, integration tests occupy the middle tier, and end-to-end (E2E) tests sit at the top.

Unit tests dominate the base because they are cheap to write, execute in milliseconds, require no external infrastructure, and pinpoint failures to a single class or method. A project like `GradeCalculator` can have dozens of unit tests that run in under a second and tell you exactly which score boundary broke.

Integration tests verify that multiple components work together correctly — for example, a service that reads from a database and calls `GetLetterGrade` to store a grade. They are slower and require a test database or in-memory substitute. They should be fewer in number than unit tests.

E2E tests simulate a real user driving the full application stack, from HTTP request through controller, service, database, and back to the HTTP response. They are the slowest, most brittle, and most expensive to maintain, so they should cover only the most critical user journeys.

Inverting the pyramid — having many E2E tests and few unit tests — leads to slow CI pipelines, flaky failures caused by network or infrastructure issues, and poor diagnostic precision when something breaks. The pyramid shape keeps the feedback loop tight: most bugs are caught immediately in the unit layer, while integration and E2E tests validate wiring and end-to-end behavior.

---

## Q4. What naming convention should you use for test methods, and why does it matter?

**Concepts**
- MethodName_Scenario_ExpectedResult convention
- test as documentation
- failure diagnostics
- alternative naming styles
- xUnit test output

**Answer**

The most widely adopted convention in .NET is `MethodName_Scenario_ExpectedResult`. Each segment is separated by an underscore and answers a different question: which method is under test, what input or state condition was arranged, and what the test expects to observe. Examples from `GradeCalculatorTests` include `GetLetterGrade_Score95_ReturnsA` and `IsPassing_Score60_ReturnsTrue`.

This convention matters because test names serve as living documentation. When a CI build fails and you see `GetLetterGrade_ScoreNegative1_ThrowsArgumentOutOfRangeException` in the failure list, you know immediately which method failed, under what input, and what behavior regressed — before reading a single line of test code.

Alternative conventions exist: BDD-style names such as `WhenScoreIs95_GetLetterGrade_ShouldReturnA` are common in teams that treat tests as specifications. Some teams omit the method prefix and write descriptive phrases using underscores, like `Score_below_zero_should_throw`. What matters is that the pattern is consistent across the codebase so developers can scan failure reports without context switching.

In xUnit the test name appears in the test runner output, the Visual Studio Test Explorer, and GitHub Actions summaries. A descriptive name turns the test suite into a readable specification of the system's behavior, which is especially valuable when the production code is unfamiliar or when onboarding new team members.

---

## Q5. How does xUnit's `[Fact]` attribute work, and how does it differ from `[Theory]`?

**Concepts**
- xUnit Fact attribute
- parameterized tests with Theory
- InlineData attribute
- test discovery
- data-driven testing

**Answer**

The `[Fact]` attribute marks a method as a single, self-contained test with no external input parameters. The xUnit test runner discovers any public, parameterless method decorated with `[Fact]` and executes it. If no exception is thrown and all assertions pass, the test is green. `[Fact]` is the right choice when a test scenario involves exactly one specific case, such as verifying that a score of 95 returns "A".

The `[Theory]` attribute, combined with one or more `[InlineData]` attributes, supports parameterized tests — one test method executed multiple times with different inputs. This is valuable for boundary value testing where the same assertion logic applies across many input values:

```csharp
[Theory]
[InlineData(100, "A")]
[InlineData(90, "A")]
[InlineData(89, "B")]
[InlineData(60, "D")]
[InlineData(0, "F")]
public void GetLetterGrade_BoundaryScores_ReturnsCorrectGrade(int score, string expected)
{
    var calculator = new GradeCalculator();
    Assert.Equal(expected, calculator.GetLetterGrade(score));
}
```

Each `[InlineData]` row appears as a separate test case in the test runner with its own pass/fail status, giving you precise feedback about which boundary value regressed. Using `[Theory]` eliminates duplication across similar `[Fact]` methods while keeping diagnostic granularity.

---

## Q6. What does it mean for the System Under Test (SUT) to be a "good" unit test target?

**Concepts**
- SUT definition
- pure logic vs I/O-bound logic
- deterministic behavior
- dependency isolation
- testability design

**Answer**

A good SUT is a class or function whose behavior is entirely determined by its inputs, with no side effects that reach outside the process boundary. No file system access, no database queries, no HTTP calls, no reading from the clock. When the SUT depends only on its arguments, every execution is deterministic and every test is fast and isolated by default.

`GradeCalculator` is an excellent SUT for unit testing. Both `GetLetterGrade(int score)` and `IsPassing(int score)` take a single integer, perform arithmetic comparisons, and return a value or throw an exception. There is no state carried between calls, no injected service, no external dependency. You can construct `new GradeCalculator()` in every test without setup overhead and the tests will run in microseconds.

When a class does I/O — for example, a `GradeRepository` that reads scores from SQL Server — it is not a good direct SUT for unit tests. The correct approach is to abstract the I/O behind an interface (`IGradeRepository`), inject it into the class under test, and substitute a test double (mock or stub) in unit tests. This keeps the unit test fast and isolated while leaving integration tests to verify the real database interaction.

Designing for testability often produces better architecture: classes with fewer responsibilities, explicit dependencies, and no hidden global state.

---

## Q7. How do you test that a method throws the correct exception using xUnit?

**Concepts**
- Assert.Throws generic method
- ArgumentOutOfRangeException
- exception type verification
- exception message verification
- Act-Assert pattern for exceptions

**Answer**

xUnit provides `Assert.Throws<TException>` to verify that a specific exception type is thrown during the Act phase. The method accepts a delegate (typically a lambda) that invokes the operation expected to throw, and it returns the caught exception so you can perform additional assertions on its message or properties.

```csharp
[Fact]
public void GetLetterGrade_ScoreNegative1_ThrowsArgumentOutOfRangeException()
{
    // Arrange
    var calculator = new GradeCalculator();

    // Act & Assert
    var ex = Assert.Throws<ArgumentOutOfRangeException>(
        () => calculator.GetLetterGrade(-1)
    );

    Assert.Contains("score", ex.ParamName, StringComparison.OrdinalIgnoreCase);
}
```

The Act and Assert phases are sometimes merged when testing exceptions because the act is the lambda argument to `Assert.Throws`. This is idiomatic xUnit and acceptable as long as the test still covers a single scenario.

If you wrap the call in a `try/catch` block instead and assert inside the catch, you risk the test passing silently when no exception is thrown — because the assertion line is never reached. `Assert.Throws` guarantees that the test fails if the exception is not thrown, if a different exception type is thrown, or if no exception occurs at all.

For async methods, use `await Assert.ThrowsAsync<TException>` with the async lambda form. Always test boundary inputs such as -1, 101, and `int.MinValue` for a method like `GetLetterGrade` that validates its input range.

---

## Q8. What is boundary value testing and which score values should you test for `GradeCalculator`?

**Concepts**
- boundary value analysis
- off-by-one errors
- partition boundaries
- equivalence classes
- test coverage completeness

**Answer**

Boundary value analysis is a testing technique that focuses on values at and immediately adjacent to the edges of valid input ranges and grade partitions. Most bugs in range-based logic are off-by-one errors: a `>=` written as `>`, or a `<=` written as `<`. Testing at exact boundaries and their neighbors catches these with minimal test cases.

For `GetLetterGrade`, which maps 0–100 to letter grades and throws on values outside that range, the meaningful boundary values are:

- **-1**: just below the valid minimum — should throw `ArgumentOutOfRangeException`
- **0**: minimum valid score — should return "F"
- **59**: highest score that returns "F"
- **60**: lowest score that returns "D"
- **69/70**: D/C boundary
- **79/80**: C/B boundary
- **89/90**: B/A boundary
- **100**: maximum valid score — should return "A"
- **101**: just above the valid maximum — should throw `ArgumentOutOfRangeException`

Testing only representative mid-range values (50, 75, 95) would miss a misplaced `>` vs `>=` at any of these boundaries. A `[Theory]` with `[InlineData]` for each boundary value is the idiomatic way to cover all of them without duplicating test logic. Equivalence class partitioning — choosing one representative value per grade band — complements boundary testing but should not replace it.

---

## Q9. What is the difference between state testing and interaction testing?

**Concepts**
- state verification
- interaction verification
- mocks vs assertions on return values
- test doubles
- over-specification risk

**Answer**

State testing verifies what the SUT returns or what state it produces after the act. You examine the return value or the observable state of an object and compare it against the expected value using assertions like `Assert.Equal` and `Assert.True`. This is the appropriate style for `GradeCalculator` because both methods return values and have no side effects:

```csharp
Assert.Equal("B", calculator.GetLetterGrade(85));
Assert.True(calculator.IsPassing(60));
```

Interaction testing verifies that the SUT called a collaborator in a specific way — for example, that a service invoked `repository.Save(grade)` exactly once. You use a mock framework such as Moq or NSubstitute to record and verify calls on injected dependencies.

Interaction testing is appropriate when the observable outcome of an operation is a side effect rather than a return value: sending an email, writing to a database, raising an event. Over-using interaction testing creates brittle tests that break whenever you refactor internal implementation details, even if the external behavior is unchanged.

For pure logic classes like `GradeCalculator`, state testing is always preferred. Introduce interaction testing only when the SUT's purpose is to orchestrate calls to collaborators. Mixing both styles in the same test is a common source of test fragility and should be avoided unless there is a genuine need to verify both the return value and a required side effect in one scenario.

---

## Q10. How should you structure a .NET test project relative to the production code project?

**Concepts**
- test project separation
- naming conventions for test projects
- folder mirroring
- project references
- build isolation

**Answer**

The standard .NET convention is to place unit tests in a separate class library project whose name matches the production project with a `.Tests` suffix. If the production code lives in `GradeBook.Core`, the tests go in `GradeBook.Core.Tests`. Both projects live in the same solution, but the test project is never referenced by the production project — the dependency flows only one way: the test project references the production project.

Within the test project, folder and namespace structure mirrors the production project. If production has `Services/GradeCalculator.cs`, the test project has `Services/GradeCalculatorTests.cs`. This mirroring makes it trivial to find the tests for any given class and ensures the test namespace matches the pattern `GradeBook.Core.Tests.Services`.

Using a dedicated project rather than mixing tests into the production assembly keeps the production binary free of test frameworks and test-only NuGet packages. It also enables separate build configurations and allows the CI pipeline to run `dotnet test` only on changed test projects.

In .NET 10, a new xUnit test project is created with:

```
dotnet new xunit -n GradeBook.Core.Tests
dotnet add GradeBook.Core.Tests/GradeBook.Core.Tests.csproj reference GradeBook.Core/GradeBook.Core.csproj
```

Multiple test classes within the same project can be organized into files by either the class under test (`GradeCalculatorTests.cs`) or by test concern (`GradeCalculatorValidationTests.cs`), as demonstrated in the `GradeCalculatorValidationTests` class that groups exception-throwing tests separately.

---

## Q11. What assertion methods does xUnit provide, and when should you choose each one?

**Concepts**
- Assert.Equal semantics
- Assert.True and Assert.False
- Assert.Throws generic
- Assert.Null and Assert.NotNull
- meaningful assertion messages

**Answer**

xUnit's `Assert` class provides specific assertion methods designed to produce clear failure messages that explain exactly what went wrong.

`Assert.Equal(expected, actual)` is the most common assertion. It compares two values for equality and on failure shows both the expected and actual values side by side. The convention is expected first, actual second — deviating from this order produces misleading failure messages. Use it for return values: `Assert.Equal("A", calculator.GetLetterGrade(95))`.

`Assert.True(condition)` and `Assert.False(condition)` are appropriate when the method returns a boolean and the assertion reads naturally as a truth claim: `Assert.True(calculator.IsPassing(60))`. Avoid using them to compare non-boolean values — `Assert.True(result == "A")` gives a less informative failure message than `Assert.Equal("A", result)`.

`Assert.Throws<TException>` verifies that a specific exception type is raised by a delegate. It returns the caught exception for further inspection, such as checking `ParamName` or `Message`.

`Assert.Null(value)` and `Assert.NotNull(value)` check reference nullability explicitly — useful when testing factory methods that should or should not return null.

`Assert.Contains(expected, collection)` and `Assert.InRange(actual, low, high)` are useful for collection membership and numeric range checks respectively.

Prefer the most specific assertion available: `Assert.Equal` over `Assert.True(a == b)`, `Assert.Throws` over a bare `try/catch`, `Assert.InRange` over `Assert.True(x >= low && x <= high)`. Specific assertions produce specific failure messages.

---

## Q12. What is a test fixture, and how does xUnit manage shared setup and teardown?

**Concepts**
- test fixture lifetime
- constructor and Dispose pattern
- IClassFixture interface
- shared context vs per-test setup
- test isolation risk

**Answer**

A test fixture is the context — objects, data, and state — that tests rely on. In xUnit, each test class is instantiated fresh for every test method. The constructor runs before each test and `Dispose` (if the class implements `IDisposable`) runs after. This per-test instantiation is the primary mechanism for test isolation and is the reason you should set up your `GradeCalculator` instance in either the constructor or directly in the Arrange block of each test.

```csharp
public class GradeCalculatorTests : IDisposable
{
    private readonly GradeCalculator _calculator;

    public GradeCalculatorTests()
    {
        _calculator = new GradeCalculator(); // runs before each test
    }

    public void Dispose()
    {
        // runs after each test — release resources here
    }
}
```

When a fixture is expensive to create (a database connection, a file system stub), you can share it across all tests in a class using `IClassFixture<TFixture>`. The fixture is constructed once, shared across all test methods in the class, and disposed after the last test completes. This trades isolation for performance and should only be used for read-only, immutable shared state.

For `GradeCalculator`, per-test instantiation is correct — the object is cheap to construct and has no mutable shared state. Sharing a stateful SUT across tests through a class fixture would create hidden ordering dependencies and violate the Isolated principle of FIRST.

---

## Q13. What does single responsibility mean in the context of unit tests?

**Concepts**
- one logical assertion per test
- test as specification
- failure localization
- multiple assertions anti-pattern
- assertion granularity

**Answer**

Single responsibility in unit testing means each test method verifies exactly one behavior or outcome. In practice this usually means one logical assertion, though in some cases a small cluster of closely related assertions is acceptable when they together verify a single concept.

When a test contains many unrelated assertions, a failure in the first assertion causes the test runner to skip all subsequent assertions in that method. You learn that something is wrong but not whether the remaining assertions would also fail. Decomposing into separate focused tests gives you a complete failure picture: you can see that score 89 returns "B" correctly but score 90 incorrectly returns "B" instead of "A" — both failures visible simultaneously.

Consider this anti-pattern:

```csharp
[Fact]
public void GetLetterGrade_AllGrades_ReturnCorrectLetters() // too broad
{
    var calc = new GradeCalculator();
    Assert.Equal("A", calc.GetLetterGrade(95));
    Assert.Equal("B", calc.GetLetterGrade(85));
    Assert.Equal("C", calc.GetLetterGrade(75));
    Assert.Equal("D", calc.GetLetterGrade(65));
    Assert.Equal("F", calc.GetLetterGrade(50));
}
```

If the "B" assertion fails, the "C", "D", and "F" assertions never run. Splitting these into individual `[Fact]` methods — or a `[Theory]` with `[InlineData]` — keeps each failure independent and the test name self-documenting. The test name `GetLetterGrade_Score85_ReturnsB` precisely identifies the failing scenario without reading the test body.

---

## Q14. What is the difference between a unit test and an integration test in the context of a grading system?

**Concepts**
- test scope boundary
- production dependency involvement
- database and I/O in integration tests
- test speed trade-offs
- test double substitution

**Answer**

A unit test for `GradeCalculator` isolates the class completely: it creates the object, passes inputs, and asserts on outputs with no database, no file system, and no network. The test exercises only the logic inside `GetLetterGrade` and `IsPassing`. Every external dependency is either absent (as in this case) or replaced by a test double.

An integration test for a grading system might test a `GradeService` that calls `GradeCalculator` internally, reads student records from a SQL Server database, and writes the computed letter grade back. This test involves real I/O, requires a test database with seed data, and verifies that all the components wired together produce the correct end result.

The key distinction is the number of real components involved. A unit test has exactly one real component: the SUT. An integration test has multiple real components. This makes integration tests slower, harder to set up, and more likely to fail for environmental reasons unrelated to the code logic.

For `GradeCalculator`, writing integration tests would add no value — there are no external dependencies to verify. The correct choice is unit tests only. Integration tests become valuable when you introduce a `StudentRepository` with real SQL access, a `ReportGenerator` that writes to Azure Blob Storage, or an API controller that routes HTTP requests through the service layer.

---

## Q15. How does xUnit test discovery work in .NET 10, and what are the requirements for a method to be discovered?

**Concepts**
- xUnit test runner discovery
- public method requirement
- no-parameter requirement for Fact
- test class instantiation
- dotnet test CLI integration

**Answer**

xUnit discovers tests by scanning assemblies for public classes that contain public methods decorated with `[Fact]` or `[Theory]`. The class does not need to inherit from a base class or carry any attribute — xUnit uses reflection to find decorated methods at runtime.

For a method to be discovered as a `[Fact]`, it must be public, return `void` or `Task` (for async tests), and accept no parameters. For a `[Theory]`, it must be public, return `void` or `Task`, and have parameters that match the data attributes (`[InlineData]`, `[MemberData]`, or `[ClassData]`).

In .NET 10, `dotnet test` invokes the xUnit test runner through the test adapter registered in the test project's `.csproj`:

```xml
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
```

The `Microsoft.NET.Test.Sdk` package enables the `dotnet test` integration. Without it, xUnit tests can be run via the xUnit console runner but will not be discovered by the standard .NET CLI or Visual Studio Test Explorer.

Private or internal test methods are silently ignored — they do not cause errors but are not executed. A common pitfall is marking a test class as `internal` by accident (the default when adding a new class in some IDEs without adjusting the access modifier), which causes xUnit to skip discovery for that entire class.

---

## Q16. Why should tests never depend on execution order, and how does xUnit enforce this?

**Concepts**
- test execution order
- shared mutable state
- test parallelism in xUnit
- class-level isolation
- ordering anti-patterns

**Answer**

Tests that depend on execution order are fragile because test runners may run tests in any order, run test classes in parallel, or reorder tests across reruns. A test that passes only because a previous test populated a shared static dictionary is not testing behavior — it is testing execution order, which is not a property of the production code.

xUnit enforces isolation at the class level by default: it creates a new instance of the test class for each test method. This means no instance fields carry state between tests within the same class. However, xUnit does not protect against static fields or process-level singletons used across test classes.

xUnit also parallelizes test classes by default — different test classes run concurrently on separate threads. Tests within the same class run sequentially on one thread. This means any static mutable state shared between test classes produces race conditions that cause intermittent, hard-to-reproduce failures.

The practical rule: never use `static` fields in test classes to share mutable state. If you need shared state within a class, use `IClassFixture<T>` with immutable or thread-safe data. If you need shared state across classes, use `ICollectionFixture<T>` and explicitly group the classes into a named collection — which also disables parallel execution for that collection.

For `GradeCalculatorTests`, there is no shared state concern because each test constructs its own `GradeCalculator` instance and passes constants as inputs.

---

## Q17. What is the role of a test double, and when would you introduce one into tests for a class that uses `GradeCalculator`?

**Concepts**
- test double types (stub, mock, fake, spy)
- dependency injection for testability
- isolating the SUT from collaborators
- Moq and NSubstitute in .NET
- interface-based abstraction

**Answer**

A test double is any object that stands in for a real dependency during a test. There are several varieties: a stub returns pre-programmed values without real logic; a mock records calls and lets you assert that specific interactions occurred; a fake has a working but simplified implementation (an in-memory list instead of a real database); a spy is a real object instrumented to record how it is used.

`GradeCalculator` itself needs no test doubles — it has no dependencies. But consider a `StudentGradeService` that takes a `GradeCalculator` and an `IStudentRepository`:

```csharp
public class StudentGradeService
{
    private readonly GradeCalculator _calculator;
    private readonly IStudentRepository _repository;

    public StudentGradeService(GradeCalculator calculator, IStudentRepository repository)
    {
        _calculator = calculator;
        _repository = repository;
    }

    public string ComputeAndSaveGrade(int studentId, int score)
    {
        string grade = _calculator.GetLetterGrade(score);
        _repository.SaveGrade(studentId, grade);
        return grade;
    }
}
```

To unit-test `StudentGradeService`, you stub `IStudentRepository` so `SaveGrade` does nothing. You then verify that `ComputeAndSaveGrade` returns the correct letter grade (state test) and optionally verify that `SaveGrade` was called with the correct arguments (interaction test). Using Moq in .NET 10:

```csharp
var mockRepo = new Mock<IStudentRepository>();
var service = new StudentGradeService(new GradeCalculator(), mockRepo.Object);
string result = service.ComputeAndSaveGrade(1, 95);
Assert.Equal("A", result);
mockRepo.Verify(r => r.SaveGrade(1, "A"), Times.Once);
```

---

## Q18. What is the minimal xUnit project setup for .NET 10 and how does `dotnet test` execute the tests?

**Concepts**
- xUnit NuGet packages
- Microsoft.NET.Test.Sdk
- csproj test SDK target
- dotnet test CLI
- test output and exit codes

**Answer**

In .NET 10, an xUnit test project is a class library that targets `net10.0` and includes three NuGet packages: `xunit` (the framework), `xunit.runner.visualstudio` (the test adapter), and `Microsoft.NET.Test.Sdk` (the .NET test platform integration). A minimal `.csproj` looks like:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\GradeBook.Core\GradeBook.Core.csproj" />
  </ItemGroup>
</Project>
```

Running `dotnet test` from the solution root builds all test projects, discovers all `[Fact]` and `[Theory]` methods, executes them, and reports results to the console. The process exits with code 0 if all tests pass and a non-zero code if any fail, enabling CI pipelines to gate merges on test success.

`dotnet test --filter "FullyQualifiedName~GradeCalculator"` runs only tests matching a name pattern. `dotnet test --logger trx` writes an XML results file for CI reporting. In Visual Studio and VS Code, the Test Explorer window integrates with the same adapter to run and debug individual tests.

---

# Gotcha Questions

---

## Q19. If `Assert.Equal("A", result)` and `Assert.Equal(result, "A")` look similar, does argument order matter?

**Concepts**
- Assert.Equal argument order
- expected vs actual convention
- failure message readability
- xUnit message format
- common mistake in test authorship

**Answer**

Yes, argument order matters significantly for the failure message, even though the equality check itself is symmetric. xUnit's convention is `Assert.Equal(expected, actual)` — the known good value first, the value produced by the code second. When a test fails, xUnit formats the message as:

```
Assert.Equal() Failure
Expected: A
Actual:   B
```

If you accidentally swap the arguments and write `Assert.Equal(result, "A")`, a failure produces a confusing message where "Expected" shows the computed value and "Actual" shows the literal constant. Developers reading the CI output will be misled: they see "Expected: B, Actual: A" and must mentally invert the meaning to understand what the code returned versus what the test intended.

This is a subtle but persistent gotcha because the test still correctly identifies a failure — the check is symmetric — but the diagnostic message is backwards. In large codebases, inverted assert arguments slow down debugging and erode trust in failure messages.

Some teams enforce correct argument order through static analysis rules (e.g., the `xunit.analyzers` NuGet package includes a rule that warns when the arguments appear swapped based on naming conventions). The safest habit is to always write the literal expected value first and the variable holding the computed result second.

---

## Q20. Can a test with no assertions pass in xUnit, and is that a problem?

**Concepts**
- empty test body
- false-positive green test
- assertion-free tests
- xUnit analyzer warnings
- test quality signals

**Answer**

Yes, a test with no assertions passes in xUnit because xUnit defines a passing test as one that does not throw an exception. A method decorated with `[Fact]` that contains only the Arrange and Act phases — and no Assert — will be reported as green. This is a false positive: the test produces no evidence of correct behavior.

```csharp
[Fact]
public void GetLetterGrade_Score95_ReturnsA() // no assertion — always green
{
    var calculator = new GradeCalculator();
    var result = calculator.GetLetterGrade(95); // result unused
}
```

This pattern often appears when developers write a test skeleton intending to fill in the assertion later, then forget, or when an assertion is accidentally commented out during debugging. The test suite reports 100% green while providing zero coverage of the expected behavior.

The `xunit.analyzers` package (a Roslyn analyzer) detects this pattern and raises a warning: "Test method should have at least one assertion." Adding this package to the test project and treating its warnings as build errors prevents assertion-free tests from reaching the main branch.

Another manifestation is `Assert.True(true)` or `Assert.Equal(1, 1)` — assertions that are always true regardless of the SUT's behavior. These are equally worthless and harder to detect statically. Code review is the primary defense against this variety.

---

## Q21. Does xUnit guarantee that tests within the same class run sequentially?

**Concepts**
- intra-class test ordering
- inter-class parallelism
- xUnit collection behavior
- shared state race conditions
- ITestCaseOrderer

**Answer**

xUnit guarantees that tests within the same test class run sequentially on a single thread. What it does not guarantee is the order in which those sequential tests run. By default, xUnit randomizes test execution order within a class across test runs to surface hidden ordering dependencies. If your tests pass in one order but fail in another, you have an ordering dependency that must be fixed by eliminating shared mutable state.

Between test classes, xUnit runs classes in parallel by default (each class on its own thread). This is the most common source of hard-to-reproduce test failures in .NET projects: two test classes that independently set a static property, modify a shared in-memory cache, or write to the same file path will race against each other.

You can disable parallelism for a group of classes using collection fixtures:

```csharp
[Collection("GradeBook Integration")]
public class GradeCalculatorTests { }

[Collection("GradeBook Integration")]
public class GradeValidationTests { }
```

Classes sharing a `[Collection]` name run sequentially. You can also disable parallelism globally by placing an `[assembly: CollectionBehavior(DisableTestParallelization = true)]` attribute in the test project, at the cost of longer overall test runtime.

The gotcha: forgetting that parallelism is the default leads developers to believe their tests are isolated when they are actually silently sharing state through statics.

---

## Q22. If `GradeCalculator.GetLetterGrade` uses a chain of `if/else if` statements and you only test score 95, what percentage of your branches are covered?

**Concepts**
- branch coverage vs line coverage
- coverage gaps
- false confidence from partial tests
- equivalence partitioning
- coverage tools in .NET

**Answer**

Testing only score 95 covers the first branch (the "A" path) and the input validation for values in range, but leaves every other branch untested. In a typical implementation with five grade bands and one validation check, you would cover roughly two of ten or more distinct branches — somewhere around 15–20% branch coverage, even though the single test may show 40–50% line coverage because many lines are shared infrastructure.

This is the core gotcha of using line coverage as a quality metric: a method with five grade bands can appear half-covered by lines (the shared guard clause and return statement) while all four lower-grade branches remain completely untested. A regression that changes the B/A boundary from 90 to 91 would be invisible to a test suite that only asserts on score 95.

In .NET 10, you can measure coverage using `dotnet test --collect:"XPlat Code Coverage"` with Coverlet, which generates a `coverage.cobertura.xml` report. Visualizing this in ReportGenerator shows uncovered branches highlighted in red.

The fix is not to chase a coverage number blindly but to combine equivalence partitioning (one representative per grade band) with boundary value analysis (the exact boundary scores). Together they force you to write tests for all five grade paths and all invalid-input paths, achieving genuine branch coverage with a small, focused set of tests.

---

## Q23. What happens if you throw `Exception` instead of `ArgumentOutOfRangeException` in `GetLetterGrade`, and your test asserts `Assert.Throws<ArgumentOutOfRangeException>`?

**Concepts**
- exception type specificity
- Assert.Throws type parameter
- inheritance hierarchy for exceptions
- catching base vs derived exceptions
- test precision

**Answer**

`Assert.Throws<ArgumentOutOfRangeException>` will fail. It checks the exact runtime type of the thrown exception against the generic type parameter using a strict type comparison. If the thrown exception is a plain `Exception`, `InvalidOperationException`, or any other type that is not `ArgumentOutOfRangeException` or a subclass of it, the assertion fails with a message indicating that the expected exception type was not thrown.

This is correct and desirable behavior. The test is a specification that says "this method communicates invalid input via `ArgumentOutOfRangeException`." If the implementation accidentally throws a different exception type — even a more general `Exception` with the same message — the test correctly reports a violation of that specification.

A related subtlety: `Assert.Throws<ArgumentException>` would pass when the implementation throws `ArgumentOutOfRangeException`, because `ArgumentOutOfRangeException` derives from `ArgumentException`. Whether this is correct depends on your specification — if callers catch `ArgumentException` to handle validation errors, asserting `ArgumentOutOfRangeException` gives callers more information and the test should reflect the more specific type.

The practical rule: assert on the most specific exception type that is part of your method's public contract. Using `Assert.Throws<Exception>` in a test is almost always wrong — it will pass even for unexpected exceptions like `NullReferenceException`, giving false confidence that invalid-input handling works correctly.

---

## Q24. Can two test methods in different test classes have the same name without causing a conflict in xUnit?

**Concepts**
- fully qualified test name
- namespace-based uniqueness
- test class scoping
- naming collision risk
- test output readability

**Answer**

Yes, xUnit identifies tests by their fully qualified name, which includes the namespace, class name, and method name. Two test classes in different namespaces or simply different classes in the same namespace can both have a method called `GetLetterGrade_Score95_ReturnsA` without conflict.

The fully qualified names would be, for example:
- `GradeBook.Core.Tests.Services.GradeCalculatorTests.GetLetterGrade_Score95_ReturnsA`
- `GradeBook.Core.Tests.Validation.GradeCalculatorValidationTests.GetLetterGrade_Score95_ReturnsA`

These are distinct test identifiers and xUnit runs both independently.

However, having identical method names across classes can cause confusion in two practical scenarios. First, when filtering with `dotnet test --filter "Method~GetLetterGrade_Score95_ReturnsA"`, both tests match the filter and both run — which is usually what you want, but can be surprising. Second, in test output logs and CI summaries that truncate the class name, two tests with identical method names may appear duplicated, making it harder to see which class's test failed.

The convention `GradeCalculatorTests` for happy-path tests and `GradeCalculatorValidationTests` for exception tests avoids this by making the class name itself part of the test's identity. Keeping method names unique within a class and descriptive across the suite is still the clearest approach.

---

# Real-World Scenario Questions

---

## Q25. You inherit a `GradeCalculator` with the following test file. Identify the defects and propose fixes.

**Concepts**
- test code review
- assertion anti-patterns
- FIRST principle violations
- naming convention violations
- test independence

```csharp
public class GradeCalculatorTests
{
    private static GradeCalculator _sharedCalculator = new GradeCalculator();
    private static List<string> _results = new List<string>();

    [Fact]
    public void Test1()
    {
        var result = _sharedCalculator.GetLetterGrade(95);
        _results.Add(result);
        Assert.True(true);
    }

    [Fact]
    public void Test2()
    {
        Assert.Equal(1, _results.Count);
        Assert.Equal("A", _results[0]);
    }

    [Fact]
    public void GetLetterGrade_works()
    {
        Assert.True(_sharedCalculator.GetLetterGrade(75) == "C");
    }

    [Fact]
    public void ExceptionTest()
    {
        try
        {
            _sharedCalculator.GetLetterGrade(-1);
        }
        catch (ArgumentOutOfRangeException)
        {
            Assert.True(true);
        }
    }
}
```

| Category | Problem | Impact |
|---|---|---|
| Isolation | `_sharedCalculator` and `_results` are static, shared across all tests and potentially across parallel class runs | Race conditions; state contamination between tests |
| Ordering dependency | `Test2` asserts on `_results.Count == 1`, which only passes if `Test1` ran first | Tests fail when order changes; violates Isolated in FIRST |
| Assertion | `Assert.True(true)` in `Test1` is always true regardless of SUT behavior | False-positive green; test provides zero coverage guarantee |
| Naming | `Test1`, `Test2`, `GetLetterGrade_works`, `ExceptionTest` are not descriptive | Failure messages give no diagnostic information |
| Exception pattern | `try/catch` around the act passes silently when no exception is thrown | `ExceptionTest` is a false positive if `GetLetterGrade(-1)` stops throwing |
| Assertion style | `Assert.True(_sharedCalculator.GetLetterGrade(75) == "C")` produces a minimal failure message | Only says "True was not True", not what grade was returned |

**Fix priority:**

1. Remove `static` from both fields and move initialization into the constructor so each test gets a fresh `GradeCalculator` and a fresh `_results` list.
2. Delete `Test2` entirely; replace `Test1` with `GetLetterGrade_Score95_ReturnsA` that uses `Assert.Equal("A", result)` directly.
3. Replace `Assert.True(_sharedCalculator.GetLetterGrade(75) == "C")` with `Assert.Equal("C", calculator.GetLetterGrade(75))`.
4. Replace the `try/catch` pattern in `ExceptionTest` with `Assert.Throws<ArgumentOutOfRangeException>(() => calculator.GetLetterGrade(-1))`.
5. Rename all methods to follow `MethodName_Scenario_ExpectedResult`.

---

## Q26. A new developer on your team asks why the test project has both `GradeCalculatorTests` and `GradeCalculatorValidationTests` classes instead of one big class. How do you explain the design decision?

**Concepts**
- test class cohesion
- single responsibility at class level
- test organization strategies
- test explorer readability
- parallel execution granularity

**Answer**

Splitting tests across multiple classes is an application of the single-responsibility principle at the class level. Each class groups tests that share the same arrangement context — the same kind of setup and the same category of behavior being verified.

`GradeCalculatorTests` groups tests for the happy path: valid inputs that produce expected grade letters. All tests in that class follow the pattern of creating a `GradeCalculator`, passing a valid score, and asserting on the returned string. The shared arrangement is implicit in the class name.

`GradeCalculatorValidationTests` groups tests for the error path: inputs that should trigger validation exceptions. These tests share a different concern — they all use `Assert.Throws`, they all pass invalid inputs, and they document the contract that the method raises `ArgumentOutOfRangeException` for out-of-range values. Keeping them separate makes that contract explicit through the class name itself.

From a practical standpoint, splitting into classes also improves the Visual Studio Test Explorer and CI output: you can collapse or filter by class name to see all happy-path results or all validation results at once, rather than scrolling through a mix of assertion and exception tests.

A secondary benefit is xUnit's parallelism model. Because xUnit runs test classes in parallel, having more classes means more concurrency. For `GradeCalculator` this is irrelevant (tests are microsecond-fast), but for integration test classes with slower setup, finer-grained class decomposition can meaningfully reduce total test time.

---

## Q27. Your team decides to add a `GetLetterGrade` overload that accepts a `double` score for weighted grades. How would you approach writing unit tests before implementing the method (TDD style)?

**Concepts**
- test-driven development cycle
- red-green-refactor
- test as specification before implementation
- boundary values for floating-point input
- TDD in xUnit

**Answer**

In TDD, you write a failing test first, then write the minimal code to make it pass, then refactor. The failing test is your specification.

Start by writing the simplest test that defines the desired behavior:

```csharp
[Fact]
public void GetLetterGrade_DoubleScore94Point5_ReturnsA()
{
    var calculator = new GradeCalculator();
    Assert.Equal("A", calculator.GetLetterGrade(94.5));
}
```

This test will not compile until you add the `double` overload signature. The compilation failure is the first "red" state — the test cannot even run. Add the method signature with a `throw new NotImplementedException()` body to make it compile. Now the test fails at runtime (red). Implement the minimal logic to pass this one test (green). Then add the next test:

```csharp
[Fact]
public void GetLetterGrade_DoubleScore89Point9_ReturnsB()
{
    var calculator = new GradeCalculator();
    Assert.Equal("B", calculator.GetLetterGrade(89.9));
}
```

Continue the red-green cycle for each boundary. The discipline of writing tests first forces you to specify how rounding works (does 89.9 round to 90 and return "A", or is the comparison strict and returns "B"?), what constitutes an invalid double (negative, NaN, Infinity, values above 100), and whether the double overload delegates to the integer overload internally (which would make `GetLetterGrade(95.0)` call `GetLetterGrade(95)` via truncation or rounding).

TDD also naturally produces the exact test method names and boundary values you need, because you are encoding your assumptions as assertions before writing any implementation logic.

---

## Q28. A CI build is showing intermittent test failures in the `GradeCalculatorTests` suite — sometimes three tests pass, sometimes two. The failures are non-deterministic. What do you investigate?

**Concepts**
- flaky test diagnosis
- shared mutable state detection
- test parallelism race conditions
- static field inspection
- xUnit collection configuration

**Answer**

Non-deterministic failures that affect different tests on different runs are almost always caused by shared mutable state combined with parallel execution. The investigation follows a structured approach.

First, check for `static` fields or properties in the test classes and any classes they depend on. A static `GradeCalculator` instance could be acceptable if the class is truly stateless, but a static `List<string>`, counter, or configuration flag shared between test methods is a race condition.

Second, check the production code for static state. If `GradeCalculator` has a static cache, logger, or counter that accumulates state across calls, concurrent test execution will produce non-deterministic results even when the test class itself looks clean.

Third, run the tests with parallelism disabled to confirm the hypothesis:

```
dotnet test -- xunit.parallelizeAssembly=false
```

If the failures disappear, parallel execution is the root cause. This confirms shared state rather than a genuine logic bug.

Fourth, add `[assembly: CollectionBehavior(DisableTestParallelization = true)]` temporarily to the test project to pinpoint whether the issue is between classes in the same assembly. Then systematically re-enable parallelism class by class until the failure reappears.

For `GradeCalculator` specifically, the fix is almost always to remove any static field from the test classes and ensure each test constructs its own instance. The permanent fix should avoid disabling parallelism globally, because that hides the underlying issue and slows down the full test run for the lifetime of the project.

---

## Q29. Your manager asks you to add code coverage to the CI pipeline and enforce a minimum of 80% branch coverage. How do you set this up with .NET 10 and Coverlet?

**Concepts**
- Coverlet integration in .NET
- dotnet test coverage collection
- ReportGenerator for HTML reports
- minimum coverage threshold enforcement
- CI pipeline gate

**Answer**

Coverlet is the de facto code coverage tool for .NET and ships as a NuGet package. Add it to the test project:

```
dotnet add GradeBook.Core.Tests package coverlet.collector
```

Run tests with coverage collection enabled:

```
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

This generates a `coverage.cobertura.xml` file in `./TestResults`. To enforce a minimum branch coverage threshold, pass the threshold argument directly:

```
dotnet test --collect:"XPlat Code Coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura \
     DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Threshold=80 \
     DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ThresholdType=branch \
     DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ThresholdStat=total
```

When branch coverage falls below 80%, the `dotnet test` process exits with a non-zero code, failing the CI stage. In GitHub Actions, this becomes a required check that blocks pull request merges.

To produce a human-readable HTML report for the coverage results, add `reportgenerator` as a .NET tool and run it over the Cobertura file. The HTML report highlights uncovered branches in red, making it easy to see which grade band or exception path lacks test coverage.

For `GradeCalculator`, achieving 80% branch coverage requires testing at least four of the five grade bands and both the valid and invalid input paths, which aligns naturally with boundary value testing.

---

## Q30. A colleague argues that writing tests for `GradeCalculator` is unnecessary because the logic is simple. How do you counter this argument?

**Concepts**
- regression protection
- specification as documentation
- refactoring confidence
- simple code evolves into complex code
- test ROI argument

**Answer**

The argument that simple code does not need tests conflates current complexity with future complexity. `GetLetterGrade` is simple today, but production code accumulates requirements: weighted grades, localized letter systems (German Noten, French mentions), grade curves, extra credit, incomplete status codes. Without tests, each new requirement risks silently breaking the existing grade boundaries.

Tests serve four roles beyond catching current bugs. First, they are a regression safety net: if a future developer changes the boundary from `>= 90` to `> 90` (a single character change), every test that touches the 90-point boundary fails immediately, catching the regression in seconds rather than in a production incident affecting student records.

Second, they document intended behavior precisely. The test `GetLetterGrade_Score89_ReturnsB` and `GetLetterGrade_Score90_ReturnsA` together specify the exact boundary between B and A more precisely than any comment or prose document. Tests cannot go stale in the way that comments do — a test that contradicts the code fails.

Third, they enable confident refactoring. If the team decides to replace the `if/else if` chain with a dictionary lookup or a range-based data structure, the existing tests provide immediate verification that the refactored implementation is equivalent.

Fourth, they set a culture standard. A codebase where "simple" classes skip tests is one where the definition of "simple enough to skip" gradually expands until no class has tests. The discipline of writing tests for every class — including simple ones — maintains the habit and ensures the test suite grows with the codebase.
