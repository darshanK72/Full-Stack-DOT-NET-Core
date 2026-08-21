# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/09. Unit Testing/03. MSTest`

---

#### Q1. (R) A `[TestClass]` has two `[TestInitialize]` methods. One test passes in Visual Studio, another fails on CI — and swapping test names changes which one fails. Review:

```csharp
[TestClass]
public class DiscountSetupOrderTests
{
    private DiscountCalculator _calculator = null!;
    private decimal _seededTotal;

    [TestInitialize]
    public void SeedFromTierDiscount()
    {
        _seededTotal = _calculator.ApplyTierDiscount(100.00m, CustomerTier.Silver);
    }

    [TestInitialize]
    public void CreateCalculator()
    {
        _calculator = new DiscountCalculator();
    }

    [TestMethod]
    public void SeededTotal_IsNinety_WhenSilverOnOneHundred()
    {
        Assert.AreEqual(90.00m, _seededTotal);
    }
}
```

What lifecycle rule did the author violate, and how do you fix it?

**Answer:** MSTest runs every `[TestInitialize]` method before each test, but **does not guarantee order** between those methods — `SeedFromTierDiscount` can run before `CreateCalculator`, leaving `_calculator` null and causing `NullReferenceException` or wrong state depending on discovery order.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifecycle | Two `[TestInitialize]` methods with hidden dependency | Non-deterministic setup — pass/fail varies by runner ordering |
| Design | `_calculator` used before guaranteed initialization | Intermittent CI failures when test discovery order changes |
| Maintainability | Split setup across unordered hooks | Next refactor breaks tests silently |

**Fix (priority order):**

1. Merge setup into **one** `[TestInitialize]` with explicit top-to-bottom order — create SUT first, then seed derived state (matches `DiscountCalculatorCoreTests.cs` in this chapter).
2. Never assume one `[TestInitialize]` runs before another; if phases are required, call private helpers from a single hook in the order you need.
3. Keep each `[TestMethod]` independent — no reliance on another test having run first.
4. Run `dotnet test` on CI (not only Test Explorer) to catch ordering assumptions early.

```csharp
[TestInitialize]
public void SetUp()
{
    _calculator = new DiscountCalculator();
    _seededTotal = _calculator.ApplyTierDiscount(100.00m, CustomerTier.Silver);
}
```

**Production takeaway:** `[TestInitialize]` is "before each test," not "ordered pipeline stages" — Karat uses multi-hook classes to test whether you know MSTest lifecycle is per-test but unordered across hooks. See **Program.cs** Section 5 and `DiscountCalculatorCoreTests.cs`.

---

#### Q2. (R) A developer ports money tests from `DiscountCalculatorDataTests.cs` and the project no longer builds. Review:

```csharp
[DataTestMethod]
[DataRow(100.00m, 0.10m, 90.00m)]
[DataRow(50.00m, 0.20m, 40.00m)]
public void ApplyPercentDiscount_ReturnsExpectedTotal(
    decimal orderTotal,
    decimal discountRate,
    decimal expectedTotal)
{
    var calculator = new DiscountCalculator();
    decimal result = calculator.ApplyPercentDiscount(orderTotal, discountRate);
    Assert.AreEqual(expectedTotal, result);
}
```

What is wrong with this parameterized test, and what pattern does this chapter use instead?

**Answer:** `[DataRow]` attribute arguments must be compile-time constants of types MSTest supports — **`decimal` literals are not valid `[DataRow]` arguments**, so the build fails even though `decimal` works fine in normal C# code and in the SUT.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `[DataRow(100.00m, …)]` — decimal not accepted | CS1503 / attribute argument errors — tests never run |
| Domain | Money calculations need `decimal` in production code | Forcing `double` in rows risks precision loss if cast is forgotten |
| Pattern drift | Ignores chapter convention in `DiscountCalculatorDataTests.cs` | Team copies broken snippet into CI |

**Fix (priority order):**

1. Keep `[DataRow]` values as **`double` literals** (or other supported types like `int`, `string`, `enum`).
2. Cast to `decimal` **inside** the test body before calling `ApplyPercentDiscount` — exactly as this chapter does.
3. Assert with `decimal` via `Assert.AreEqual((decimal)expectedTotal, result)` for money.
4. For large or computed tables, use `[DynamicData]` from a static method returning `IEnumerable<object[]>` when `[DataRow]` constants are too limiting.

```csharp
[DataTestMethod]
[DataRow(100.00, 0.10, 90.00)]
public void ApplyPercentDiscount_ReturnsExpectedTotal(
    double orderTotal, double discountRate, double expectedTotal)
{
    decimal result = new DiscountCalculator().ApplyPercentDiscount(
        (decimal)orderTotal, (decimal)discountRate);
    Assert.AreEqual((decimal)expectedTotal, result);
}
```

