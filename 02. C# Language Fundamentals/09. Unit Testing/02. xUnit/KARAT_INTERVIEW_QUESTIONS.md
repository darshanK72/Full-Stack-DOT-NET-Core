# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/09. Unit Testing/02. xUnit`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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

---

#### Q5. (M) A developer asks: "We have three test classes using `IClassFixture<OrderCatalogFixture>`. How many `OrderCatalogFixture` instances does xUnit create for one `dotnet test` run, and when does the fixture constructor run?" They also add `IDisposable` to the fixture to reset state. Explain xUnit's lifecycle for class vs collection fixtures and whether `IDisposable` on the fixture replaces proper test isolation.

---

#### Q6. (P) Your pipeline runs `dotnet test` on a 4-core Linux agent with no extra flags. Over time, unrelated test classes start failing together — one writes a temp file by fixed name, another reads a static `ConcurrentDictionary`, a third assumes an empty in-memory registry. Locally (often single-threaded or ReSharper's sequential runner) everything passes. What xUnit parallelism rules explain this, and what is your prioritized strategy for CI isolation without disabling all parallelism?

---
