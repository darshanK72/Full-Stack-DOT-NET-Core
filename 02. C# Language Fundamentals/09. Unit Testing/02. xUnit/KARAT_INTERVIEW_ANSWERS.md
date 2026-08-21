# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/09. Unit Testing/02. xUnit`

---

#### Q1. (R) A teammate refactors `DiscountTheoryTests` into separate methods "for clarity." CI passes locally but a new `[Fact]` fails intermittently on the build agent. Review:

```csharp
public sealed class DiscountRefactorTests
{
    private readonly OrderPricingService _pricing = new();
    private decimal _lastDiscountedAmount;

    [Theory]
    [InlineData(100, 10, 90)]
    [InlineData(200, 25, 150)]
    public void ApplyDiscount_ReturnsExpected(
        decimal subtotal, decimal discountPercent, decimal expected)
    {
        _lastDiscountedAmount = _pricing.ApplyDiscount(subtotal, discountPercent);
        Assert.Equal(expected, _lastDiscountedAmount);
    }

    [Fact]
    public void ApplyDiscount_LastRun_Leaves150AfterBulkRow()
    {
        Assert.Equal(150m, _lastDiscountedAmount);
    }
}
```

What is wrong with mixing `[Fact]` and `[Theory]` here, and how should discount tables be tested instead?

**Answer:** The `[Fact]` reads instance field state left behind by whichever `[Theory]` row ran last in the same class — that is an order-dependent coupling, not an isolated scenario. `[Theory]` is for parameterized rows with self-contained assertions; `[Fact]` is for one fixed scenario with no parameters.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Test design | `[Fact]` depends on side effect of `[Theory]` rows | Pass/fail varies with theory row order or whether the fact runs alone |
| Misuse of attributes | `[Theory]` used but follow-up scenario not expressed as another data row | Hidden coupling — looks like two tests, behaves like one ordered script |
| Isolation | Mutable `_lastDiscountedAmount` shared across methods in one class | Methods in a class run sequentially but still share instance state |

**Fix (priority order):**

1. Delete the dependent `[Fact]` — if "150 after bulk row" matters, add an explicit `[InlineData(200, 25, 150)]` row or a dedicated `[Fact]` that **arranges its own** inputs.
2. Keep each theory row fully self-contained: arrange, act, assert inside the method — no "last result" fields.
3. Use `[Fact]` only when a single AAA path is enough (see **FactTests.cs**); use `[Theory]` + `[InlineData]` / `[MemberData]` for tables (see **DiscountTheoryTests.cs**).
4. If two scenarios must share expensive setup, use a fixture for **read-only** shared data — not for passing values between tests.

**Production takeaway:** Karat uses `[Fact]` vs `[Theory]` to test whether you know parameterized tests are not a loop you can observe from the next test method. Discount tables belong in `[InlineData]`, not in cross-test field memory.

---

#### Q2. (R) A `[Theory]` backed by `[MemberData]` passes in Visual Studio's Test Explorer but fails unpredictably in `dotnet test` on CI. Review the data source and test:

```csharp
private static readonly List<OrderLine> SharedLines =
[
    new() { Sku = "A", Quantity = 2, UnitPrice = 25m },
    new() { Sku = "B", Quantity = 1, UnitPrice = 10m },
];

public static IEnumerable<object[]> SubtotalRows =>
    new List<object[]> { new object[] { SharedLines, 60m } };

[Theory]
[MemberData(nameof(SubtotalRows))]
public void CalculateSubtotal_MemberDataRow(
    IReadOnlyList<OrderLine> lines, decimal expected)
{
    lines.Add(new OrderLine { Sku = "EXTRA", Quantity = 1, UnitPrice = 99m });
    decimal subtotal = _pricing.CalculateSubtotal(lines);
    Assert.Equal(expected, subtotal);
}
```

Identify the misuse of `[Theory]` / `[MemberData]` and why the failure is order-dependent.