**Production takeaway:** Parameterized MSTest tests carry a compile-time type contract separate from your domain model — always cast at the boundary. See **Program.cs** Section 6 pitfall and `DiscountCalculatorDataTests.cs`.

---

#### Q3. (R) A `[DataTestMethod]` is tagged `[TestMethod]` by mistake during a merge. Test Explorer shows one test; the team assumes all discount rows are covered. Review:

```csharp
[TestClass]
public class DiscountRowCoverageTests
{
    private DiscountCalculator _calculator = null!;

    [TestInitialize]
    public void SetUp() => _calculator = new DiscountCalculator();

    [TestMethod]
    [DataRow(100.00, 0.10, 90.00)]
    [DataRow(50.00, 0.20, 40.00)]
    [DataRow(0.00, 0.05, 0.00)]
    public void ApplyPercentDiscount_ReturnsExpectedTotal(
        double orderTotal, double discountRate, double expectedTotal)
    { /* ... */ }
}
```

What breaks at compile time or discovery time, and why is `[DataTestMethod]` required here?

**Answer:** `[TestMethod]` methods **must have no parameters** — a parameterized signature with `[DataRow]` requires `[DataTestMethod]`, otherwise the compiler rejects the method or the adapter will not expand rows into separate test cases.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile / discovery | `[TestMethod]` + parameters + `[DataRow]` mismatch | Build error or single undiscoverable method — not three row cases |
| Coverage illusion | One green test in Explorer | Two discount scenarios never executed — regression slips to prod |
| Merge hygiene | Attribute regression during conflict resolution | Silent loss of data-driven coverage |

**Fix (priority order):**

1. Replace `[TestMethod]` with **`[DataTestMethod]`** on any method that takes `[DataRow]` arguments.
2. Verify Test Explorer / `dotnet test` lists **one result per row** (e.g., `…(100,0.1,90)`, `…(50,0.2,40)`).
3. Add CI coverage gates or `--filter` spot checks on critical `[DataTestMethod]` classes after merges.
4. Prefer explicit naming so row failures identify inputs in the log.

**Production takeaway:** `[DataRow]` without `[DataTestMethod]` is a common merge accident — Karat checks whether you know MSTest distinguishes single-case `[TestMethod]` from parameterized `[DataTestMethod]`. See **Program.cs** Sections 3 and 6.

---

#### Q4. (R) A pricing test passes locally but fails on a parallel CI agent. Review the class-level setup and two tests:

```csharp
[TestClass]
public class TierRateTableTests
{
    private static Dictionary<CustomerTier, decimal> _tierRateTable = null!;
    // ... ClassInitialize loads dictionary ...
    // OverrideGoldRate_ForPromoScenario mutates _tierRateTable[Gold] = 0.25m
    // GoldRate_MatchesClassInitializeDefault expects 0.15m
}
```

What does `[ClassInitialize]` actually share across tests, and why is the second test flaky?

**Answer:** `[ClassInitialize]` runs **once per `[TestClass]`** and the static `_tierRateTable` is **shared by every test in that class** — mutating it in one `[TestMethod]` leaks state into others, and parallel runners make execution order non-deterministic so the "default rate" test flakes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Mutable static dictionary written in one test, read in another | Order-dependent pass/fail — classic flake on parallel CI |
| Lifecycle misuse | `[ClassInitialize]` treated as "read-only reference data" but not enforced | Promo override permanently changes shared table for the class run |
| Parallelism | MSTest can run tests concurrently within an assembly (V3+ / `[Parallelize]`) | Shared mutable statics race across threads |

**Fix (priority order):**

1. Treat `[ClassInitialize]` data as **immutable** — `IReadOnlyDictionary<CustomerTier, decimal>` or copy-on-read, never mutate in tests (pattern in `DiscountCalculatorLifecycleTests.cs`).
2. If a test needs a modified table, **clone** the dictionary in `[TestInitialize]` or build local expected values without touching static state.
3. Reset statics in `[ClassCleanup]` only for teardown of external resources — not as a substitute for per-test isolation.
4. For expensive shared **services**, prefer instance fields set in `[TestInitialize]` with a fresh SUT; reserve class-level hooks for truly read-only, expensive setup.

```csharp
[ClassInitialize]
public static void InitClass(TestContext context)
{
    _tierRateTable = new ReadOnlyDictionary<CustomerTier, decimal>(
        new Dictionary<CustomerTier, decimal> { /* rates */ });
}

// In test: decimal expectedRate = _tierRateTable[tier]; — read only
```

**Production takeaway:** `[ClassInitialize]` is one-time shared setup, not a per-test reset — same judgment as xUnit `IClassFixture` mutation traps (Chapter 02). See **Program.cs** Section 8 and `DiscountCalculatorLifecycleTests.cs`.

