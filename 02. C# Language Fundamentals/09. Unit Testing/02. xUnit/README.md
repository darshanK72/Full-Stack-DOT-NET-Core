# xUnit.net — Interview Q&A

---


## Table of Contents

1. [Q1. What is the difference between `[Fact]` and `[Theory]` in xUnit?](#q1-what-is-the-difference-between-fact-and-theory-in-xunit)
2. [Q2. How do `[InlineData]`, `[MemberData]`, and `[ClassData]` differ as data sources for a `[Theory]`?](#q2-how-do-inlinedata-memberdata-and-classdata-differ-as-data-sources-for-a-theory)
3. [Q3. How does xUnit handle per-test setup and teardown without `[SetUp]` and `[TearDown]`?](#q3-how-does-xunit-handle-per-test-setup-and-teardown-without-setup-and-teardown)
4. [Q4. What is `IClassFixture<T>` and when should you use it?](#q4-what-is-iclassfixturet-and-when-should-you-use-it)
5. [Q5. How does `ICollectionFixture<T>` differ from `IClassFixture<T>`, and how is a collection declared?](#q5-how-does-icollectionfixturet-differ-from-iclassfixturet-and-how-is-a-collection-declared)
6. [Q6. How do you write async test methods in xUnit?](#q6-how-do-you-write-async-test-methods-in-xunit)
7. [Q7. What is `ITestOutputHelper` and why is it preferred over `Console.WriteLine` in tests?](#q7-what-is-itestoutputhelper-and-why-is-it-preferred-over-consolewriteline-in-tests)
8. [Q8. How does `[Trait]` work for test categorization and filtering?](#q8-how-does-trait-work-for-test-categorization-and-filtering)
9. [Q9. What xUnit `Assert` members are most commonly used, and how do they differ from NUnit equivalents?](#q9-what-xunit-assert-members-are-most-commonly-used-and-how-do-they-differ-from-nunit-equivalents)
10. [Q10. How does xUnit's default parallel execution model work, and how do you control it?](#q10-how-does-xunits-default-parallel-execution-model-work-and-how-do-you-control-it)
11. [Q11. How do you skip a test in xUnit, and what are the alternatives?](#q11-how-do-you-skip-a-test-in-xunit-and-what-are-the-alternatives)
12. [Q12. What improvements does xUnit v3 bring over v2?](#q12-what-improvements-does-xunit-v3-bring-over-v2)
13. [Q13. How does the constructor injection model in xUnit compare to MSTest's `[TestInitialize]`?](#q13-how-does-the-constructor-injection-model-in-xunit-compare-to-mstests-testinitialize)
14. [Q14. How does `Assert.Throws<T>` differ from wrapping the call in a `try`/`catch` block?](#q14-how-does-assertthrowst-differ-from-wrapping-the-call-in-a-trycatch-block)
15. [Q15. How do you test a method that returns `IAsyncEnumerable<T>` in xUnit?](#q15-how-do-you-test-a-method-that-returns-iasyncenumerablet-in-xunit)
16. [Q16. What is `Assert.Collection` and when should you use it over `Assert.Equal` on a list?](#q16-what-is-assertcollection-and-when-should-you-use-it-over-assertequal-on-a-list)
17. [Q17. How do you use `[ClassData]` to supply test data from a separate class?](#q17-how-do-you-use-classdata-to-supply-test-data-from-a-separate-class)
18. [Q18. How do you verify that a mock or stub was called with specific arguments in xUnit?](#q18-how-do-you-verify-that-a-mock-or-stub-was-called-with-specific-arguments-in-xunit)
19. [Q19. Why does a `[Theory]` with no data attributes cause a runtime error rather than a compile error?](#q19-why-does-a-theory-with-no-data-attributes-cause-a-runtime-error-rather-than-a-compile-error)
20. [Q20. Why do tests that depend on static shared state become flaky when xUnit parallelism is enabled?](#q20-why-do-tests-that-depend-on-static-shared-state-become-flaky-when-xunit-parallelism-is-enabled)
21. [Q21. Why does `Assert.Equal` sometimes fail for two objects that look identical?](#q21-why-does-assertequal-sometimes-fail-for-two-objects-that-look-identical)
22. [Q22. What happens if a fixture's constructor or `InitializeAsync` throws an exception?](#q22-what-happens-if-a-fixtures-constructor-or-initializeasync-throws-an-exception)
23. [Q23. Why should you avoid `async void` in test methods, and what goes wrong in practice?](#q23-why-should-you-avoid-async-void-in-test-methods-and-what-goes-wrong-in-practice)
24. [Q24. Code review: what is wrong with the following test class?](#q24-code-review-what-is-wrong-with-the-following-test-class)
25. [Q25. Code review: what is wrong with the following theory?](#q25-code-review-what-is-wrong-with-the-following-theory)
26. [Q26. How would you structure fixtures to share a real database connection across multiple test classes for integration tests?](#q26-how-would-you-structure-fixtures-to-share-a-real-database-connection-across-multiple-test-classes-for-integration-tests)
27. [Q27. How would you test a method that uses `CancellationToken` and verify cancellation is handled correctly?](#q27-how-would-you-test-a-method-that-uses-cancellationtoken-and-verify-cancellation-is-handled-correctly)
28. [Q28. How do you organize a test suite for a pricing service that has both fast unit tests and slow integration tests, and run only the fast tests in pull request builds?](#q28-how-do-you-organize-a-test-suite-for-a-pricing-service-that-has-both-fast-unit-tests-and-slow-integration-tests-and-run-only-the-fast-tests-in-pull-request-builds)
29. [Q29. Code review: what is wrong with the following use of `IClassFixture`?](#q29-code-review-what-is-wrong-with-the-following-use-of-iclassfixture)

---
## Q1. What is the difference between `[Fact]` and `[Theory]` in xUnit?

**Concepts**
- `[Fact]` — single-scenario, parameterless test method
- `[Theory]` — data-driven test that runs once per data row
- data source attributes (`[InlineData]`, `[MemberData]`, `[ClassData]`)
- test identity — each row generates a separately named test case
- failure isolation — one failing row does not abort remaining rows

**Answer**

`[Fact]` marks a method that represents one logical test case with no external parameters. The method has a fixed Arrange–Act–Assert body and xUnit runs it exactly once. Use `[Fact]` when only a single input combination is meaningful or when the scenario is inherently unique, such as verifying that an empty order returns a subtotal of zero.

`[Theory]` marks a method that accepts parameters and is expected to run once for every data row supplied by one or more companion attributes on the same method. xUnit discovers each row, instantiates the test class, calls the method with the row's arguments, and reports the result as a separate test entry. This means a five-row theory produces five individual test results in Test Explorer, each labeled with its argument values, so a failure in row three does not prevent rows four and five from running. Without at least one data attribute, a `[Theory]` method will throw `InvalidOperationException` at runtime because xUnit finds no rows to execute.

The practical heuristic is: if you find yourself copy-pasting a `[Fact]` body and changing only the literal values, convert it to a `[Theory]` with `[InlineData]`.

---

## Q2. How do `[InlineData]`, `[MemberData]`, and `[ClassData]` differ as data sources for a `[Theory]`?

**Concepts**
- `[InlineData]` — compile-time constant arguments on the attribute
- `[MemberData]` — static property or method returning `IEnumerable<object[]>`
- `[ClassData]` — separate class implementing `IEnumerable<object[]>`
- argument type constraints for attribute parameters
- when to prefer each source

**Answer**

`[InlineData]` places arguments directly on the attribute: `[InlineData(100, 10, 90)]`. Because attribute arguments must be compile-time constants, you cannot pass `new List<OrderLine>()` or any heap-allocated object here. Use it for small, primitive tables where the values read naturally alongside the method.

`[MemberData]` points to a static property or method on any class that returns `IEnumerable<object[]>`. Each inner array corresponds to one row. This removes the constant restriction, so you can construct complex objects, lists, or computed values. The `nameof` operator keeps the reference refactor-safe: `[MemberData(nameof(SubtotalMemberData))]`. The data member can live in the test class itself or in a shared static helper class.

`[ClassData]` points to a separate class that implements `IEnumerable<object[]>` directly. It is useful when the data generation logic is complex enough to warrant its own file, or when the same data set is reused across multiple test classes. Internally, xUnit calls `GetEnumerator()` on an instance of that class at discovery time.

A fourth option introduced in xUnit v2 is custom `DataAttribute` subclasses, which allow reading rows from CSV files, databases, or other sources by overriding `GetData`.

---

## Q3. How does xUnit handle per-test setup and teardown without `[SetUp]` and `[TearDown]`?

**Concepts**
- constructor-per-test instantiation model
- `IDisposable` for synchronous teardown
- `IAsyncLifetime` for async setup and teardown
- contrast with NUnit's `[SetUp]`/`[TearDown]` and MSTest's `[TestInitialize]`/`[TestCleanup]`
- isolation guarantee from fresh instance per test

**Answer**

xUnit creates a new instance of the test class before every test method and disposes it afterwards. This means the class constructor is your `[SetUp]` replacement: any initialization you put in the constructor runs fresh for each test. If you store shared objects as `readonly` fields initialized in the constructor, each test starts from a clean state without any explicit reset.

Teardown is handled via `IDisposable`. Implement `Dispose()` on the test class and xUnit will call it after each method completes, regardless of whether the test passed or failed. For cleanup that requires `await`, implement `IAsyncLifetime` instead: `InitializeAsync()` runs after the constructor, and `DisposeAsync()` runs after each test in place of `Dispose()`.

This model has an important architectural consequence: if you add a field and forget to reset it between tests, xUnit's isolation model prevents accidental cross-test contamination, because each test has its own instance. By contrast, NUnit's `[SetUp]` attribute runs a method on the same class instance for each test, which makes it easier to accidentally share mutable state across methods if a field is not reset in `[SetUp]`.

---

## Q4. What is `IClassFixture<T>` and when should you use it?

**Concepts**
- fixture lifetime — one instance per test class
- constructor injection of the fixture
- expensive shared resource (HTTP client, seeded database, service instance)
- test isolation versus resource cost trade-off
- `IDisposable` and `IAsyncLifetime` on the fixture class

**Answer**

`IClassFixture<T>` tells xUnit to construct one instance of `T` before the first test in the class runs, inject it into every test method's constructor, and dispose it after the last test in the class completes. The test class declares the interface as a generic constraint and adds a constructor parameter of type `T`.

This pattern is the right choice when creating the shared object is expensive — for example, spinning up an HTTP client, seeding an in-memory database, or loading a product catalog from disk — and when the object is either immutable or carefully designed to be safe for sequential reuse. In the project's `ClassFixtureTests`, `OrderCatalogFixture` builds an `OrderPricingService` and a pre-seeded `Order` once; every test in the class reads from those without recreating them.

The key constraint is that the fixture instance is shared only within one test class. A second test class that also implements `IClassFixture<OrderCatalogFixture>` receives its own separate fixture instance. If you need multiple classes to share a single instance, you need `ICollectionFixture<T>` instead. Within the class, tests still run sequentially, so you do not need locks when reading from the fixture — but you should avoid mutating it, as mutation breaks test isolation.

---

## Q5. How does `ICollectionFixture<T>` differ from `IClassFixture<T>`, and how is a collection declared?

**Concepts**
- `[CollectionDefinition]` marker class
- `ICollectionFixture<T>` registration
- `[Collection("name")]` on test classes
- single shared fixture instance across multiple classes
- parallelism boundary — collection members run serially

**Answer**

`ICollectionFixture<T>` shares one fixture instance across every test class that belongs to the same named collection, whereas `IClassFixture<T>` provides a separate instance per class. The setup involves three parts. First, a marker class decorated with `[CollectionDefinition("name")]` and implementing `ICollectionFixture<T>` declares the collection and registers the fixture type — the class itself has no members and is never instantiated directly. Second, each test class that wants to join the collection adds `[Collection("name")]` to its declaration. Third, those test classes accept the fixture via a constructor parameter, exactly as with `IClassFixture<T>`.

In the project, `OrderCatalogCollection` declares the `"OrderCatalog"` collection with `ICollectionFixture<OrderCatalogFixture>`. Both `CollectionFixtureTestsA` and `CollectionFixtureTestsB` carry `[Collection("OrderCatalog")]` and receive the exact same `OrderCatalogFixture` instance — confirmed by the fact that both tests read `_fixture.StandardOrder.OrderId` and see `"ORD-TEST-001"` without recreating the order.

An important side effect: all test classes in the same collection run serially with respect to each other, even though xUnit normally runs different classes in parallel. This serialization prevents race conditions when the shared fixture holds mutable state, but it also reduces throughput, so only group classes into a collection when sharing the fixture is genuinely necessary.

---

## Q6. How do you write async test methods in xUnit?

**Concepts**
- `Task` return type requirement
- `async`/`await` inside test methods
- `IAsyncLifetime` for async fixture setup and teardown
- `Assert.ThrowsAsync<T>` for async exception testing
- why `async void` test methods are dangerous

**Answer**

xUnit supports async test methods natively: declare the method as `async Task` (or `async Task<T>` though the return value is ignored) and use `await` inside. xUnit's test runner awaits the returned `Task` and captures any unhandled exceptions from it as test failures, including exceptions thrown after an `await` continuation.

```csharp
[Fact]
public async Task FetchOrder_ValidId_ReturnsExpectedLines()
{
    var service = new OrderFetchService(new HttpClient());

    Order order = await service.FetchAsync("ORD-001");

    Assert.Equal(2, order.Lines.Count);
}
```

For async exception assertions, use `Assert.ThrowsAsync<T>` instead of `Assert.Throws<T>`:

```csharp
[Fact]
public async Task FetchOrder_InvalidId_ThrowsNotFoundException()
{
    await Assert.ThrowsAsync<OrderNotFoundException>(
        () => service.FetchAsync("NONEXISTENT"));
}
```

Never use `async void` for test methods. xUnit cannot await a `void`-returning async method, so any exception thrown after the first `await` continuation escapes the test runner entirely, crashes the test process, and is not reported as a test failure. The compiler does not prevent this, so the error is silent and misleading.

For async fixture lifecycle, implement `IAsyncLifetime` on the fixture class: `InitializeAsync()` runs before any test in the class, and `DisposeAsync()` runs after all tests complete.

---

## Q7. What is `ITestOutputHelper` and why is it preferred over `Console.WriteLine` in tests?

**Concepts**
- per-test output buffer
- constructor injection by xUnit
- CI log visibility (`--logger "console;verbosity=detailed"`)
- output attachment to a specific test result
- contrast with `Console.WriteLine` which is captured globally or lost

**Answer**

`ITestOutputHelper` is an interface provided by xUnit that exposes a `WriteLine(string message)` method. When a test class declares it as a constructor parameter, xUnit injects an implementation whose output is captured per-test and attached to that test's result.

`Console.WriteLine` is unreliable in test contexts for two reasons. First, many CI environments and test runners suppress standard output by default, so the lines simply disappear. Second, because xUnit runs test classes in parallel, lines from multiple tests interleave in the captured output, making it impossible to associate a line with the test that wrote it.

With `ITestOutputHelper`, each test's output appears only when that test is selected in Test Explorer or when you run `dotnet test --logger "console;verbosity=detailed"`. A failing test's diagnostic lines are shown alongside its failure message, which is exactly where they are useful during debugging. A passing test's output is suppressed in terse mode and shown only in detailed mode, avoiding noise in normal runs.

In `OutputHelperTests`, the constructor receives `ITestOutputHelper output` and stores it. During the test, `_output.WriteLine(...)` logs each line's breakdown and the subtotal. If the assertion fails, those lines appear in the failure report immediately above the assertion message.

---

## Q8. How does `[Trait]` work for test categorization and filtering?

**Concepts**
- key-value metadata on a test method or class
- `dotnet test --filter` with `Trait` expressions
- Test Explorer category grouping
- combining multiple traits
- custom `TraitDiscoverer` for typed traits (xUnit extensibility)

**Answer**

`[Trait("key", "value")]` attaches arbitrary string metadata to a test method or class. You can apply multiple `[Trait]` attributes to the same target, and the same key can appear multiple times with different values.

```csharp
[Fact]
[Trait("Category", "Pricing")]
[Trait("Priority", "High")]
public void ApplyDiscount_ZeroPercent_ReturnsOriginalSubtotal() { ... }
```

The primary use case is filtering from the command line. `dotnet test --filter "Trait[Category]=Pricing"` runs only tests with that trait. You can combine conditions: `--filter "Trait[Category]=Pricing&Trait[Priority]=High"`. The `|` operator is also supported for OR expressions.

In Visual Studio and VS Code, Test Explorer groups tests by traits, so teams commonly use traits to separate slow integration tests from fast unit tests: `[Trait("Speed", "Fast")]` versus `[Trait("Speed", "Slow")]`. CI pipelines can then run fast tests on every commit and slow tests only on merge.

For strongly typed trait aliases, xUnit's extensibility model allows you to create a custom `Attribute` that applies `[Trait]` under the hood via a `TraitDiscoverer`. This removes the magic string risk. In xUnit v3, traits are a first-class concept with improved discovery and filtering support through the new runner architecture.

---

## Q9. What xUnit `Assert` members are most commonly used, and how do they differ from NUnit equivalents?

**Concepts**
- `Assert.Equal`, `Assert.NotEqual`
- `Assert.True`, `Assert.False`
- `Assert.Throws<T>`, `Assert.ThrowsAsync<T>`
- `Assert.Contains`, `Assert.DoesNotContain`
- `Assert.InRange`, `Assert.Null`, `Assert.NotNull`
- NUnit constraint model versus xUnit static calls

**Answer**

xUnit's `Assert` class uses static methods rather than NUnit's fluent `Assert.That(actual, Is.EqualTo(expected))` syntax. The most used members are `Assert.Equal(expected, actual)` for value equality; `Assert.True(condition)` and `Assert.False(condition)` for Boolean results; `Assert.Throws<T>(action)` which returns the thrown exception so you can assert on its message or properties; `Assert.ThrowsAsync<T>(asyncAction)` for the async equivalent; `Assert.Contains(expected, collection)` which works for both substring-in-string and item-in-collection; and `Assert.InRange(actual, low, high)` for bounds checking.

For decimals, `Assert.Equal` accepts an optional `precision` parameter (number of decimal places) that avoids brittle floating-point comparisons: `Assert.Equal(10.00728m, result, precision: 5)`.

`Assert.Same(expected, actual)` checks reference equality (`Object.ReferenceEquals`), which is distinct from `Assert.Equal` which uses `IEquatable<T>` or `Equals`. `Assert.IsType<T>(obj)` and `Assert.IsAssignableFrom<T>(obj)` cover type checking without casting.

The key NUnit difference is style: NUnit encourages `Assert.That(actual, Is.EqualTo(expected).Within(0.001))` while xUnit keeps assertions as direct static calls. Both styles are effective; the xUnit style is arguably easier to grep and refactor since the structure is uniform.

---

## Q10. How does xUnit's default parallel execution model work, and how do you control it?

**Concepts**
- class-level parallelism (default unit of parallelism)
- sequential execution within a single test class
- `[Collection]` as a serialization boundary
- `CollectionBehavior` assembly attribute
- `MaxParallelThreads` configuration

**Answer**

By default, xUnit runs test classes in parallel using as many threads as there are logical processors. Tests within a single class always run sequentially on the same thread — the new-instance-per-test model means each method owns its instance, but they do not overlap. Tests in different classes run concurrently unless constrained.

The parallelism boundaries are controlled at three levels. First, test classes that share `[Collection("name")]` run serially with respect to each other, as if they were a single class. Second, `[assembly: CollectionBehavior(DisableTestParallelization = true)]` in an `AssemblyInfo.cs` file switches the entire assembly to sequential execution — useful when you have a legacy suite with shared mutable statics that cannot be refactored quickly. Third, `[assembly: CollectionBehavior(MaxParallelThreads = 4)]` caps the thread pool.

The recommended approach for most suites is to leave the default on and use `[Collection]` selectively to serialize the few classes that share expensive or stateful resources. Turning off parallelism globally sacrifices the significant time savings that come from running unrelated test classes concurrently on multi-core CI agents.

In xUnit v3, the parallelism model is refined further, with explicit support for assembly-level isolation and configurable execution strategies via `xunit.runner.json`.

---

## Q11. How do you skip a test in xUnit, and what are the alternatives?

**Concepts**
- `Skip` property on `[Fact]` and `[Theory]`
- `[Fact(Skip = "reason")]` syntax
- skipped tests appear in results (not ignored silently)
- conditional skip pattern using custom attributes
- xUnit v3 `[SkipWhen]` and `[SkipUnless]`

**Answer**

xUnit does not have a separate `[Skip]` attribute. Instead, both `[Fact]` and `[Theory]` expose a `Skip` named parameter: `[Fact(Skip = "Feature not yet implemented")]`. When `Skip` is set to any non-null string, xUnit marks the test as skipped and reports the string as the reason. Skipped tests appear in the test result output with a distinct status, so they are visible rather than silently excluded.

This is intentionally different from NUnit's `[Ignore]` attribute in that it is inline with the test marker, making it harder to miss during code review. The reason string is mandatory when skipping, which encourages teams to document why the test is disabled rather than silently commenting it out.

For conditional skipping — for example, running a test only on Linux — the xUnit ecosystem provides custom `DataAttribute` subclasses and community packages. xUnit v3 introduces `[SkipWhen]` and `[SkipUnless]` attributes that accept a property name on the test class returning a `bool`, enabling environment-driven decisions without external packages.

A common anti-pattern is commenting out a test method instead of using `Skip`. The commented-out test disappears from the results entirely, so regressions are invisible. Always prefer `Skip` with an explanatory reason.

---

## Q12. What improvements does xUnit v3 bring over v2?

**Concepts**
- source-generator-based test discovery (no reflection scanning)
- `net6.0`+ target framework requirement
- built-in `[SkipWhen]` / `[SkipUnless]`
- improved parallelism and assembly isolation
- `xunit.runner.json` configuration file
- removal of `xunit.abstractions` dependency

**Answer**

xUnit v3, released in 2024, is a major rewrite focused on performance, correctness, and modern .NET compatibility. The most architecturally significant change is that test discovery no longer relies on scanning assemblies at runtime via reflection. Instead, a Roslyn source generator emits a registration table at compile time, making discovery instantaneous and eliminating the dependency on `xunit.abstractions.dll`. Test projects targeting xUnit v3 require `net6.0` or later.

Parallelism is improved through assembly-level process isolation: each test assembly can run in its own process when using the v3 runners, preventing static state leakage between assemblies. The `MaxParallelThreads` configuration now supports `unlimited` and `default` symbolic values in addition to integers.

The built-in `[SkipWhen]` and `[SkipUnless]` attributes remove the need for third-party packages for conditional skipping. Configuration is centralized in an `xunit.runner.json` file per project, covering parallelism, output verbosity, and filter expressions without requiring `AssemblyInfo.cs` attributes.

The `Assert` class gains several new members including `Assert.Fail(message)` (a long-requested omission from v2) and improved collection assertion messages that show the full diff between expected and actual sequences. Migration from v2 to v3 is mostly a NuGet package version bump with small attribute namespace changes.

---

## Q13. How does the constructor injection model in xUnit compare to MSTest's `[TestInitialize]`?

**Concepts**
- xUnit fresh instance per test
- MSTest shared instance with `[TestInitialize]` method
- `[TestInitialize]` runs before each method on the same instance
- risk of shared mutable state in MSTest
- `IDisposable` versus `[TestCleanup]`

**Answer**

MSTest creates one instance of the test class for the entire test run within that class and calls the method marked `[TestInitialize]` before each test. This means fields that are not reset inside `[TestInitialize]` retain their values from the previous test, creating a subtle shared-state risk. If a test mutates a collection held in a field and `[TestInitialize]` only initializes it on first call, subsequent tests see the mutated version.

xUnit solves this by construction: each test gets a brand new instance. The constructor runs, fields are initialized, the test executes, and `IDisposable.Dispose()` is called. There is no way for one test to leave field state that the next test sees, because the next test has its own instance. This forces stateless or immutably initialized fields, which is a design improvement.

The tradeoff is that if construction is expensive — say, building an object graph — that cost is paid once per test rather than once per class. This is exactly the problem that `IClassFixture<T>` solves: the expensive resource lives on the fixture, not on the test class, and is shared across all tests while each test still gets a fresh class instance.

Cleanup in xUnit is `IDisposable.Dispose()`, whereas MSTest uses `[TestCleanup]`. Both run after each test, but xUnit's approach integrates cleanup with the standard C# resource management pattern rather than requiring a new attribute.

---

## Q14. How does `Assert.Throws<T>` differ from wrapping the call in a `try`/`catch` block?

**Concepts**
- `Assert.Throws<T>` returns the exception for further assertions
- test failure on no exception versus test error on wrong exception
- `Assert.ThrowsAsync<T>` for async paths
- explicit exception type check
- anti-pattern: `try/catch` with `Assert.True(false)`

**Answer**

`Assert.Throws<T>(action)` executes the action and asserts that it throws an exception of exactly type `T` (not a subtype). It returns the caught exception, so you can continue asserting on its properties immediately:

```csharp
ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(
    () => _pricing.ApplyDiscount(100m, 150m));
Assert.Contains("Discount must be between 0 and 100", ex.Message);
```

If the action throws the wrong exception type, `Assert.Throws<T>` rethrows it, so the test fails with the actual exception as the cause — much more informative than a generic "expected exception was not thrown" message. If the action does not throw at all, `Assert.Throws<T>` fails with a clear message stating that no exception was thrown.

The anti-pattern is a manual `try/catch`:

```csharp
// BAD
try
{
    _pricing.ApplyDiscount(100m, 150m);
    Assert.True(false, "Expected exception");
}
catch (ArgumentOutOfRangeException) { }
```

This fails to assert on the exception's content, and if a different exception type is thrown the `catch` misses it and the test errors rather than fails, making the test report misleading. It also requires extra boilerplate. Always prefer `Assert.Throws<T>`.

---

## Q15. How do you test a method that returns `IAsyncEnumerable<T>` in xUnit?

**Concepts**
- `async Task` test method with `await foreach`
- collecting results before asserting
- `Assert.Collection` for ordered element assertions
- `IAsyncDisposable` fixture cleanup
- avoiding `ToListAsync()` on infinite sequences

**Answer**

xUnit's async support extends naturally to `IAsyncEnumerable<T>`. Because the test method itself is `async Task`, you can use `await foreach` directly inside it:

```csharp
[Fact]
public async Task StreamOrders_ReturnsExpectedIds()
{
    var service = new OrderStreamService();
    var ids = new List<string>();

    await foreach (string id in service.StreamOrderIdsAsync(CancellationToken.None))
    {
        ids.Add(id);
    }

    Assert.Equal(3, ids.Count);
    Assert.Contains("ORD-001", ids);
}
```

When using `Assert.Collection`, collect into a `List<T>` first because `Assert.Collection` expects an `IEnumerable<T>`, not an `IAsyncEnumerable<T>`. The `System.Linq.Async` NuGet package (from the Reactive Extensions team) provides `ToListAsync()`, which simplifies collection: `var results = await asyncEnumerable.ToListAsync();`.

For scenarios where the async stream is potentially infinite or very long, pass a `CancellationToken` and cancel after retrieving enough elements to satisfy the assertion, to avoid the test hanging or running out of memory.

If the service producing the stream implements `IAsyncDisposable`, declare the service inside a `await using` block so cleanup is guaranteed even when assertions throw.

---

## Q16. What is `Assert.Collection` and when should you use it over `Assert.Equal` on a list?

**Concepts**
- element-by-element assertion with individual lambdas
- positional assertion — count must match exactly
- contrast with `Assert.Equal` on sequences
- `Assert.Single`, `Assert.Empty` for count shortcuts
- failure message shows which element failed and why

**Answer**

`Assert.Collection(collection, element => { ... }, element => { ... }, ...)` asserts that the collection has exactly as many elements as there are lambdas, then invokes each lambda with the corresponding element. The lambdas contain per-element assertions, so you can check different properties on each item in order.

```csharp
Assert.Collection(order.Lines,
    line => Assert.Equal("KB-001", line.Sku),
    line => { Assert.Equal("MS-010", line.Sku); Assert.Equal(2, line.Quantity); });
```

When a lambda throws, `Assert.Collection` reports which element index failed and the inner assertion message, giving precise context. If the collection has the wrong count, the count mismatch is reported before any element checks run.

`Assert.Equal(expectedList, actualList)` uses the sequence's `Equals` implementation and reports a general mismatch without element-level context. For simple value sequences like `int[]` or `string[]` where full equality is sufficient, `Assert.Equal` is terser. For collections of objects where you want to assert on specific properties without overriding `Equals`, `Assert.Collection` is clearer and produces better failure messages.

`Assert.Single(collection)` is a shortcut for asserting exactly one element and returns that element. `Assert.Empty(collection)` asserts zero elements. Both are more readable than `Assert.Equal(1, collection.Count())`.

---

## Q17. How do you use `[ClassData]` to supply test data from a separate class?

**Concepts**
- separate class implementing `IEnumerable<object[]>`
- `GetEnumerator()` called once at discovery time
- reuse across multiple test classes
- organizing large data sets in their own file
- `yield return` for lazy enumeration

**Answer**

`[ClassData(typeof(MyDataClass))]` tells xUnit to create an instance of `MyDataClass`, call `GetEnumerator()` on it, and use each returned `object[]` as one test row. The data class must implement `IEnumerable<object[]>` or `IEnumerable<TheoryDataRow<T>>` in xUnit v3.

```csharp
public sealed class DiscountTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { 100m, 0m, 100m };
        yield return new object[] { 100m, 10m, 90m };
        yield return new object[] { 200m, 25m, 150m };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[Theory]
[ClassData(typeof(DiscountTestData))]
public void ApplyDiscount_ClassData_ReturnsExpected(
    decimal subtotal, decimal percent, decimal expected) { ... }
```

`[ClassData]` is the right choice when the data set is large enough to warrant its own file, when it needs to be reused in multiple test classes, or when generating rows requires logic that would clutter the test class. For simpler cases, `[MemberData]` pointing to a static property in the same file is sufficient.

In xUnit v3, the strongly typed `TheoryData<T1, T2>` helper class is recommended over raw `object[]` arrays. It provides compile-time type checking so you cannot accidentally supply mismatched arguments.

---

## Q18. How do you verify that a mock or stub was called with specific arguments in xUnit?

**Concepts**
- xUnit has no built-in mock library
- `Moq` or `NSubstitute` integration
- `Mock.Verify` after the act phase
- argument matchers (`It.Is<T>`, `It.IsAny<T>`)
- xUnit's role as the test runner, not the mock framework

**Answer**

xUnit is a test runner and assertion library only — it does not include mocking or stubbing capabilities. You add a separate mocking library such as Moq, NSubstitute, or FakeItEasy alongside the xUnit NuGet packages.

With Moq, the pattern is: create the mock before the act, call the system under test, then verify on the mock after the act:

```csharp
[Fact]
public void PlaceOrder_ValidOrder_CallsRepositorySave()
{
    var repoMock = new Mock<IOrderRepository>();
    var service = new OrderService(repoMock.Object);
    var order = new Order { OrderId = "ORD-NEW", Lines = [] };

    service.PlaceOrder(order);

    repoMock.Verify(r => r.Save(It.Is<Order>(o => o.OrderId == "ORD-NEW")), Times.Once);
}
```

`It.Is<T>(predicate)` is an argument matcher that checks the argument satisfies a condition. `It.IsAny<T>()` accepts any argument of that type. `Times.Once`, `Times.Never`, and `Times.AtLeastOnce` express call count expectations.

xUnit runs the test exactly as it does any `[Fact]` — the mocking framework's `Verify` method throws an exception when the expectation is not met, which xUnit catches and reports as a test failure. There is no special integration required beyond referencing the Moq NuGet package.

---

# Gotchas

---

## Q19. Why does a `[Theory]` with no data attributes cause a runtime error rather than a compile error?

**Concepts**
- `[Theory]` requires at least one `[InlineData]`, `[MemberData]`, or `[ClassData]`
- discovery happens at runtime, not compile time
- `InvalidOperationException` at test execution
- no compiler enforcement of the constraint
- easy to miss in CI if the test is not run

**Answer**

The C# compiler treats `[Theory]` as just another attribute — it has no knowledge of xUnit's requirement that at least one data attribute accompanies it. The compiler only checks that the attribute is valid syntax; it does not cross-reference whether companion attributes are present.

xUnit enforces the constraint at runtime during test discovery. When the runner finds a method marked `[Theory]` with no data rows, it reports the test as errored with a message such as "No data found for XUnit.Tests.DiscountTheoryTests.ApplyDiscount_ReturnsExpectedAmount". The test appears in Test Explorer as failed rather than as skipped or not discovered, which can be surprising if you are used to compile-time safety.

This is a common gotcha when: refactoring removes `[InlineData]` lines while leaving `[Theory]`; copying a `[Fact]` method to use as a template and forgetting to add data; or when `[MemberData(nameof(MyData))]` points to a property that returns an empty sequence. In the last case, the test is silently skipped rather than errored in some xUnit versions, which is even harder to notice.

The fix is to change `[Theory]` to `[Fact]` if you intended a single scenario, or to add the appropriate data attributes. xUnit v3's analyzer NuGet package (`xunit.analyzers`) adds Roslyn diagnostics that surface the missing data attribute as a warning at compile time.

---

## Q20. Why do tests that depend on static shared state become flaky when xUnit parallelism is enabled?

**Concepts**
- test classes run in parallel by default
- static fields are process-wide, not instance-wide
- race condition between classes modifying the same static
- `[Collection]` as the correct fix, not `DisableTestParallelization`
- singleton services and thread safety

**Answer**

xUnit instantiates each test class on its own thread and runs different classes concurrently. Instance fields are safe because each test has its own instance. Static fields, however, are shared across all instances in the same process, so two classes running at the same time can read and write the same static concurrently without synchronization.

A concrete failure mode: `ClassA` sets a static `_connectionString` field in its first test, and `ClassB` sets the same static to a different value in its constructor before `ClassA`'s test has finished asserting. `ClassA` now reads `ClassB`'s value and the assertion fails intermittently depending on thread scheduling — a classic flaky test.

The correct fix is to eliminate the static state entirely by moving initialization into the constructor or into a fixture, not into a static field. If the static exists for performance reasons (e.g., a singleton that is expensive to build), wrap the relevant test classes in a `[Collection]`, which forces them to run serially and removes the race.

Disabling all parallelism with `[assembly: CollectionBehavior(DisableTestParallelization = true)]` also suppresses the failure but at the cost of dramatically longer test run times. It is a last resort for codebases that cannot be refactored quickly, not a permanent solution.

---

## Q21. Why does `Assert.Equal` sometimes fail for two objects that look identical?

**Concepts**
- `Assert.Equal` calls `IEquatable<T>.Equals` or `object.Equals`
- reference types without `Equals` override use reference equality
- value types use structural equality by default
- `Assert.Equivalent` (xUnit v2.4.2+) for deep structural comparison
- `IEqualityComparer<T>` overload on `Assert.Equal`

**Answer**

`Assert.Equal` invokes the `Equals` method of the type being compared. For value types like `int`, `decimal`, and `struct`, `Equals` compares by value. For reference types that do not override `Equals`, the default implementation inherited from `object` compares by reference — two separate instances with identical fields are not equal.

This trips up developers who create a new object in the test's Act phase and compare it to an expected object created in the Arrange phase:

```csharp
var expected = new OrderLine { Sku = "A", Quantity = 1, UnitPrice = 10m };
var actual = service.BuildLine("A", 1, 10m);

Assert.Equal(expected, actual); // FAILS unless OrderLine overrides Equals
```

The options are: override `Equals` and `GetHashCode` on `OrderLine` (appropriate when the type has a natural equality); use a C# `record` instead of a `class`, since records generate structural equality automatically; assert on individual properties instead of the whole object; or use `Assert.Equivalent(expected, actual)` available in xUnit v2.4.2+, which performs a deep structural comparison without requiring an `Equals` override.

`Assert.Equal` also accepts an `IEqualityComparer<T>` as a third argument for cases where you need custom comparison logic without modifying the type.

---

## Q22. What happens if a fixture's constructor or `InitializeAsync` throws an exception?

**Concepts**
- fixture construction failure prevents all tests in the class from running
- error is reported at the test level, not as a build failure
- `IAsyncLifetime.InitializeAsync` exception propagation
- all tests in the collection fail if a collection fixture throws
- defensive initialization pattern

**Answer**

If an `IClassFixture<T>` constructor throws, xUnit cannot inject the fixture, so every test in the affected class fails with an error message indicating the fixture initialization failed. The test methods themselves are never called — xUnit reports each as errored before execution. This behavior is consistent and predictable, but it means a single infrastructure failure (such as a database being unavailable) wipes out an entire class of tests.

The same applies to `IAsyncLifetime.InitializeAsync`: if it throws, xUnit propagates the exception and all tests in the class or collection fail. For `ICollectionFixture<T>`, a constructor failure in the shared fixture causes every test in every class that belongs to the collection to fail.

Defensive patterns help here. First, guard expensive resource acquisition with a try-catch in the fixture and store a nullable resource plus an error flag; individual tests can check `if (_fixture.Resource is null) { Assert.Fail("Fixture failed to initialize: " + _fixture.Error); }`. Second, keep fixture constructors as lightweight as possible and move I/O into `InitializeAsync` so failures are clearly asynchronous and easier to trace. Third, emit diagnostic output from the fixture using `IMessageSink` (injectable via a constructor overload) so the CI log shows exactly what failed.

---

## Q23. Why should you avoid `async void` in test methods, and what goes wrong in practice?

**Concepts**
- `async void` is fire-and-forget — no `Task` to await
- exceptions escape the `SynchronizationContext` and crash the process
- xUnit cannot catch exceptions from `async void` tests
- test appears to pass even when the assertion fails
- `async Task` is the correct return type

**Answer**

When you declare a test method as `async void` instead of `async Task`, xUnit calls the method synchronously up to the first `await`, sees the method return (it returns `void` immediately after the first yield), marks the test as passed, and moves on. Any code after the first `await` continuation runs on a thread pool thread that xUnit is no longer listening to. If an assertion throws there, the exception is unhandled, which in .NET terminates the entire test process with an unhandled exception on a background thread.

The failure mode is deceptive: the test shows as green in Test Explorer even though the assertion would have failed. This is one of the most dangerous subtle bugs in async test code because it produces false confidence.

```csharp
// WRONG — always appears green regardless of assertions after await
[Fact]
public async void GetOrder_ShouldReturnCorrectId_BAD()
{
    var order = await service.GetAsync("ORD-001");
    Assert.Equal("ORD-001", order.OrderId); // never runs in the test runner
}

// CORRECT
[Fact]
public async Task GetOrder_ShouldReturnCorrectId()
{
    var order = await service.GetAsync("ORD-001");
    Assert.Equal("ORD-001", order.OrderId);
}
```

The xUnit.Analyzers NuGet package includes an analyzer (`xUnit1048`) that reports `async void` test methods as a warning during compilation, catching the mistake before the misleading green result occurs.

---

# Real-World Scenarios

---

## Q24. Code review: what is wrong with the following test class?

```csharp
public class OrderTests
{
    private static OrderPricingService _pricing = new();

    [Fact]
    public void Test1_Subtotal_SetOnStatic()
    {
        _pricing = new OrderPricingService();
        decimal result = _pricing.CalculateSubtotal([]);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void Test2_Subtotal_ReadFromStatic()
    {
        decimal result = _pricing.CalculateSubtotal([]);
        Assert.Equal(0m, result);
    }
}
```

**Concepts**
- static mutable field shared across test instances
- race condition when parallelism is enabled
- constructor injection pattern as the fix
- test naming — `Test1`/`Test2` imply order dependence
- `sealed` modifier on test classes

**Answer**

| Category | Problem | Impact |
|----------|---------|--------|
| Thread safety | `_pricing` is `static` — shared across all instances of `OrderTests` and any other class in the assembly that runs in parallel | Two threads can read/write `_pricing` simultaneously; `Test1_Subtotal_SetOnStatic` reassigns it while `Test2_Subtotal_ReadFromStatic` reads it, creating a race condition |
| Test isolation | `Test2` depends on `_pricing` having been initialized by `Test1`, implying execution order | xUnit does not guarantee method order; if `Test2` runs before `Test1`, `_pricing` may be whatever the field initializer produced, which is fine here but the pattern is fragile |
| Naming | Method names `Test1_*` and `Test2_*` encode an expected run order in the name | Communicates to readers that these tests are coupled, even when they are not |
| Style | `class` is not `sealed` | Mutable static state and inheritance are a dangerous combination in test classes |

**Fix priority**

1. Change `static OrderPricingService _pricing` to an instance field: `private readonly OrderPricingService _pricing = new();`. xUnit's per-test instantiation then gives each test its own service, eliminating the race entirely.
2. Remove the `_pricing = new OrderPricingService();` reassignment inside `Test1` — it is now unnecessary.
3. Rename both methods to express what they assert, not their expected order: `CalculateSubtotal_EmptyLines_ReturnsZero`.
4. Mark the class `sealed` to prevent subclassing from reintroducing shared state.

---

## Q25. Code review: what is wrong with the following theory?

```csharp
[Theory]
[InlineData(null, 10, 0)]
[InlineData(100, 10, 90)]
[InlineData(100, -5, 0)]
public void ApplyDiscount_Theory(decimal? subtotal, decimal discount, decimal expected)
{
    var svc = new OrderPricingService();
    decimal result = svc.ApplyDiscount(subtotal ?? 0m, discount);
    Assert.Equal(expected, result);
}
```

**Concepts**
- negative discount percent should throw, not return zero
- hiding expected exceptions with null-coalescing logic
- `[InlineData]` row testing an invalid input without `Assert.Throws`
- misalignment between data row intent and assertion
- testing multiple behaviors in one theory

**Answer**

| Category | Problem | Impact |
|----------|---------|--------|
| Incorrect assertion | Row `[InlineData(100, -5, 0)]` expects zero for a negative discount, but `OrderPricingService.ApplyDiscount` throws `ArgumentOutOfRangeException` for `discountPercent < 0` | The test will error, not fail, masking the real behavior |
| Missing exception test | The negative-discount case is an error path that should be tested with `Assert.Throws<ArgumentOutOfRangeException>`, not as a normal-result row | A row that is expected to throw cannot be expressed as an `[InlineData]` row on a non-throwing theory |
| Null masking | The `subtotal ?? 0m` coalescion hides what happens when a null argument reaches the SUT | The `null` row becomes functionally identical to `[InlineData(0, 10, 0)]`; no additional coverage is gained |
| Mixed concerns | The theory mixes valid input rows with an invalid input row that requires a different assertion strategy | Makes the theory harder to read and impossible to express correctly with a single assertion |

**Fix priority**

1. Remove `[InlineData(100, -5, 0)]` from the theory entirely.
2. Add a separate `[Fact]` that calls `Assert.Throws<ArgumentOutOfRangeException>(() => svc.ApplyDiscount(100m, -5m))` to cover the invalid-input path.
3. Remove `[InlineData(null, 10, 0)]` and change the parameter type from `decimal?` to `decimal` — `null` is not a valid subtotal and the coalescion adds no value.
4. Consider adding `[InlineData(0, 10, 0)]` explicitly if a zero-subtotal case is meaningful to document.

---

## Q26. How would you structure fixtures to share a real database connection across multiple test classes for integration tests?

**Concepts**
- `ICollectionFixture<T>` for cross-class sharing
- `IAsyncLifetime` for async database setup and teardown
- `[CollectionDefinition]` binding fixture to collection name
- one transaction per test for rollback isolation
- connection string from environment variable or `appsettings.Test.json`

**Answer**

The standard pattern is to put the expensive connection setup in a fixture class implementing `IAsyncLifetime`, register it with a `[CollectionDefinition]`, and have all integration test classes join that collection.

```csharp
public sealed class DatabaseFixture : IAsyncLifetime
{
    public SqlConnection Connection { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        string cs = Environment.GetEnvironmentVariable("TEST_DB")
            ?? throw new InvalidOperationException("TEST_DB not set");
        Connection = new SqlConnection(cs);
        await Connection.OpenAsync();
        await ApplyMigrationsAsync(Connection);
    }

    public async Task DisposeAsync()
    {
        await Connection.DisposeAsync();
    }
}

[CollectionDefinition("Database")]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }

[Collection("Database")]
public sealed class OrderRepositoryTests
{
    private readonly DatabaseFixture _db;
    public OrderRepositoryTests(DatabaseFixture db) => _db = db;

    [Fact]
    public async Task Save_ValidOrder_PersistsToDatabase()
    {
        await using SqlTransaction tx = _db.Connection.BeginTransaction();
        try
        {
            var repo = new OrderRepository(_db.Connection, tx);
            await repo.SaveAsync(new Order { OrderId = "ORD-IT-001", Lines = [] });
            Order? loaded = await repo.GetAsync("ORD-IT-001", tx);
            Assert.NotNull(loaded);
        }
        finally { await tx.RollbackAsync(); }
    }
}
```

Key design decisions: the fixture opens the connection once for all tests; each test wraps its operations in a transaction that is always rolled back in a `finally` block, so the database returns to a clean state after each test without truncating tables; `IAsyncLifetime` rather than a constructor is used so the async `OpenAsync` and migration step do not block synchronously. The connection string comes from an environment variable set in the CI pipeline, not hardcoded.

---

## Q27. How would you test a method that uses `CancellationToken` and verify cancellation is handled correctly?

**Concepts**
- `CancellationTokenSource` creation in the test
- pre-cancelled token versus cancellation during execution
- `Assert.ThrowsAsync<OperationCanceledException>`
- `TaskCanceledException` is a subclass of `OperationCanceledException`
- cooperative cancellation pattern in the SUT

**Answer**

Testing cancellation involves two distinct scenarios: passing an already-cancelled token to verify the method exits immediately, and cancelling mid-execution to verify the method respects the token during long-running work.

For immediate cancellation, create a `CancellationTokenSource`, cancel it before passing, and assert `OperationCanceledException`:

```csharp
[Fact]
public async Task ProcessOrdersAsync_PreCancelledToken_ThrowsOperationCancelled()
{
    using var cts = new CancellationTokenSource();
    cts.Cancel();

    await Assert.ThrowsAsync<OperationCanceledException>(
        () => _service.ProcessOrdersAsync(cts.Token));
}
```

For mid-execution cancellation, use `CancelAfter` with a short timeout and ensure the SUT checks `token.IsCancellationRequested` or calls `token.ThrowIfCancellationRequested()` inside its loop:

```csharp
[Fact]
public async Task ProcessOrdersAsync_CancelledDuringExecution_StopsProcessing()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

    await Assert.ThrowsAsync<OperationCanceledException>(
        () => _service.ProcessOrdersAsync(cts.Token));
}
```

Note that `TaskCanceledException` is a subclass of `OperationCanceledException`, so `Assert.ThrowsAsync<OperationCanceledException>` catches both. If you need to distinguish between a task that was cancelled versus a timeout, check `ex.CancellationToken == cts.Token`. Avoid using `Thread.Sleep` in the test to time the cancellation — use `CancelAfter` and let the SUT's own cancellation check determine when it stops.

---

## Q28. How do you organize a test suite for a pricing service that has both fast unit tests and slow integration tests, and run only the fast tests in pull request builds?

**Concepts**
- `[Trait("Category", "Unit")]` and `[Trait("Category", "Integration")]`
- `dotnet test --filter "Category=Unit"` in CI pipeline
- separate test projects versus traits in one project
- `xunit.runner.json` for default filter
- test pyramid strategy

**Answer**

There are two main structural approaches: separate test projects or a single project with trait-based filtering. The single-project approach with traits is simpler to maintain and is sufficient for most teams.

Apply `[Trait("Category", "Unit")]` to all fast, in-memory tests and `[Trait("Category", "Integration")]` to tests that require a database, HTTP endpoint, or file system:

```csharp
[Fact]
[Trait("Category", "Unit")]
public void CalculateSubtotal_TwoLines_ReturnsSumOfLineTotals() { ... }

[Fact]
[Trait("Category", "Integration")]
public async Task SaveOrder_PersistsToDatabase() { ... }
```

In the pull request CI pipeline, run `dotnet test --filter "Trait[Category]=Unit"`. In the nightly or merge pipeline, run `dotnet test` without a filter to include all tests. This keeps PR builds fast (seconds) while full validation still runs.

The `xunit.runner.json` file placed alongside the test project allows setting a default filter for local development:

```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "methodDisplay": "classAndMethod",
  "diagnosticMessages": false
}
```

For very large codebases, separate projects provide stronger isolation: integration tests can reference different NuGet packages (Testcontainers, Respawn) without those appearing in the unit test project's dependency graph. The unit test project then builds faster on its own and can be run without any infrastructure.

---

## Q29. Code review: what is wrong with the following use of `IClassFixture`?

```csharp
public sealed class OrderIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;

    public OrderIntegrationTests(DatabaseFixture db) => _db = db;

    [Fact]
    public async Task Test_CreateOrder_SetsId()
    {
        await _db.Connection.ExecuteAsync("DELETE FROM Orders");
        var repo = new OrderRepository(_db.Connection);
        await repo.SaveAsync(new Order { OrderId = "ORD-001", Lines = [] });
        int count = await _db.Connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Orders");
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Test_CountOrders_AfterCreate()
    {
        int count = await _db.Connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Orders");
        Assert.Equal(1, count);
    }
}
```

**Concepts**
- shared mutable database state between tests in the same class
- `DELETE FROM Orders` in one test affects subsequent tests
- order dependence when `IClassFixture` shares state
- transaction-per-test rollback pattern
- test isolation requirement

**Answer**

| Category | Problem | Impact |
|----------|---------|--------|
| State mutation | `Test_CreateOrder_SetsId` deletes all rows and inserts one row directly into the shared database via the fixture's connection | The mutation persists to the next test because `IClassFixture<T>` shares one fixture instance across all tests in the class |
| Order dependence | `Test_CountOrders_AfterCreate` asserts `count == 1`, which is only true if `Test_CreateOrder_SetsId` ran first and was not cleaned up | xUnit does not guarantee test method order; this will fail intermittently depending on execution order |
| Missing cleanup | There is no `DELETE` or rollback after `Test_CreateOrder_SetsId` | Any tests added later will see leftover rows from this test |
| Implicit coupling | The two tests are secretly coupled through the database table | Renaming, reordering, or deleting one test silently breaks the other |

**Fix priority**

1. Wrap each test's database operations in a `SqlTransaction` that is unconditionally rolled back in a `finally` block, ensuring the table is clean after every test regardless of pass or fail.
2. Remove the `DELETE FROM Orders` statement from `Test_CreateOrder_SetsId` — if the table is clean at the start of each test via the rollback pattern, the delete is unnecessary.
3. Rewrite `Test_CountOrders_AfterCreate` to perform its own insert inside its own transaction, assert the count, and roll back — it should not depend on another test's data.
4. Rename both tests to describe their individual behavior rather than implying a sequence: `SaveOrder_ValidOrder_PersistsRow` and `GetOrderCount_AfterSingleInsert_ReturnsOne`.


## Gotchas — xUnit (Interview Traps)

---

#### Gotcha 1. `[Fact]` vs `[Theory]` — Fact is a single test; Theory runs multiple times with `[InlineData]`

**Concepts**
- `[Fact]`: one execution, no parameters
- `[Theory]`: parameterized; requires at least one `[InlineData]`, `[MemberData]`, or `[ClassData]`
- `[Theory]` without a data attribute causes a runtime error, not a compile error
- each `[InlineData]` row appears as a separate test in the test runner

**Answer**

`[Fact]` marks a single, non-parameterized test method. `[Theory]` marks a method that accepts parameters and is driven by one or more data attributes; the test runner invokes it once per data row. A `[Theory]` method with no data attributes compiles successfully but throws `InvalidOperationException: No data found for Theory` at runtime — the compiler does not catch this. Each `[InlineData]` row appears as a separate named test in the test results, making it easy to see which specific input caused a failure. `[MemberData]` and `[ClassData]` are used when the data is too complex or large to inline as attribute arguments.

---

#### Gotcha 2. xUnit creates a new test class instance per test — `IClassFixture<T>` for shared setup

**Concepts**
- xUnit instantiates the test class constructor for each test method
- no shared state between tests via instance fields (by design)
- `IClassFixture<T>` provides one shared fixture instance per test class
- `ICollectionFixture<T>` shares one fixture across multiple test classes

**Answer**

Unlike MSTest (which reuses the test class instance by default), xUnit creates a brand new instance of the test class for each test method. This means instance fields initialized in the constructor are fresh for every test — there is no accidental shared mutable state between tests. When expensive setup (e.g., starting a test database or loading a file) must be shared, implement `IClassFixture<T>` by declaring `public class MyTests : IClassFixture<DatabaseFixture>` and injecting the fixture via the constructor. The fixture is constructed once per test class and disposed after the last test in the class. For sharing across multiple test classes, use `ICollectionFixture<T>` with a `[Collection("name")]` attribute.

---

#### Gotcha 3. `Assert.Throws<T>` vs `Record.Exception` — Throws asserts AND returns the exception

**Concepts**
- `Assert.Throws<T>(action)` asserts that the action throws exactly `T` and returns the exception
- `Record.Exception(action)` captures any exception without asserting; returns `null` if none thrown
- `await Assert.ThrowsAsync<T>(asyncAction)` for async code
- `Assert.Throws<T>` fails if a derived exception type is thrown instead of exactly `T`

**Answer**

`var ex = Assert.Throws<ArgumentException>(() => sut.Process(null))` both asserts that `ArgumentException` is thrown and returns the exception object for further inspection (`ex.ParamName`, `ex.Message`). If no exception or a different exception type is thrown, the test fails with a descriptive message. `Record.Exception(() => sut.Process(null))` captures whatever exception occurs (or `null` if none) without asserting — useful when you want to check multiple conditions on the exception or test that no exception is thrown. `Assert.Throws<T>` uses exact type matching, not `is`: if the code throws `ArgumentNullException` (which derives from `ArgumentException`), `Assert.Throws<ArgumentException>` still fails; use `Assert.ThrowsAny<ArgumentException>` for "this type or derived".

---

#### Gotcha 4. `Assert.Equal` collection overload — compares elements, not reference

**Concepts**
- `Assert.Equal(expected, actual)` on collections compares element-by-element using `Equals`
- `Assert.Same(expected, actual)` checks reference identity
- order matters: `[1, 2, 3]` is not equal to `[3, 2, 1]` by default
- `Assert.Equivalent` (xUnit 2.7+) for order-insensitive deep comparison

**Answer**

`Assert.Equal(new[] { 1, 2, 3 }, new[] { 1, 2, 3 })` passes because xUnit's `Equal` for `IEnumerable<T>` compares elements in order using the element's `Equals`. Two distinct array objects with the same elements are considered equal — this is value equality, not reference equality. `Assert.Same` would fail for separately allocated arrays even with identical content. Element order matters: `Assert.Equal(new[] { 1, 2, 3 }, new[] { 3, 2, 1 })` fails. When order should not matter, use `Assert.Equivalent(expected, actual, strict: false)` (xUnit 2.7+) or sort both collections before comparing. For dictionaries and complex objects, `Assert.Equivalent` performs a deep structural comparison.

---

#### Gotcha 5. `[Collection]` attribute — tests in the same collection share a `ICollectionFixture<T>`

**Concepts**
- `[Collection("IntegrationTests")]` groups test classes into one collection
- all classes in the collection share one `ICollectionFixture<T>` instance
- xUnit does NOT run test classes in the same collection in parallel (by design)
- a collection with no `ICollectionFixture<T>` still serializes test class execution

**Answer**

`[Collection("IntegrationTests")]` applied to two test classes tells xUnit they belong to the same collection. xUnit then: (a) creates a single shared `ICollectionFixture<T>` instance that is injected into all test classes in the collection, and (b) does not run the test classes in parallel with each other. This is the mechanism for sharing an expensive resource (a database container, a test server) across multiple test classes without reinitializing it for each class. It is also the mechanism for serializing classes that must not run concurrently (e.g., tests that mutate a shared database). Applying `[Collection]` without a corresponding `ICollectionFixture<T>` still serializes the classes, which is sometimes used deliberately to prevent parallelism even without shared state.

---

#### Gotcha 6. `IAsyncLifetime` — async setup and teardown for xUnit fixtures

**Concepts**
- `IAsyncLifetime` provides `InitializeAsync()` and `DisposeAsync()` for async fixture lifecycle
- replaces constructor (synchronous) when setup involves async I/O
- xUnit awaits `InitializeAsync` before running any test in the class
- `IAsyncLifetime` on the test class itself provides per-test async setup/teardown

**Answer**

xUnit calls the test class constructor synchronously, which means async initialization cannot be awaited in the constructor. `IAsyncLifetime` provides `InitializeAsync()` and `DisposeAsync()` methods that xUnit awaits around each test. Implementing `IAsyncLifetime` on a fixture class (used with `IClassFixture<T>`) allows async initialization — starting a container, opening a database connection, seeding data — before the first test runs. Implementing it on the test class itself provides per-test async setup (like `[TestInitialize]` in MSTest). Using `Task.Run(() => asyncSetup()).GetAwaiter().GetResult()` in the constructor as a workaround can cause deadlocks in synchronization-context environments and should never be used.

---

#### Gotcha 7. `Assert.True(condition)` vs meaningful assertion — always prefer specific assertions for better error messages

**Concepts**
- `Assert.True(a == b)` reports only "True was expected to be True" on failure
- `Assert.Equal(a, b)` reports the expected and actual values
- `Assert.True` is appropriate only for custom conditions with no specific assertion
- specific assertions (`Equal`, `Contains`, `Null`, `Empty`) produce actionable failure messages

**Answer**

`Assert.True(order.Total == 99.99m)` is legal xUnit but produces a useless failure message: "Assert.True() Failure. Expected: True. Actual: False." — you have to debug to find the actual value. `Assert.Equal(99.99m, order.Total)` produces "Assert.Equal() Failure. Expected: 99.99. Actual: 105.00" — the failure is self-describing. Always use the most specific assertion available: `Assert.Equal` for values, `Assert.Contains` for collections and strings, `Assert.Null` / `Assert.NotNull`, `Assert.Empty` / `Assert.NotEmpty`, `Assert.Throws` for exceptions. Reserve `Assert.True` and `Assert.False` for conditions where no specific assertion exists, and always add a failure message: `Assert.True(result, "Expected valid result for input X")`.

---

#### Gotcha 8. Skip attribute: `[Fact(Skip = "reason")]` — test is not run but still appears in results

**Concepts**
- `[Fact(Skip = "...")]` skips the test; it appears as "Skipped" in the test report
- reason string is mandatory best practice — documents why the test is skipped
- skipped tests still count toward the test report but not toward pass/fail
- skipping should be temporary; accumulating skipped tests reduces trust in the suite

**Answer**

`[Fact(Skip = "Blocked by issue #1234 — API not yet implemented")]` prevents the test from running while keeping it visible in the test report as "Skipped". This is different from commenting out the test — a skipped test is still discovered and reported, serving as a visible reminder that coverage is temporarily incomplete. The skip reason should reference an issue or explain the temporary condition so the team can decide when to re-enable it. A test suite with dozens of permanent skips is a sign of deferred work; treat the skip reason as a to-do comment and set up a process to review and clear skipped tests periodically.

---

#### Gotcha 9. Parallel test execution — xUnit runs test classes in parallel by default; use `[Collection]` to serialize

**Concepts**
- xUnit default: test CLASSES run in parallel; test METHODS within a class run sequentially
- shared mutable static state between classes causes intermittent failures under parallelism
- `[Collection("name")]` groups classes that must not run in parallel with each other
- `xunit.runner.json` with `"parallelizeAssembly": false` disables all parallelism

**Answer**

xUnit's default behavior runs test classes in parallel threads, which finds concurrency bugs in production code but also exposes test suite design flaws. Tests in different classes that mutate shared static fields, a shared database without row-level isolation, or a shared port number will fail intermittently depending on execution order and timing. The `[Collection("SharedResource")]` attribute groups classes into one collection that xUnit runs sequentially. For integration tests that all need database access, placing them in one collection with a shared `DatabaseFixture` ensures they run sequentially against the same database instance, eliminating race conditions without sacrificing the shared setup cost.

---

#### Gotcha 10. `Output` via `ITestOutputHelper` — Console.WriteLine output is not captured; inject ITestOutputHelper

**Concepts**
- `Console.WriteLine` output is not visible in the xUnit test report
- inject `ITestOutputHelper` via constructor and call `_output.WriteLine(...)`
- output is captured per-test and shown only when the test fails (reduces noise)
- useful for diagnostic information without polluting the pass output

**Answer**

xUnit does not capture `Console.WriteLine` output because tests run in threads without a bound console. To produce diagnostic output visible in the test results, inject `ITestOutputHelper` via the test class constructor: `public MyTests(ITestOutputHelper output) => _output = output;` and call `_output.WriteLine($"Processing {item}")` inside the test. xUnit captures this output per test and includes it in the test result when the test fails — it does not appear for passing tests, which keeps the output noise-free for the majority of green tests. This is the correct pattern for logging intermediate values during a debugging session or for capturing data that helps diagnose a flaky test failure.

---