**Answer:** The theory mutates `SharedLines`, a static list reused by every row and potentially across test runs — the first execution grows the list, so later runs see the wrong line count and subtotal. `[MemberData]` should supply **fresh, immutable** inputs per row, not shared mutable collections the test body edits.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `lines.Add(...)` mutates shared static data | Expected 60m fails after list already contains `EXTRA` |
| Theory misuse | Single shared `List<OrderLine>` referenced from member data | Re-runs, parallel theories, or future extra rows all stomp the same list |
| Design | `IReadOnlyList<T>` parameter does not imply immutability | API suggests read-only; implementation is a mutable `List<T>` |

**Fix (priority order):**

1. Remove mutation from the test — pass lines that already include every SKU under test, or copy before act: `var copy = lines.ToList();`.
2. Return **new** lists per row from `MemberData` (pattern in **DiscountTheoryTests.cs** `SubtotalMemberData`).
3. Prefer immutable fixtures or `IReadOnlyList` backed by arrays/`List.AsReadOnly()` for shared catalogs.
4. Add a second theory row with empty lines to prove rows do not leak state into each other.

```csharp
public static IEnumerable<object[]> SubtotalRows =>
    new List<object[]>
    {
        new object[]
        {
            new List<OrderLine>
            {
                new() { Sku = "A", Quantity = 2, UnitPrice = 25m },
                new() { Sku = "B", Quantity = 1, UnitPrice = 10m },
            },
            60m,
        },
    };
```

**Production takeaway:** `[MemberData]` is for computed tables, not shared scratch buffers — treat each row like a pure function input. See **Program.cs** QUICK REFERENCE — `[MemberData]` for complex inputs.

---

#### Q3. (R) `OrderCatalogFixture` is extended for integration-style tests. Two methods in the same class pass locally but one fails when run with the full suite. Review:

```csharp
public sealed class OrderCatalogFixture
{
    public OrderPricingService Service { get; } = new();
    public Order StandardOrder { get; } = OrderCatalogFixture.CreateStandardOrder();
    public List<OrderLine> WorkingLines { get; } = new();
}

public sealed class MutableFixtureTests : IClassFixture<OrderCatalogFixture>
{
    private readonly OrderCatalogFixture _fixture;

    public MutableFixtureTests(OrderCatalogFixture fixture) => _fixture = fixture;

    [Fact]
    public void AddPromoLine_IncludesLineInWorkingSet()
    {
        _fixture.WorkingLines.Add(
            new OrderLine { Sku = "PROMO", Quantity = 1, UnitPrice = 5m });
        Assert.Single(_fixture.WorkingLines);
    }

    [Fact]
    public void WorkingLines_StartsEmptyEachTest()
    {
        Assert.Empty(_fixture.WorkingLines);
    }
}
```

What does `IClassFixture<T>` guarantee about instance lifetime, and why does the second test fail?

**Answer:** xUnit creates **one** `OrderCatalogFixture` per test class and injects the same instance into every test method constructor in that class — it does **not** reset mutable state between tests. After `AddPromoLine` runs, `WorkingLines` still holds the promo line when `WorkingLines_StartsEmptyEachTest` runs.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Fixture lifetime | `IClassFixture<T>` = one shared instance per class | Mutable lists on the fixture leak across tests |
| False isolation assumption | Treating fixture like per-test `[SetUp]` | Second test fails depending on method order within the class |
| Design | Expensive **read-only** seed mixed with per-test scratch state | Same object used for two incompatible purposes |

**Fix (priority order):**

1. Keep fixtures **immutable** after construction — match **OrderCatalogFixture.cs** (`Service`, `StandardOrder` only).
2. Put per-test mutable state in local variables or a fresh `List<OrderLine>` inside each test method.
3. If reset is unavoidable, implement `IDisposable` on the fixture and clear `WorkingLines` in `Dispose()` — xUnit calls it after all tests in the class finish (not between each test).
4. For per-test reset, use a constructor on the test class or `IAsyncLifetime` per test — not shared mutable fixture fields.