---

#### Q5. (R) A teammate adds a "quick sanity" test for computed discount rates using `float`. It fails in CI with mismatched expected vs actual. Review:

```csharp
[TestMethod]
public void ApplyPercentDiscount_TenPercentOnHundred_ReturnsNinetyAsFloat()
{
    float discounted = (float)calculator.ApplyPercentDiscount(100.0m, 0.10m);
    Assert.AreEqual(90.0f, discounted);
}

[TestMethod]
public void FloatSum_IsPointThree()
{
    double sum = 0.1 + 0.2;
    Assert.AreEqual(0.3, sum);
}
```

What is wrong with exact equality on floating-point types in MSTest, and how does `Assert.AreEqual` help?

**Answer:** Binary floating-point (`float`/`double`) cannot represent many decimal fractions exactly — exact `Assert.AreEqual` without tolerance fails even when the math is "correct" at business precision (e.g., `0.1 + 0.2 != 0.3`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Exact equality on `float`/`double` | False failures — tests red while production code is fine |
| Domain | Money path cast to `float` for assertion | Loses decimal precision the SUT was designed for |
| API misuse | Two-parameter `AreEqual` on floats | Ignores MSTest's delta overload meant for this scenario |

**Fix (priority order):**

1. For **money**, stay on `decimal` end-to-end — `Assert.AreEqual(90.00m, result)` as in `DiscountCalculatorAssertTests.cs`.
2. When asserting `float`/`double` is unavoidable, use the **delta overload**: `Assert.AreEqual(90.0f, discounted, 0.001f)` or appropriate epsilon.
3. Delete or rewrite "sanity" tests that cast pricing results to `float` — they test the wrong type.
4. Document team standard: financial asserts use `decimal`; floating-point asserts always specify tolerance.

```csharp
Assert.AreEqual(0.3, sum, 1e-10);           // double with delta
Assert.AreEqual(90.0f, discounted, 0.01f);  // float with delta — prefer decimal for money
```

**Production takeaway:** `Assert.AreEqual` has value-type and reference-type paths — for `double`/`float` the three-argument delta form is the production-safe pattern. See **Program.cs** Section 7.

---

#### Q6. (D) Your team starts a greenfield .NET 8 microservice. `dotnet new xunit` is the SDK default; Visual Studio still offers an MSTest template. Half the org maintains legacy MSTest suites from .NET Framework migrations. For **new** unit-test projects on this service, which framework would you standardize on and why? Cover discovery, lifecycle hooks, parallelism defaults, and when staying on MSTest is the pragmatic choice.

**Answer:** For most greenfield .NET 8 services, **standardize on xUnit** — SDK templates, community samples, and OSS libraries target it first — while **keeping MSTest** where enterprise templates, existing suites, or Visual Studio-centric workflows dominate.

- **Greenfield default → xUnit:** `dotnet new xunit` is zero-friction; `[Fact]` / `[Theory]` + `[InlineData]` map cleanly from MSTest once attribute names are known (see **Program.cs** Section 9 table). Constructor + `IDisposable` replace `[TestInitialize]` / `[TestCleanup]` with explicit C# lifecycle.
- **Discovery & CLI:** Both run under `Microsoft.NET.Test.Sdk` and `dotnet test`; filter syntax differs slightly but CI integration is equivalent. Pick one framework per solution/service to avoid duplicate adapter packages and mixed conventions.
- **Parallelism:** xUnit parallelizes by default across collections — good for speed, requires immutable fixtures (Chapter 02). MSTest historically ran more sequentially per assembly; V3+ improves parallelism but teams must opt in with `[Parallelize]` and avoid mutable statics from `[ClassInitialize]`.
- **Lifecycle mapping:** `[ClassInitialize]` / `[ClassCleanup]` ↔ `IClassFixture<T>` / `IDisposable`; `[AssemblyInitialize]` ↔ custom assembly fixtures or `AssemblyFixture` patterns — xUnit's fixture model is more composable for large suites.
- **When MSTest is pragmatic:** Large legacy MSTest bank being extended incrementally; org mandate / Azure DevOps templates; mixed-skill teams already trained on `[TestClass]` / Test Explorer groupings; internal libraries with MSTest examples only.
- **Migration strategy:** New microservice → xUnit; shared test utilities abstract SUT setup so framework attributes stay thin; do not run MSTest and xUnit test projects with conflicting static state in one CI job without understanding parallel scope.

**Production takeaway:** Framework choice is a team maintainability decision, not a capability gap — both run on `dotnet test`. Prefer one convention per repo boundary; know the attribute map in **Program.cs** QUICK REFERENCE when reading either style in code review.

---