**Production takeaway:** `IClassFixture<T>` shares setup cost, not test isolation. ClassFixtureTests in this chapter shares a **read-only** catalog — Karat checks whether you would add mutable `WorkingLines` and expect a clean slate.

---

#### Q4. (R) Two test classes should share one catalog database seed via `ICollectionFixture<OrderCatalogFixture>`, but tests flake only on multi-core CI agents. Review:

```csharp
[CollectionDefinition("OrderCatalog")]
public sealed class OrderCatalogCollection : ICollectionFixture<OrderCatalogFixture> { }

[Collection("OrderCatalog")]
public sealed class CatalogTestsA : IClassFixture<OrderCatalogFixture>
{
    private readonly OrderCatalogFixture _fixture;
    public CatalogTestsA(OrderCatalogFixture fixture) => _fixture = fixture;

    [Fact]
    public void A_MarksCatalogLoaded() => _fixture.StandardOrder.OrderId = "LOADED";
}

// Missing [Collection("OrderCatalog")] — "just uses the fixture type"
public sealed class CatalogTestsB : IClassFixture<OrderCatalogFixture>
{
    private readonly OrderCatalogFixture _fixture;
    public CatalogTestsB(OrderCatalogFixture fixture) => _fixture = fixture;

    [Fact]
    public void B_ExpectsDefaultOrderId()
    {
        Assert.Equal("ORD-TEST-001", _fixture.StandardOrder.OrderId);
    }
}
```

What parallelization and fixture-lifetime mistakes stack here, and how do you fix them?

**Answer:** `CatalogTestsB` is **not** in the `"OrderCatalog"` collection, so xUnit gives it a **separate** `OrderCatalogFixture` and may run it **in parallel** with collection members — while `CatalogTestsA` mutates shared `StandardOrder`, creating cross-class interference and nondeterministic order IDs. Combining `[Collection]` with redundant `IClassFixture<T>` on the same class also confuses which fixture instance applies.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Collection membership | `CatalogTestsB` missing `[Collection("OrderCatalog")]` | Different fixture instance; parallel with collection tests |
| Parallelism | Non-collection classes run in parallel by default | **ParallelExecutionTests.cs** — unrelated classes use separate threads |
| Shared mutation | `StandardOrder.OrderId = "LOADED"` on collection-shared object | Other tests in the collection see mutated order |
| Fixture wiring | `IClassFixture<OrderCatalogFixture>` **and** `[Collection]` for same type | Extra instance per class unless only collection fixture is used |

**Fix (priority order):**

1. Add `[Collection("OrderCatalog")]` to **every** class that must share the seed and serialize together — see **CollectionFixtureTestsA.cs** / **CollectionFixtureTestsB.cs**.
2. Inject `OrderCatalogFixture` via collection fixture only; drop redundant `IClassFixture<OrderCatalogFixture>` on collection classes unless you intentionally want a second instance.
3. Stop mutating shared fixture objects — treat `StandardOrder` as read-only; clone before changes.
4. If global mutation is required, keep all writers/readers in one collection and document serial execution — do not rely on "we only have two classes."

**Production takeaway:** `[CollectionDefinition]` + `[Collection]` is xUnit's lock for shared expensive resources **and** parallel scheduling. Missing `[Collection]` is a classic CI-only flake when cores > 1. See **OrderCatalogCollection.cs** and **Program.cs** Section 7–8.

---

#### Q5. (M) A developer asks: "We have three test classes using `IClassFixture<OrderCatalogFixture>`. How many `OrderCatalogFixture` instances does xUnit create for one `dotnet test` run, and when does the fixture constructor run?" They also add `IDisposable` to the fixture to reset state. Explain xUnit's lifecycle for class vs collection fixtures and whether `IDisposable` on the fixture replaces proper test isolation.

**Answer:** Three unrelated classes each implementing `IClassFixture<OrderCatalogFixture>` get **three separate instances** — one per class, constructed before the first test in that class runs and disposed (if `IDisposable`) after the last test in that class finishes. Classes in the same `[Collection("OrderCatalog")]` share **one** collection fixture instance for the whole collection, and tests in that collection do not run in parallel with each other.

- **`IClassFixture<T>`:** One `T` per test class; ctor runs once per class; same reference injected into every test method ctor on that class; tests in the class run **sequentially**.
- **`ICollectionFixture<T>`:** One `T` shared by all classes with the same `[Collection("Name")]`; ctor runs once for the collection; disposed after all tests in all member classes complete.
- **`IDisposable` on fixture:** Called after the **class** (class fixture) or **collection** (collection fixture) completes — **not** between individual tests. Clearing mutable state in `Dispose()` does not help test B if test A mutated the fixture mid-class.
- **Proper isolation:** Immutable fixture data, per-test local state, or `IAsyncLifetime.InitializeAsync` per test class instance — not "reset in Dispose only."
- **Contrast with this chapter:** `ClassFixtureTests` gets its own fixture; `CollectionFixtureTestsA/B` share one — compare subtotals and order IDs across files to prove instance boundaries.

**Production takeaway:** Know instance counts when diagnosing "why did my seed run twice" or "why do these classes see different catalogs." `IDisposable` is for teardown of external resources (files, connections), not a substitute for tests that do not mutate shared state.

---

#### Q6. (P) Your pipeline runs `dotnet test` on a 4-core Linux agent with no extra flags. Over time, unrelated test classes start failing together — one writes a temp file by fixed name, another reads a static `ConcurrentDictionary`, a third assumes an empty in-memory registry. Locally (often single-threaded or ReSharper's sequential runner) everything passes. What xUnit parallelism rules explain this, and what is your prioritized strategy for CI isolation without disabling all parallelism?

**Answer:** xUnit runs **different test classes in parallel** by default while methods inside the same class run sequentially — so static fields, fixed temp paths, and process-wide singletons race across classes on a 4-core agent. Locally, sequential IDE runners or fewer cores hide the overlap.

- **Default rules:** Different classes → parallel; same class → serial; same `[Collection]` → serial with shared collection fixture (**ParallelExecutionTests.cs**, **Program.cs** QUICK REFERENCE).
- **Why CI differs:** Agents use full CPU; `--parallel` / default xUnit worker count > 1; no guaranteed class order; reruns and TRX merge do not reset static process state.
- **Prioritized strategy:**
  1. **Fix tests first** — remove static mutable state, use `Path.GetTempFileName()` or `Guid` suffixes, inject fakes instead of static registries.
  2. **Group offenders** into a named `[Collection("SerialIntegration")]` so they share one synchronization boundary and optional shared fixture.
  3. **Run truly integration-heavy** projects in a separate CI job with `-- xUnit.parallelizeTestCollections=false` or `[assembly: CollectionBehavior(DisableTestParallelization = true)]` only for that assembly — not the whole solution.
  4. **Keep unit tests parallel** — stateless `[Fact]` / `[Theory]` tests on `OrderPricingService`-style pure logic scale well and match **FactTests.cs** / **DiscountTheoryTests.cs** patterns.
  5. **Detect flakes** — repeat `dotnet test --repeat 50` on PR for known parallel-sensitive assemblies; fail on nondeterminism before merge.
- **Avoid:** Global disable of parallelism for every test project because one legacy suite uses static state — that punishes fast unit tests and lengthens every pipeline.

**Production takeaway:** Test isolation in CI is an xUnit scheduling problem plus code hygiene. Prefer collection-scoped serialization over killing parallelism solution-wide — aligns with **Program.cs** Section 10 and production pipelines that rely on multi-core `dotnet test`.

---
